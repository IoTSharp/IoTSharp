using Microsoft.EntityFrameworkCore;
using SonnetDB.EntityFrameworkCore.Extensions;

namespace IoTSharp.SonnetDb.Contracts;

/// <summary>用独立关系表复现 IoTSharp 配置租约的参数和多表事务合同，不连接业务数据库。</summary>
internal sealed class ContractDbContext(string connectionString) : DbContext
{
    internal DbSet<LeaseNode> Nodes => Set<LeaseNode>();

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSonnetDB(connectionString);

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var node = modelBuilder.Entity<LeaseNode>();
        node.ToTable("ContractEdgeNodes");
        node.HasKey(item => item.Id);
        node.HasIndex(item => item.GatewayId).IsUnique();
        node.Property(item => item.ConfigurationOperationExpiresAt).HasColumnType("DATETIME2");
        node.Property(item => item.ConfigurationOperationOwner).HasMaxLength(256);
        Configure<ConfigurationVersion>(modelBuilder, "ContractVersions");
        Configure<Assignment>(modelBuilder, "ContractAssignments");
        Configure<EdgeTask>(modelBuilder, "ContractTasks");
        Configure<Receipt>(modelBuilder, "ContractReceipts");
        Configure<Audit>(modelBuilder, "ContractAudit");
        modelBuilder.Entity<BusinessRow<ConfigurationVersion>>()
            .HasIndex(item => new { item.GatewayId, item.Version }).IsUnique();
    }

    /// <summary>分别映射五种业务记录，保留共同的操作编号以便提交未知后对账。</summary>
    private static void Configure<T>(ModelBuilder modelBuilder, string table) where T : class
    {
        var row = modelBuilder.Entity<BusinessRow<T>>();
        row.ToTable(table);
        row.HasKey(item => item.Id);
        row.HasIndex(item => item.OperationId);
    }

    /// <summary>复现业务的 nullable GUID、UTC 时钟、布尔条件和三个 setter；必须检查精确影响行数。</summary>
    internal Task<int> AcquireAsync(Guid gatewayId, Guid operationId, CancellationToken token)
    {
        var now = DateTime.UtcNow;
        return Nodes.Where(node => node.GatewayId == gatewayId && !node.Deleted
                && (node.ConfigurationOperationId == null || node.ConfigurationOperationExpiresAt == null
                    || node.ConfigurationOperationExpiresAt <= now))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(node => node.ConfigurationOperationId, operationId)
                .SetProperty(node => node.ConfigurationOperationExpiresAt, now.AddMinutes(5))
                .SetProperty(node => node.ConfigurationOperationOwner, "contract-consumer"), token);
    }

    /// <summary>仅释放同一 Gateway 上属于当前操作的租约。</summary>
    internal Task<int> ReleaseAsync(Guid gatewayId, Guid operationId, CancellationToken token)
        => Nodes.Where(node => node.GatewayId == gatewayId && node.ConfigurationOperationId == operationId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(node => node.ConfigurationOperationId, (Guid?)null)
                .SetProperty(node => node.ConfigurationOperationExpiresAt, (DateTime?)null)
                .SetProperty(node => node.ConfigurationOperationOwner, (string?)null), token);

    /// <summary>逐阶段写入配置版本、assignment、任务、回执和审计，每阶段都实际执行 SaveChanges。</summary>
    internal async Task WriteStageAsync(int stage, Guid operationId, Guid gatewayId, long version, CancellationToken token)
    {
        switch (stage)
        {
            case 0: Add(Row<ConfigurationVersion>()); break;
            case 1: Add(Row<Assignment>()); break;
            case 2: Add(Row<EdgeTask>()); break;
            case 3: Add(Row<Receipt>()); break;
            case 4: Add(Row<Audit>()); break;
            default: throw new ArgumentOutOfRangeException(nameof(stage));
        }
        await SaveChangesAsync(token);
        BusinessRow<T> Row<T>() where T : class => new()
        {
            Id = Guid.NewGuid(),
            OperationId = operationId,
            GatewayId = gatewayId,
            Version = version
        };
    }

    /// <summary>从五张表独立读取同一操作的记录数量，不能以没有异常代替持久化对账。</summary>
    internal async Task<int[]> ReadOperationAsync(Guid operationId, CancellationToken token) =>
    [
        await Set<BusinessRow<ConfigurationVersion>>().CountAsync(item => item.OperationId == operationId, token),
        await Set<BusinessRow<Assignment>>().CountAsync(item => item.OperationId == operationId, token),
        await Set<BusinessRow<EdgeTask>>().CountAsync(item => item.OperationId == operationId, token),
        await Set<BusinessRow<Receipt>>().CountAsync(item => item.OperationId == operationId, token),
        await Set<BusinessRow<Audit>>().CountAsync(item => item.OperationId == operationId, token)
    ];
}

internal sealed class LeaseNode
{
    public Guid Id { get; set; }
    public Guid GatewayId { get; set; }
    public bool Deleted { get; set; }
    public Guid? ConfigurationOperationId { get; set; }
    public DateTime? ConfigurationOperationExpiresAt { get; set; }
    public string? ConfigurationOperationOwner { get; set; }
}

internal sealed class BusinessRow<T> where T : class
{
    public Guid Id { get; set; }
    public Guid OperationId { get; set; }
    public Guid GatewayId { get; set; }
    public long Version { get; set; }
}

internal sealed class ConfigurationVersion;
internal sealed class Assignment;
internal sealed class EdgeTask;
internal sealed class Receipt;
internal sealed class Audit;
