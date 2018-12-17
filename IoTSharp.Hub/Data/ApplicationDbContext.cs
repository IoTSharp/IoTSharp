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
                    Database.Migrate();
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

                modelBuilder.Entity<Device>()
                .HasOne(p => p.Tenant)
                .WithMany(b => b.Devices);

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
            modelBuilder.Entity<ClientSideAttribute>()
            .Property(b => b.Value_Json)
            .HasColumnType("jsonb");

            modelBuilder.Entity<ClientSideAttribute>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");

            modelBuilder.Entity<ServerSideAttribute>()
            .Property(b => b.Value_Json)
            .HasColumnType("jsonb");

            modelBuilder.Entity<ServerSideAttribute>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");

            modelBuilder.Entity<SharedSideAttribute>()
            .Property(b => b.Value_Json)
            .HasColumnType("jsonb");

            modelBuilder.Entity<SharedSideAttribute>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");
        }
        private void ForSqlServer(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientSideAttribute>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");
            modelBuilder.Entity<ServerSideAttribute>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");
            modelBuilder.Entity<SharedSideAttribute>()
            .Property(b => b.Value_XML)
            .HasColumnType("xml");
        }
        public DbSet<Tenant> Tenant { get; set; }
        public DbSet<Customer> Customer { get; set; }

        public DbSet<Relationship> Relationship { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<ClientSideAttribute> ClientSideAttribute { get; set; }
        public DbSet<ServerSideAttribute> ServerSideAttribute { get; set; }
        public DbSet<SharedSideAttribute> SharedSideAttribute { get; set; }


    }
}