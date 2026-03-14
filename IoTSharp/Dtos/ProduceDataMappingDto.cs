using IoTSharp.Contracts;
using System;
using System.Collections.Generic;

namespace IoTSharp.Dtos
{
    /// <summary>
    /// DTO for a single produce-device data mapping entry.
    /// </summary>
    public class ProduceDataMappingDto
    {
        public Guid Id { get; set; }
        /// <summary>
        /// The product's abstract key name (from ProduceData.KeyName).
        /// </summary>
        public string ProduceKeyName { get; set; }
        /// <summary>
        /// Whether this is a telemetry or attribute mapping.
        /// </summary>
        public DataCatalog DataCatalog { get; set; }
        /// <summary>
        /// The real device id.
        /// </summary>
        public Guid DeviceId { get; set; }
        /// <summary>
        /// The device's actual key name.
        /// </summary>
        public string DeviceKeyName { get; set; }
        /// <summary>
        /// Optional description.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Request body for saving (bulk-replace) the mappings for a product.
    /// </summary>
    public class SaveProduceDataMappingsDto
    {
        public Guid ProduceId { get; set; }
        public List<ProduceDataMappingDto> Mappings { get; set; }
    }
}
