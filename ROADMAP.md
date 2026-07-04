# IoTSharp 最终路线图

本文档是 IoTSharp 当前阶段的执行版路线图。它不再记录所有可能方向，而是明确下一轮应该做什么、哪些已经完成、哪些后做、哪些概念需要删除或合并。

## 最终判断

IoTSharp 的目标不是继续做一个单纯设备平台，而是演进为三层协同的工业采集与运营平台：

1. 接入与采集层：Product、Device、Asset、Gateway、EdgeNode、Collection Template。
2. 实时规则层：Telemetry、Attribute、Event、Alarm、RuleChain、短动作分发。
3. 运营与发布层：配置版本、软件包、OTA、灰度发布、回滚、任务确认。

AI 是横切能力，不是第四条主线。AI 必须构建在 IoTSharp 控制平面、权限、审计、租户隔离、领域服务和 MCP 工具边界之上。

## 已完成的基线

以下内容视为已有平台基线，不再放入下一步任务清单：

- 设备接入、Device、Gateway、Telemetry、Attribute、Alarm、Tenant、Customer、权限和基础审计。
- Product 工作台第一版：列表、创建、属性、字典、数据映射和创建设备入口。
- Asset 基础模型和资产页面。
- FlowRule 规则链基础、规则绑定、模拟和事件记录基础。
- EdgeNode 第一版：模型、迁移、注册、心跳、能力上报、列表、详情、接入信息、任务分发、回执和历史。
- CollectionTask 草稿：DTO、草稿生成、基础校验和预览接口。
- MCP 和 AISettings 最小基础。
- SonnetDB 作为可选时序存储的基础接入。

这些内容后续可以增强，但不能继续写成“从零开始建设”。

## 近期主线

近期只保留一条主线：先完成领域概念清理，再把 Edge 与采集闭环做实。

| 阶段 | 状态 | 目标 | 交付 |
| --- | --- | --- | --- |
| P0 | 立即做 | 领域概念清理 | Product 正式替代 Produce；DeviceModel、旧设备图、旧场景等重复概念完成删除、合并或废弃标记。 |
| P1 | 立即做 | EdgeNode 正式模型 | EdgeTask、EdgeTaskReceipt、EdgeCapability、EdgeRuntimeStatus、EdgeCollectionAssignment 等正式表和 API。 |
| P2 | 接着做 | Product 下的采集模板 | CollectionTemplate、ConnectionTemplate、PointTemplate、TransformTemplate、SamplingPolicy、MappingPolicy。 |
| P3 | 接着做 | 配置发布到 Edge/Gateway | 从 Product 采集模板生成运行时配置，发布到 EdgeNode 或 Gateway，并形成任务闭环。 |
| P4 | 后续做 | Release Center | OTA、采集器软件包、配置版本、灰度、回滚、确认和审计。 |
| P5 | 后续做 | RuleChain 增强 | 标准节点、版本化、仿真、观测、审计；不承担长周期发布和运维编排。 |

## 当前立即执行项

### 1. Product 正式化

直接采用 Product 作为正式领域名。`Produce` 是旧概念，应从新代码、新 DTO、新 UI、新文档和新 API 中移除。

执行要求：

- `ProducesController`、Produce DTO、前端 `produce` 目录和接口逐步改为 Product 命名。
- 数据迁移可以按实际成本选择一次改表或过渡视图，但领域语言不再保留 Produce。
- `ProduceToken` 改为 `ProductToken`。
- Product 只负责模板：能力、点位、属性、命令、协议、采集模板、默认规则、告警模板、凭据策略和兼容矩阵。
- Product 不负责设备在线状态、遥测最新值、告警实例、命令执行记录和业务层级。

### 2. 删除或合并重复旧概念

以下概念不再作为核心路线继续扩展：

- `DeviceModel`：与 Product 能力模板重叠，合并到 Product 的命令/能力定义。
- 旧 `DeviceGraph`、`DeviceDiagram`、`DeviceGraphToolBox`：如果只是历史设计器数据，标记废弃；新拓扑进入 Asset、Collection 或 Edge 设计器。
- `Scene`：如果只是规则链或资产视图包装，不作为核心领域；合并到 RuleChain 视图或 Asset View。
- Product/Produce 上的旧 Gateway 配置字段：迁移到 CollectionTemplate。
- 把 Device 当业务层级、发布范围或模板容器的用法：停止扩展。

### 3. EdgeNode 正式化

现有 Edge 功能已经打通第一版，但状态和任务仍大量依赖 AttributeLatest 的 `_edge.*` 键。下一步必须沉淀正式模型。

必须新增或明确：

- EdgeRuntimeStatus：注册、心跳、版本、实例、主机、健康、指标。
- EdgeCapability：协议、点位类型、转换能力、任务能力和版本兼容。
- EdgeTask：平台下发的配置、诊断、软件、固件、重启等任务。
- EdgeTaskReceipt：Accepted、Running、Succeeded、Failed、TimedOut、Cancelled 等回执。
- EdgeCollectionAssignment：哪个采集配置发布到了哪个 EdgeNode 或 Gateway。

AttributeLatest 可以继续展示最近状态，但不再作为任务闭环的主存储。

### 4. Collection Template 产品化

CollectionTaskDto 当前只能算任务草稿，不是完整采集建模。下一步要把采集配置变成 Product 下的一等模板。

必须新增：

- CollectionTemplate：采集模板集合。
- ProtocolTemplate：协议类型和协议参数。
- ConnectionTemplate：连接地址、端口、串口、认证和超时。
- PointTemplate：点位地址、数据类型、长度、读写属性。
- TransformTemplate：缩放、偏移、表达式、枚举、位解析、字节序等转换链。
- SamplingPolicy：周期、变化上报、死区、质量变化和聚合提示。
- MappingPolicy：映射到 Telemetry、Attribute、AlarmInput 或 CommandFeedback。

模板属于 Product 或模板库；运行时任务下发给 EdgeNode、Gateway 或选定 Device。

### 5. Gateway 与 EdgeNode 边界

Gateway 是南向协议和子设备接入运行时。EdgeNode 是受管边缘运行时。

Gateway 负责：

- 协议适配。
- 子设备发现、创建和状态上报。
- 子设备遥测、属性、事件和告警上传。
- 采集任务执行和错误上报。

EdgeNode 负责：

- 注册、心跳、版本、实例身份和健康状态。
- 能力上报和运行诊断。
- 接收配置、软件、固件和诊断任务。
- 报告任务接收、执行、完成、失败、超时和回滚结果。

两者可以部署在同一进程，但平台模型必须分清。

## 后续池

以下内容保留方向，但不进入近期主线：

- Release Center 完整产品化：OTA、采集器软件包、配置版本、灰度、回滚和确认。
- AI Workbench：模型接入、技能目录、MCP 扩展、上下文会话、成本和审计。
- 本地语音、桌面 companion、本地模型协同、多智能体协作和记忆系统。
- 平台高可用、多服务器、灾备、备份恢复演练。
- SonnetDB 全面生产化路线和跨存储兼容矩阵。
- 前端实时遥测订阅增强。

这些内容不能和 P0-P3 混在一次改造里。

## 明确不做

- 不做全量重写。
- 不把 RuleChain 改造成长周期工作流引擎。
- 不让 AI 直接访问数据库或绕过权限审计。
- 不把 Device 当 Product 模板、Asset 树或 Release Scope 的替代品。
- 不把 Gateway 或 EdgeNode 契约藏在 IoTSharp 内部数据库假设里。
- 不因为清理依赖而删除 NSwag.AspNetCore、RulesEngine、Jint 等当前保留基础组件。

## 验收标准

近期路线完成时，应满足：

- 新文档、新菜单、新 DTO、新 API 使用 Product，不再新增 Produce 命名。
- Product、Device、Asset、Gateway、EdgeNode 和 Collection Template 边界清楚。
- Edge 注册、心跳、能力、任务、回执和历史有正式模型承载。
- 平台能知道某个采集模板发布到了哪个 EdgeNode 或 Gateway。
- Device 页面只展示实例状态、凭据、遥测、属性、告警、命令和关系。
- Product 页面只负责能力模板、采集模板、点位、命令、默认规则和兼容矩阵。
- Release、OTA、配置发布和回滚不进入 RuleChain。
- AI 只通过授权领域服务和 MCP 工具使用 IoTSharp 能力。

## 文档关系

详细领域定义和分步改造见：`docs/docs/architecture/domain-model-realignment.md`。
