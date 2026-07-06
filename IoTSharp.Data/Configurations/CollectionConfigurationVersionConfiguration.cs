using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTSharp.Data.Configurations
{
    /// <summary>
    /// 采集配置版本快照持久化映射。
    /// </summary>
    public class CollectionConfigurationVersionConfiguration : IEntityTypeConfiguration<CollectionConfigurationVersion>
    {
        /// <summary>
        /// 配置采集配置版本的主键、字段长度和常用查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<CollectionConfigurationVersion> builder)
        {
            builder.ToTable("CollectionConfigVersions");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ContractVersion).HasMaxLength(64);
            builder.Property(c => c.ConfigurationHash).HasMaxLength(128);
            builder.Property(c => c.SourceType).HasMaxLength(128);
            builder.Property(c => c.SourceId).HasMaxLength(128);
            builder.Property(c => c.SourceVersion).HasMaxLength(64);
            builder.Property(c => c.CreatedBy).HasMaxLength(256);
            builder.Property(c => c.UpdatedBy).HasMaxLength(256);

            builder.HasIndex(c => new { c.GatewayId, c.Version }).IsUnique()
                .HasDatabaseName("IX_ColCfgVer_GwVersion");
            builder.HasIndex(c => new { c.GatewayId, c.CreatedAt })
                .HasDatabaseName("IX_ColCfgVer_GwCreated");
            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted })
                .HasDatabaseName("IX_ColCfgVer_CustTenDel");
            builder.HasIndex(c => c.EdgeNodeId)
                .HasDatabaseName("IX_ColCfgVer_EdgeNode");
            builder.HasIndex(c => c.ConfigurationHash)
                .HasDatabaseName("IX_ColCfgVer_Hash");
            builder.HasIndex(c => new { c.SourceType, c.SourceId, c.SourceVersion })
                .HasDatabaseName("IX_ColCfgVer_Source");
            builder.HasIndex(c => c.CustomerId)
                .HasDatabaseName("IX_ColCfgVer_Customer");
            builder.HasIndex(c => c.TenantId)
                .HasDatabaseName("IX_ColCfgVer_Tenant");

            builder.HasOne(c => c.Gateway)
                .WithMany()
                .HasForeignKey(c => c.GatewayId)
                .HasConstraintName("FK_ColCfgVer_Device")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId)
                .HasConstraintName("FK_ColCfgVer_Tenant");
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId)
                .HasConstraintName("FK_ColCfgVer_Customer");
        }
    }
}
