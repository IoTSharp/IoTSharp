using System.Runtime.Serialization;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoTSharp.Contracts.Semantic;

/// <summary>
/// Shared JSON settings for the open IoTSharp Semantic Core v1 contract.
/// </summary>
public static class SemanticCoreJson
{
    /// <summary>
    /// Current Semantic Core schema version.
    /// </summary>
    public const string SchemaVersion = "1.0";

    /// <summary>
    /// Creates System.Text.Json options that match the public wire format.
    /// </summary>
    public static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        return options;
    }
}

/// <summary>
/// Protocol-neutral Semantic Core document shared by IoTSharp, IoTCoWork, edge runtimes, and compatible service wrappers.
/// </summary>
public sealed record SemanticModel
{
    /// <summary>
    /// Semantic Core version; v1 uses "1.0".
    /// </summary>
    public string SchemaVersion { get; init; } = SemanticCoreJson.SchemaVersion;

    /// <summary>
    /// Stable model identifier.
    /// </summary>
    public string ModelId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable model name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional model summary for tools and documentation.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// L2 assets that own semantic points and process nodes.
    /// </summary>
    public List<Asset> Assets { get; init; } = [];

    /// <summary>
    /// L1 semantic points that describe what each value means.
    /// </summary>
    public List<SemanticPoint> SemanticPoints { get; init; } = [];

    /// <summary>
    /// L0 source bindings that connect semantic points to protocol addresses.
    /// </summary>
    public List<ProtocolBinding> ProtocolBindings { get; init; } = [];

    /// <summary>
    /// Optional quantity catalog used by points and derived points.
    /// </summary>
    public List<Quantity> Quantities { get; init; } = [];

    /// <summary>
    /// Optional unit catalog used by points and derived points.
    /// </summary>
    public List<Unit> Units { get; init; } = [];

    /// <summary>
    /// L3 process graph draft. Industry packages must extend this model instead of replacing it.
    /// </summary>
    public ProcessGraph? ProcessGraph { get; init; }

    /// <summary>
    /// Forward-compatible extension slots for later L3/L4 profile work.
    /// </summary>
    public List<SemanticExtensionSlot> ExtensionSlots { get; init; } = [];

    /// <summary>
    /// Non-secret extension metadata.
    /// </summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// L2 asset model: site, area, line, device, component, sensor, actuator, or custom asset node.
/// </summary>
public sealed record Asset
{
    /// <summary>
    /// Stable asset identifier.
    /// </summary>
    public string AssetId { get; init; } = string.Empty;

    /// <summary>
    /// Code-friendly asset name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional display name for UI or documentation.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Optional asset description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Asset type in the L2 hierarchy.
    /// </summary>
    public SemanticAssetType AssetType { get; init; } = SemanticAssetType.Custom;

    /// <summary>
    /// Parent asset identifier when the asset belongs to a hierarchy.
    /// </summary>
    public string? ParentAssetId { get; init; }

    /// <summary>
    /// Stable path from site to this asset, for example ["factory-a", "boiler-room", "boiler-01"].
    /// </summary>
    public List<string> AssetPath { get; init; } = [];

    /// <summary>
    /// Semantic point IDs that belong to this asset.
    /// </summary>
    public List<string> Points { get; init; } = [];

    /// <summary>
    /// Non-owning references to external systems such as IoTSharp, OPC UA, AAS, ERP, MES, or CMMS.
    /// </summary>
    public List<AssetExternalReference> ExternalReferences { get; init; } = [];

    /// <summary>
    /// Non-secret tags used for search and grouping.
    /// </summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Non-secret extension metadata.
    /// </summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Opaque pointer to an external asset representation. It must not copy live device state.
/// </summary>
public sealed record AssetExternalReference
{
    /// <summary>
    /// Reference type, for example iotsharp.asset, opcua.node, aas.asset, erp.equipment, or cmms.asset.
    /// </summary>
    public string ReferenceType { get; init; } = string.Empty;

    /// <summary>
    /// External system name or namespace.
    /// </summary>
    public string? System { get; init; }

    /// <summary>
    /// Opaque external identifier.
    /// </summary>
    public string ReferenceId { get; init; } = string.Empty;

    /// <summary>
    /// Optional jump URL or non-secret URI.
    /// </summary>
    public string? Uri { get; init; }

    /// <summary>
    /// Non-secret extension metadata.
    /// </summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// L1 semantic point model: the meaning, shape, quality expectation, and source of a value.
/// </summary>
public sealed record SemanticPoint
{
    /// <summary>
    /// Stable semantic identifier used by rules, generated code, and interop profiles.
    /// </summary>
    public string SemanticId { get; init; } = string.Empty;

    /// <summary>
    /// Code-friendly point name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional display name for UI or documentation.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Optional point description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Asset that owns this point.
    /// </summary>
    public string? AssetId { get; init; }

    /// <summary>
    /// Quantity represented by the point, for example temperature, pressure, energy, state, or command.
    /// </summary>
    public Quantity Quantity { get; init; } = new();

    /// <summary>
    /// Engineering unit for this point.
    /// </summary>
    public Unit Unit { get; init; } = new();

    /// <summary>
    /// Logical value type.
    /// </summary>
    public SemanticDataType DataType { get; init; } = SemanticDataType.String;

    /// <summary>
    /// Read, write, command, or configuration access semantics.
    /// </summary>
    public SemanticPointAccess Access { get; init; } = SemanticPointAccess.Read;

    /// <summary>
    /// Quality expectation or quality source mapping. This is metadata, not a live sampled quality value.
    /// </summary>
    public Quality Quality { get; init; } = new();

    /// <summary>
    /// Expected normal range for numeric values.
    /// </summary>
    public RangeDefinition? NormalRange { get; init; }

    /// <summary>
    /// Alarm range draft for L1/L3 validation.
    /// </summary>
    public RangeDefinition? AlarmRange { get; init; }

    /// <summary>
    /// Primary protocol source for this point.
    /// </summary>
    public ProtocolSource Source { get; init; } = new();

    /// <summary>
    /// Non-secret tags used for search and grouping.
    /// </summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Non-secret extension metadata.
    /// </summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Physical or business quantity represented by a point.
/// </summary>
public sealed record Quantity
{
    /// <summary>
    /// Stable quantity kind such as temperature, pressure, energy, state, or command.
    /// </summary>
    public string QuantityKind { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable quantity name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Dimension expression or interoperable dimension reference.
    /// </summary>
    public string? Dimension { get; init; }

    /// <summary>
    /// Optional standard or profile that defines this quantity.
    /// </summary>
    public string? Standard { get; init; }

    /// <summary>
    /// Non-secret extension metadata.
    /// </summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Engineering unit definition, compatible with UCUM or OPC UA EngineeringUnits references.
/// </summary>
public sealed record Unit
{
    /// <summary>
    /// Unit code such as Cel, kPa, kWh, 1, or a custom code.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Optional display name.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Optional short symbol such as C, kPa, or kWh.
    /// </summary>
    public string? Symbol { get; init; }

    /// <summary>
    /// Unit system or profile, for example ucum, opcua, si, or custom.
    /// </summary>
    public UnitSystem System { get; init; } = UnitSystem.Custom;

    /// <summary>
    /// Quantity kind this unit is normally used with.
    /// </summary>
    public string? QuantityKind { get; init; }

    /// <summary>
    /// Non-secret extension metadata.
    /// </summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Quality metadata for a semantic point or protocol binding.
/// </summary>
public sealed record Quality
{
    /// <summary>
    /// Default or expected quality status. This is not a live business value.
    /// </summary>
    public QualityStatus Status { get; init; } = QualityStatus.Unknown;

    /// <summary>
    /// Source of quality information, for example protocol-status, payload-field, derived, or not-provided.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Optional field path or protocol reference that carries quality metadata.
    /// </summary>
    public string? FieldPath { get; init; }

    /// <summary>
    /// Optional textual reason for defaults or uncertainty.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Non-secret extension metadata.
    /// </summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Numeric range definition used by normal ranges and alarm ranges.
/// </summary>
public sealed record RangeDefinition
{
    /// <summary>
    /// Inclusive minimum value when present.
    /// </summary>
    public decimal? Min { get; init; }

    /// <summary>
    /// Inclusive maximum value when present.
    /// </summary>
    public decimal? Max { get; init; }

    /// <summary>
    /// Optional range kind, for example normal, warning, high, or low.
    /// </summary>
    public string? Kind { get; init; }

    /// <summary>
    /// Optional severity for alarm-oriented ranges.
    /// </summary>
    public AlarmSeverity? Severity { get; init; }
}

/// <summary>
/// Reference from a semantic point to a protocol binding.
/// </summary>
public sealed record ProtocolSource
{
    /// <summary>
    /// Binding identifier in SemanticModel.ProtocolBindings.
    /// </summary>
    public string BindingId { get; init; } = string.Empty;

    /// <summary>
    /// Optional source role such as primary, fallback, quality, timestamp, or derived.
    /// </summary>
    public string? Role { get; init; }
}

/// <summary>
/// Protocol-neutral binding for raw addresses, topics, node IDs, payload fields, and decoding metadata.
/// </summary>
public sealed record ProtocolBinding
{
    /// <summary>
    /// Stable binding identifier.
    /// </summary>
    public string BindingId { get; init; } = string.Empty;

    /// <summary>
    /// Protocol family for this binding.
    /// </summary>
    public SemanticProtocolKind ProtocolKind { get; init; } = SemanticProtocolKind.Custom;

    /// <summary>
    /// Non-secret endpoint reference. Credentials must be external references, never inline secrets.
    /// </summary>
    public string? EndpointRef { get; init; }

    /// <summary>
    /// Protocol-native address such as Modbus register, MQTT topic, OPC UA NodeId, HTTP path, or CAN frame.
    /// </summary>
    public string Address { get; init; } = string.Empty;

    /// <summary>
    /// Field path inside a payload or structured protocol response.
    /// </summary>
    public string? FieldPath { get; init; }

    /// <summary>
    /// Source data type before normalization.
    /// </summary>
    public SemanticDataType SourceDataType { get; init; } = SemanticDataType.String;

    /// <summary>
    /// Polling or subscription metadata.
    /// </summary>
    public ProtocolPolling? Polling { get; init; }

    /// <summary>
    /// Decoding and transformation metadata.
    /// </summary>
    public ProtocolDecode? Decode { get; init; }

    /// <summary>
    /// Modbus TCP/RTU address and register metadata for Modbus protocol bindings.
    /// </summary>
    public ModbusBinding? Modbus { get; init; }

    /// <summary>
    /// MQTT topic, payload, UNS namespace, and reserved Sparkplug profile metadata.
    /// </summary>
    public MqttBinding? Mqtt { get; init; }

    /// <summary>
    /// OPC UA NodeId, browse path, type, engineering unit, and reference metadata.
    /// </summary>
    public OpcUaBinding? OpcUa { get; init; }

    /// <summary>
    /// Quality metadata provided by the protocol or payload.
    /// </summary>
    public Quality? Quality { get; init; }

    /// <summary>
    /// Non-secret protocol-specific options.
    /// </summary>
    public Dictionary<string, JsonElement> Options { get; init; } = [];

    /// <summary>
    /// Non-secret extension metadata.
    /// </summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Polling or subscription metadata for a binding.
/// </summary>
public sealed record ProtocolPolling
{
    /// <summary>
    /// Poll interval for polling protocols.
    /// </summary>
    public TimeSpan? Interval { get; init; }

    /// <summary>
    /// Timeout for a single read or request.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// True when the protocol source is subscription based.
    /// </summary>
    public bool? Subscription { get; init; }
}

/// <summary>
/// Decode metadata used to normalize protocol-native values.
/// </summary>
public sealed record ProtocolDecode
{
    /// <summary>
    /// Byte order such as bigEndian or littleEndian.
    /// </summary>
    public string? ByteOrder { get; init; }

    /// <summary>
    /// Word order such as bigEndian or littleEndian.
    /// </summary>
    public string? WordOrder { get; init; }

    /// <summary>
    /// Multiplicative scale factor.
    /// </summary>
    public decimal? Scale { get; init; }

    /// <summary>
    /// Additive offset after scaling.
    /// </summary>
    public decimal? Offset { get; init; }

    /// <summary>
    /// Optional text encoding.
    /// </summary>
    public string? Encoding { get; init; }

    /// <summary>
    /// Non-secret decoder-specific options.
    /// </summary>
    public Dictionary<string, JsonElement> Options { get; init; } = [];
}

/// <summary>
/// Modbus TCP/RTU binding metadata. Address is the zero-based protocol data address; the parent
/// ProtocolBinding.Address can preserve user-facing PLC notation such as holding-register:40001.
/// </summary>
public sealed record ModbusBinding
{
    /// <summary>
    /// Modbus function code, for example 1, 2, 3, 4, 5, 6, 15, or 16.
    /// </summary>
    public int FunctionCode { get; init; }

    /// <summary>
    /// Modbus data area addressed by this binding.
    /// </summary>
    public ModbusRegisterType RegisterType { get; init; } = ModbusRegisterType.HoldingRegister;

    /// <summary>
    /// Zero-based Modbus protocol data address, from 0 to 65535.
    /// </summary>
    public int Address { get; init; }

    /// <summary>
    /// Modbus unit identifier or RTU slave id. Valid values are 0 through 247.
    /// </summary>
    public int UnitId { get; init; } = 1;

    /// <summary>
    /// Number of coils, discrete inputs, or 16-bit registers addressed by this binding.
    /// </summary>
    public int RegisterCount { get; init; } = 1;

    /// <summary>
    /// Byte order inside each 16-bit register.
    /// </summary>
    public ModbusByteOrder ByteOrder { get; init; } = ModbusByteOrder.BigEndian;

    /// <summary>
    /// Word order when multiple 16-bit registers are combined.
    /// </summary>
    public ModbusWordOrder WordOrder { get; init; } = ModbusWordOrder.BigEndian;

    /// <summary>
    /// Multiplicative scale factor applied to the raw value.
    /// </summary>
    public decimal Scale { get; init; } = 1m;

    /// <summary>
    /// Additive offset applied after scaling.
    /// </summary>
    public decimal Offset { get; init; }

    /// <summary>
    /// Applies the standard Modbus v1 numeric transform: raw * scale + offset.
    /// </summary>
    public decimal ApplyScale(decimal rawValue) => rawValue * Scale + Offset;
}

/// <summary>
/// MQTT binding metadata for UNS, Sparkplug, or custom MQTT namespaces. This describes topic shape
/// and payload fields only; it does not define a broker or copy live telemetry values.
/// </summary>
public sealed record MqttBinding
{
    /// <summary>
    /// Creates a UNS MQTT binding using the Semantic Core v1 generated topic convention.
    /// </summary>
    public static MqttBinding CreateUns(
        IEnumerable<string> assetPath,
        string semanticId,
        string valueField = "$.value",
        string? timestampField = "$.timestamp",
        string? qualityField = "$.quality",
        MqttPayloadSchema payloadSchema = MqttPayloadSchema.Json,
        bool retain = false,
        int qos = 0)
    {
        var topic = MqttUnsTopicBuilder.GenerateTopic(assetPath, semanticId);
        return new MqttBinding
        {
            Topic = topic,
            NamespaceStyle = MqttNamespaceStyle.Uns,
            PayloadSchema = payloadSchema,
            TimestampField = timestampField,
            QualityField = qualityField,
            ValueField = valueField,
            Retain = retain,
            Qos = qos
        };
    }

    /// <summary>
    /// MQTT publish or subscribe topic. ProtocolBinding.Address must carry the same value.
    /// </summary>
    public string Topic { get; init; } = string.Empty;

    /// <summary>
    /// MQTT namespace style used by the topic, for example UNS or Sparkplug.
    /// </summary>
    public MqttNamespaceStyle NamespaceStyle { get; init; } = MqttNamespaceStyle.Custom;

    /// <summary>
    /// Payload envelope shape used by this topic.
    /// </summary>
    public MqttPayloadSchema PayloadSchema { get; init; } = MqttPayloadSchema.Json;

    /// <summary>
    /// Optional payload field path that carries the source timestamp.
    /// </summary>
    public string? TimestampField { get; init; }

    /// <summary>
    /// Optional payload field path that carries quality metadata.
    /// </summary>
    public string? QualityField { get; init; }

    /// <summary>
    /// Payload field path that carries the semantic point value.
    /// </summary>
    public string ValueField { get; init; } = string.Empty;

    /// <summary>
    /// MQTT retain flag intended for publishing generated edge output.
    /// </summary>
    public bool Retain { get; init; }

    /// <summary>
    /// MQTT QoS level. Valid values are 0, 1, and 2.
    /// </summary>
    public int Qos { get; init; }

    /// <summary>
    /// Reserved Sparkplug profile metadata for future Sparkplug generation or import.
    /// </summary>
    public SparkplugProfile? SparkplugProfile { get; init; }
}

/// <summary>
/// Sparkplug profile reservation for MQTT bindings. It names the Sparkplug hierarchy and birth/death
/// status topics without implementing a broker or Sparkplug runtime in SaaS.
/// </summary>
public sealed record SparkplugProfile
{
    /// <summary>
    /// Sparkplug group identifier.
    /// </summary>
    public string GroupId { get; init; } = string.Empty;

    /// <summary>
    /// Sparkplug edge node identifier.
    /// </summary>
    public string EdgeNodeId { get; init; } = string.Empty;

    /// <summary>
    /// Sparkplug device identifier.
    /// </summary>
    public string DeviceId { get; init; } = string.Empty;

    /// <summary>
    /// Sparkplug metric name that maps to the semantic point.
    /// </summary>
    public string MetricName { get; init; } = string.Empty;

    /// <summary>
    /// Reserved Sparkplug birth status metadata.
    /// </summary>
    public SparkplugLifecycleState Birth { get; init; } = new();

    /// <summary>
    /// Reserved Sparkplug death status metadata.
    /// </summary>
    public SparkplugLifecycleState Death { get; init; } = new();
}

/// <summary>
/// Reserved Sparkplug lifecycle status metadata.
/// </summary>
public sealed record SparkplugLifecycleState
{
    /// <summary>
    /// True when this lifecycle message should be modeled by the profile.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Optional Sparkplug lifecycle topic, such as NBIRTH, DBIRTH, NDEATH, or DDEATH.
    /// </summary>
    public string? Topic { get; init; }

    /// <summary>
    /// Optional lifecycle metric name when it differs from the profile metric name.
    /// </summary>
    public string? MetricName { get; init; }
}

/// <summary>
/// OPC UA binding metadata for Browse or NodeSet imports. The parent ProtocolBinding.Address keeps
/// the canonical NodeId text, while this object preserves the strong OPC UA node references.
/// </summary>
public sealed record OpcUaBinding
{
    /// <summary>
    /// OPC UA NodeId of the source node. ProtocolBinding.Address must match NodeId.Text.
    /// </summary>
    public OpcUaNodeId NodeId { get; init; } = new();

    /// <summary>
    /// Browse path from the imported root to this node. Each element keeps its qualified browse name
    /// and optional reference traversal metadata instead of collapsing the path to a string.
    /// </summary>
    public List<OpcUaBrowsePathElement> BrowsePath { get; init; } = [];

    /// <summary>
    /// Node BrowseName as an OPC UA QualifiedName.
    /// </summary>
    public OpcUaQualifiedName BrowseName { get; init; } = new();

    /// <summary>
    /// Localized node DisplayName when supplied by Browse or NodeSet.
    /// </summary>
    public OpcUaLocalizedText? DisplayName { get; init; }

    /// <summary>
    /// OPC UA DataType node reference for the variable value.
    /// </summary>
    public OpcUaTypeReference DataType { get; init; } = new();

    /// <summary>
    /// OPC UA EUInformation value from the EngineeringUnits property, when present.
    /// </summary>
    public OpcUaEngineeringUnits? EngineeringUnits { get; init; }

    /// <summary>
    /// Original OPC UA references discovered by Browse or NodeSet. These references provide
    /// traceability for type definitions, properties, components, and semantic links.
    /// </summary>
    public List<OpcUaReference> References { get; init; } = [];

    /// <summary>
    /// Object type definition for the owning OPC UA object, when known.
    /// </summary>
    public OpcUaTypeReference? ObjectType { get; init; }

    /// <summary>
    /// Variable type definition for this OPC UA variable, when known.
    /// </summary>
    public OpcUaTypeReference? VariableType { get; init; }
}

/// <summary>
/// OPC UA NodeId with both canonical text and structured namespace information.
/// </summary>
public sealed record OpcUaNodeId
{
    /// <summary>
    /// Canonical NodeId text, for example ns=2;s=Line01.Motor.Temperature.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Identifier text without the namespace prefix.
    /// </summary>
    public string Identifier { get; init; } = string.Empty;

    /// <summary>
    /// OPC UA identifier type.
    /// </summary>
    public OpcUaNodeIdType IdentifierType { get; init; } = OpcUaNodeIdType.String;

    /// <summary>
    /// Namespace index from the source server or NodeSet, when available.
    /// </summary>
    public int? NamespaceIndex { get; init; }

    /// <summary>
    /// Namespace URI from the namespace table, when available.
    /// </summary>
    public string? NamespaceUri { get; init; }
}

/// <summary>
/// OPC UA QualifiedName with namespace information.
/// </summary>
public sealed record OpcUaQualifiedName
{
    /// <summary>
    /// QualifiedName text without namespace decoration.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Namespace index from the source server or NodeSet, when available.
    /// </summary>
    public int? NamespaceIndex { get; init; }

    /// <summary>
    /// Namespace URI from the namespace table, when available.
    /// </summary>
    public string? NamespaceUri { get; init; }

    /// <summary>
    /// Source QualifiedName text, for example 2:MotorTemperature.
    /// </summary>
    public string? Text { get; init; }
}

/// <summary>
/// OPC UA LocalizedText value.
/// </summary>
public sealed record OpcUaLocalizedText
{
    /// <summary>
    /// Localized text.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Locale such as en-US or zh-CN, when supplied.
    /// </summary>
    public string? Locale { get; init; }
}

/// <summary>
/// One structured element in an OPC UA browse path.
/// </summary>
public sealed record OpcUaBrowsePathElement
{
    /// <summary>
    /// BrowseName reached by this path element.
    /// </summary>
    public OpcUaQualifiedName BrowseName { get; init; } = new();

    /// <summary>
    /// Reference type used to traverse to this element, when known.
    /// </summary>
    public OpcUaTypeReference? ReferenceType { get; init; }

    /// <summary>
    /// True when the browse step follows an inverse reference.
    /// </summary>
    public bool IsInverse { get; init; }

    /// <summary>
    /// True when subtypes of the reference type are included.
    /// </summary>
    public bool IncludeSubtypes { get; init; } = true;
}

/// <summary>
/// OPC UA type or reference node pointer.
/// </summary>
public sealed record OpcUaTypeReference
{
    /// <summary>
    /// NodeId of the referenced type node.
    /// </summary>
    public OpcUaNodeId NodeId { get; init; } = new();

    /// <summary>
    /// BrowseName of the referenced type node, when known.
    /// </summary>
    public OpcUaQualifiedName? BrowseName { get; init; }

    /// <summary>
    /// DisplayName of the referenced type node, when known.
    /// </summary>
    public OpcUaLocalizedText? DisplayName { get; init; }
}

/// <summary>
/// OPC UA EUInformation metadata from a variable's EngineeringUnits property.
/// </summary>
public sealed record OpcUaEngineeringUnits
{
    /// <summary>
    /// EUInformation namespace URI.
    /// </summary>
    public string? NamespaceUri { get; init; }

    /// <summary>
    /// EUInformation unitId, commonly the UNECE Common Code integer value.
    /// </summary>
    public int? UnitId { get; init; }

    /// <summary>
    /// EUInformation display name.
    /// </summary>
    public OpcUaLocalizedText? DisplayName { get; init; }

    /// <summary>
    /// EUInformation description.
    /// </summary>
    public OpcUaLocalizedText? Description { get; init; }
}

/// <summary>
/// Original OPC UA reference discovered from Browse results or NodeSet XML.
/// </summary>
public sealed record OpcUaReference
{
    /// <summary>
    /// ReferenceType node, such as HasComponent, HasProperty, Organizes, or HasTypeDefinition.
    /// </summary>
    public OpcUaTypeReference ReferenceType { get; init; } = new();

    /// <summary>
    /// Target node of the reference.
    /// </summary>
    public OpcUaNodeId TargetNodeId { get; init; } = new();

    /// <summary>
    /// BrowseName of the target node, when known.
    /// </summary>
    public OpcUaQualifiedName? TargetBrowseName { get; init; }

    /// <summary>
    /// DisplayName of the target node, when known.
    /// </summary>
    public OpcUaLocalizedText? TargetDisplayName { get; init; }

    /// <summary>
    /// NodeClass of the target node, when known.
    /// </summary>
    public OpcUaNodeClass? TargetNodeClass { get; init; }

    /// <summary>
    /// True for forward references and false for inverse references.
    /// </summary>
    public bool IsForward { get; init; } = true;
}

/// <summary>
/// Deterministic UNS MQTT topic generator used by Semantic Core contracts and edge code generation.
/// </summary>
public static class MqttUnsTopicBuilder
{
    public const string DefaultRoot = "uns";

    /// <summary>
    /// Creates a stable UNS topic from an asset path and semantic point identifier.
    /// </summary>
    public static string GenerateTopic(IEnumerable<string> assetPath, string semanticId, string root = DefaultRoot)
    {
        ArgumentNullException.ThrowIfNull(assetPath);

        if (string.IsNullOrWhiteSpace(semanticId))
        {
            throw new ArgumentException("semanticId is required.", nameof(semanticId));
        }

        var segments = new List<string>
        {
            NormalizeTopicSegment(root)
        };

        foreach (var segment in assetPath)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                throw new ArgumentException("assetPath cannot contain empty segments.", nameof(assetPath));
            }

            segments.Add(NormalizeTopicSegment(segment));
        }

        if (segments.Count == 1)
        {
            throw new ArgumentException("assetPath must contain at least one segment.", nameof(assetPath));
        }

        segments.Add(NormalizeTopicSegment(semanticId));

        return string.Join('/', segments);
    }

    /// <summary>
    /// Returns true when a topic is a publish-safe MQTT topic without wildcards or empty hierarchy levels.
    /// </summary>
    public static bool IsValidPublishTopic(string? topic)
    {
        if (string.IsNullOrWhiteSpace(topic) || topic != topic.Trim())
        {
            return false;
        }

        if (Encoding.UTF8.GetByteCount(topic) > 65535)
        {
            return false;
        }

        if (topic.IndexOf('\0') >= 0 || topic.IndexOf('+') >= 0 || topic.IndexOf('#') >= 0)
        {
            return false;
        }

        return topic.Split('/').All(segment => segment.Length > 0);
    }

    /// <summary>
    /// Returns true when a topic follows the Semantic Core UNS v1 generated-topic shape.
    /// </summary>
    public static bool IsValidUnsTopic(string? topic)
    {
        if (!IsValidPublishTopic(topic))
        {
            return false;
        }

        var segments = topic!.Split('/');
        return segments.Length >= 3
            && string.Equals(segments[0], DefaultRoot, StringComparison.Ordinal)
            && segments.All(IsSafeUnsSegment);
    }

    private static string NormalizeTopicSegment(string value)
    {
        var builder = new StringBuilder();
        var lastWasSeparator = false;

        foreach (var character in value.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var lower = char.ToLowerInvariant(character);
            if ((lower >= 'a' && lower <= 'z') || (lower >= '0' && lower <= '9'))
            {
                builder.Append(lower);
                lastWasSeparator = false;
                continue;
            }

            if (!lastWasSeparator && builder.Length > 0)
            {
                builder.Append('-');
                lastWasSeparator = true;
            }
        }

        while (builder.Length > 0 && builder[^1] == '-')
        {
            builder.Length--;
        }

        return builder.Length == 0 ? "x" : builder.ToString();
    }

    private static bool IsSafeUnsSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            return false;
        }

        if (!((segment[0] >= 'a' && segment[0] <= 'z') || (segment[0] >= '0' && segment[0] <= '9')))
        {
            return false;
        }

        foreach (var character in segment)
        {
            if ((character >= 'a' && character <= 'z')
                || (character >= '0' && character <= '9')
                || character is '.' or '_' or '-')
            {
                continue;
            }

            return false;
        }

        return true;
    }
}

/// <summary>
/// L3 process graph draft that references L1 semantic points and L2 assets.
/// </summary>
public sealed record ProcessGraph
{
    /// <summary>
    /// Stable process graph identifier.
    /// </summary>
    public string ProcessGraphId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable graph name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional graph description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Process nodes such as pump, valve, boiler, station, line segment, or workstation.
    /// </summary>
    public List<ProcessNode> Nodes { get; init; } = [];

    /// <summary>
    /// Process relationships between nodes.
    /// </summary>
    public List<ProcessEdge> Edges { get; init; } = [];

    /// <summary>
    /// Derived point drafts that compute values from semantic point dependencies.
    /// </summary>
    public List<DerivedPoint> DerivedPoints { get; init; } = [];

    /// <summary>
    /// State model drafts.
    /// </summary>
    public List<StateModel> StateModels { get; init; } = [];

    /// <summary>
    /// Alarm semantics drafts.
    /// </summary>
    public List<AlarmSemantics> Alarms { get; init; } = [];

    /// <summary>
    /// Control policies for writable points and commands.
    /// </summary>
    public List<ControlPolicy> ControlPolicies { get; init; } = [];

    /// <summary>
    /// Non-secret extension metadata.
    /// </summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Process node that links process logic back to assets and semantic points.
/// </summary>
public sealed record ProcessNode
{
    public string NodeId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string NodeType { get; init; } = string.Empty;

    public string? AssetId { get; init; }

    public List<string> InputSemanticIds { get; init; } = [];

    public List<string> OutputSemanticIds { get; init; } = [];

    public List<string> Tags { get; init; } = [];

    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Directed or typed relationship between process nodes.
/// </summary>
public sealed record ProcessEdge
{
    public string EdgeId { get; init; } = string.Empty;

    public string FromNodeId { get; init; } = string.Empty;

    public string ToNodeId { get; init; } = string.Empty;

    public ProcessRelation Relation { get; init; } = ProcessRelation.Dependency;

    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// L3 computed point definition. Expressions reference dependencies through ref("semanticId").
/// </summary>
public sealed record DerivedPoint
{
    /// <summary>
    /// Stable semantic identifier for the computed value.
    /// </summary>
    public string SemanticId { get; init; } = string.Empty;

    /// <summary>
    /// Code-friendly derived point name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Arithmetic expression that must reference semantic IDs with ref("semanticId").
    /// </summary>
    public string Expression { get; init; } = string.Empty;

    /// <summary>
    /// Semantic IDs read by the expression. Entries may reference L1 SemanticPoint IDs or other DerivedPoint IDs.
    /// </summary>
    public List<string> DependsOnSemanticIds { get; init; } = [];

    /// <summary>
    /// Quantity represented by the computed value.
    /// </summary>
    public Quantity Quantity { get; init; } = new();

    /// <summary>
    /// Engineering unit emitted by the expression.
    /// </summary>
    public Unit Unit { get; init; } = new();

    /// <summary>
    /// Refresh strategy: onDependencyChange, fixedInterval, or manual.
    /// </summary>
    public string? RefreshPolicy { get; init; }

    /// <summary>
    /// Required positive interval when refreshPolicy is fixedInterval.
    /// </summary>
    public TimeSpan? RefreshInterval { get; init; }

    /// <summary>
    /// Optional period after which the computed value is considered stale.
    /// </summary>
    public TimeSpan? StaleAfter { get; init; }

    /// <summary>
    /// Optional lookback window for aggregate-style expressions.
    /// </summary>
    public TimeSpan? EvaluationWindow { get; init; }

    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// L3 process or asset state model.
/// </summary>
public sealed record StateModel
{
    public string StateModelId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? AppliesToAssetId { get; init; }

    public string? AppliesToNodeId { get; init; }

    /// <summary>
    /// State used when no other state condition matches.
    /// </summary>
    public string? DefaultStateId { get; init; }

    /// <summary>
    /// True when exactly one state may be active at a time.
    /// </summary>
    public bool MutuallyExclusive { get; init; } = true;

    public List<StateDefinition> States { get; init; } = [];

    public List<StateTransition> Transitions { get; init; } = [];

    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Named state and the semantic references used to determine it.
/// </summary>
public sealed record StateDefinition
{
    public string StateId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public ProcessStateKind StateKind { get; init; } = ProcessStateKind.Unknown;

    public string? Condition { get; init; }

    public List<string> DependsOnSemanticIds { get; init; } = [];

    public List<string> DependsOnNodeIds { get; init; } = [];
}

/// <summary>
/// State transition draft.
/// </summary>
public sealed record StateTransition
{
    public string FromStateId { get; init; } = string.Empty;

    public string ToStateId { get; init; } = string.Empty;

    public string? Condition { get; init; }
}

/// <summary>
/// Draft alarm semantics for L3 validation.
/// </summary>
public sealed record AlarmSemantics
{
    public string AlarmId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Business meaning of the alarm, not just a technical threshold.
    /// </summary>
    public string BusinessMeaning { get; init; } = string.Empty;

    public AlarmSeverity Severity { get; init; } = AlarmSeverity.Unknown;

    public string Condition { get; init; } = string.Empty;

    public List<string> DependsOnSemanticIds { get; init; } = [];

    public List<string> DependsOnNodeIds { get; init; } = [];

    public List<string> DependsOnStateModelIds { get; init; } = [];

    public TimeSpan? Duration { get; init; }

    public string? SuppressionCondition { get; init; }

    public string? RecoveryCondition { get; init; }

    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Draft control policy for writable semantic points and commands.
/// </summary>
public sealed record ControlPolicy
{
    public string ControlPolicyId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public List<string> AppliesToSemanticIds { get; init; } = [];

    public ControlRisk Risk { get; init; } = ControlRisk.Normal;

    public bool RequiresApproval { get; init; }

    public AiOperationMode AiOperationMode { get; init; } = AiOperationMode.RecommendOnly;

    /// <summary>
    /// Explicit opt-in for AI or automation to execute writes without a human approval step.
    /// </summary>
    public bool AllowAutomaticExecution { get; init; }

    /// <summary>
    /// Optional approval scope such as operator, supervisor, maintenance, or safety.
    /// </summary>
    public string? ApprovalScope { get; init; }

    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

/// <summary>
/// Reserved extension slot for future L3/L4 profiles.
/// </summary>
public sealed record SemanticExtensionSlot
{
    public SemanticLayer Layer { get; init; } = SemanticLayer.L3;

    public string Key { get; init; } = string.Empty;

    public string? Description { get; init; }

    public Dictionary<string, JsonElement> Metadata { get; init; } = [];
}

[JsonConverter(typeof(JsonStringEnumConverter<SemanticLayer>))]
public enum SemanticLayer
{
    [EnumMember(Value = "l1")]
    [JsonStringEnumMemberName("l1")]
    L1,

    [EnumMember(Value = "l2")]
    [JsonStringEnumMemberName("l2")]
    L2,

    [EnumMember(Value = "l3")]
    [JsonStringEnumMemberName("l3")]
    L3,

    [EnumMember(Value = "l4")]
    [JsonStringEnumMemberName("l4")]
    L4
}

[JsonConverter(typeof(JsonStringEnumConverter<SemanticAssetType>))]
public enum SemanticAssetType
{
    [EnumMember(Value = "site")]
    [JsonStringEnumMemberName("site")]
    Site,

    [EnumMember(Value = "area")]
    [JsonStringEnumMemberName("area")]
    Area,

    [EnumMember(Value = "line")]
    [JsonStringEnumMemberName("line")]
    Line,

    [EnumMember(Value = "cell")]
    [JsonStringEnumMemberName("cell")]
    Cell,

    [EnumMember(Value = "device")]
    [JsonStringEnumMemberName("device")]
    Device,

    [EnumMember(Value = "component")]
    [JsonStringEnumMemberName("component")]
    Component,

    [EnumMember(Value = "sensor")]
    [JsonStringEnumMemberName("sensor")]
    Sensor,

    [EnumMember(Value = "actuator")]
    [JsonStringEnumMemberName("actuator")]
    Actuator,

    [EnumMember(Value = "custom")]
    [JsonStringEnumMemberName("custom")]
    Custom
}

[JsonConverter(typeof(JsonStringEnumConverter<SemanticDataType>))]
public enum SemanticDataType
{
    [EnumMember(Value = "boolean")]
    [JsonStringEnumMemberName("boolean")]
    Boolean,

    [EnumMember(Value = "int")]
    [JsonStringEnumMemberName("int")]
    Int,

    [EnumMember(Value = "float")]
    [JsonStringEnumMemberName("float")]
    Float,

    [EnumMember(Value = "decimal")]
    [JsonStringEnumMemberName("decimal")]
    Decimal,

    [EnumMember(Value = "string")]
    [JsonStringEnumMemberName("string")]
    String,

    [EnumMember(Value = "enum")]
    [JsonStringEnumMemberName("enum")]
    Enum,

    [EnumMember(Value = "struct")]
    [JsonStringEnumMemberName("struct")]
    Struct
}

[JsonConverter(typeof(JsonStringEnumConverter<SemanticPointAccess>))]
public enum SemanticPointAccess
{
    [EnumMember(Value = "read")]
    [JsonStringEnumMemberName("read")]
    Read,

    [EnumMember(Value = "write")]
    [JsonStringEnumMemberName("write")]
    Write,

    [EnumMember(Value = "readWrite")]
    [JsonStringEnumMemberName("readWrite")]
    ReadWrite,

    [EnumMember(Value = "command")]
    [JsonStringEnumMemberName("command")]
    Command,

    [EnumMember(Value = "config")]
    [JsonStringEnumMemberName("config")]
    Config
}

[JsonConverter(typeof(JsonStringEnumConverter<UnitSystem>))]
public enum UnitSystem
{
    [EnumMember(Value = "ucum")]
    [JsonStringEnumMemberName("ucum")]
    Ucum,

    [EnumMember(Value = "opcua")]
    [JsonStringEnumMemberName("opcua")]
    OpcUa,

    [EnumMember(Value = "si")]
    [JsonStringEnumMemberName("si")]
    Si,

    [EnumMember(Value = "custom")]
    [JsonStringEnumMemberName("custom")]
    Custom
}

[JsonConverter(typeof(JsonStringEnumConverter<QualityStatus>))]
public enum QualityStatus
{
    [EnumMember(Value = "unknown")]
    [JsonStringEnumMemberName("unknown")]
    Unknown,

    [EnumMember(Value = "good")]
    [JsonStringEnumMemberName("good")]
    Good,

    [EnumMember(Value = "uncertain")]
    [JsonStringEnumMemberName("uncertain")]
    Uncertain,

    [EnumMember(Value = "bad")]
    [JsonStringEnumMemberName("bad")]
    Bad
}

[JsonConverter(typeof(JsonStringEnumConverter<ModbusRegisterType>))]
public enum ModbusRegisterType
{
    [EnumMember(Value = "coil")]
    [JsonStringEnumMemberName("coil")]
    Coil,

    [EnumMember(Value = "discrete-input")]
    [JsonStringEnumMemberName("discrete-input")]
    DiscreteInput,

    [EnumMember(Value = "input-register")]
    [JsonStringEnumMemberName("input-register")]
    InputRegister,

    [EnumMember(Value = "holding-register")]
    [JsonStringEnumMemberName("holding-register")]
    HoldingRegister
}

[JsonConverter(typeof(JsonStringEnumConverter<ModbusByteOrder>))]
public enum ModbusByteOrder
{
    [EnumMember(Value = "bigEndian")]
    [JsonStringEnumMemberName("bigEndian")]
    BigEndian,

    [EnumMember(Value = "littleEndian")]
    [JsonStringEnumMemberName("littleEndian")]
    LittleEndian
}

[JsonConverter(typeof(JsonStringEnumConverter<ModbusWordOrder>))]
public enum ModbusWordOrder
{
    [EnumMember(Value = "bigEndian")]
    [JsonStringEnumMemberName("bigEndian")]
    BigEndian,

    [EnumMember(Value = "littleEndian")]
    [JsonStringEnumMemberName("littleEndian")]
    LittleEndian
}

[JsonConverter(typeof(JsonStringEnumConverter<MqttNamespaceStyle>))]
public enum MqttNamespaceStyle
{
    [EnumMember(Value = "uns")]
    [JsonStringEnumMemberName("uns")]
    Uns,

    [EnumMember(Value = "sparkplug")]
    [JsonStringEnumMemberName("sparkplug")]
    Sparkplug,

    [EnumMember(Value = "custom")]
    [JsonStringEnumMemberName("custom")]
    Custom
}

[JsonConverter(typeof(JsonStringEnumConverter<MqttPayloadSchema>))]
public enum MqttPayloadSchema
{
    [EnumMember(Value = "json")]
    [JsonStringEnumMemberName("json")]
    Json,

    [EnumMember(Value = "raw")]
    [JsonStringEnumMemberName("raw")]
    Raw,

    [EnumMember(Value = "sparkplugB")]
    [JsonStringEnumMemberName("sparkplugB")]
    SparkplugB,

    [EnumMember(Value = "custom")]
    [JsonStringEnumMemberName("custom")]
    Custom
}

[JsonConverter(typeof(JsonStringEnumConverter<OpcUaNodeIdType>))]
public enum OpcUaNodeIdType
{
    [EnumMember(Value = "numeric")]
    [JsonStringEnumMemberName("numeric")]
    Numeric,

    [EnumMember(Value = "string")]
    [JsonStringEnumMemberName("string")]
    String,

    [EnumMember(Value = "guid")]
    [JsonStringEnumMemberName("guid")]
    Guid,

    [EnumMember(Value = "opaque")]
    [JsonStringEnumMemberName("opaque")]
    Opaque
}

[JsonConverter(typeof(JsonStringEnumConverter<OpcUaNodeClass>))]
public enum OpcUaNodeClass
{
    [EnumMember(Value = "object")]
    [JsonStringEnumMemberName("object")]
    Object,

    [EnumMember(Value = "variable")]
    [JsonStringEnumMemberName("variable")]
    Variable,

    [EnumMember(Value = "method")]
    [JsonStringEnumMemberName("method")]
    Method,

    [EnumMember(Value = "objectType")]
    [JsonStringEnumMemberName("objectType")]
    ObjectType,

    [EnumMember(Value = "variableType")]
    [JsonStringEnumMemberName("variableType")]
    VariableType,

    [EnumMember(Value = "referenceType")]
    [JsonStringEnumMemberName("referenceType")]
    ReferenceType,

    [EnumMember(Value = "dataType")]
    [JsonStringEnumMemberName("dataType")]
    DataType,

    [EnumMember(Value = "view")]
    [JsonStringEnumMemberName("view")]
    View
}

[JsonConverter(typeof(JsonStringEnumConverter<SemanticProtocolKind>))]
public enum SemanticProtocolKind
{
    [EnumMember(Value = "modbusTcp")]
    [JsonStringEnumMemberName("modbusTcp")]
    ModbusTcp,

    [EnumMember(Value = "modbusRtu")]
    [JsonStringEnumMemberName("modbusRtu")]
    ModbusRtu,

    [EnumMember(Value = "mqtt")]
    [JsonStringEnumMemberName("mqtt")]
    Mqtt,

    [EnumMember(Value = "opcUa")]
    [JsonStringEnumMemberName("opcUa")]
    OpcUa,

    [EnumMember(Value = "bacnet")]
    [JsonStringEnumMemberName("bacnet")]
    Bacnet,

    [EnumMember(Value = "s7")]
    [JsonStringEnumMemberName("s7")]
    S7,

    [EnumMember(Value = "iec104")]
    [JsonStringEnumMemberName("iec104")]
    Iec104,

    [EnumMember(Value = "iec61850")]
    [JsonStringEnumMemberName("iec61850")]
    Iec61850,

    [EnumMember(Value = "can")]
    [JsonStringEnumMemberName("can")]
    Can,

    [EnumMember(Value = "lorawan")]
    [JsonStringEnumMemberName("lorawan")]
    Lorawan,

    [EnumMember(Value = "http")]
    [JsonStringEnumMemberName("http")]
    Http,

    [EnumMember(Value = "coap")]
    [JsonStringEnumMemberName("coap")]
    Coap,

    [EnumMember(Value = "ble")]
    [JsonStringEnumMemberName("ble")]
    Ble,

    [EnumMember(Value = "local")]
    [JsonStringEnumMemberName("local")]
    Local,

    [EnumMember(Value = "custom")]
    [JsonStringEnumMemberName("custom")]
    Custom
}

[JsonConverter(typeof(JsonStringEnumConverter<ProcessRelation>))]
public enum ProcessRelation
{
    [EnumMember(Value = "input")]
    [JsonStringEnumMemberName("input")]
    Input,

    [EnumMember(Value = "output")]
    [JsonStringEnumMemberName("output")]
    Output,

    [EnumMember(Value = "upstream")]
    [JsonStringEnumMemberName("upstream")]
    Upstream,

    [EnumMember(Value = "downstream")]
    [JsonStringEnumMemberName("downstream")]
    Downstream,

    [EnumMember(Value = "bypass")]
    [JsonStringEnumMemberName("bypass")]
    Bypass,

    [EnumMember(Value = "control")]
    [JsonStringEnumMemberName("control")]
    Control,

    [EnumMember(Value = "dependency")]
    [JsonStringEnumMemberName("dependency")]
    Dependency
}

[JsonConverter(typeof(JsonStringEnumConverter<AlarmSeverity>))]
public enum AlarmSeverity
{
    [EnumMember(Value = "unknown")]
    [JsonStringEnumMemberName("unknown")]
    Unknown = -1,

    [EnumMember(Value = "info")]
    [JsonStringEnumMemberName("info")]
    Info,

    [EnumMember(Value = "warning")]
    [JsonStringEnumMemberName("warning")]
    Warning,

    [EnumMember(Value = "minor")]
    [JsonStringEnumMemberName("minor")]
    Minor,

    [EnumMember(Value = "major")]
    [JsonStringEnumMemberName("major")]
    Major,

    [EnumMember(Value = "critical")]
    [JsonStringEnumMemberName("critical")]
    Critical
}

[JsonConverter(typeof(JsonStringEnumConverter<ProcessStateKind>))]
public enum ProcessStateKind
{
    [EnumMember(Value = "unknown")]
    [JsonStringEnumMemberName("unknown")]
    Unknown,

    [EnumMember(Value = "running")]
    [JsonStringEnumMemberName("running")]
    Running,

    [EnumMember(Value = "standby")]
    [JsonStringEnumMemberName("standby")]
    Standby,

    [EnumMember(Value = "fault")]
    [JsonStringEnumMemberName("fault")]
    Fault,

    [EnumMember(Value = "maintenance")]
    [JsonStringEnumMemberName("maintenance")]
    Maintenance,

    [EnumMember(Value = "manual")]
    [JsonStringEnumMemberName("manual")]
    Manual,

    [EnumMember(Value = "automatic")]
    [JsonStringEnumMemberName("automatic")]
    Automatic,

    [EnumMember(Value = "stopped")]
    [JsonStringEnumMemberName("stopped")]
    Stopped,

    [EnumMember(Value = "custom")]
    [JsonStringEnumMemberName("custom")]
    Custom
}

[JsonConverter(typeof(JsonStringEnumConverter<ControlRisk>))]
public enum ControlRisk
{
    [EnumMember(Value = "normal")]
    [JsonStringEnumMemberName("normal")]
    Normal,

    [EnumMember(Value = "hazardous")]
    [JsonStringEnumMemberName("hazardous")]
    Hazardous
}

[JsonConverter(typeof(JsonStringEnumConverter<AiOperationMode>))]
public enum AiOperationMode
{
    [EnumMember(Value = "recommendOnly")]
    [JsonStringEnumMemberName("recommendOnly")]
    RecommendOnly,

    [EnumMember(Value = "draftOnly")]
    [JsonStringEnumMemberName("draftOnly")]
    DraftOnly,

    [EnumMember(Value = "allowWithApproval")]
    [JsonStringEnumMemberName("allowWithApproval")]
    AllowWithApproval,

    [EnumMember(Value = "allowAutomatic")]
    [JsonStringEnumMemberName("allowAutomatic")]
    AllowAutomatic
}
