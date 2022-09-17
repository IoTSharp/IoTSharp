using IoTSharp.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace IoTSharp.Data.Configurations
{

    public class DataStorageConfiguration : IEntityTypeConfiguration<DataStorage>
    {
        public void Configure(EntityTypeBuilder<DataStorage> builder)
        {
            builder.HasKey(c => new { c.Catalog, c.DeviceId, c.KeyName });
            builder.HasIndex(c => c.Catalog);
            builder.HasIndex(c => new { c.Catalog, c.DeviceId });
            builder.HasDiscriminator<DataCatalog>(nameof(Data.DataStorage.Catalog))
               .HasValue<DataStorage>(DataCatalog.None)
               .HasValue<AttributeLatest>(DataCatalog.AttributeLatest)
               .HasValue<TelemetryLatest>(DataCatalog.TelemetryLatest)
               .HasValue<ProduceData>(DataCatalog.ProduceData);
        }
    }
}
