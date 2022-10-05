using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data.MySQL
{
    public class MySqlModelBuilderOptions : IDataBaseModelBuilderOptions
    {
        public MySqlModelBuilderOptions()
        {

        }

        public IInfrastructure<IServiceProvider> Infrastructure { get; set; }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            var sv = Infrastructure.GetService<ServerVersion>();

            modelBuilder.Entity<TelemetryData>()
            .Property(b => b.DateTime)
            .HasColumnType("timestamp");
            modelBuilder.Entity<TelemetryData>()
            .Property(b => b.Value_DateTime)
            .HasColumnType("timestamp");


            modelBuilder.Entity<TelemetryLatest>()
            .Property(b => b.DateTime)
            .HasColumnType("timestamp");
            modelBuilder.Entity<TelemetryLatest>()
            .Property(b => b.Value_DateTime)
            .HasColumnType("timestamp");

            modelBuilder.Entity<AttributeLatest>()
            .Property(b => b.DateTime)
            .HasColumnType("timestamp");
            modelBuilder.Entity<AttributeLatest>()
            .Property(b => b.Value_DateTime)
            .HasColumnType("timestamp");

            modelBuilder.Entity<AuditLog>()
            .Property(b => b.ActiveDateTime)
            .HasColumnType("timestamp");

            if (sv.Supports.Json)
            {
                    modelBuilder.Entity<TelemetryData>()
                    .Property(b => b.Value_Json)
                    .HasColumnType("JSON");
                    modelBuilder.Entity<TelemetryData>()
                    .Property(b => b.Value_Json)
                    .HasColumnType("JSON");

                    modelBuilder.Entity<AttributeLatest>()
                    .Property(b => b.Value_Json)
                    .HasColumnType("JSON");

                    modelBuilder.Entity<TelemetryLatest>()
                    .Property(b => b.Value_Json)
                    .HasColumnType("JSON");

                modelBuilder.Entity<AuditLog>()
                .Property(b => b.ActionData)
                .HasColumnType("JSON");

                modelBuilder.Entity<AuditLog>()
                .Property(b => b.ActionResult)
                .HasColumnType("JSON");
            }
        }
    }
}
