# OPC UA Binding v1 设计

本设计属于 IoTSharp Semantic Core 的 L0 Protocol Binding Profile。OPC UA 只作为强语义工业信息模型的导入、导出和互操作来源，内部主模型仍然是 Semantic Core：L2 `Asset` 承载资产层级，L1 `SemanticPoint` 承载点位含义，L0 `ProtocolBinding.opcUa` 保留 OPC UA 原始节点与引用。

## 字段

`ProtocolBinding.protocolKind` 为 `opcUa` 时必须提供 `opcUa` 子对象。`ProtocolBinding.address` 保留 canonical NodeId 文本，例如 `ns=2;s=Line01.Motor01.Temperature`，并且必须与 `opcUa.nodeId.text` 一致。

`opcUa` 字段如下：

| 字段 | 含义 | 映射要求 |
| --- | --- | --- |
| `nodeId` | 变量或对象的 OPC UA NodeId | 保留 `text`、`identifier`、`identifierType`、`namespaceIndex`、`namespaceUri`，不能只保存业务地址字符串。 |
| `browsePath` | 从导入根到当前节点的结构化 BrowsePath | 每级使用 `browseName`，可携带 `referenceType`、`isInverse`、`includeSubtypes`。 |
| `browseName` | 当前节点的 QualifiedName | 保留 name、namespaceIndex、namespaceUri 和原始 text。 |
| `displayName` | 当前节点 LocalizedText | 保留 text 和 locale。 |
| `dataType` | 变量 DataType 节点引用 | 使用 `OpcUaTypeReference` 指向 DataType NodeId、BrowseName、DisplayName。 |
| `engineeringUnits` | EngineeringUnits 属性的 EUInformation | 保留 namespaceUri、unitId、displayName、description，并同步到 `SemanticPoint.unit.metadata.engineeringUnits`。 |
| `references` | Browse 或 NodeSet 中原始 Reference 列表 | 至少保留 `HasTypeDefinition`，并按需保留 `HasProperty`、`HasComponent`、`Organizes`、Companion Spec 语义引用等。 |
| `objectType` | 拥有变量的对象 TypeDefinition | 从对象节点 `HasTypeDefinition` 推导，未知时可为空。 |
| `variableType` | 变量 TypeDefinition | 通常来自变量节点 `HasTypeDefinition`，例如 `BaseDataVariableType`。 |

## Browse 映射

1. 从用户选择的 OPC UA 根节点开始 Browse，只导入节点元数据，不复制实时值、状态、报警或历史数据。
2. `Object` 节点映射为 L2 `Asset` 草稿：`browseName` 生成 code-friendly `name` 和 `assetPath`，`displayName` 写入 `displayName`。
3. `Variable` 且具备可读 Value 的节点映射为 L1 `SemanticPoint`：`displayName` 或 `browseName` 生成 `name`，`DataType` 映射 `dataType`，`EngineeringUnits` 映射 `unit`。
4. 每个可采集变量生成一个 `ProtocolBinding`：`address = nodeId.text`，`fieldPath = "Value"`，`source.bindingId` 指向该 binding。
5. Browse 过程中得到的 `ReferenceDescription` 必须写入 `opcUa.references`，用于后续审计、Companion Spec 映射和导出，不允许只把 BrowsePath 拼成普通字符串。
6. 质量来源固定表达为 metadata：`SemanticPoint.quality.source = "opcua-status-code"`，不存储任何实时 StatusCode 采样值。

## NodeSet 映射

1. 解析 NodeSet 的 NamespaceTable，并把 namespace index 与 namespace URI 一起写入 `nodeId`、`browseName` 和引用目标。
2. `UAObject` 映射为 `Asset`，`HasComponent` / `Organizes` 推导父子层级，`HasTypeDefinition` 写入 `objectType` 或资产 `externalReferences.metadata.objectType`。
3. `UAVariable` 映射为 `SemanticPoint + ProtocolBinding`。`DataType` 指向 `opcUa.dataType`；`HasTypeDefinition` 指向 `opcUa.variableType`；`EngineeringUnits` 属性解析为 `opcUa.engineeringUnits`。
4. Companion Spec、AAS、ISA-95 或厂商语义引用保持在 `opcUa.references` 或后续 L4 interop profile 中，Semantic Core 不把它们作为内部唯一模型。

## 单位规则

优先使用 `EngineeringUnits` 属性：

- 能映射 UCUM 时，`SemanticPoint.unit.code` 使用 UCUM code，`unit.system` 可用 `ucum` 或 `opcua`。
- 不能稳定映射 UCUM 时，`unit.system = "opcua"`，`unit.code` 可采用 OPC UA displayName 或约定 code。
- 无论是否映射到 UCUM，都必须在 `unit.metadata.engineeringUnits` 与 `opcUa.engineeringUnits` 中保留原始 EUInformation。

## 边界

OPC UA Binding v1 只固定契约、schema、校验规则和样例，不实现完整 OPC UA Client。SaaS 不接收 OPC UA 实时值，不保存业务遥测、属性、事件或告警；采集运行时后续由边缘端或用户自有 IoTSharp 实例承担。
