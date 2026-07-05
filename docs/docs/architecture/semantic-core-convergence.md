---
title: Semantic Core 收敛决策
---

# Semantic Core 收敛决策

本文对应 M2 #029，用于关闭 M3 前置门：`semantic-core-v1`、M3 Collection Template 持久化模型、IoTEdge 本地采集模型必须收敛为一套点位与协议描述，避免平台、契约包和边缘运行时各自维护一套互不兼容的模型。

## 决策

M3 采用 `IoTSharp.Contracts/semantic-core.v1.schema.json` 作为采集模板的语义和协议绑定基础，不废弃 semantic-core。

`semantic-core-v1` 是跨仓语义合同，负责描述：

- L2 Asset 草稿与外部引用。
- L1 SemanticPoint，包括点位含义、数据类型、读写语义、单位、质量来源和业务标签。
- L0 ProtocolBinding，包括 Modbus TCP/RTU、OPC UA、MQTT 和自定义协议绑定。
- 低频的过程图、派生点、状态、告警和控制策略草稿。

它不负责保存运行时连接凭据、实时遥测值、属性最新值、告警实例、任务执行状态、发布批次或边缘本地缓存。

## 分层关系

| 层 | 责任 | 来源或消费者 |
| --- | --- | --- |
| `semantic-core-v1` | 定义点位语义、协议绑定、单位、质量来源和跨系统引用。 | `IoTSharp.Contracts`，平台、IoTEdge、SDK 和 AI 工具只读消费。 |
| M3 Collection Template | Product 下可编辑、可审核、可版本化的采集模板持久化模型。 | 平台控制面。以 semantic-core 字段为基础，同时保留 Product、租户、审批和 UI 所需字段。 |
| `collection-config-v1` | 面向 EdgeNode/Gateway 的运行时配置快照。 | 平台生成，IoTEdge 拉取或接收。它是执行载荷，不是模板源数据。 |
| IoTEdge 本地采集模型 | 执行端缓存、调度和断网自治所需的本地模型。 | IoTEdge 从 `collection-config-v1` 转换生成，并保留 `semanticId`、`bindingId` 等追踪字段。 |

## M3 映射规则

| Semantic Core | Collection Template | 说明 |
| --- | --- | --- |
| `SemanticModel.modelId` | `CollectionTemplate.SemanticModelId` | 模板批次或导入来源标识。 |
| `SemanticPoint.semanticId` | `PointTemplate.SemanticId` | 跨模板、规则、AI 和运行时追踪的稳定点位语义 ID。 |
| `SemanticPoint.name/displayName` | `PointTemplate.Name/DisplayName` | UI 显示和代码友好名称。 |
| `SemanticPoint.dataType/access` | `PointTemplate.ValueType/Access` | 点位数据类型和读写/命令语义。 |
| `SemanticPoint.quantity/unit/quality` | `PointTemplate.Quantity/Unit/QualityPolicy` | 业务含义、工程单位和质量来源。 |
| `ProtocolBinding.bindingId` | `PointTemplate.BindingId` | 运行时配置和执行回执的追踪键。 |
| `ProtocolBinding.protocolKind` | `ProtocolTemplate.Protocol` | 协议族归一化。 |
| `ProtocolBinding.endpointRef` | `ConnectionTemplate.EndpointRef` | 只保存非敏感端点引用，凭据由平台安全配置或发布流程注入。 |
| `ProtocolBinding.address/fieldPath` | `PointTemplate.Address/FieldPath` | 协议原生地址或载荷字段路径。 |
| `ProtocolBinding.polling` | `SamplingPolicy` | 轮询周期、超时和订阅型采集提示。 |
| `ProtocolBinding.decode` | `TransformTemplate` | 字节序、字序、缩放、偏移和解码选项。 |
| `ProtocolBinding.modbus` | `PointTemplate.ProtocolOptions` | 功能码、寄存器类型、零基地址、单元号、寄存器数量和字节序。 |
| `ProtocolBinding.opcUa` | `PointTemplate.ProtocolOptions` | NodeId、BrowsePath、BrowseName、DataType、EngineeringUnits 和引用元数据。 |
| `ProtocolBinding.mqtt` | `PointTemplate.ProtocolOptions` | Topic、UNS/Sparkplug/custom 命名、payload schema、value/timestamp/quality 字段、QoS 和 retain。 |
| `ProcessGraph` | M6 或后续 Asset View/RuleChain 草稿 | M3 第一波只保留引用，不把它变成实时工作流或发布编排。 |

## 兼容策略

- `CollectionTaskDto` 继续作为 M2/M3 过渡期的草稿、校验和预览 DTO；M3 正式模板落地后，它由 Collection Template 生成，不再作为模板主存储。
- `EdgeCollectionConfigurationDto` 和 `collection-config-v1` 继续是运行时配置合同，不能反向替代 Product 侧模板。
- IoTEdge 本地模型以 `collection-config-v1` 为输入，以执行效率和断网自治为目标，不反向定义平台字段。
- M3 新增字段必须采用 extend-only 策略，已有 `edge-node-v1`、`collection-config-v1`、`edge-task-v1` 不做破坏性改名。
- semantic-core 中任何端点引用、metadata 或 options 都不得携带明文密码、token、连接串或访问密钥。

## 协议绑定结论

### Modbus

以 `ProtocolBinding.protocolKind = modbusTcp/modbusRtu` 和 `modbus` 子对象为准。M3 PointTemplate 必须保留功能码、寄存器类型、零基地址、单元号、寄存器数量、字节序、字序、scale 和 offset。用户界面可以显示 `40001` 这类工程地址，但运行时必须能还原为明确的零基协议地址。

### OPC UA

以 `ProtocolBinding.protocolKind = opcUa` 和 `opcUa` 子对象为准。M3 必须保留 NodeId、BrowsePath、BrowseName、DataType、EngineeringUnits、HasTypeDefinition 等引用信息，不能只保存一个业务地址字符串。OPC UA Browse 或 NodeSet 导入只进入模板草稿，不复制实时值、历史值或告警实例。

### MQTT

以 `ProtocolBinding.protocolKind = mqtt` 和 `mqtt` 子对象为准。M3 必须保留 topic、namespaceStyle、payloadSchema、valueField、timestampField、qualityField、retain 和 qos。UNS 和 Sparkplug 是 MQTT binding 的 profile，不另起一套点位模型。

## #029 完成标准

- `IoTSharp.Contracts` 发布 `semantic-core.v1.schema.json`、DTO、validator、样例和 OPC UA binding 文档。
- README 和架构文档明确 semantic-core 是 M3 Collection Template 的语义和协议绑定基础。
- 测试覆盖 semantic-core schema、有效样例和非法样例，保证契约包发布物不会无声漂移。
- ROADMAP 中 #029 标记完成，M3 #030 可以在该决策基础上开始。

