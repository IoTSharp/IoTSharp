using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
namespace IoTSharp.Hub.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        IConfiguration _configuration;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
            var _DataBase = configuration["DataBase"] ?? "sqlite";
            if (Enum.TryParse(_DataBase, out DatabaseType databaseType))
            {
                DatabaseType = databaseType;
            }
            if (this.Database.EnsureCreated())
            {
                if (Database.GetPendingMigrations().Count() > 0)
                {
                //    Database.Migrate();
                }
            }
        }
        public DatabaseType DatabaseType { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer>()
                .HasOne(p => p.Tenant)
                .WithMany(b => b.Customers);

            modelBuilder.Entity<Device>()
                .HasOne(p => p.Customer)
                .WithMany(b => b.Devices);

            //modelBuilder.Entity<KeyValueSharedSide>()
            //    .HasOne(p => p.Device )
            //    .WithMany(b => b.SharedSide);



            //modelBuilder.Entity<KeyValueServerSide>()
            //    .HasOne(p => p.Device)
            //    .WithMany(b => b.ServerSide);


            //   modelBuilder.Entity<KeyValueClientSide>()
            //       .HasOne(p => p.Device)
            //       .WithMany(b=>b.ClientSide);

            //   modelBuilder.Entity<KevValueTelemetry>()
            //.HasOne(p => p.Device)
            //.WithMany(b => b.Telemetry);

            //   modelBuilder.Entity<KeyValueDeviceLatest>()
            //    .HasOne(p => p.Device)
            //   .WithMany(b => b.DeviceLatest);



            //   modelBuilder.Entity<Device>()
            //.HasOne(p => p.Tenant)
            //.WithMany(b => b.Devices);
            //   modelBuilder.Entity<Device>()
            //.HasOne(p => p.Tenant)
            //.WithMany(b => b.Devices);

            modelBuilder.Entity<Relationship>()
             .HasOne(p => p.Tenant);
            modelBuilder.Entity<Relationship>()
          .HasOne(p => p.Customer);

            modelBuilder.Entity<Relationship>()
            .HasOne(p => p.Identity);
            switch (DatabaseType)
            {
                case DatabaseType.mssql:
                    ForSqlServer(modelBuilder);
                    break;
                case DatabaseType.npgsql:
                    ForNpgsql(modelBuilder);
                    break;
                case DatabaseType.sqlite:
                    break;
                default:
                    break;
            }
        }

        private void ForNpgsql(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeyValueClientSide>()
            .Property(b => b.Value_Json)
            .HasColumnType("jsonb");

            modelBuilder.Entity<KeyValueClientSide>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");

            modelBuilder.Entity<KeyValueServerSide>()
            .Property(b => b.Value_Json)
            .HasColumnType("jsonb");

            modelBuilder.Entity<KeyValueServerSide>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");

            modelBuilder.Entity<KeyValueSharedSide>()
            .Property(b => b.Value_Json)
            .HasColumnType("jsonb");

            modelBuilder.Entity<KeyValueSharedSide>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");
        }
        private void ForSqlServer(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeyValueClientSide>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");
            modelBuilder.Entity<KeyValueServerSide>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");
            modelBuilder.Entity<KeyValueSharedSide>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");
        }
        public DbSet<Tenant> Tenant { get; set; }
        public DbSet<Customer> Customer { get; set; }

        public DbSet<Relationship> Relationship { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<KeyValueClientSide> ClientSide { get; set; }
        public DbSet<KeyValueServerSide> ServerSide { get; set; }
        public DbSet<KeyValueSharedSide> SharedSide { get; set; }
        public DbSet<KevValueTelemetry> TelemetryData { get; set; }
        public DbSet<KeyValueDeviceLatest> DeviceLatest{ get; set; }


    }
}