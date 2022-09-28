using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data.Configurations
{

 
    public class TelemetryDataConfiguration : IEntityTypeConfiguration<TelemetryData>
    {
        public void Configure(EntityTypeBuilder<TelemetryData> builder)
        {
            builder.HasIndex(c => new { c.DeviceId });
            builder.HasIndex(c => new { c.DeviceId, c.KeyName });
            builder.HasIndex(c => new { c.KeyName });
            builder.HasKey(c => new { c.DeviceId, c.KeyName, c.DateTime });
            builder.Property(t => t.DeviceId)
                    .IsRequired();
            builder.Property(t => t.KeyName)
                 .IsRequired();
            builder.Property(t => t.DateTime)
                 .IsRequired();
        }
    }
}
