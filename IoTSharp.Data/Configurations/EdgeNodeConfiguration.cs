using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTSharp.Data.Configurations
{
    /// <summary>
    /// Edge 节点持久化映射。
    /// </summary>
    public class EdgeNodeConfiguration : IEntityTypeConfiguration<EdgeNode>
    {
        /// <summary>
        /// 配置 EdgeNode 的主键、Gateway 一一对应关系和常用查询索引。
        /// </summary>
        /// <param name="builder">实体配置构建器。</param>
        public void Configure(EntityTypeBuilder<EdgeNode> builder)
        {
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.GatewayId).IsUnique();
            builder.HasIndex(c => new { c.CustomerId, c.TenantId, c.Deleted });
            builder.HasIndex(c => new { c.RuntimeType, c.Status });
            builder.HasIndex(c => c.InstanceId);
            builder.HasOne(c => c.Gateway).WithOne().HasForeignKey<EdgeNode>(c => c.GatewayId);
            builder.HasOne(c => c.Tenant).WithMany().HasForeignKey(c => c.TenantId);
            builder.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);
        }
    }
}
