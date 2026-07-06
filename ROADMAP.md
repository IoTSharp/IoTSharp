# IoTSharp 执行路线图

本文档是 IoTSharp 当前阶段开始执行的路线图。它用于跟踪顺序、里程碑和完成情况，不再记录所有可能方向。

本路线图同时对齐产品矩阵各层的接口承诺：平台层事项在本仓库实现；边缘层、设备层与 AI 底座事项在对应仓库实现，但契约、验收和推进顺序以本路线图为准。

## 状态标签

- `✅️` 已完成：已经具备可用基础，不再作为下一步主任务。
- `🚧` 进行中：当前正在推进或下一批立即开始。
- `⬜` 待开始：已确定要做，但排在当前进行中任务之后。
- `🕒` 后续池：保留方向，但不进入当前里程碑。
- `🗑️` 删除或停止扩展：旧概念、重复能力或不再作为核心领域。
- `⏸️` 暂缓：需要等待前置任务完成后再决定。

## 最终方向

IoTSharp 从单纯设备平台演进为三层协同的工业采集与运营平台：

1. 接入与采集层：Product、Device、Asset、Gateway、EdgeNode、Collection Template。
2. 实时规则层：Telemetry、Attribute、Event、Alarm、RuleChain、短动作分发。
3. 运营与发布层：配置版本、软件包、OTA、灰度发布、回滚、任务确认。

AI 是横切能力，不是第四条主线。AI 必须通过 IoTSharp 控制平面、权限、审计、租户隔离、领域服务和 MCP 工具边界使用平台能力。

## 产品矩阵与仓库分工

IoTSharp 是覆盖「平台 - 边缘 - 设备」三层加一个 AI 底座的开源产品矩阵，云边一体、全栈自研：

| 层级 | 仓库 | 职责 | 与平台的契约 |
| --- | --- | --- | --- |
| 平台层 | [IoTSharp](https://github.com/IoTSharp/IoTSharp)（本仓库） | 控制面：设备接入、遥测、规则链、多租户、EdgeNode 管理、配置与发布运营 | 契约的定义方和单一事实来源 |
| 边缘层 | [IoTEdge](https://github.com/IoTSharp/IoTEdge) | 边缘网关运行时：单宿主程序 + 本地管理界面，Modbus/OPC UA/PLC 采集驱动、脚本转换、断网自治 | EdgeNode 契约第一执行端：注册、心跳、能力上报、采集配置拉取、任务回执 |
| 设备层 | [IoTEmbedded](https://github.com/IoTSharp/IoTEmbedded) | 嵌入式设备运行时：MCU/RTOS 固件级运行时，BASIC 脚本引擎、Modbus RTU、MQTT 接入，脚本双槽存储与失败回滚 | 设备侧 MQTT 协议族；设备脚本/固件发布的执行端 |
| AI 底座 | [Tomur](https://github.com/IoTSharp/Tomur) | 离线/内网本地模型运行时：llama.cpp/whisper.cpp/stable-diffusion.cpp/OCR 多模态推理，GGUF 模型资产管理，OpenAI/Ollama/Anthropic 三套兼容 API，Native AOT 单文件 | 平台 AI 能力的离线 Provider：AISettings 指向 Tomur 兼容端点即可在无外网环境运行 AI；AI 仍经平台权限、审计与 MCP 边界 |

跨仓原则：

- 平台侧契约（EdgeNode 注册/心跳/能力、CollectionConfig、EdgeTask）以 `IoTSharp.Contracts` 为单一事实来源并版本化，边缘与设备仓库作为契约消费者。
- 边缘与设备运行时不依赖 IoTSharp 内部数据库结构作为跨项目契约。
- AI 横切能力对接 Tomur 时只依赖其 OpenAI 兼容 API，不依赖 Tomur 内部实现；在线（云端模型）与离线（Tomur）通过 Provider 配置切换，平台侧代码不感知差异。
- 三层可独立使用，组合时形成端到端的「云端定义、边缘执行、断网自治、结果回传」闭环。
- 涉及边缘层、设备层与 AI 底座的事项在本路线图登记编号并跟踪验收，代码在各自仓库落地。

差异化定位（不针对具体厂商，仅指导取舍）：全栈零外部依赖交付是本矩阵独有优势——平台可用 SonnetDB 单依赖部署，边缘网关可直连 SonnetDB，AI 可由 Tomur 离线承载，形成「云边同底座 + 内网全功能」。该叙事由并行轨道 T 承载。

## 里程碑

| 里程碑 | 状态 | 目标 | 完成标准 |
| --- | --- | --- | --- |
| M0 | `✅️` | 确认最终路线图 | 明确 Product、Device、Asset、Gateway、EdgeNode、Collection Template、RuleChain 和 Release Center 边界。 |
| M1 | `✅️` | 领域概念清理 | 旧 Produce 已直接迁移为 Product；DeviceModel、旧设备图完成删除或合并，旧 Scene 收口到后续 Asset View 建设；Gateway 旧配置迁移方案已明确。 |
| M2 | `✅️` | EdgeNode 正式模型 + 跨仓契约固化 | EdgeRuntimeStatus、EdgeCapability、EdgeTask、EdgeTaskReceipt、EdgeCollectionAssignment 落地；edge-node/collection-config/edge-task 契约进入 `IoTSharp.Contracts` 并版本化；semantic-core 收敛决策完成。 |
| M3 | `✅️` | Collection Template 产品化 | Product 下可维护协议、连接、点位、转换、采样和映射模板；模板生成的运行时配置已可被 IoTEdge 直接消费执行。 |
| M4 | `🚧` | 配置与最小软件包发布闭环 | 采集配置和最小软件包可发布到 EdgeNode/Gateway，形成下发、接收、执行、回执和历史；边缘侧数据断网缓存与续传落地。 |
| M5 | `⬜` | Release Center 第一版 | OTA（含设备脚本/固件）、采集器软件包、配置版本、灰度、回滚、确认和审计具备基础闭环。 |
| M6 | `🕒` | RuleChain 增强 | 标准节点、版本化、仿真、观测和审计增强，但不承担长周期发布；AI 节点与 MCP 扩写在此评估。 |
| T | `🚧` | SonnetDB 单依赖交付叙事（并行轨道） | README/文档首推单依赖体验路径；容量与可靠性基准报告；云边同底座参考部署。 |
| E | `⬜` | 附属运行时硬化（并行轨道） | IoTEdge 与 IoTEmbedded 达产：发布纪律、测试、资源占用、协议插件化、远程诊断、硬件矩阵。 |
| A | `⬜` | 离线 AI 底座（并行轨道） | Tomur 从孵化走向可依赖：发布纪律与测试；平台 AI Provider 抽象打通在线/离线切换；内网全功能参考部署。 |

## 执行顺序

### 已完成基线

这些能力已经存在，只作为后续改造基础，不再写成“从零开始”。

| 顺序 | 状态 | 事项 | 说明 |
| --- | --- | --- | --- |
| #000 | `✅️` | 平台基础 | Device、Gateway、Telemetry、Attribute、Alarm、Tenant、Customer、权限和基础审计已具备。 |
| #001 | `✅️` | Product 工作台第一版 | 列表、创建、属性、字典、数据映射和创建设备入口已具备。 |
| #002 | `✅️` | Asset 基础 | Asset 模型和资产页面已具备。 |
| #003 | `✅️` | FlowRule 基础 | 规则链、规则绑定、模拟和事件记录已有基础。 |
| #004 | `✅️` | EdgeNode 第一版 | 模型、迁移、注册、心跳、能力上报、列表、详情、接入信息、任务分发、回执和历史已具备。 |
| #005 | `✅️` | CollectionTask 草稿 | DTO、草稿生成、基础校验和预览接口已具备。 |
| #006 | `✅️` | AI 最小基础 | MCP 和 AISettings 已具备。 |
| #007 | `✅️` | SonnetDB 可选接入 | 关系库、遥测、缓存、EventBus 和 Blob 均可走 SonnetDB，单依赖部署 profile 已具备。 |
| #008 | `✅️` | IoTEdge 对接基线 | 边缘网关已打通注册、心跳、能力上报、CollectionConfig 拉取（版本化落本地缓存）、EdgeTask 拉取/接受/回执（edge-task-v1）；Modbus、OPC UA 及主流 PLC 驱动为真实实现；本地离线配置自治已落地。 |
| #009 | `✅️` | IoTEmbedded 基线 | 固件级运行时已在 STM32 双板落地；BASIC 脚本引擎、Modbus RTU Master、MQTT 设备协议（注册/心跳/命令响应）具备；EEPROM 双槽脚本 + CRC + 签名 + 启动失败回滚已成型。 |
| #00A | `✅️` | Tomur 孵化基线 | 本地模型运行时已具雏形：llama.cpp/whisper.cpp/stable-diffusion.cpp/OCR 多后端推理、GGUF 模型资产管理、OpenAI/Ollama/Anthropic 三套兼容 API、Native AOT 单文件发布已验证。当前为孵化期：无测试、无正式 Release、未与平台集成。 |
| #00B | `✅️` | MySQL .NET 10 Provider 对齐 | MySQL 主库和 HealthChecks UI MySQL storage 均使用 Microting 系列包；`Microting.EntityFrameworkCore.MySql` 与 `DotNetDiag.HealthChecks.UI.MySql.Storage` 的 net10 依赖版本已对齐，主项目全量构建通过。 |

### M1 - 领域概念清理

M1 已完成。目标是先把概念和命名收干净，再扩展 Edge 和采集能力。

范围控制：旧 `Produce` 不再保留兼容层，模型、DTO、Controller、前端目录/API、Token/枚举和当前迁移快照统一使用 Product。历史 EF migration 只作为数据库演进记录保留旧名。

| 顺序 | 状态 | 事项 | 交付 |
| --- | --- | --- | --- |
| #010 | `✅️` | Product 正式替代旧 Produce | 旧 Produce 领域模型、控制器、DTO、前端入口和当前 EF 快照已迁移到 Product。 |
| #011 | `✅️` | Produce 命名迁移清单 | `Produce`、`ProduceToken`、`ProducesController`、前端 `produce` 目录、DTO、API 和文档引用已清理或迁移。 |
| #012 | `✅️` | Product API 命名迁移 | Product API 已替换旧 Produce API，旧 Produce 入口不再保留兼容。 |
| #013 | `✅️` | 前端 Product 工作台迁移 | 前端 `produce` 目录和 API 模块已迁移为 `product`。 |
| #014 | `✅️` | ProductToken 替代 ProduceToken | 注册 Token 命名已统一到 ProductToken，UI、DTO、文档和接口字段已清理。 |
| #015 | `✅️` | DeviceModel 合并 | `DeviceModel` 的命令和能力含义已合并到 Product，活动 API 使用 ProductCommand。 |
| #016 | `✅️` | 旧设备图处理 | `DeviceGraph`、`DeviceDiagram`、`DeviceGraphToolBox` 无活动 API/前端引用，已删除实体、DbSet 和当前模型；新增迁移删除历史表。 |
| #017 | `✅️` | Scene 概念处理 | Scene 与 Asset View 重叠，不再作为独立核心领域；独立菜单和前端页移除，后续三维/业务可视化能力并入 Asset View 建设。 |
| #018 | `✅️` | Gateway 旧配置迁移方案 | 已明确 Product 上旧 Gateway 配置字段到 Collection Template 的映射、干跑、人工确认、发布和关闭旧入口方案，详见 `docs/docs/architecture/gateway-legacy-configuration-migration.md`。 |

### M2 - EdgeNode 正式模型与跨仓契约固化

M2 的目标是把现有 Edge 第一版能力从属性键和临时 DTO 沉淀为正式模型，并把云边契约固化为可版本化的单一事实来源。IoTEdge 已作为真实消费者打通全部端点（#008），契约固化必须以它为第一验证对象，避免纸面契约。

| 顺序 | 状态 | 事项 | 层级 | 交付 |
| --- | --- | --- | --- | --- |
| #020 | `✅️` | EdgeRuntimeStatus | 平台 | 已新增 `edge-runtime-status-v1`、`EdgeRuntimeStatusDto`、EdgeNode 内嵌 runtimeStatus 和 `/api/Edge/{id}/RuntimeStatus` 只读接口，覆盖注册、心跳、版本、实例、主机、健康和低频指标模型。 |
| #021 | `✅️` | EdgeCapability | 平台 | 已新增 `edge-capability-v1`、`EdgeCapabilityDto`、EdgeNode 内嵌 capability 和 `/api/Edge/{id}/Capability` 只读接口，覆盖协议、点位类型、转换能力、任务能力和合同版本兼容模型。 |
| #022 | `✅️` | EdgeTask | 平台 | 已新增正式 `EdgeTask` 主模型、`EdgeTasks` 迁移和 `EdgeTaskDto` 状态快照；Dispatch/Pull/Accept/Receipt 以正式表承载任务当前态。 |
| #023 | `✅️` | EdgeTaskReceipt | 平台 | 已新增正式 `EdgeTaskReceipt` 历史模型和 `EdgeTaskReceipts` 迁移；Receipt/Accept 写入正式回执表并同步 EdgeTask 当前态。 |
| #024 | `✅️` | EdgeCollectionAssignment | 平台 | 已新增正式 `EdgeCollectionAssignment` 模型、`EdgeCollectionAssignments` 迁移和 `EdgeCollectionAssignmentDto`；保存 CollectionConfig 时生成 Active 分配，旧版本转 Superseded，执行端拉取时记录 `lastPulledAt`，并提供 `/api/Edge/CollectionAssignments` 与 `/api/Edge/{id}/CollectionAssignments` 查询。 |
| #025 | `✅️` | Edge API 改造 | 平台 | 状态和能力查询从 EdgeNode 正式模型生成；任务、回执和历史查询使用 `EdgeTask`/`EdgeTaskReceipt`；任务状态流转不再写入或回退到 AttributeLatest/TelemetryData 主存储。 |
| #026 | `✅️` | Edge 前端改造 | 平台 | Edge 详情页已优先消费正式运行态、能力、任务历史和配置分配查询模型，并保留旧扁平字段兜底。 |
| #027 | `✅️` | 跨仓契约固化 | 平台 | `edge-node-v1`、`collection-config-v1`、`edge-task-v1` 的 DTO、JSON Schema、样例和包元数据已进入 `IoTSharp.Contracts`；IoTSharp 侧移除 Edge 合同影子 DTO，IoTEdge 消费切换在边缘仓按该契约包验收。 |
| #028 | `✅️` | IoTEdge 任务回执转正 | 边缘 | 已将参考实现级任务回执组件升级为正式 `EdgeTaskReceiptReporter` 与常驻任务分发循环，遵循 #027 `edge-task-v1` 契约，覆盖 Accepted/Running/Succeeded/Failed/TimedOut 全状态回执。 |
| #029 | `✅️` | semantic-core 收敛决策（M3 前置门） | 平台 | 已决策采用 `IoTSharp.Contracts/semantic-core.v1.schema.json`（含 Modbus/OPC UA/MQTT 绑定）作为 M3 Collection Template 的语义和协议绑定基础；Collection Template、`collection-config-v1` 与 IoTEdge 本地采集模型分层消费，避免三套点位模型并存。 |

### M3 - Collection Template 产品化

M3 的目标是把采集配置从草稿 DTO 升级为 Product 下的一等模板。#030 依赖 #029 决策结论。

| 顺序 | 状态 | 事项 | 层级 | 交付 |
| --- | --- | --- | --- | --- |
| #030 | `✅️` | CollectionTemplate | 平台 | 已新增 Product 下正式采集模板聚合、DTO、API 和多 provider 迁移，模型基础按 #029 决策。 |
| #031 | `✅️` | ProtocolTemplate | 平台 | 已新增协议模板，保存协议类型、semantic-core protocolKind 和协议级非敏感参数。 |
| #032 | `✅️` | ConnectionTemplate | 平台 | 已新增连接模板，覆盖地址、端口、串口、认证类型、超时和重试，不保存明文凭据。 |
| #033 | `✅️` | PointTemplate | 平台 | 已新增点位模板，覆盖 semanticId、bindingId、点位地址、数据类型、长度和读写属性。 |
| #034 | `✅️` | TransformTemplate | 平台 | 已新增转换链模板，覆盖缩放、偏移、表达式、枚举、位解析、字节序和错误默认值等扩展参数。 |
| #035 | `✅️` | SamplingPolicy | 平台 | 已新增采样策略，覆盖周期、变化上报、死区、质量变化、订阅型采集和聚合提示。 |
| #036 | `✅️` | MappingPolicy | 平台 | 已新增映射策略，可映射到 Telemetry、Attribute、AlarmInput 或 CommandFeedback。 |
| #037 | `✅️` | 模板校验和预览 | 平台 | 已基于正式模板提供校验、非敏感参数检查、预览和运行时配置生成接口。 |
| #038 | `✅️` | 边缘执行链路对齐 | 边缘 | 平台从 Product Collection Template 生成的 `collection-config-v1` 已携带 `ProductCollectionTemplate` 来源信息；IoTEdge 已对齐当前合同版本，完成平台载荷反序列化、本地映射、轮询执行、转换和上传链路验收。 |

### M4 - 配置与最小软件包发布闭环

M4 的目标是从 Product 采集模板生成配置版本，发布到 EdgeNode 或 Gateway，并把最小软件包发布和边缘数据可靠性一并闭环。任务通道复用 M2 的 EdgeTask，不另建机制。

| 顺序 | 状态 | 事项 | 层级 | 交付 |
| --- | --- | --- | --- | --- |
| #040 | `✅️` | Collection Configuration Version | 平台 | 已新增 `CollectionConfigurationVersion` 正式模型、`CollectionConfigVersions` 多 provider 迁移和 `CollectionConfigurationVersionDto`；保存 CollectionConfig 时生成版本快照并关联 Active assignment，提供 `/api/Edge/CollectionConfigVersions`、`/api/Edge/{id}/CollectionConfigVersions` 和版本详情查询。 |
| #041 | `✅️` | 配置发布任务 | 平台 | 已新增 `POST /api/CollectionTemplates/{id}/PublishConfig`，从 Active Product Collection Template 生成 `CollectionConfigurationVersion`，创建 Active assignment，并生成 `ConfigPullRequest` EdgeTask，任务参数包含配置版本 ID、版本号和哈希。 |
| #042 | `✅️` | Edge/Gateway 拉取或接收配置 | 平台 | 已新增 `GET /api/Edge/{access_token}/CollectionConfig/Pull`，执行端可按接入令牌获取当前目标配置、Active assignment、配置版本 ID 和哈希；旧 `CollectionConfig` 短接口继续返回配置正文并记录 `LastPulledAt`。 |
| #043 | `✅️` | 配置执行回执 | 平台 | 已接收 Accepted、Running、Succeeded、Failed 等回执，校验配置版本/哈希，并回写 assignment 最近执行态和已应用版本。 |
| #044 | `✅️` | 当前版本和目标版本展示 | 平台 | 已新增 `EdgeCollectionVersionStatusDto` 与 `/api/Edge/{id}/CollectionVersionStatus`，Edge 列表和详情展示当前版本、目标版本、差异摘要和最近配置发布结果。 |
| #045 | `⬜` | 失败重试和审计 | 平台 | 发布结果可查询、可审计、可失败重试。 |
| #046 | `⬜` | 最小软件包发布 | 平台 | ReleasePackage 最小模型（包上传、校验和、版本、目标运行时），新增 EdgeTask 软件更新任务类型与回执；不含灰度、批次和审批（留 M5）。 |
| #047 | `⬜` | IoTEdge 软件更新执行器 | 边缘 | 接收 #046 任务：下载、校验、切换、失败回滚、全程回执。 |
| #048 | `⬜` | 边缘数据断网缓存与续传 | 边缘 | 平台不可达时本地缓存采集数据，恢复后批量续传（对应 IoTEdge 自有路线 A6）；M4 数据可靠性闭环的验收项。 |

### M5 - Release Center 第一版

M5 排在 Edge 和采集闭环之后。它不进入 RuleChain。#050 在 #046 最小模型上兼容扩展，不重建。

| 顺序 | 状态 | 事项 | 层级 | 交付 |
| --- | --- | --- | --- | --- |
| #050 | `⬜` | ReleasePackage | 平台 | OTA、采集器软件包、配置包模型（扩展 #046）。 |
| #051 | `⬜` | ReleasePlan | 平台 | 发布范围、批次、灰度策略和确认策略。 |
| #052 | `⬜` | ReleaseTask | 平台 | 面向 EdgeNode、Gateway、Device 或范围集合的发布任务。 |
| #053 | `⬜` | ReleaseReceipt | 平台 | 接收、执行、完成、失败、超时和回滚结果。 |
| #054 | `⬜` | 回滚和暂停继续 | 平台 | 支持暂停、继续、回滚和人工确认。 |
| #055 | `⬜` | 发布审计 | 平台 | 每次发布有状态、结果、操作人和审计记录。 |
| #056 | `⬜` | 设备脚本 OTA | 设备 | 面向 IoTEmbedded 的脚本包发布：复用其双槽存储、CRC、签名和启动失败回滚基建，走设备 MQTT 通道下发与回执。 |
| #057 | `⬜` | 设备固件 OTA | 设备 | 固件包发布与升级回执；依赖设备侧 bootloader 支持，范围在 M5 内单独评估。 |

### M6 - RuleChain 增强

M6 后做，目标是增强实时规则，而不是把规则链改成工作流引擎。IoTSharp 的方向不是复制通用 Node-RED，也不是完整复刻 ThingsBoard，而是形成面向 Product、Device、Asset、Gateway、EdgeNode 和 Collection Template 的实时规则图：借鉴 ThingsBoard 的消息语义、关系路由和可靠执行，借鉴 Node-RED 的节点体验、调试体验和基础节点丰富度。

M6 的第一原则是先把规则链运行时的“命名、消息、关系、节点、版本、观测”产品化，再评估 AI 节点和 MCP 写扩展。脚本节点继续保留，但标准节点应覆盖大多数常见场景，避免用户把清洗、路由、富化、告警和发布全部写成脚本。

M6 基础设计要求：

- 先做命名治理：新 API、DTO、UI、文档和节点库统一使用 RuleChain 语言；旧 `FlowRule`、`Flow`、`NodeProcess*`、`Mata/MataData`、`Excutor` 等命名只作为兼容层或迁移来源，不再扩展为新的公开概念。
- 定义 `RuleMessage v1`：包含 `messageId`、`traceId`、`tenantId`、`originatorType`、`originatorId`、`productId`、`assetId`、`gatewayId`、`edgeNodeId`、`messageType`、`payload` 和 `metadata`，并兼容现有 FlowRule 动态数据输入。
- 定义 `RuleRelation v1`：保留连线表达式，同时支持 `Success`、`Failure`、`True`、`False`、`Matched`、`Unmatched`、`Timeout`、`Error` 和 `Custom` 等关系，便于失败兜底、超时处理和未匹配分支。
- 定义节点 manifest：描述节点类型、分类、显示名、输入输出、配置 JSON Schema、默认值、权限需求、超时、重试、副作用和帮助文档。
- 运行器增强应优先服务实时链路：最大深度、最大耗时、取消、失败关系、超时关系、背压、指标和错误定位；不引入长周期审批、灰度、回滚或发布编排。

M6 命名收口要求：

| 标准名称 | 含义 | 迁移来源或禁止继续扩展的旧名 |
| --- | --- | --- |
| `RuleChain` | 一条实时规则链定义 | `FlowRule` 仅作旧存储/兼容名 |
| `RuleChainVersion` | 不可变发布快照 | `Version`/`SubVersion` 需收口语义 |
| `RuleNode` | 规则链中的节点实例 | `Flow` 中的节点语义 |
| `RuleRelation` | 节点之间的连接和路由关系 | `Flow` 中的连线语义、`SourceId`/`TargetId` 组合 |
| `RuleCondition` | 关系上的条件表达式 | `Conditionexpression` |
| `RuleMessage` | 规则链运行时传递的标准消息 envelope | 裸 `data`/动态对象 |
| `RuleNodeDefinition` 或 `RuleNodeManifest` | 节点库中的可复用节点定义 | 临时执行器列表、硬编码前端说明 |
| `RuleNodeConfig` | 节点实例配置 | `NodeProcessParams` |
| `RuleExecution` | 一次规则链执行 | `BaseEvent` 中的规则运行语义 |
| `RuleStepTrace` | 单个节点或关系的执行轨迹 | `FlowOperation` |
| `RuleBinding` | Product、Device 等对象与规则链版本的绑定 | 零散设备规则绑定 |
| `metadata` | 运行上下文元数据 | `Mata`/`MataData` 不再新增 |
| `Executor` | 内置执行器/动作节点实现 | `Excutor` 拼写只保留兼容别名 |

| 顺序 | 状态 | 事项 | 交付 |
| --- | --- | --- | --- |
| #060 | `🕒` | 命名治理、RuleMessage 与关系路由 | 统一 RuleChain、RuleNode、RuleRelation、RuleMessage、RuleExecution 等标准名称；定义标准消息 envelope、metadata、originator、traceId 和 relation 语义；兼容现有 `FlowRule`/`Flow` 输入输出，并保留连线条件表达式。 |
| #061 | `🕒` | 标准节点库与节点 manifest | 减少脚本依赖；第一批覆盖过滤/路由、转换、上下文富化、发布/动作、外部集成、观测调试和停止节点；每类节点提供配置 Schema、输入输出约定和帮助说明。 |
| #062 | `🕒` | 规则版本和发布 | 支持草稿、发布版本、复制、回滚和不可变发布快照；规则绑定应指向明确版本，避免编辑中规则影响生产流量。 |
| #063 | `🕒` | 仿真、观测和审计 | 保存仿真输入、输出、命中 relation、节点耗时、运行路径、错误堆栈、动作副作用和审计记录，支持按 traceId 回看一次规则执行。 |
| #064 | `🕒` | 规则链 AI 节点 | LLM 调用节点：提示词模板、结构化输出，复用 #090 的 Provider 抽象（在线/离线均可），全程审计。 |
| #065 | `🕒` | MCP 工具扩写与受控动作 | 从只读设备查询扩展到受权限、审计、租户隔离和人工确认约束的写操作；高风险动作不得由规则链静默触发。 |
| #066 | `🕒` | 派生计算与运行器增强评估 | 评估遥测派生字段/计算属性、内部 work queue、重试、超时、背压和失败分支；只服务实时链路，不演化为长周期工作流引擎。 |

### 并行轨道 T - SonnetDB 单依赖交付叙事

低成本、以文档和 CI 为主，不占用主线开发资源，可与任何里程碑并行。目标是把「全栈零外部依赖、云边同底座」确立为对外首要卖点。

| 顺序 | 状态 | 事项 | 交付 |
| --- | --- | --- | --- |
| #070 | `⬜` | 单依赖体验路径首推 | README 和文档把 SonnetDB profile（`docker-compose.sonnetdb.yml` 或单文件二进制）定为 5 分钟快速体验的默认入口。 |
| #071 | `⬜` | 容量与可靠性基准报告 | 依托 SonnetDB 可观测性指标，给出遥测写入吞吐、存储容量和恢复能力的基准数据与文档。 |
| #072 | `⬜` | 云边同底座参考部署 | IoTEdge 直连 SonnetDB 的参考架构与部署文档，展示平台与边缘共用同一数据底座。 |

### 并行轨道 E - 附属运行时硬化（IoTEdge / IoTEmbedded）

IoTEdge 与 IoTEmbedded 是本项目的附属产品，其改进事项在本路线图统一登记和跟踪，代码在各自仓库落地。目标是把两个运行时从「骨架已立」推进到「可交付达产」。与主线的交叉项（#028、#038、#047、#048、#056、#057）仍按里程碑节奏走，本轨道承载不阻塞主线的工程化硬化项。

| 顺序 | 状态 | 事项 | 仓库 | 交付 |
| --- | --- | --- | --- | --- |
| #080 | `⬜` | IoTEdge 发布纪律 | IoTEdge | 版本号策略、Release/tag、变更记录；恢复持续提交节奏；与平台契约版本的兼容矩阵。 |
| #081 | `⬜` | IoTEdge 测试基线 | IoTEdge | 驱动层（Modbus/OPC UA）与平台对接 Worker 的自动化测试覆盖；契约回归用例对齐 `IoTSharp.Contracts`。 |
| #082 | `⬜` | IoTEdge 轻量化交付 | IoTEdge | Native AOT/裁剪评估，单文件发布，linux-arm64 目标（主流边缘盒子架构）；资源占用基线报告。 |
| #083 | `⬜` | IoTEdge 协议驱动插件化 | IoTEdge | 驱动以插件形式装载，新协议不改宿主；插件清单与能力上报（EdgeCapability）联动。 |
| #084 | `⬜` | IoTEdge 远程诊断 | IoTEdge | 日志摘要上报、远程诊断任务（复用 EdgeTask 诊断类型）、本地故障自检。 |
| #085 | `⬜` | IoTEmbedded 板卡与网络矩阵 | IoTEmbedded | 扩展支持的 MCU 板卡与网络模组清单；低资源 Linux Profile 评估。 |
| #086 | `⬜` | IoTEmbedded 设备协议对齐 | IoTEmbedded | 设备侧 MQTT 协议（注册/心跳/命令/升级响应）与平台 Device 契约对齐并文档化，作为第三方固件接入的参考实现。 |
| #087 | `⬜` | IoTEmbedded 发布纪律 | IoTEmbedded | 版本化、Release、板卡适配指南与烧录文档。 |

### 并行轨道 A - 离线 AI 底座（Tomur）

Tomur 是矩阵的 AI 底座：离线或内网环境下，平台的 AI 横切能力由它承载。当前处于孵化期（无测试、无正式发布、未与平台集成），本轨道目标是把它推进到「平台可依赖的离线 AI Provider」。原则不变：AI 只能通过平台权限、审计、租户隔离和 MCP 边界使用平台能力，Tomur 只是模型算力的提供方，不是旁路。

| 顺序 | 状态 | 事项 | 仓库 | 交付 |
| --- | --- | --- | --- | --- |
| #090 | `⬜` | 平台 AI Provider 抽象 | 平台 | AISettings 扩展为 Provider 配置（在线云端模型 / 离线 Tomur 端点可切换）；平台侧统一走 OpenAI 兼容客户端，不感知 Provider 差异；连接测试与健康检查。 |
| #091 | `⬜` | Tomur 发布纪律 | Tomur | License、版本号、Release/tag、变更记录；smoke 记录转为可重复的自动化测试基线。 |
| #092 | `⬜` | Tomur 测试基线 | Tomur | 推理 API（chat/embeddings）、模型资产管理、服务生命周期的自动化测试；三套兼容 API 的契约回归。 |
| #093 | `⬜` | 内网全功能参考部署 | 平台 | 「IoTSharp + SonnetDB + Tomur」无外网参考部署文档与 compose/安装脚本：平台 AI 功能（MCP、后续规则链 AI 节点）全部指向本地 Tomur 端点验证通过。 |
| #094 | `⬜` | Tomur 边缘就位评估 | Tomur | linux-arm64 发布目标；小内存 GGUF 模型档位推荐（边缘盒子级硬件）；与 IoTEdge 同机部署的资源占用评估。 |
| #095 | `⏸️` | 边缘 AI 场景试点 | IoTEdge | 在边缘侧调用本地 Tomur 做数据摘要/异常描述等试点场景；依赖 #094 完成后再决定范围。 |

## 层级分工总览

| 里程碑/轨道 | 平台层（本仓库） | 边缘层（IoTEdge） | 设备层（IoTEmbedded） | AI 底座（Tomur） |
| --- | --- | --- | --- | --- |
| M1 | #010~#018 | — | — | — |
| M2 | #020~#027、#029 | #028 回执转正 | — | — |
| M3 | #030~#037 | #038 执行链路对齐 | — | — |
| M4 | #040~#046 | #047 软件更新执行器、#048 断网续传 | — | — |
| M5 | #050~#055 | 灰度/回滚执行端配合 | #056 脚本 OTA、#057 固件 OTA | — |
| M6 | #060~#066 | — | — | #064 经 #090 消费离线推理 |
| T | #070、#071 | #072 同底座示例 | — | — |
| E | — | #080~#084 | #085~#087 | — |
| A | #090、#093 | #095 边缘 AI 试点 | — | #091、#092、#094 |

## 删除和停止扩展

| 顺序 | 状态 | 事项 | 处理 |
| --- | --- | --- | --- |
| #900 | `🗑️` | Product 作为新领域名 | 停止扩展；统一改为 Product。 |
| #901 | `🗑️` | DeviceModel 作为独立核心概念 | 合并到 Product 的能力、命令和配置定义。 |
| #902 | `🗑️` | 旧 DeviceGraph/DeviceDiagram 作为核心拓扑 | 不再扩展；新拓扑进入 Asset、Collection 或 Edge 设计器。 |
| #903 | `🗑️` | Scene 作为独立核心领域 | 已收口到 Asset View 后续建设，不再维护独立 Scene 领域、菜单或页面。 |
| #904 | `🗑️` | Product 上的 Gateway 旧配置 | 按 #018 方案迁移到 Collection Template，正式落表和发布闭环分别归 #030 与 #040 之后推进。 |
| #905 | `🗑️` | Device 作为业务层级或发布范围替代品 | 停止扩展。 |
| #906 | `🗑️` | EdgeTask 状态以 AttributeLatest/TelemetryData 为主存储 | 已随 M2 #025 停止作为 API 查询和状态流转主路径。 |

## 后续池

| 顺序 | 状态 | 事项 | 说明 |
| --- | --- | --- | --- |
| #800 | `🕒` | AI Workbench | 模型接入、技能目录、MCP 扩展、上下文会话、成本和审计；离线算力由轨道 A 的 Tomur 承载。 |
| #801 | `🕒` | 本地语音和桌面 companion | 保持独立运行时，不耦合进主 Web 应用；语音能力（ASR/TTS）复用 Tomur 多模态后端。 |
| #802 | `🕒` | 多智能体协作和记忆系统 | 等核心领域模型稳定后再设计。 |
| #803 | `🕒` | 平台高可用和灾备 | 等 Edge、采集、发布模型稳定后再推进。 |
| #804 | `🕒` | SonnetDB 引擎生产化 | 引擎侧能力推进按 SonnetDB 自有路线图执行；平台侧叙事走并行轨道 T。 |
| #805 | `🕒` | 前端实时遥测订阅增强 | 后续单独进入体验优化。 |

## 明确不做

- 不做全量重写。
- 不把 RuleChain 改造成长周期工作流引擎。
- 不让 AI 直接访问数据库或绕过权限审计；Tomur 只是模型算力 Provider，不是权限旁路。
- 不把 Device 当 Product 模板、Asset 树或 Release Scope 的替代品。
- 不把 Gateway 或 EdgeNode 契约藏在 IoTSharp 内部数据库假设里。
- 不在本仓库直接实现边缘采集驱动、设备固件逻辑和模型推理，它们分属 IoTEdge、IoTEmbedded 与 Tomur。
- 不把 Tomur 做成多租户推理服务器；平台多租户隔离在平台侧完成。
- 不因为清理依赖而删除 NSwag.AspNetCore、RulesEngine、Jint 等当前保留基础组件。

## 当前执行窗口

M1（#010~#018）、M2（#020~#029）和 M3（#030~#038）已完成；M4 已完成 #040 Collection Configuration Version、#041 配置发布任务、#042 Edge/Gateway 拉取或接收配置、#043 配置执行回执和 #044 当前版本/目标版本展示，下一步推进 #045 失败重试和审计。并行轨道 T（#070~#072）为文档层面工作，可随时穿插。

M3 已关闭，采集模板和边缘执行链路已经形成最小闭环。M4 已具备采集配置版本快照、模板发布到 EdgeTask、执行端目标配置获取、执行回执和配置版本差异展示的基础，接下来进入失败重试和审计；M5 仍需等待 M4 的最小软件包能力和数据可靠性闭环稳定后再启动正式实现。

并行轨道 E（附属运行时硬化）与 A（离线 AI 底座）按带宽穿插推进，不阻塞主线；但存在两个硬依赖：#047/#048 是 M4 验收项，#090（AI Provider 抽象）是 M6 #064/#065 与 #093 内网部署的前置。

涉及 IoTEdge（#028、#038、#047、#048、#072、#080~#084、#095）、IoTEmbedded（#056、#057、#085~#087）和 Tomur（#091、#092、#094）的事项在各自仓库实现，验收标准和契约版本以本路线图与 `IoTSharp.Contracts` 为准。

详细领域定义和分步改造见：`docs/docs/architecture/domain-model-realignment.md`。
