# IoTSharp 执行路线图

本文档是 IoTSharp 当前阶段开始执行的路线图。它用于跟踪顺序、里程碑和完成情况，不再记录所有可能方向。

本路线图同时对齐产品矩阵各层的接口承诺：平台层事项在本仓库实现；边缘层、设备层与 AI 底座事项在对应仓库实现，但契约、验收和推进顺序以本路线图为准。

2026-09-05 定位与现场核验已更新，完整依据见 [设备监测与预测运维规划](docs/strategy/2026-09-device-health-strategy.md)。用户已确认优先发展设备状态监测、AI 与预测运维，以高速公路收费系统为首个验证现场；设备状态已有接入，当前工作从现网核验和业务语义补齐开始。

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

当前面向用户的产品定位是：**面向存量设备和分布式现场、支持内网部署的设备状态监测与预测运维平台。** 首个行业包服务收费站设备健康、故障定位和维护验证；平台核心保持行业中立。收费交易与车道控制继续由原业务系统负责。预测必须通过真实历史、故障标签和独立回测验收，不能把聊天能力或在线状态包装成预测维护。

## 产品矩阵与仓库分工

IoTSharp 是覆盖「平台 - 边缘 - 设备」三层加可选 AI 底座的开源产品矩阵，各组件按稳定契约协作：

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

差异化以现场效果验证：已有设备状态可复用，站点/车道业务上下文可追溯，数据过期与设备故障可区分，诊断与维护结果可闭环。SonnetDB 和 Tomur 提供可选的低组件数、内网交付路径，由轨道 T/A 验证安装、资源与恢复成本；不再宣称未经竞品和现场验证的“独有零外部依赖”。

## 里程碑

| 里程碑 | 状态 | 目标 | 完成标准 |
| --- | --- | --- | --- |
| M0 | `✅️` | 确认最终路线图 | 明确 Product、Device、Asset、Gateway、EdgeNode、Collection Template、RuleChain 和 Release Center 边界。 |
| M1 | `✅️` | 领域概念清理 | 旧 Produce 已直接迁移为 Product；DeviceModel、旧设备图完成删除或合并，旧 Scene 收口到后续 Asset View 建设；Gateway 旧配置迁移方案已明确。 |
| M2 | `✅️` | EdgeNode 正式模型 + 跨仓契约固化 | EdgeRuntimeStatus、EdgeCapability、EdgeTask、EdgeTaskReceipt、EdgeCollectionAssignment 落地；edge-node/collection-config/edge-task 契约进入 `IoTSharp.Contracts` 并版本化；semantic-core 收敛决策完成。 |
| M3 | `✅️` | Collection Template 产品化 | Product 下可维护协议、连接、点位、转换、采样和映射模板；模板生成的运行时配置已可被 IoTEdge 直接消费执行。 |
| M4 | `✅️` | 配置与最小软件包发布闭环 | 采集配置和最小软件包可发布到 EdgeNode/Gateway，形成下发、接收、执行、回执和历史；边缘侧数据断网缓存与续传落地。 |
| M5 | `✅️` | Release Center 第一版 | EdgeNode/Gateway 运行时软件发布、灰度批次、回滚、确认、回执和审计已具备平台第一版闭环；#056 设备脚本 OTA 与 #057 固件 OTA 已完成平台合同、Gateway 投递通道、执行回执验收和回滚闭环。 |
| M6 | `⬜` | RuleChain 增强 | O4 生产灰度与回滚闭环完成后启动；增强标准节点、版本化、仿真、观测、审计和背压，但不承担长周期发布。 |
| P | `✅️` | CoAP 接入 route 化（并行轨道） | MQTT 接入保持现状；CoAP 独立补 route 风格入口、低分配 payload 处理和平台侧业务分发。 |
| T | `🚧` | SonnetDB 单依赖交付与集成边界（并行轨道） | 已完成单依赖体验、容量基准和云边同底座参考部署；当前整改数据库内核能力误落平台层的问题。 |
| E | `⬜` | 附属运行时硬化（并行轨道） | IoTEdge 与 IoTEmbedded 达产：发布纪律、测试、资源占用、协议插件化、远程诊断、硬件矩阵。 |
| A | `⬜` | 离线 AI 底座（并行轨道） | Tomur 从孵化走向可依赖：发布纪律与测试；平台 AI Provider 抽象打通在线/离线切换；内网全功能参考部署。 |

## 当前主线：生产业务建模与实际启用

M1~M5 的 `✅️` 表示平台模型、接口和最小执行链路已经具备，不表示真实生产业务已经建模，也不表示真实 Gateway/EdgeNode、采集配置、灰度发布和回滚已经完成现场验收。当前主线从“继续开发平台功能”切换为“使用已有能力完成一个有证据的生产闭环”。

以下步骤严格串行；前一步没有真实数据和执行证据时，不得仅凭本地测试、模拟回执、接口可用或进程健康推进到下一步。

| 顺序 | 状态 | 生产启用事项 | 完成证据 |
| --- | --- | --- | --- |
| O1 | `🚧` | 整理 Product 能力模型和 Asset 归属 | 为选定业务范围建立经确认的 Product 能力定义、Asset 层级与责任归属；真实 Device 明确绑定 Product 和 Asset；模板能力与实例状态、业务层级不混用。 |
| O2 | `⬜` | 接入真实 Gateway/EdgeNode | 真实运行时完成注册、连续心跳、能力和版本上报；平台可定位其 Gateway、EdgeNode 和管理范围；任务接收与诊断使用真实回执闭环，不使用 mock 代替。 |
| O3 | `⬜` | 建立采集模板与配置版本 | 从已确认 Product 建立 Collection Template，生成不可变配置版本并分配到 O2 运行时；执行端回报接收和执行结果，平台的当前版本、目标版本及哈希收敛一致。 |
| O4 | `⬜` | 完成一次灰度发布和回滚 | 明确灰度范围、成功阈值、观察窗口、暂停条件和回滚基线；Release Center 对真实目标完成发布、确认和回滚；保留审批、任务、回执、审计及回滚后版本/健康证据。 |
| O5 | `⬜` | 增强 RuleChain 生产能力 | O4 闭环完成后再推进 trace、不可变发布版本、审计和背压；以真实规则流量验证路径回看、版本绑定、过载策略和失败可定位性。 |

O1 的第一批输入不是新页面或新 DTO，而是选定生产范围的业务清单：Product 能力项、Asset 树、Device 绑定、责任人、数据点语义和现有配置来源。若这些输入尚未确认，先完成盘点和评审，不在代码或数据库中臆造生产模型。

## 2026-09 设备监测专项

### 现网起点

2026-09-05 在用户提供的内网实例登录后只读核验：当前设备列表共 2,809 条，首屏 20 条中 15 条显示在线；Product 列表已有 1 个 LaneApp 产品；Asset 和 EdgeNode 列表均显示 0 条。首页却显示在线 0、Product 0。上述数量仅代表本次账号和页面范围，不能外推整个收费网络。

首页在线统计未查询真实在线数，列表使用 `connected`；抽样设备同时存在过时的 Active/Connected/最后活动信息，两台抽样的最新遥测为空。告警查询与规则事件查询分别因 ClearDateTime/RuleId 参数类型返回 400。因此先修复统计、数据新鲜度与查询合同，再判断哪些设备真的异常。现有 LaneApp 及状态接入继续复用，不重新创建一套并行接入。

### 最高优先级

下列事项是已有能力的正确性和交付修复，不是提前启动 M6 引擎扩展。

| 编号 | 状态 | 事项 | 责任与完成证据 |
| --- | --- | --- | --- |
| #100 | `⬜` | 恢复可复现 CI | 平台：使用实际 slnx/JsonDB 项目路径、递归子模块；干净 checkout 构建、合同测试、业务 profile 和前端构建有明确结果。 |
| #101 | `⬜` | MCP 租户隔离与只读工具边界 | 平台：修复 GetDeviceAttribute 丢失授权过滤；按稳定 ID 查询，补跨租户、同名、无对象和分页测试；新工具统一权限及审计。 |
| #102 | `⬜` | 固定云边交付矩阵 | 平台/IoTEdge：处理本机领先远端 4 个提交的可交付性，版本化消费 Contracts；记录 commit、package、digest、合同和现场证据，核验边缘 SonnetDB 1.2.0 消费路径。 |
| #103 | `⬜` | SonnetDB 实际使用路径回归 | 平台/SonnetDB：验证现用 EF、时序、MQ 和备份恢复；明确跨表/跨模型恢复边界，保留已有部署回退方案。 |
| #104 | `⬜` | 统一在线统计与新鲜度 | 平台/前端：修复首页统计与列表矛盾；区分连接、活跃、设备功能、未知和维护状态；缓存回退明确标识，不能把未知显示为 0 或正常。 |
| #105 | `⬜` | 修复现网查询合同 | 平台/前端：修复告警 ClearDateTime、规则事件 RuleId、空列表 pageSize=0；验证空筛选、设备详情与独立页面，避免请求失败显示为无告警。 |
| #106 | `⬜` | 设备属性中的凭据隔离 | 平台/上报端/前端：连接配置引用受管凭据；普通属性、导出、日志与 AI 上下文不返回明文连接凭据；核验既有暴露范围并制定轮换方案。 |

### 监测与预测交付

| 编号 | 状态 | 对应门槛 | 事项与完成证据 |
| --- | --- | --- | --- |
| #110 | `🚧` | O1 | 核验现有 LaneApp Product、Device、状态和业务元数据，将路段/站/车道归属映射到 Asset 草稿；确认设备与外设身份、字段含义、责任人和维护口径。当前完成的是调查与规划，真实映射待评审。 |
| #111 | `⬜` | O1 -> O3 | 基于 semantic-core 建立健康观测的时间、质量、过期、来源及版本口径；保留旧上报兼容；确认周期状态真正进入可查询历史。 |
| #112 | `⬜` | O1/O2 | 设备健康与诊断工作台：Asset 范围、异常/未知设备、统一时间线、影响范围、稳定链接和上下文连续；复用 Vue/Element Plus 及现有主题。 |
| #120 | `⬜` | O2 | 真实观测运行时注册、心跳、能力、版本与诊断；已有 Device 状态上报不代替 EdgeNode 受管生命周期证明。 |
| #121 | `⬜` | O2 | IoTEdge 有界缓存、磁盘预算、去重、重连限流、断网配置来源和故障恢复；用真实运行时日志及回执验收。 |
| #130 | `⬜` | O3 | 从现有状态映射建立批准的 Collection Template，发布不可变配置并验证当前/目标版本与哈希收敛。 |
| #140 | `⬜` | O4 | Release Center 完整前端：包、目标展开、兼容预检、审批、批次、观察、暂停及回滚；以已有 API 为基础补工作流。 |
| #141 | `⬜` | O4 | IoTEdge 实际进程/服务/容器切换与失败恢复；区分下载、暂存、指针、进程运行和采集恢复证据；未支持的脚本/固件任务按 capability 拒绝。 |
| #150 | `⬜` | O5/M6 | O4 通过后按现有 #060~#066 增强实时规则；先做 trace、不可变版本、失败定位和有界执行，不另造长期工作流。 |
| #151 | `⬜` | O5 后 | 告警关联为故障事件、共因及维护结果；优先对接现有工单，记录处置与维修标签。 |
| #160 | `⬜` | #101/#106/#111 | 只读 AI 诊断：限定设备/时间/权限，检索手册及状态证据，输出引用、审计与不确定性；在线/离线 Provider 分别验证。 |
| #161 | `⬜` | #111/O4 | 可解释健康政策与正常工况基线；健康分数给出数据覆盖和政策版本，无数据保持未知。 |
| #162 | `⬜` | #151/#161 | 异常检测影子运行：时间/站点隔离验证、事件级误报/漏报及处理成本；不能等同于预测故障。 |
| #163 | `⬜` | #162 + 标签门槛 | 明确故障类型和未来窗口的风险预测；足够故障/维修样本、无泄漏、超越简单基线并有可操作提前量才发布。 |
| #170 | `⬜` | O4 + 首站监测验收 | 第二站点复制：模板复用、部署人时、资源、维护效果与独立数据验证。 |
| #171 | `⏸️` | #170 + 商业验证 | 向相邻分布式设施监测扩展；条件维护、必要 HA/灾备、UNS/Sparkplug 等按真实需求进入。 |

### 建议排期与控制点

1. 第 1-2 周：#100~#106 与 O1 现网核验、状态/归属映射草稿。
2. 第 2-4 周：#111/#112 与 O2 真实运行时、诊断和缓存可靠性。
3. 第 4-6 周：O3 配置收敛与 O4 对观测运行时的 canary/回滚。
4. 第 6-9 周，O4 后：O5 最小规则增强、故障事件和维护结果闭环。
5. 第 8-12 周，数据门槛具备后：只读诊断、健康基线与异常检测影子运行；3-6 个月评估有标签预测及第二站复制。

以上是具备人员、数据和现场窗口时的估算，不是交付承诺。任何后续阶段均不得因为排期到期而跳过前置门槛。现有状态和规则可持续用于观察；M6 引擎增强仍严格等待 O4。人员不足时集中一条主线，不同时扩展数据库、MCU、桌面端和多模态底座。

验收指标：数据新鲜度/完整率、设备日误报、事件级漏报、诊断时长、维护后恢复、配置/实际程序版本收敛、回滚结果和新站部署成本。阈值由现场基线与负责人共同确定，不使用数据库内核 benchmark 或合成测试作为平台 SLA。

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
| #00A | `✅️` | Tomur 独立运行时基线 | 2026-09-05 公开文档已有 managed/native 双路径、模型资产、兼容 API、测试与 Agent 接线；最新包/模型/硬件组合仍须验证。平台 Provider 集成和收费诊断效果尚未验收，不再称“无测试”。 |
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
| #045 | `✅️` | 失败重试和审计 | 平台 | 已新增 EdgeTask 失败重试 API、审计查询 API、终态回执审计和任务时间线审计事件；配置发布失败后可创建新 TaskId 重试同一配置版本，并保留原失败任务历史。 |
| #046 | `✅️` | 最小软件包发布 | 平台 | 已新增 `ReleasePackage` 最小模型、`ReleasePackages` 多 provider 迁移、上传/下载/列表/详情/发布 API、`release-package-v1` 合同和 `SoftwareUpdate` EdgeTask；成功回执需核对包 ID、版本和 SHA256，不含灰度、批次和审批（留 M5）。 |
| #047 | `✅️` | IoTEdge 软件更新执行器 | 边缘 | IoTEdge 已接收 #046 `SoftwareUpdate` 任务，完成下载、SHA256 校验、package store 暂存、本地版本指针切换、旧指针回滚和进度回执；平台继续按包 ID、版本和哈希校验成功回执。 |
| #048 | `✅️` | 边缘数据断网缓存与续传 | 边缘 | IoTEdge 已为 `UploadChannel.BufferingEnabled` 接入本地上传缓存和后台续传；平台不可达或上传目标异常时先落本地队列，恢复后按通道快照批量续传。 |

### M5 - Release Center 第一版

M5 排在 Edge 和采集闭环之后。它不进入 RuleChain。#050 在 #046 最小模型上兼容扩展，不重建。第一版已闭环 EdgeNode/Gateway 运行时软件发布；#056 已完成平台侧 `DeviceScriptOta`、单 Device 目标、AssetScope/DeviceScope 范围展开、Gateway 投递通道和脚本 CRC 成功回执校验；#057 已完成 `FirmwareOta` 合同、设备范围发布、bootloader 验收、回滚准备和回滚确认回执校验。

| 顺序 | 状态 | 事项 | 层级 | 交付 |
| --- | --- | --- | --- | --- |
| #050 | `✅️` | ReleasePackage | 平台 | 已兼容扩展 #046：`Software` 枚举值保留，追加 `CollectorSoftware`、`Configuration`、`DeviceScript`、`Firmware`；第一版执行支持 EdgeNode/Gateway 运行时软件包。 |
| #051 | `✅️` | ReleasePlan | 平台 | 已新增发布计划 API，支持发布范围、批次大小、灰度批次、自动开始和人工确认策略。 |
| #052 | `✅️` | ReleaseTask | 平台 | 已新增发布任务模型并绑定 EdgeTask；支持 EdgeNode/Gateway 运行时目标，并可将 Device、AssetScope、DeviceScope 展开为面向单设备的设备 OTA 发布任务。 |
| #053 | `✅️` | ReleaseReceipt | 平台 | 已将 EdgeTask Accepted/Running/Succeeded/Failed/TimedOut 等回执投影为 ReleaseReceipt，并同步 ReleaseTask/ReleasePlan 当前态。 |
| #054 | `✅️` | 回滚和暂停继续 | 平台 | 已提供 Start、Confirm、Pause、Resume、Rollback 端点；回滚使用运行时软件包创建回滚 ReleaseTask/EdgeTask。 |
| #055 | `✅️` | 发布审计 | 平台 | 创建、启动、确认、暂停、继续和回滚操作写入审计记录；端到端测试覆盖发布计划、任务、回执和审计落库。 |
| #056 | `✅️` | 设备脚本 OTA | 平台 | 已追加 `DeviceScriptOta` EdgeTask、`Device` 目标、脚本包元数据透传、AssetScope/DeviceScope 单设备展开、Gateway 投递通道和成功回执包/目标/`scriptCrc32` 校验；集成测试覆盖 AssetScope 与 DeviceScope 经 Gateway 下发到子设备的脚本 OTA 闭环。 |
| #057 | `✅️` | 设备固件 OTA | 平台/设备 | 已追加 `FirmwareOta` EdgeTask、固件包元数据透传、设备范围发布和包/设备回执校验；成功回执必须证明 bootloader 已验收、分区已切换，并按普通升级/回滚任务分别确认 `rollbackReady` 或 `rollbackConfirmed`。 |

### M6 - RuleChain 增强

M6 仅在生产启用 O4 完成后启动，目标是增强实时规则，而不是把规则链改成工作流引擎。IoTSharp 的方向不是复制通用 Node-RED，也不是完整复刻 ThingsBoard，而是形成面向 Product、Device、Asset、Gateway、EdgeNode 和 Collection Template 的实时规则图：借鉴 ThingsBoard 的消息语义、关系路由和可靠执行，借鉴 Node-RED 的节点体验、调试体验和基础节点丰富度。

M6 的第一原则是先把规则链运行时的“命名、消息、关系、节点、版本、观测”产品化，再评估 AI 节点和 MCP 写扩展。脚本节点继续保留，但标准节点应覆盖大多数常见场景，避免用户把清洗、路由、富化、告警和发布全部写成脚本。

M6 基础设计要求：

- 先做命名治理：新 API、DTO、UI、文档和节点库统一使用 RuleChain 语言；旧 `FlowRule`、`Flow`、`NodeProcess*`、`Mata/MataData`、`Excutor` 等命名只作为兼容层或迁移来源，不再扩展为新的公开概念。
- 定义 `RuleMessage v1`：包含 `messageId`、`traceId`、`tenantId`、`originatorType`、`originatorId`、`productId`、`assetId`、`gatewayId`、`edgeNodeId`、`messageType`、`payload` 和 `metadata`，并兼容现有 FlowRule 动态数据输入。
- 定义 `RuleRelation v1`：保留连线表达式，同时支持 `Success`、`Failure`、`True`、`False`、`Matched`、`Unmatched`、`Timeout`、`Error` 和 `Custom` 等关系，便于失败兜底、超时处理和未匹配分支。
- 定义节点 manifest：描述节点类型、分类、显示名、输入输出、配置 JSON Schema、默认值、权限需求、超时、重试、副作用和帮助文档。
- 运行器增强应优先服务实时链路：最大深度、最大耗时、取消、失败关系、超时关系、背压、指标和错误定位；不引入长周期审批、灰度、回滚或发布编排。

M6 脚本执行器定位：

- 脚本节点继续保留，但 M6 不继续用“多语言脚本堆叠”替代标准节点；清洗、路由、富化、告警、发布和观测等高频场景优先沉淀为标准节点。
- 平台核心需要抽象 `IRuleScriptEngine`/`IRuleNodeExecutor` 和脚本 manifest，通过注册表发现能力，避免 `FlowRuleProcessor` 继续按语言硬编码分支。
- JavaScript/Jint 可作为默认轻量脚本能力；SQL/JSON 查询更应产品化为查询、投影、过滤类标准节点；Python、C#、Lua、AI/MCP、高级集成节点可按插件或商业插件形态扩展。
- BASIC 脚本必须纳入执行端兼容考虑：优先复用 IoTEdge 执行端脚本能力，并与 `DeviceScriptOta`、采集转换、仿真和回执合同对齐；平台 RuleChain 不重复维护一套脱离执行端行为的 BASIC 语义。
- C 脚本保留价值：现有实现基于 C# 侧 C 语言解释器，适合承接嵌入式、网关和工业公式迁移场景；M6 应将其作为受控脚本引擎纳入设计，但必须明确超时、内存、输出大小、禁止系统副作用和审计要求。
- 所有脚本输入统一使用 `RuleMessage v1`，并额外注入 `config`、`node`、只读 `context`、受控 `services` 和密钥引用；脚本输出统一返回 `relation`、`payload`、`metadata`、`actions` 和 `diagnostics`，禁止依赖隐式全局状态。
- 脚本必须满足租户隔离、权限声明、能力声明、输入输出 schema、仿真输入、traceId、节点耗时、错误堆栈和审计记录要求；高风险动作只能通过受控动作节点或 MCP 工具执行，不允许脚本静默绕过确认链路。

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
| #061 | `🕒` | 标准节点库、节点 manifest 与脚本执行器抽象 | 减少脚本依赖；第一批覆盖过滤/路由、转换、上下文富化、发布/动作、外部集成、观测调试和停止节点；每类节点提供配置 Schema、输入输出约定和帮助说明；脚本执行器通过 manifest/registry 注册，BASIC 对齐 IoTEdge 执行端能力，C 脚本作为受控解释器能力保留评估。 |
| #062 | `🕒` | 规则版本和发布 | 支持草稿、发布版本、复制、回滚和不可变发布快照；规则绑定应指向明确版本，避免编辑中规则影响生产流量。 |
| #063 | `🕒` | 仿真、观测和审计 | 保存仿真输入、输出、命中 relation、节点耗时、运行路径、错误堆栈、动作副作用和审计记录，支持按 traceId 回看一次规则执行。 |
| #064 | `🕒` | 规则链 AI 节点 | LLM 调用节点：提示词模板、结构化输出，复用 #090 的 Provider 抽象（在线/离线均可），全程审计。 |
| #065 | `🕒` | MCP 工具扩写与受控动作 | 从只读设备查询扩展到受权限、审计、租户隔离和人工确认约束的写操作；高风险动作不得由规则链静默触发。 |
| #066 | `🕒` | 派生计算与运行器增强评估 | 评估遥测派生字段/计算属性、内部 work queue、重试、超时、背压和失败分支；只服务实时链路，不演化为长周期工作流引擎。 |

### 并行轨道 P - CoAP 接入 route 化

P 轨道目标是改善 CoAP 在 IoTSharp 平台内的命名、组织和性能。MQTT 接入当前已经具备 controller routing、topic 组织、payload 处理和现有业务入口，保持现状，不进入本轨道改造范围。CoAP 的 method、Uri-Path、Content-Format、Accept、response code、Blockwise、Observe、DTLS、Resource、resource tree 和 resource discovery 继续保留协议原貌。

命名约束：CoAP.NET 协议栈尊重 RFC 语义，继续使用 `Resource`、`IResource`、`ResourceAttributes`、`DiscoveryResource` 等标准术语，不做个人化改名。IoTSharp 平台接入层和对外 API、DTO、文档、控制器不继续暴露裸 `Resource` 作为业务概念，统一使用 route / endpoint / handler。旧 CoAP 短路径不再作为兼容目标；新实现只注册推荐 route 风格入口。

| 顺序 | 状态 | 事项 | 仓库/组件 | 交付 |
| --- | --- | --- | --- | --- |
| #096 | `✅` | CoAP route 命名与路径约定 | 平台 | 已定义推荐 CoAP path、命名规范、旧短路径移除策略和错误响应映射；`docs/docs/integrations/coap.md` 与 `CoapPlatformRouteConventions` 固化平台约定；明确 MQTT routing 保持现状。 |
| #097 | `✅` | CoAP route adapter | CoAP.NET / 平台消费侧 | CoAP.NET 新增 `CoAP.Server.Routing` 通用 route adapter，负责 Resource tree 到 `CoapRouteContext` 的协议映射；IoTSharp 只消费 route adapter，不在业务层实现 CoAP.NET 应具备的动态 Resource 能力。 |
| #098 | `✅` | CoAP 业务分发服务 | 平台 | 已新增 CoAP 业务分发入口，把推荐 route 命中后的上下文映射为设备、网关、遥测、属性和告警事件；认证、目标校验和 Product 能力解释复用现有平台服务。RPC 等待单独定义推荐 CoAP route 模板后接入。 |
| #099 | `✅` | CoAP 旧短路径移除 | 平台 | 不再注册旧 `Telemetry`、`Attributes`、`Alarm` 短路径；推荐 path 固定为 `devices/{device}/telemetry`、`devices/{device}/attributes`、`devices/{device}/alarm`、`gateways/{gateway}/telemetry` 和 `gateways/{gateway}/attributes`。 |
| #09A | `✅` | CoAP 性能与压测 | 平台 | 已新增 `tools/IoTSharp.Benchmarks`，覆盖 CoAP path 匹配、route endpoint 构建、UTF-8 payload 解析和告警 source generation 基准；提供真实 UDP 压测 runner，支持并发、超时、payload 和 block-size 参数；业务热路径改为 `ReadOnlyMemory<byte>` / `JsonDocument` 解析，payload 字典因会逃逸到事件总线不做对象池复用；单测覆盖大 payload Block1 元数据、Observe 关系和 DTLS PSK 压测配置。 |
| #09B | `✅` | 文档收口 | 平台 | 已输出平台 CoAP 接入说明、推荐路径与旧短路径差异表、组件消费、版本回滚、运维配置说明和错误映射；明确旧短路径不兼容、MQTT 不属于本轨道改造范围；平台接入与消费说明维护在 IoTSharp 文档中，CoAP.NET 组件文档只保留协议栈与框架说明。 |
| #09C | `✅` | CoAP 运维与安全收口 | 平台 | 已完成 CoAP 启停和监听边界：`CoapServer` 支持 `Enabled`、绑定地址和端口配置；明确 UDP `coap://` 与 PSK `coaps://` 的启用条件，默认部署不再把 `5684/udp` 描述或发布成可用端口；无效 token 与缺失 token 统一按 `4.01 Unauthorized` 处理，避免 token 枚举；`CoapLogging.LoggerFactory` 已接入宿主日志；启动、端口冲突、认证失败和安全映射回归测试通过。 |
| #09D | `✅` | CoAP 依赖与版本收口 | 平台 / CoAP.NET | 已移除根 `Directory.Packages.props` 中未消费的旧 `IoTSharp.CoAP.NET` 2.0.8 声明；平台源码继续通过本地 `ProjectReference` 消费 3.0.0 fork；文档明确 NuGet 3.0.0 包、外部消费方式和显式回滚路径，避免协议栈版本漂移。 |

### 并行轨道 T - SonnetDB 单依赖交付与集成边界

按监测试点需要并行。目标是验证低组件数、内网交付的实际安装与恢复成本。SonnetDB 负责数据库引擎、查询执行、备份恢复和维护能力，IoTSharp 只负责平台数据映射、稳定接口适配、权限、审计与受控运维编排；不把“全栈零外部依赖”作为未经证实的首要卖点。

| 顺序 | 状态 | 事项 | 交付 |
| --- | --- | --- | --- |
| #070 | `✅` | 单依赖体验路径首推 | README 和文档已把 SonnetDB Profile（`docker-compose.sonnetdb.yml`，并保留发布包/单文件二进制同 Profile 语义）定为 5 分钟快速体验的默认入口。 |
| #071 | `✅` | 容量与可靠性基准报告 | 已新增 SonnetDB 容量与可靠性基准报告，串联遥测吞吐、bytes/value 容量口径、备份恢复、故障回放和 `/metrics` 最小观测指标；IoTSharp storage tests 已加入容量 smoke。 |
| #072 | `✅` | 云边同底座参考部署 | 已新增 `Deployments/cloud_edge_sonnetdb` 云端/边缘 compose 与环境变量模板，并在文档中说明 IoTEdge 直连本地 SonnetDB 的参考架构、云边合同边界、验收和回滚。 |
| #073 | `🚧` | 备份恢复边界整改 | 已删除 IoTSharp 对 SonnetDB 数据目录、私有 manifest、segment、measurement schema 和魔数的重复实现，嵌入式模式已委托 SonnetDB `BackupService` 并覆盖 checkpoint、校验、dry-run、恢复和全文索引选项；下一步在 SonnetDB 提供稳定远程入口后接入 maintenance API，IoTSharp 只保留权限、审计、任务编排和结果映射。 |
| #074 | `⬜` | latest 查询下推整改 | `GetTelemetryLatest` 使用 SonnetDB 的 `ORDER BY time DESC LIMIT 1` 或等价稳定 API，不在 IoTSharp 全量扫描历史数据；覆盖稀疏字段、乱序写入、flush 前后和远程 provider 回归。 |
| #075 | `⬜` | 时间桶聚合下推整改 | 用 SonnetDB `GROUP BY time(...)` 和多字段聚合替代“时间桶 × 字段”的逐查询循环；明确不支持聚合的兼容策略，并以查询次数、结果语义和大时间范围测试验收。 |
| #076 | `⬜` | 故障回放语义整改 | 停止把普通范围查询命名为故障回放。数据库 WAL/segment 恢复归 SonnetDB；业务遥测重放若保留，则进入 IoTSharp 独立运维任务，具备租户权限、审计、幂等、人工确认、进度和失败闭环，不隐藏在存储适配器中。 |

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

Tomur 是可选离线 AI Provider。2026-09-05 公开文档已有测试、真实模型证据及 Agent 接线，同时保留兼容 API、最新升级和硬件矩阵的验证边界。此轨道聚焦平台真实消费和收费诊断效果，不重复开发其已有底座。AI 只能通过平台权限、审计、租户隔离和 MCP 边界使用能力，Tomur 不是权限旁路，也不自动构成时序预测服务。

| 顺序 | 状态 | 事项 | 仓库 | 交付 |
| --- | --- | --- | --- | --- |
| #090 | `⬜` | 平台 AI Provider 抽象 | 平台 | AISettings 扩展为 Provider 配置（在线云端模型 / 离线 Tomur 端点可切换）；平台侧统一走 OpenAI 兼容客户端，不感知 Provider 差异；连接测试与健康检查。 |
| #091 | `⬜` | Tomur 发布纪律 | Tomur | License、版本号、Release/tag、变更记录；smoke 记录转为可重复的自动化测试基线。 |
| #092 | `🚧` | Tomur 消费侧验证 | Tomur/平台 | 复用已有测试，针对实际选定模型、版本、硬件验证 chat/embeddings、工具调用、超时、释放与 API 兼容；不把已有单仓测试视为平台集成完成。 |
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
| M5 | #050~#057 | 灰度/回滚执行端配合 | #056 设备侧执行器、#057 固件 OTA 按 IoTEmbedded 仓库验收 | — |
| M6 | #060~#066 | — | — | #064 经 #090 消费离线推理 |
| P | #096~#09D | 后续按 CoAP route 入口消费 | — | — |
| T | #070~#076 | — | — | — |
| E | — | #080~#084 | #085~#087 | — |
| A | #090、#093 | #095 边缘 AI 试点 | — | #091、#092、#094 |

## 删除和停止扩展

| 顺序 | 状态 | 事项 | 处理 |
| --- | --- | --- | --- |
| #900 | `🗑️` | Produce 作为旧领域名 | 停止扩展；统一改为 Product。 |
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

当前执行窗口是生产启用 O1：先完成选定业务范围的 Product 能力模型、Asset 归属和 Device 绑定。O1 评审通过后，才依次进入真实 Gateway/EdgeNode 接入（O2）、采集模板与配置版本启用（O3）、Release Center 灰度发布与回滚实操（O4），最后进入 RuleChain trace、版本、审计和背压增强（O5）。

2026-09-05 起，O1 具体对应已有 LaneApp 收费现场数据。立即处理 #100~#106 已发现的统计、查询、隔离和交付问题，并推进 #110 的现有对象/状态核验；Asset 和 EdgeNode 的真实启用仍待证据。以“设备健康、诊断、维护结果、逐步预测”为用户价值主线，前端按 [监测工作流规划](docs/design/iotsharp-ui-redesign-roadmap.md) 实施。

M1（#010~#018）、M2（#020~#029）、M3（#030~#038）、M4（#040~#048）和 M5（#050~#057）已经形成平台代码基线。M4 已具备采集配置版本快照、模板发布到 EdgeTask、目标配置获取、执行回执、版本差异、失败重试和审计；M5 已具备发布计划、灰度、回滚、暂停继续、确认策略以及设备脚本/固件 OTA 平台合同。这些能力是 O1~O4 的工具和前提，不替代真实生产数据、执行端回执、观察窗口或回滚后的业务验证。

并行轨道 P（CoAP 接入 route 化）已完成 #096~#09D，E（附属运行时硬化）与 A（离线 AI 底座）按带宽穿插推进，不阻塞主线；其中 P 轨道只处理 CoAP 接入命名、route 风格入口和性能优化，MQTT 接入保持现状；#090（AI Provider 抽象）是 M6 #064/#065 与 #093 内网部署的前置。

#072 已在本仓库沉淀参考部署与平台侧验收文档；P 轨道已以 #096~#09D 验收 CoAP route 风格入口、性能优化、运维安全、依赖版本和文档收口；其他涉及 IoTEdge（#028、#038、#047、#048、#080~#084、#095）、IoTEmbedded（#056、#057、#085~#087）和 Tomur（#091、#092、#094）的事项在各自仓库实现，验收标准和契约版本以本路线图与 `IoTSharp.Contracts` 为准。

详细领域定义和分步改造见：`docs/docs/architecture/domain-model-realignment.md`。
