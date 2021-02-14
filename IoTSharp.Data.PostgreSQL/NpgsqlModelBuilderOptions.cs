using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTSharp.Data.PostgreSQL
{
    public class NpgsqlModelBuilderOptions : IDataBaseModelBuilderOptions
    {
        public NpgsqlModelBuilderOptions()
        {

        }
        public void OnModelCreating(ModelBuilder modelBuilder)
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
    }
}
