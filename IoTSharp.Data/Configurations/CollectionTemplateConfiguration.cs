using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTSharp.Data.Configurations
{
    /// <summary>
    /// Collection Template 持久化映射。
    /// </summary>
    public class CollectionTemplateConfiguration : IEntityTypeConfiguration<CollectionTemplate>
    {
        /// <summary>
        /// 配置采集模板根模型、子模板关系、枚举字符串存储和常用查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<CollectionTemplate> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TemplateKey).HasMaxLength(128);
            builder.Property(c => c.Name).HasMaxLength(256);
            builder.Property(c => c.SemanticModelId).HasMaxLength(128);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.CreatedBy).HasMaxLength(256);
            builder.Property(c => c.UpdatedBy).HasMaxLength(256);

            builder.HasIndex(c => new { c.ProductId, c.TemplateKey, c.Deleted });
            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted });
            builder.HasIndex(c => new { c.ProductId, c.Status });
            builder.HasIndex(c => c.SemanticModelId);

            builder.HasOne(c => c.Product)
                .WithMany(c => c.CollectionTemplates)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);

            builder.HasOne(c => c.Protocol)
                .WithOne(c => c.CollectionTemplate)
                .HasForeignKey<CollectionProtocolTemplate>(c => c.CollectionTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Connections)
                .WithOne(c => c.CollectionTemplate)
                .HasForeignKey(c => c.CollectionTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Points)
                .WithOne(c => c.CollectionTemplate)
                .HasForeignKey(c => c.CollectionTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// 协议模板持久化映射。
    /// </summary>
    public class CollectionProtocolTemplateConfiguration : IEntityTypeConfiguration<CollectionProtocolTemplate>
    {
        /// <summary>
        /// 配置协议模板的主键和协议字段存储方式。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<CollectionProtocolTemplate> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Protocol).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.ProtocolKind).HasMaxLength(128);
            builder.HasIndex(c => c.CollectionTemplateId).IsUnique();
        }
    }

    /// <summary>
    /// 连接模板持久化映射。
    /// </summary>
    public class CollectionConnectionTemplateConfiguration : IEntityTypeConfiguration<CollectionConnectionTemplate>
    {
        /// <summary>
        /// 配置连接模板字段长度和模板内连接键索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<CollectionConnectionTemplate> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.ConnectionKey).HasMaxLength(128);
            builder.Property(c => c.ConnectionName).HasMaxLength(256);
            builder.Property(c => c.Transport).HasMaxLength(64);
            builder.Property(c => c.EndpointRef).HasMaxLength(256);
            builder.Property(c => c.Host).HasMaxLength(256);
            builder.Property(c => c.SerialPort).HasMaxLength(128);
            builder.Property(c => c.AuthType).HasMaxLength(128);
            builder.HasIndex(c => new { c.CollectionTemplateId, c.ConnectionKey });
        }
    }

    /// <summary>
    /// 点位模板持久化映射。
    /// </summary>
    public class CollectionPointTemplateConfiguration : IEntityTypeConfiguration<CollectionPointTemplate>
    {
        /// <summary>
        /// 配置点位模板字段长度、枚举字符串存储和点位子模型关系。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<CollectionPointTemplate> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.ConnectionKey).HasMaxLength(128);
            builder.Property(c => c.PointKey).HasMaxLength(128);
            builder.Property(c => c.SemanticId).HasMaxLength(128);
            builder.Property(c => c.BindingId).HasMaxLength(128);
            builder.Property(c => c.Name).HasMaxLength(256);
            builder.Property(c => c.DisplayName).HasMaxLength(256);
            builder.Property(c => c.SourceType).HasMaxLength(128);
            builder.Property(c => c.Address).HasMaxLength(512);
            builder.Property(c => c.FieldPath).HasMaxLength(512);
            builder.Property(c => c.RawValueType).HasMaxLength(64);
            builder.Property(c => c.ValueType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.Access).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.Quantity).HasMaxLength(128);
            builder.Property(c => c.Unit).HasMaxLength(64);

            builder.HasIndex(c => new { c.CollectionTemplateId, c.PointKey });
            builder.HasIndex(c => new { c.CollectionTemplateId, c.SemanticId });
            builder.HasIndex(c => new { c.CollectionTemplateId, c.BindingId });
            builder.HasIndex(c => c.ConnectionKey);

            builder.HasOne(c => c.SamplingPolicy)
                .WithOne(c => c.PointTemplate)
                .HasForeignKey<CollectionSamplingPolicy>(c => c.PointTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Mapping)
                .WithOne(c => c.PointTemplate)
                .HasForeignKey<CollectionMappingPolicy>(c => c.PointTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Transforms)
                .WithOne(c => c.PointTemplate)
                .HasForeignKey(c => c.PointTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// 值转换模板持久化映射。
    /// </summary>
    public class CollectionTransformTemplateConfiguration : IEntityTypeConfiguration<CollectionTransformTemplate>
    {
        /// <summary>
        /// 配置转换模板的枚举字符串存储和执行顺序索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<CollectionTransformTemplate> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.TransformType).HasConversion<string>().HasMaxLength(64);
            builder.HasIndex(c => new { c.PointTemplateId, c.Order });
        }
    }

    /// <summary>
    /// 采样策略模板持久化映射。
    /// </summary>
    public class CollectionSamplingPolicyConfiguration : IEntityTypeConfiguration<CollectionSamplingPolicy>
    {
        /// <summary>
        /// 配置采样策略字段和触发枚举存储方式。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<CollectionSamplingPolicy> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Trigger).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.AggregateHint).HasMaxLength(128);
            builder.Property(c => c.Group).HasMaxLength(128);
            builder.HasIndex(c => c.PointTemplateId).IsUnique();
        }
    }

    /// <summary>
    /// 平台映射策略持久化映射。
    /// </summary>
    public class CollectionMappingPolicyConfiguration : IEntityTypeConfiguration<CollectionMappingPolicy>
    {
        /// <summary>
        /// 配置映射目标、值类型和模板内点位唯一关系。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<CollectionMappingPolicy> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.TargetType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.TargetName).HasMaxLength(256);
            builder.Property(c => c.ValueType).HasConversion<string>().HasMaxLength(64);
            builder.Property(c => c.DisplayName).HasMaxLength(256);
            builder.Property(c => c.Unit).HasMaxLength(64);
            builder.Property(c => c.Group).HasMaxLength(128);
            builder.HasIndex(c => c.PointTemplateId).IsUnique();
        }
    }
}
