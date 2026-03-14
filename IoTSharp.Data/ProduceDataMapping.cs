using IoTSharp.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    /// <summary>
    /// Maps a product (Produce) abstract key to a real device's key.
    /// This allows virtual aggregation: reading/writing product-level data is transparently
    /// routed through the mapped device keys.
    /// </summary>
    public class ProduceDataMapping
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The product this mapping belongs to.
        /// </summary>
        public Produce Produce { get; set; }

        /// <summary>
        /// The product's abstract key name (references ProduceData.KeyName).
        /// </summary>
        public string ProduceKeyName { get; set; }

        /// <summary>
        /// Whether this mapping is for telemetry data or attribute data.
        /// </summary>
        public DataCatalog DataCatalog { get; set; }

        /// <summary>
        /// The real device that holds the actual data.
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// The device's actual key name (in AttributeLatest or TelemetryLatest).
        /// </summary>
        public string DeviceKeyName { get; set; }

        /// <summary>
        /// Optional description for this mapping.
        /// </summary>
        public string Description { get; set; }

        public bool Deleted { get; set; }
    }
}
