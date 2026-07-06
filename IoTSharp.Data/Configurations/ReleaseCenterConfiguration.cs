using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTSharp.Data.Configurations
{
    /// <summary>
    /// Release Center 发布计划持久化映射。
    /// </summary>
    public class ReleasePlanConfiguration : IEntityTypeConfiguration<ReleasePlan>
    {
        /// <summary>
        /// 配置 ReleasePlan 的主键、枚举字符串存储、外键和常用查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<ReleasePlan> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).HasMaxLength(256);
            builder.Property(c => c.Description).HasMaxLength(1024);
            builder.Property(c => c.PlanType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.ConfirmationPolicy).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.CreatedBy).HasMaxLength(256);
            builder.Property(c => c.UpdatedBy).HasMaxLength(256);

            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted });
            builder.HasIndex(c => new { c.TenantId, c.Status, c.CreatedAt });
            builder.HasIndex(c => c.PackageId);
            builder.HasIndex(c => c.RollbackPackageId);
            builder.HasIndex(c => c.CreatedAt);

            builder.HasOne(c => c.Package).WithMany().HasForeignKey(c => c.PackageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(c => c.RollbackPackage).WithMany().HasForeignKey(c => c.RollbackPackageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);
        }
    }

    /// <summary>
    /// Release Center 发布任务持久化映射。
    /// </summary>
    public class ReleaseTaskConfiguration : IEntityTypeConfiguration<ReleaseTask>
    {
        /// <summary>
        /// 配置 ReleaseTask 的主键、枚举字符串存储、外键和常用查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<ReleaseTask> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TargetType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.TargetKey).HasMaxLength(450);
            builder.Property(c => c.RuntimeType).HasMaxLength(128);
            builder.Property(c => c.InstanceId).HasMaxLength(128);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.Message).HasMaxLength(1024);

            builder.HasIndex(c => new { c.PlanId, c.BatchNo, c.Status });
            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted });
            builder.HasIndex(c => new { c.TargetKey, c.Status });
            builder.HasIndex(c => c.EdgeTaskId);
            builder.HasIndex(c => c.GatewayId);
            builder.HasIndex(c => c.EdgeNodeId);
            builder.HasIndex(c => c.PackageId);

            builder.HasOne(c => c.Plan)
                .WithMany(c => c.Tasks)
                .HasForeignKey(c => c.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Package).WithMany().HasForeignKey(c => c.PackageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(c => c.EdgeTask).WithMany().HasForeignKey(c => c.EdgeTaskId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);
        }
    }

    /// <summary>
    /// Release Center 发布回执持久化映射。
    /// </summary>
    public class ReleaseReceiptConfiguration : IEntityTypeConfiguration<ReleaseReceipt>
    {
        /// <summary>
        /// 配置 ReleaseReceipt 的主键、枚举字符串存储、外键和回执查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<ReleaseReceipt> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TargetType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.TargetKey).HasMaxLength(450);
            builder.Property(c => c.RuntimeType).HasMaxLength(128);
            builder.Property(c => c.InstanceId).HasMaxLength(128);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.Message).HasMaxLength(1024);

            builder.HasIndex(c => new { c.PlanId, c.ReportedAt });
            builder.HasIndex(c => new { c.ReleaseTaskId, c.ReportedAt });
            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted });
            builder.HasIndex(c => new { c.TargetKey, c.Status });
            builder.HasIndex(c => c.EdgeTaskId);
            builder.HasIndex(c => c.GatewayId);
            builder.HasIndex(c => c.EdgeNodeId);

            builder.HasOne(c => c.Plan)
                .WithMany()
                .HasForeignKey(c => c.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.ReleaseTask)
                .WithMany(c => c.Receipts)
                .HasForeignKey(c => c.ReleaseTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);
        }
    }
}
