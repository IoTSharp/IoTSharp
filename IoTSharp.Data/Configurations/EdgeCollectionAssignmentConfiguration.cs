using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTSharp.Data.Configurations
{
    /// <summary>
    /// Edge 采集配置分配持久化映射。
    /// </summary>
    public class EdgeCollectionAssignmentConfiguration : IEntityTypeConfiguration<EdgeCollectionAssignment>
    {
        /// <summary>
        /// 配置 EdgeCollectionAssignment 的主键、枚举字符串存储和配置分配查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<EdgeCollectionAssignment> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ContractVersion).HasMaxLength(64);
            builder.Property(c => c.TargetType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.TargetKey).HasMaxLength(450);
            builder.Property(c => c.RuntimeType).HasMaxLength(128);
            builder.Property(c => c.InstanceId).HasMaxLength(128);
            builder.Property(c => c.ConfigurationHash).HasMaxLength(128);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.SourceType).HasMaxLength(128);
            builder.Property(c => c.SourceId).HasMaxLength(128);
            builder.Property(c => c.SourceVersion).HasMaxLength(64);
            builder.Property(c => c.CreatedBy).HasMaxLength(256);
            builder.Property(c => c.UpdatedBy).HasMaxLength(256);

            builder.HasIndex(c => new { c.GatewayId, c.Status, c.ConfigurationVersion });
            builder.HasIndex(c => new { c.GatewayId, c.ConfigurationVersion });
            builder.HasIndex(c => new { c.TargetKey, c.Status });
            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted });
            builder.HasIndex(c => c.EdgeNodeId);
            builder.HasIndex(c => c.ConfigurationHash);
            builder.HasIndex(c => c.AssignedAt);

            builder.HasOne(c => c.Gateway)
                .WithMany()
                .HasForeignKey(c => c.GatewayId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);
        }
    }
}
