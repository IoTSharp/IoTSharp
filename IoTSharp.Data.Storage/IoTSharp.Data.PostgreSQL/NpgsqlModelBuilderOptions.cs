using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace IoTSharp.Data.PostgreSQL
{
    public class NpgsqlModelBuilderOptions : IDataBaseModelBuilderOptions
    {
        public NpgsqlModelBuilderOptions()
        {
        }

        public IInfrastructure<IServiceProvider> Infrastructure { get; set; }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

            modelBuilder.Entity<TelemetryData>()
            .Property(b => b.DateTime)
            .HasColumnType("timestamp with time zone");
            modelBuilder.Entity<TelemetryData>()
            .Property(b => b.Value_DateTime)
            .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<TelemetryLatest>()
            .Property(b => b.DateTime)
            .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<TelemetryLatest>()
            .Property(b => b.Value_DateTime)
            .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<AttributeLatest>()
            .Property(b => b.DateTime)
            .HasColumnType("timestamp with time zone");
            modelBuilder.Entity<AttributeLatest>()
            .Property(b => b.Value_DateTime)
            .HasColumnType("timestamp with time zone");

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
    }
}