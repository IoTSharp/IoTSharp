# Edge Contracts V1

本文记录 M2 #027 固化后的云边合同入口。`IoTSharp.Contracts` 是单一事实来源，IoTSharp、IoTEdge、SDK 和后续自动化验收都应从该包消费 DTO、JSON Schema 和样例。

M2 #029 进一步确认 `semantic-core-v1` 保留为 M3 Collection Template 的语义和协议绑定基础；它不是运行时配置合同，但会作为 Product 采集模板、IoTEdge 本地采集模型和 AI 工具理解点位语义的共同来源。

## 发布物

| 合同 | DTO 入口 | JSON Schema | 样例 |
| --- | --- | --- | --- |
| `edge-node-v1` | `EdgeNodeDto`、`EdgeRuntimeStatusDto`、`EdgeCapabilityDto`、`EdgeRegistrationDto`、`EdgeHeartbeatDto` | `IoTSharp.Contracts/edge-node.v1.schema.json` | `IoTSharp.Contracts/examples/edge-node.v1.sample.json` |
| `collection-config-v1` | `EdgeCollectionConfigurationDto`、`EdgeCollectionAssignmentDto`、`CollectionTaskDto` | `IoTSharp.Contracts/collection-config.v1.schema.json` | `IoTSharp.Contracts/examples/collection-config.v1.sample.json` |
| `edge-task-v1` | `EdgeTaskRequestDto`、`EdgeTaskDto`、`EdgeTaskReceiptDto`、`EdgeTaskStateMachine` | `IoTSharp.Contracts/edge-task.v1.schema.json` | `IoTSharp.Contracts/examples/edge-task.v1.sample.json` |
| `semantic-core-v1` | `SemanticModel`、`SemanticPoint`、`ProtocolBinding`、`SemanticModelValidator` | `IoTSharp.Contracts/semantic-core.v1.schema.json` | `IoTSharp.Contracts/examples/semantic-core.v1.sample.json` |

`IoTSharp.Contracts` 打包时会把 schema 放入 `schemas/`，把样例放入 `examples/`。边缘仓可用 PackageReference 消费 DTO，也可用 schema 对 HTTP 或消息载荷做回归验证。

## 版本规则

- 合同版本只追加，不重定义已发布字段语义。
- DTO 字段和 JSON Schema 属性只能做兼容扩展；删除、重命名和枚举重排都必须进入新合同版本。
- `collection-config-v1` 是 M2 #027 后的正式采集配置版本名；`edge-collection-v1` 只作为历史载荷识别值保留，不再作为新载荷输出。
- 执行端必须容忍新增字段；平台必须容忍旧执行端缺失非关键字段。
- 任务状态机以 `EdgeTaskStateMachine` 为准，终态不可回退。

## 边界

- EdgeNode 合同描述受管运行时生命周期，不承载 Product 模板或采集点位大对象。
- CollectionConfig 合同描述运行时采集配置和分配快照，不替代 M3 的 Product Collection Template。
- EdgeTask 合同描述下发、接受、执行和回执闭环，不承载 Release Center 的长周期编排模型。
- SemanticCore 合同描述点位含义、单位、质量来源和协议绑定，不保存运行时凭据、实时值、任务结果或发布状态。
- IoTEdge 不依赖 IoTSharp 内部数据库表、属性键或控制器内部 DTO；跨仓验收以 `IoTSharp.Contracts` 发布物为准。
