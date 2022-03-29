using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IoTSharp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
     

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            if (Database.IsRelational()  && Database.GetPendingMigrations().Any())
            {
                Database.Migrate();
            }
         
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>().HasOne(c => c.DeviceIdentity).WithOne(c => c.Device).HasForeignKey<DeviceIdentity>(c => c.DeviceId);
            modelBuilder.Entity<DataStorage>().HasKey(c => new { c.Catalog, c.DeviceId, c.KeyName });
            modelBuilder.Entity<DataStorage>().HasIndex(c => c.Catalog);
            modelBuilder.Entity<DataStorage>().HasIndex(c => new { c.Catalog, c.DeviceId });
            modelBuilder.Entity<DataStorage>()
           .HasDiscriminator<DataCatalog>(nameof(Data.DataStorage.Catalog))
           .HasValue<DataStorage>(DataCatalog.None)
                  .HasValue<AttributeLatest>(DataCatalog.AttributeLatest)
           .HasValue<TelemetryLatest>(DataCatalog.TelemetryLatest);

            modelBuilder.Entity<AttributeLatest>().HasDiscriminator<DataCatalog>(nameof(Data.DataStorage.Catalog));
            modelBuilder.Entity<TelemetryLatest>().HasDiscriminator<DataCatalog>(nameof(Data.DataStorage.Catalog));
            modelBuilder.Entity<Device>().HasDiscriminator<DeviceType>(nameof(Data.Device.DeviceType)).HasValue<Gateway>(DeviceType.Gateway).HasValue<Device>(DeviceType.Device);
            modelBuilder.Entity<Gateway>().HasDiscriminator<DeviceType>(nameof(Data.Device.DeviceType));
       
            var builder_options= this.GetService<IDataBaseModelBuilderOptions>();
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
        public DbSet<BaseI18N> BaseI18Ns { get; set; }
        public DbSet<AuthorizedKey> AuthorizedKeys { get; set; }

        public DbSet<BaseEvent> BaseEvents { get; set; }
        public DbSet<FlowRule> FlowRules { get; set; }
        public DbSet<Flow> Flows { get; set; }
        public DbSet<FlowOperation> FlowOperations { get; set; }
        public DbSet<DeviceRule> DeviceRules { get; set; }

        public DbSet<RuleTaskExecutor> RuleTaskExecutors { get; set; }

        public DbSet<SubscriptionTask> SubscriptionTasks { get; set; }
        public DbSet<SubscriptionEvent> SubscriptionEvents { get; set; }

        public DbSet<DeviceDiagram> DeviceDiagrams { get; set; }

        public DbSet<DeviceGraph> DeviceGraphs { get; set; }

        public DbSet<DeviceGraphToolBox> DeviceGraphToolBoxes { get; set; }

        public DbSet<DevicePort> DevicePorts { get; set; }
        public DbSet<DevicePortMapping> DevicePortMappings { get; set; }

        public DbSet<DeviceModel> DeviceModels { get; set; }
        public DbSet<DeviceModelCommand> DeviceModelCommands { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Alarm> Alarms { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetRelation> AssetRelations { get; set; }

    }
    
}