using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Text.Json;

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
            .HasConversion(
                value => SerializeAuditResult(value),
                value => DeserializeAuditResult(value))
            .HasColumnType("jsonb");
        }

        /// <summary>
        /// 将领域层的审计结果摘要编码为 PostgreSQL jsonb 可接受的 JSON 字符串。
        /// </summary>
        /// <param name="value">审计结果摘要。</param>
        /// <returns>合法的 JSON 字符串。</returns>
        private static string SerializeAuditResult(string value)
            => JsonSerializer.Serialize(value ?? string.Empty);

        /// <summary>
        /// 将 PostgreSQL jsonb 值还原为领域层摘要，并兼容历史对象或数组结果。
        /// </summary>
        /// <param name="value">数据库返回的 JSON 文本。</param>
        /// <returns>字符串标量值，或非字符串 JSON 的原始文本。</returns>
        private static string DeserializeAuditResult(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            using var document = JsonDocument.Parse(value);
            return document.RootElement.ValueKind == JsonValueKind.String
                ? document.RootElement.GetString() ?? string.Empty
                : document.RootElement.GetRawText();
        }
    }
}
