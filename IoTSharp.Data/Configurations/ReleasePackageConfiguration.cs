using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTSharp.Data.Configurations
{
    /// <summary>
    /// ReleasePackage 软件包持久化映射。
    /// </summary>
    public class ReleasePackageConfiguration : IEntityTypeConfiguration<ReleasePackage>
    {
        /// <summary>
        /// 配置 ReleasePackage 的主键、枚举字符串存储和常用查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<ReleasePackage> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ContractVersion).HasMaxLength(64);
            builder.Property(c => c.PackageType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.PackageKey).HasMaxLength(128);
            builder.Property(c => c.Name).HasMaxLength(256);
            builder.Property(c => c.Version).HasMaxLength(128);
            builder.Property(c => c.TargetRuntimeType).HasMaxLength(128);
            builder.Property(c => c.TargetRuntimeVersion).HasMaxLength(128);
            builder.Property(c => c.FileName).HasMaxLength(512);
            builder.Property(c => c.ContentType).HasMaxLength(256);
            builder.Property(c => c.Sha256).HasMaxLength(128);
            builder.Property(c => c.BlobPath).HasMaxLength(1024);
            builder.Property(c => c.DownloadToken).HasMaxLength(128);
            builder.Property(c => c.CreatedBy).HasMaxLength(256);
            builder.Property(c => c.UpdatedBy).HasMaxLength(256);

            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted });
            builder.HasIndex(c => new { c.PackageKey, c.Version, c.TargetRuntimeType, c.Deleted });
            builder.HasIndex(c => c.PackageType);
            builder.HasIndex(c => c.Sha256);
            builder.HasIndex(c => c.CreatedAt);

            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);
        }
    }
}
