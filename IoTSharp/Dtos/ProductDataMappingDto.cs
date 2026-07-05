using IoTSharp.Contracts;
using System;
using System.Collections.Generic;

namespace IoTSharp.Dtos
{
    /// <summary>
    /// DTO for a single Product-device data mapping entry.
    /// </summary>
    public class ProductDataMappingDto
    {
        public Guid Id { get; set; }
        /// <summary>
        /// The product's abstract key name (from ProductData.KeyName).
        /// </summary>
        public string ProductKeyName { get; set; }
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
    public class SaveProductDataMappingsDto
    {
        public Guid ProductId { get; set; }
        public List<ProductDataMappingDto> Mappings { get; set; }
    }
}
