using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IoTSharp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            if (Database.GetPendingMigrations().Count() > 0)
            {
                Database.Migrate();
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TelemetryData>().HasIndex(c => new { c.DeviceId });
            modelBuilder.Entity<TelemetryData>().HasIndex(c => new { c.DeviceId, c.KeyName });
            modelBuilder.Entity<TelemetryData>().HasIndex(c => new {  c.KeyName });
            modelBuilder.Entity<TelemetryData>().HasIndex(c => new { c.DeviceId, c.KeyName, c.DateTime });
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
            ForNpgsql(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }


        private void ForNpgsql(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TelemetryData>()
            .Property(b => b.Value_Json)
            .HasColumnType("jsonb");

            modelBuilder.Entity<TelemetryData>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");

            modelBuilder.Entity<AttributeLatest>()
            .Property(b => b.Value_Json)
            .HasColumnType("jsonb");

            modelBuilder.Entity<AttributeLatest>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");


            modelBuilder.Entity<TelemetryLatest>()
            .Property(b => b.Value_Json)
            .HasColumnType("jsonb");

            modelBuilder.Entity<TelemetryLatest>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");

            modelBuilder.Entity<AuditLog>()
            .Property(b => b.ActionData)
            .HasColumnType("jsonb");

            modelBuilder.Entity<AuditLog>()
            .Property(b => b.ActionResult)
            .HasColumnType("jsonb");
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

        public DbSet<RetainedMessage> RetainedMessage { get; set; }

        public DbSet<AuthorizedKey> AuthorizedKeys { get; set; }

    }
}