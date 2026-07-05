using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using IoTSharp.Data.Configurations;
using IoTSharp.Contracts;

namespace IoTSharp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfiguration(new DataStorageConfiguration());
            modelBuilder.ApplyConfiguration(new EdgeNodeConfiguration());
            modelBuilder.ApplyConfiguration(new EdgeTaskConfiguration());
            modelBuilder.ApplyConfiguration(new EdgeTaskReceiptConfiguration());
            modelBuilder.Entity<AttributeLatest>().HasDiscriminator<DataCatalog>(nameof(Data.DataStorage.Catalog));
            modelBuilder.Entity<TelemetryLatest>().HasDiscriminator<DataCatalog>(nameof(Data.DataStorage.Catalog));
            modelBuilder.Entity<ProductData>().HasDiscriminator<DataCatalog>(nameof(Data.DataStorage.Catalog));

            modelBuilder.Entity<Device>().HasOne(c => c.DeviceIdentity).WithOne(c => c.Device).HasForeignKey<DeviceIdentity>(c => c.DeviceId);
            modelBuilder.Entity<Device>().HasOne(c => c.Product).WithMany(c => c.Devices).HasForeignKey("ProductId");
            modelBuilder.Entity<ProductCommand>().HasOne(c => c.Product).WithMany(c => c.Commands).HasForeignKey(c => c.ProductId);
            modelBuilder.Entity<ProductCommand>().HasIndex(c => new { c.ProductId, c.CommandStatus });
            modelBuilder.Entity<Device>().HasIndex(nameof(IoTSharp.Data.Device.CustomerId), nameof(IoTSharp.Data.Device.TenantId), nameof(IoTSharp.Data.Device.Deleted));
            modelBuilder.Entity<Device>().HasIndex(nameof(IoTSharp.Data.Device.TenantId), nameof(IoTSharp.Data.Device.Deleted));
            modelBuilder.Entity<Device>().HasDiscriminator<DeviceType>(nameof(Data.Device.DeviceType)).HasValue<Gateway>(DeviceType.Gateway).HasValue<Device>(DeviceType.Device);
            modelBuilder.Entity<Gateway>().HasDiscriminator<DeviceType>(nameof(Data.Device.DeviceType));

            var builder_options = this.GetService<IDataBaseModelBuilderOptions>();
            builder_options.Infrastructure = this;
            builder_options.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new TelemetryDataConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Tenant> Tenant { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Relationship> Relationship { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<Gateway> Gateway { get; set; }
        public DbSet<EdgeNode> EdgeNodes { get; set; }
        public DbSet<EdgeTask> EdgeTasks { get; set; }
        public DbSet<EdgeTaskReceipt> EdgeTaskReceipts { get; set; }
        public DbSet<TelemetryData> TelemetryData { get; set; }
        public DbSet<AttributeLatest> AttributeLatest { get; set; }
        public DbSet<DataStorage> DataStorage { get; set; }
        public DbSet<TelemetryLatest> TelemetryLatest { get; set; }
        public DbSet<DeviceIdentity> DeviceIdentities { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }

        public DbSet<BaseDictionaryGroup> BaseDictionaryGroups { get; set; }
        public DbSet<BaseDictionary> BaseDictionaries { get; set; }
        public DbSet<DynamicFormFieldInfo> DynamicFormFieldInfos { get; set; }
        public DbSet<DynamicFormFieldValueInfo> DynamicFormFieldValueInfos { get; set; }
        public DbSet<DynamicFormInfo> DynamicFormInfos { get; set; }
        public DbSet<BaseEvent> BaseEvents { get; set; }
        public DbSet<FlowRule> FlowRules { get; set; }
        public DbSet<Flow> Flows { get; set; }
        public DbSet<FlowOperation> FlowOperations { get; set; }
        public DbSet<DeviceRule> DeviceRules { get; set; }

        public DbSet<RuleTaskExecutor> RuleTaskExecutors { get; set; }

        public DbSet<SubscriptionTask> SubscriptionTasks { get; set; }
        public DbSet<SubscriptionEvent> SubscriptionEvents { get; set; }

        public DbSet<DevicePort> DevicePorts { get; set; }
        public DbSet<DevicePortMapping> DevicePortMappings { get; set; }

        [Obsolete("DeviceModel 已合并到 Product，保留该 DbSet 仅用于历史数据库结构。")]
        public DbSet<DeviceModel> DeviceModels { get; set; }

        [Obsolete("DeviceModelCommand 已合并到 ProductCommand，保留该 DbSet 仅用于历史数据库结构。")]
        public DbSet<DeviceModelCommand> DeviceModelCommands { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Alarm> Alarms { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetRelation> AssetRelations { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<ProductData> ProductDatas { get; set; }

        public DbSet<ProductDictionary> ProductDictionaries { get; set; }

        public DbSet<ProductDataMapping> ProductDataMappings { get; set; }

        public DbSet<ProductCommand> ProductCommands { get; set; }

        public DbSet<AISettings> AISettings { get; set; }
    }

}
