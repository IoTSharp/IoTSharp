using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SonnetDB.Data;
using SonnetDB.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services
{
    /// <summary>
    /// 在数据库事务中原子获取 EdgeNode 配置租约，使配置检查和目标切换跨请求、跨实例串行化。
    /// </summary>
    public sealed class ConfigurationOperationLeaseService
    {
        private const string SonnetDbProviderName = "SonnetDB.EntityFrameworkCore";
        private static readonly TimeSpan AcquisitionTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(5);
        private readonly ApplicationDbContext _context;
        private readonly bool _qualifySonnetDb;

        /// <summary>
        /// 创建使用当前请求数据库上下文的配置写互斥服务。
        /// </summary>
        /// <param name="context">当前请求数据库上下文。</param>
        public ConfigurationOperationLeaseService(ApplicationDbContext context)
            : this(context, qualifySonnetDb: false)
        {
        }

        /// <summary>
        /// 为集成验证显式启用远程 SonnetDB 租约。平台的默认构造函数保持关闭。
        /// </summary>
        /// <param name="context">当前请求数据库上下文。</param>
        /// <param name="qualifySonnetDb">是否运行远程 SonnetDB 租约资格验证。</param>
        internal ConfigurationOperationLeaseService(ApplicationDbContext context, bool qualifySonnetDb)
        {
            _context = context;
            _qualifySonnetDb = qualifySonnetDb;
        }

        /// <summary>
        /// 返回当前 provider 无法提供安全配置事务时的拒绝原因。
        /// </summary>
        internal string UnavailableReason => IsSupportedProvider()
            ? "Gateway configuration operation is already in progress"
            : $"Gateway configuration operations require verified transactional atomic updates; provider {_context.Database.ProviderName} is not supported";

        private bool UsesSonnetDbLease()
            => _qualifySonnetDb && _context.Database.ProviderName == SonnetDbProviderName
                && _context.Database.GetDbConnection() is SndbConnection { ProviderMode: SndbProviderMode.Remote };

        private bool IsSupportedProvider()
            => _context.Database.ProviderName is "Microsoft.EntityFrameworkCore.Sqlite"
                or "Npgsql.EntityFrameworkCore.PostgreSQL"
                or "Microsoft.EntityFrameworkCore.SqlServer"
                or "Pomelo.EntityFrameworkCore.MySql"
                or "Oracle.EntityFrameworkCore" || UsesSonnetDbLease();

        /// <summary>
        /// 保存首次配置操作的 EdgeNode 锚点；并发请求已创建同一锚点时重新读取该记录。
        /// </summary>
        /// <param name="node">当前上下文中新建的 EdgeNode。</param>
        /// <param name="cancellationToken">请求取消令牌。</param>
        /// <returns>已保存或由其他请求创建的同一 Gateway 锚点。</returns>
        internal async Task<EdgeNode> SaveNewNodeAsync(EdgeNode node, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return node;
            }
            catch (DbUpdateException exception) when (exception.Entries.Count == 1 && exception.Entries[0].Entity == node)
            {
                _context.Entry(node).State = EntityState.Detached;
                var existing = await _context.EdgeNodes.FirstOrDefaultAsync(item => item.GatewayId == node.GatewayId
                    && item.Id == node.Id && !item.Deleted, cancellationToken);
                if (existing == null)
                {
                    throw;
                }

                return existing;
            }
        }

        /// <summary>
        /// 原子获取 Gateway 租约，并保持数据库行写锁直到配置操作提交或回滚。
        /// </summary>
        /// <param name="gatewayId">Gateway 设备标识。</param>
        /// <param name="owner">租约持有者显示名。</param>
        /// <param name="cancellationToken">请求取消令牌。</param>
        /// <returns>配置操作事务句柄；资源繁忙时返回空值。</returns>
        internal Task<ConfigurationOperationLease> TryAcquireAsync(
            Guid gatewayId, string owner, CancellationToken cancellationToken = default)
            => TryAcquireManyAsync([gatewayId], owner, cancellationToken);

        /// <summary>
        /// 按稳定顺序在一个事务中获取 Gateway 租约，任一目标繁忙时回滚已获取租约。
        /// </summary>
        /// <param name="gatewayIds">Gateway 标识集合，最多 1000 个。</param>
        /// <param name="owner">租约持有者显示名。</param>
        /// <param name="cancellationToken">请求取消令牌。</param>
        /// <returns>配置操作事务句柄；资源繁忙时返回空值。</returns>
        internal async Task<ConfigurationOperationLease> TryAcquireManyAsync(
            IEnumerable<Guid> gatewayIds, string owner, CancellationToken cancellationToken = default)
        {
            if (!IsSupportedProvider())
            {
                return null;
            }

            var ids = (gatewayIds ?? Enumerable.Empty<Guid>()).Where(id => id != Guid.Empty)
                .Distinct().OrderBy(id => id).Take(1001).ToArray();
            if (ids.Length == 0 || ids.Length > 1000)
            {
                return null;
            }

            owner = string.IsNullOrWhiteSpace(owner) ? "unknown" : owner.Trim();
            owner = owner.Length > 256 ? owner[..256] : owner;
            using var acquisitionCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            acquisitionCancellation.CancelAfter(AcquisitionTimeout);
            var usesSonnetDbLease = UsesSonnetDbLease();
            // 远程 Serializable 必须收到服务端会话及隔离级别确认。
            var transaction = usesSonnetDbLease
                ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, acquisitionCancellation.Token)
                : await _context.Database.BeginTransactionAsync(acquisitionCancellation.Token);
            var leaseId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var leaseAcquired = false;
            try
            {
                foreach (var gatewayId in ids)
                {
                    acquisitionCancellation.Token.ThrowIfCancellationRequested();
                    if (usesSonnetDbLease && !await TryLockSonnetDbNodeAsync(transaction, gatewayId, acquisitionCancellation.Token))
                    {
                        return null;
                    }

                    var affected = await _context.EdgeNodes
                        .Where(node => node.GatewayId == gatewayId && !node.Deleted
                            && (node.ConfigurationOperationId == null
                                || node.ConfigurationOperationExpiresAt == null
                                || node.ConfigurationOperationExpiresAt <= now))
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(node => node.ConfigurationOperationId, leaseId)
                            .SetProperty(node => node.ConfigurationOperationExpiresAt, now.Add(LeaseDuration))
                            .SetProperty(node => node.ConfigurationOperationOwner, owner), acquisitionCancellation.Token);
                    if (affected != 1)
                    {
                        return null;
                    }
                }

                leaseAcquired = true;
                return new ConfigurationOperationLease(_context, transaction, ids, leaseId, usesSonnetDbLease);
            }
            finally
            {
                if (!leaseAcquired)
                {
                    try
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                    }
                    finally
                    {
                        await transaction.DisposeAsync();
                    }
                }
            }
        }

        /// <summary>
        /// 在已确认隔离级别的远程会话中锁定 Gateway 锚点。竞争者可以结束本次请求并重新读取业务状态。
        /// </summary>
        private async Task<bool> TryLockSonnetDbNodeAsync(IDbContextTransaction transaction, Guid gatewayId,
            CancellationToken cancellationToken)
        {
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.Transaction = transaction.GetDbTransaction();
            command.CommandText = """SELECT "Id" FROM "EdgeNodes" WHERE "GatewayId" = @gatewayId AND NOT "Deleted" FOR UPDATE NOWAIT""";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@gatewayId";
            parameter.DbType = DbType.Guid;
            parameter.Value = gatewayId;
            command.Parameters.Add(parameter);
            try
            {
                return await command.ExecuteScalarAsync(cancellationToken) is not null and not DBNull;
            }
            catch (SndbServerException exception) when (exception.Error == SqlRowLockException.NotAvailable)
            {
                return false;
            }
        }

        /// <summary>
        /// 检查指定 Gateway 是否仍有在途配置任务，调用方须已持有相同 Gateway 的配置事务。
        /// </summary>
        /// <param name="gatewayId">Gateway 设备标识。</param>
        /// <param name="cancellationToken">请求取消令牌。</param>
        /// <returns>存在未终结配置任务时返回 true。</returns>
        internal Task<bool> HasConfigurationInFlightAsync(Guid gatewayId, CancellationToken cancellationToken = default)
            => _context.EdgeTasks.AnyAsync(task => task.GatewayId == gatewayId && !task.Deleted
                && task.TaskType == EdgeTaskType.ConfigPullRequest
                && (task.Status == EdgeTaskStatus.Pending || task.Status == EdgeTaskStatus.Sent
                    || task.Status == EdgeTaskStatus.Accepted || task.Status == EdgeTaskStatus.Running), cancellationToken);
    }

    /// <summary>
    /// 配置写操作事务句柄。未显式提交时释放会回滚，防止部分配置、任务或审计持久化。
    /// </summary>
    internal sealed class ConfigurationOperationLease : IAsyncDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IDbContextTransaction _transaction;
        private readonly IReadOnlyList<Guid> _gatewayIds;
        private readonly Guid _leaseId;
        private readonly bool _usesSonnetDbLease;
        private bool _sonnetDbCommitAttempted;
        private bool _completed;

        /// <summary>
        /// 绑定已获取的事务和租约标识，供提交或失败清理使用。
        /// </summary>
        /// <param name="context">当前请求数据库上下文。</param>
        /// <param name="transaction">持有互斥写锁的数据库事务。</param>
        /// <param name="gatewayIds">已获取租约的 Gateway 标识。</param>
        /// <param name="leaseId">本次操作租约标识。</param>
        /// <param name="usesSonnetDbLease">底层事务是否采用 SonnetDB 远程会话。</param>
        internal ConfigurationOperationLease(ApplicationDbContext context, IDbContextTransaction transaction,
            IReadOnlyList<Guid> gatewayIds, Guid leaseId, bool usesSonnetDbLease)
        {
            _context = context;
            _transaction = transaction;
            _gatewayIds = gatewayIds;
            _leaseId = leaseId;
            _usesSonnetDbLease = usesSonnetDbLease;
        }

        /// <summary>
        /// 条件释放仍由本请求持有的租约并提交配置、任务和审计的同一事务。
        /// </summary>
        /// <param name="cancellationToken">请求取消令牌。</param>
        internal async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            if (_completed)
            {
                return;
            }

            if (_sonnetDbCommitAttempted)
            {
                throw new InvalidOperationException("Reconcile the previous gateway configuration commit before starting another operation");
            }

            var affected = await _context.EdgeNodes
                .Where(node => _gatewayIds.Contains(node.GatewayId) && node.ConfigurationOperationId == _leaseId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(node => node.ConfigurationOperationId, (Guid?)null)
                    .SetProperty(node => node.ConfigurationOperationExpiresAt, (DateTime?)null)
                    .SetProperty(node => node.ConfigurationOperationOwner, (string)null), cancellationToken);
            if (affected != _gatewayIds.Count)
            {
                throw new DbUpdateConcurrencyException("Gateway configuration operation lease was lost");
            }

            if (_usesSonnetDbLease)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // SonnetDB ADO 在提交尝试失败后关闭该事务句柄。保留提交异常供调用方对账。
                _sonnetDbCommitAttempted = true;
            }

            await _transaction.CommitAsync(cancellationToken);
            _completed = true;
        }

        /// <summary>
        /// 释放事务；发生拒绝、失败或取消时回滚当前配置操作。
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!_completed && !_sonnetDbCommitAttempted)
                {
                    await _transaction.RollbackAsync(CancellationToken.None);
                }
            }
            finally
            {
                await _transaction.DisposeAsync();
            }
        }
    }
}
