using System;
using System.Collections.Generic;

namespace IoTSharp.Contracts
{
    public sealed record EdgeCollectionConfigurationDto
    {
        public string ContractVersion { get; init; } = "edge-collection-v1";
        public Guid EdgeNodeId { get; init; }
        public int Version { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string UpdatedBy { get; init; } = string.Empty;
        public IReadOnlyList<CollectionTaskDto> Tasks { get; init; } = [];
    }

    public sealed record EdgeCollectionConfigurationUpdateDto
    {
        public IReadOnlyList<CollectionTaskDto> Tasks { get; init; } = [];
    }
}
