using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTSharp.Data.Configurations
{
    /// <summary>
    /// Edge 任务回执持久化映射。
    /// </summary>
    public class EdgeTaskReceiptConfiguration : IEntityTypeConfiguration<EdgeTaskReceipt>
    {
        /// <summary>
        /// 配置 EdgeTaskReceipt 的主键、枚举字符串存储和任务回执查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<EdgeTaskReceipt> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ContractVersion).HasMaxLength(64);
            builder.Property(c => c.TargetType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.TargetKey).HasMaxLength(450);
            builder.Property(c => c.RuntimeType).HasMaxLength(128);
            builder.Property(c => c.InstanceId).HasMaxLength(128);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.Message).HasMaxLength(1024);

            builder.HasIndex(c => new { c.TaskId, c.ReportedAt });
            builder.HasIndex(c => new { c.GatewayId, c.ReportedAt });
            builder.HasIndex(c => new { c.GatewayId, c.Status, c.ReportedAt });
            builder.HasIndex(c => new { c.TargetKey, c.Status });
            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted });
            builder.HasIndex(c => c.EdgeNodeId);

            builder.HasOne(c => c.Task)
                .WithMany(c => c.Receipts)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Gateway)
                .WithMany()
                .HasForeignKey(c => c.GatewayId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);
        }
    }
}
