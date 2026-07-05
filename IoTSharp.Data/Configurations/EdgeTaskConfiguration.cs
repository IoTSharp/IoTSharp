using IoTSharp.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTSharp.Data.Configurations
{
    /// <summary>
    /// Edge 任务持久化映射。
    /// </summary>
    public class EdgeTaskConfiguration : IEntityTypeConfiguration<EdgeTask>
    {
        /// <summary>
        /// 配置 EdgeTask 的主键、枚举字符串存储和常用任务查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<EdgeTask> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ContractVersion).HasMaxLength(64);
            builder.Property(c => c.TaskType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.TargetType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.TargetKey).HasMaxLength(450);
            builder.Property(c => c.RuntimeType).HasMaxLength(128);
            builder.Property(c => c.InstanceId).HasMaxLength(128);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.Message).HasMaxLength(1024);

            builder.HasIndex(c => new { c.GatewayId, c.Status, c.CreatedAt });
            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted });
            builder.HasIndex(c => new { c.TargetKey, c.Status });
            builder.HasIndex(c => c.EdgeNodeId);
            builder.HasIndex(c => c.ExpireAt);

            builder.HasOne(c => c.Gateway).WithMany().HasForeignKey(c => c.GatewayId);
            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);
        }
    }
}
