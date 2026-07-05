---
title: 领域概念收口与改造路线
---

# 领域概念收口与改造路线

本文档是 `ROADMAP.md` 的落地细则，目标是把 IoTSharp 从设备平台收束为工业采集与运营平台。当前阶段不追求大而全，而是先把概念、模型、页面和契约理顺。

阶段编号与 `ROADMAP.md` 的里程碑（M1~M6）和事项编号（#0xx）一一对应，本文不再维护独立的阶段体系。

## 核心原则

- 不做全量重写。
- 新代码、新 DTO、新 UI、新文档和新 API 使用 Product，不再新增 Produce 命名。
- Product 是模板，Device 是实例，Asset 是业务对象，Gateway 是南向协议和子设备接入运行时，EdgeNode 是受管边缘运行时，Collection Template 是平台侧采集行为定义。
- 平台是概念和契约的定义方；IoTEdge、IoTEmbedded、Tomur 是概念的执行端产品，不是新概念。
- 跨仓契约以 `IoTSharp.Contracts` 为单一事实来源并版本化；执行端仓库是契约消费者，不反向定义契约。
- 实时规则链只处理实时数据、告警和短动作，不承载 OTA、灰度、回滚、审批等长周期运营任务。
- AI 是横切能力，只能通过授权领域服务、技能和 MCP 工具使用平台能力；离线或内网时由 Tomur 提供模型算力，但权限、审计和租户隔离仍在平台侧完成。
- 旧概念如果与新领域模型重叠，优先删除、合并或标记废弃，不再为了概念名保留兼容路线。

## 概念与产品的对应关系

领域概念（平台内的模型）和产品矩阵（独立交付的仓库）是两个维度，不能混用：

| 领域概念 | 定义方 | 执行端产品 | 关系说明 |
| --- | --- | --- | --- |
| Device | 平台 | 任意固件/SDK，参考实现为 IoTEmbedded | Device 是平台模型；IoTEmbedded 是一种以 Device 身份接入的固件级运行时产品 |
| Gateway | 平台 | IoTEdge（承担协议接入时） | Gateway 是特殊 Device；IoTEdge 部署后通常同时承担 Gateway 职责 |
| EdgeNode | 平台 | IoTEdge | EdgeNode 是平台侧受管运行时模型；IoTEdge 是实现 EdgeNode 契约的第一个（参考）执行端 |
| Collection Template | 平台 | IoTEdge 消费其生成的运行时配置 | 模板保存在平台；运行时配置经发布闭环下发到执行端 |
| AI Provider | 平台 | 云端模型服务或 Tomur | 平台通过统一 Provider 抽象调用模型；Tomur 是离线/内网场景的 Provider 实现 |

一句话判据：**概念进平台模型和 `IoTSharp.Contracts`，实现进对应产品仓库**。在平台仓库里出现采集驱动、固件逻辑或模型推理代码，都是越界。

## 核心概念

### Product

Product 是一类设备或采集对象的能力模板。

Product 应负责：

- 设备类型和能力定义。
- Telemetry、Attribute、Event、Command 和 Configuration 定义。
- 协议模板、连接模板、点位模板、转换链和采样策略。
- 默认凭据策略、默认超时、默认规则链和默认告警模板。
- 软件、固件、采集器版本和兼容矩阵。
- 批量创建设备和设备注册策略。

Product 不负责：

- 单台设备在线状态。
- 遥测最新值和历史遥测。
- 告警实例。
- 命令执行记录。
- 站点、产线、车间等业务层级。

旧 `Produce` 概念已直接迁移为 Product。`ProduceToken` 已改为 `ProductToken`。

### Device

Device 是真实接入 IoTSharp 的运行时实例。

Device 应负责：

- 身份、凭据、注册、启用、停用和删除。
- 在线状态、活跃状态、心跳、连接时间和最后活动时间。
- 遥测、属性、事件、告警和命令执行记录。
- 绑定 Product 以解释能力模板。
- 绑定 Asset 以进入业务上下文。
- 绑定 Gateway 或 EdgeNode 以描述采集路径。

Device 不负责：

- 定义型号能力。
- 定义点位模板。
- 表达业务层级树。
- 作为发布范围的唯一抽象。

设备侧运行时的参考实现是 IoTEmbedded：它以设备 MQTT 协议族（注册、心跳、命令响应、升级响应）接入，该协议族对齐平台 Device 契约并文档化（#086），作为第三方固件接入的参考。

### Asset

Asset 是用户运营世界中的业务对象。

Asset 应负责：

- 站点、园区、建筑、楼层、房间、车间、产线、工位、设施和设备系统。
- 业务归属、空间层级、运维责任、看板上下文和权限范围。
- 关联一个或多个 Device，描述这些设备观测或控制的业务对象。
- 按业务范围查看告警、诊断、配置和发布影响面。

Asset 不负责：

- 设备型号能力。
- 设备接入身份。
- 采集运行时状态。

### Gateway

Gateway 是承担南向协议接入和子设备代理的特殊 Device。

Gateway 应负责：

- 协议适配。
- 子设备发现、创建、连接状态上报。
- 子设备遥测、属性、事件和告警上传。
- 接收采集配置、软件任务和诊断任务。
- 报告运行能力、任务回执、错误和诊断信息。

Gateway 不负责：

- 保存平台侧 Product 模板。
- 维护业务资产层级。
- 绕过 IoTSharp 权限和审计。
- 依赖 IoTSharp 内部数据库结构作为跨项目契约。

### EdgeNode

EdgeNode 是受平台管理的边缘运行时实例。

EdgeNode 应负责：

- 注册、心跳、版本、实例标识、主机信息和健康状态。
- 能力上报、运行指标、日志摘要和诊断信息。
- 接收配置任务、软件任务、固件任务和诊断任务。
- 报告任务接收、执行、完成、失败、超时、取消和回滚结果。

Gateway 与 EdgeNode 可以部署在一起，但概念不同：

- Gateway 偏协议和子设备接入。
- EdgeNode 偏受管运行时生命周期和任务闭环。

EdgeNode 的第一个执行端是 IoTEdge：注册、心跳、能力上报、CollectionConfig 拉取和 EdgeTask 回执链路已经打通（基线 #008）。M2 的契约固化（#027）以它为第一验证对象，避免纸面契约；其任务回执组件在 #028 转正。

### Collection Template

Collection Template 是平台侧保存的采集行为定义。

它包含：

- ProtocolTemplate：协议类型和协议参数。
- ConnectionTemplate：连接地址、端口、串口、认证和超时。
- PointTemplate：点位地址、数据类型、长度、读写属性。
- TransformTemplate：缩放、偏移、表达式、枚举、位解析、字节序等转换链。
- SamplingPolicy：周期、变化上报、死区、质量变化和聚合提示。
- MappingPolicy：映射到 Telemetry、Attribute、AlarmInput 或 CommandFeedback。

Collection Template 属于 Product 或模板库。运行时任务由平台生成并发布到 EdgeNode、Gateway 或选定 Device。

模板的持久化模型基础须先经过 semantic-core 收敛决策（#029）：`IoTSharp.Contracts` 中已有 `semantic-core.v1.schema.json` 及 Modbus/OPC UA/MQTT 协议绑定，IoTEdge 侧也有本地采集配置模型。三者必须收敛为一套点位与协议描述，要么以 semantic-core 为基础建模，要么明确废弃并记录原因。该决策是 M3 开工（#030）的前置门。

### RuleChain

RuleChain 是实时数据处理引擎。

RuleChain 应负责：

- 入站数据转换。
- 数据路由。
- 上下文富化。
- 告警触发。
- 遥测、属性和事件发布。
- 短动作分发。

RuleChain 不负责：

- OTA。
- 软件包发布。
- 配置版本发布。
- 审批。
- 灰度。
- 回滚。
- 大批量运维编排。

后续增强（M6）可引入 AI 节点（#064），该节点经由平台统一的 AI Provider 抽象（#090）调用模型，在线云端模型与离线 Tomur 端点可切换，全程审计。

### AI 与 Tomur 的边界

AI 是横切能力，不是独立领域。概念上只有一条规则：**AI 通过平台控制平面使用平台能力**——权限、审计、租户隔离和 MCP 工具边界缺一不可。

Tomur 在这个体系里的角色是模型算力 Provider：

- 平台侧只依赖 OpenAI 兼容 API（#090 的 Provider 抽象），不感知在线模型与 Tomur 的差异。
- 离线或内网部署时，AISettings 指向本地 Tomur 端点，平台 AI 功能照常工作（#093 内网全功能参考部署）。
- Tomur 不承接多租户隔离、不直接访问平台数据库、不绕过审计——它不是权限旁路。

## 契约治理

跨仓协同依赖三个版本化契约，全部以 `IoTSharp.Contracts` 为单一事实来源（#027）：

| 契约 | 内容 | 消费方 |
| --- | --- | --- |
| `edge-node-v1` | 注册、心跳、能力上报 | IoTEdge |
| `collection-config-v1` | 采集运行时配置（点位、连接、采样、映射） | IoTEdge |
| `edge-task-v1` | 任务下发、接受、回执状态机 | IoTEdge |

规则：

- 契约变更走版本号，不做隐式破坏性修改；平台与执行端维护兼容矩阵（#080）。
- 执行端不得依赖 IoTSharp 内部数据库结构或未进契约的 DTO 字段。
- 设备侧 MQTT 协议族同样对齐文档化契约（#086），使 IoTEmbedded 之外的第三方固件可按文档接入。

## 旧概念处理

### 直接改名或迁移

- `Product` 改为 Product。
- `ProductToken` 改为 `ProductToken`。
- 前端 `Product` 目录和 API 模块改为 `product`。
- 文档、菜单、按钮、DTO 和新 API 统一使用 Product。

范围控制：存量 `Product` API 冻结为兼容实现细节，只保证新增面统一使用 Product，不做存量全量重命名（详见 ROADMAP M1 说明）。

### 合并

- `DeviceModel` 合并到 Product 的能力、命令和配置定义。
- Product 上的旧 Gateway 配置合并到 Collection Template。
- Scene 如果只是规则链或资产视图包装，合并到 RuleChain View 或 Asset View。

### 废弃

- 旧 `DeviceGraph`、`DeviceDiagram`、`DeviceGraphToolBox` 如果只是历史设计器数据，标记废弃。
- 把 Device 当业务层级、模板容器或发布范围的用法停止扩展。
- EdgeTask 状态以 AttributeLatest/TelemetryData 作为主存储是临时方案，M2 #025 完成后停止使用（#906）。

## UI 信息架构

### Product 工作台

Product 页面围绕模板建模组织：

- 基础信息。
- 能力定义。
- 遥测、属性、事件、命令和配置项。
- 协议、连接、点位、转换和采样策略。
- 默认凭据策略和注册策略。
- 默认规则链和告警模板。
- 软件、固件和采集器兼容矩阵。
- 批量创建设备和导入设备。

### Device 工作台

Device 页面围绕实例运维组织：

- 概览和运行状态。
- 凭据与接入指南。
- 所属 Product、Asset、Gateway 和 EdgeNode。
- 属性。
- 最新遥测、历史遥测和实时订阅。
- 命令、RPC 和任务记录。
- 告警。
- 诊断。

### Asset 工作台

Asset 页面围绕业务对象组织：

- 资产树和空间层级。
- 关联设备。
- 业务看板。
- 告警影响面。
- 运维责任和权限范围。
- 按业务范围查看配置、诊断和发布影响。

### Edge 工作台

Edge 页面围绕运行时闭环组织：

- EdgeNode 列表、状态、健康、版本和能力。
- 注册和接入信息。
- 心跳和运行指标。
- Gateway 和 Device 关联。
- Collection Assignment。
- 任务下发、回执和历史。
- 诊断日志和错误摘要。

## 分阶段改造

阶段与 ROADMAP 里程碑一致，事项编号见 `ROADMAP.md`。

### M1 - 领域概念清理（#010~#018）

目标：

- Product 正式替代旧 Produce。
- 删除、合并或废弃重复旧概念。
- 统一菜单、DTO、文档和接口命名。

交付：

- Produce 命名迁移清单。
- DeviceModel 命令能力合并到 Product，活动 API 使用 ProductCommand。
- 旧 DeviceGraph/DeviceDiagram/Scene 处理方案。
- Product、Device、Asset、Gateway、EdgeNode、Collection Template 页面职责说明。

验收：

- 新增代码不再出现 Produce 命名。
- 新文档不再混用产品、设备、资产和网关职责。
- Product 和 Device 的责任边界在页面上可见。

### M2 - EdgeNode 正式模型与跨仓契约固化（#020~#029）

目标：

- 把现有 Edge 第一版能力从属性键沉淀为正式模型。
- 稳定注册、心跳、能力、任务、回执和历史契约，并固化进 `IoTSharp.Contracts`。
- 完成 semantic-core 收敛决策，为 M3 扫清模型基础。

交付：

- EdgeRuntimeStatus、EdgeCapability、EdgeTask、EdgeTaskReceipt、EdgeCollectionAssignment。
- Edge 管理 API 和前端详情页改造。
- `edge-node-v1`、`collection-config-v1`、`edge-task-v1` 版本化契约（#027）。
- IoTEdge 任务回执转正（#028）。
- semantic-core 收敛决策记录（#029）。

验收：

- 平台可查询 EdgeNode 当前状态、能力和最近任务。
- 任务状态流转不依赖 AttributeLatest 作为主存储。
- Edge 任务历史可按任务、节点和时间查询。
- IoTEdge 以契约包消费者身份通过全链路回归。

### M3 - Collection Template 产品化（#030~#038）

目标：

- 把采集配置从草稿 DTO 升级为 Product 下的一等模板。
- 支持从模板生成运行时采集任务。

交付：

- CollectionTemplate、ProtocolTemplate、ConnectionTemplate、PointTemplate、TransformTemplate、SamplingPolicy、MappingPolicy。
- 模板校验和预览。
- 边缘执行链路对齐（#038）。

验收：

- Product 可以维护采集模板。
- 模板可以映射到 Telemetry、Attribute、AlarmInput 或 CommandFeedback。
- 模板生成的运行时配置可被 IoTEdge 直接消费执行——验收标准是「IoTEdge 能执行」，而非仅平台侧模型齐备。

### M4 - 配置与最小软件包发布闭环（#040~#048）

目标：

- 从 Product 采集模板生成配置版本，发布到 EdgeNode、Gateway 或选定 Device。
- 形成下发、接收、执行、成功、失败和回滚结果闭环。
- 最小软件包发布与边缘数据可靠性一并闭环，任务通道复用 EdgeTask。

交付：

- Collection Configuration Version。
- Edge/Gateway 配置发布 API、发布任务和回执。
- 配置当前版本、目标版本和差异展示。
- 最小软件包发布（#046）与 IoTEdge 软件更新执行器（#047）。
- 边缘数据断网缓存与续传（#048）。

验收：

- 平台知道哪个采集配置发布到了哪个 EdgeNode 或 Gateway。
- 执行端能拉取或接收配置和软件包，失败可回滚。
- 发布结果可审计、可查询、可失败重试。
- 平台不可达期间的采集数据不丢失，恢复后续传。

### M5 - Release Center 第一版（#050~#057）

目标：

- 建立 OTA、采集器软件包、配置模板和版本发布中心，在 #046 最小模型上兼容扩展。

交付：

- ReleasePackage、ReleasePlan、ReleaseTask、ReleaseReceipt。
- 灰度、回滚、暂停、继续和确认流程。
- 设备脚本 OTA（#056，复用 IoTEmbedded 双槽存储与失败回滚）与设备固件 OTA（#057）。

验收：

- 发布任务不进入 RuleChain。
- 发布范围可以指向 Product、Asset、EdgeNode、Gateway 或筛选后的 Device 集合。
- 每次发布有审计、状态、结果和回滚记录。

### M6 - RuleChain 增强（#060~#066）

目标：

- 保留 FlowRule 的实时处理定位。
- 增强标准节点、版本、模拟、观测和审计。
- 评估 AI 节点、MCP 写扩展和派生计算。

交付：

- 标准节点库、规则版本、仿真输入输出记录、运行观测和错误定位。
- 规则链 AI 节点（#064，经 #090 Provider 抽象）。

验收：

- RuleChain 仍只处理实时链路。
- 长周期发布、审批、灰度和回滚不进入规则链。
- AI 节点调用全程走权限和审计。

### 并行轨道

主线之外的三条轨道见 `ROADMAP.md` 对应章节，此处只记概念归属：

- 轨道 T（#070~#072）：SonnetDB 单依赖交付叙事，「云边同底座」的对外表达。
- 轨道 E（#080~#087）：IoTEdge 与 IoTEmbedded 的工程化硬化，不引入新领域概念。
- 轨道 A（#090~#095）：离线 AI 底座；唯一进入平台模型的是 #090 的 AI Provider 抽象，其余在 Tomur 仓库落地。

## 后续池

以下方向暂不进入当前里程碑（见 ROADMAP #800~#805）：

- AI Workbench、模型路由、技能目录、成本和审计（离线算力由轨道 A 的 Tomur 承载）。
- 本地语音、桌面 companion、本地模型协同、多智能体协作（语音能力复用 Tomur 多模态后端）。
- 平台高可用、多服务器、灾备和备份恢复演练。
- SonnetDB 引擎生产化（按 SonnetDB 自有路线图执行）。
- 前端实时遥测订阅增强。

## 当前第一批任务

1. 制定 Produce 命名迁移清单（#011）。
2. 将前端产品工作台从 `Product` 目录迁移到 `product`（#013）。
3. 新增 Product API 命名，替换旧 Produce API 命名（#012）。
4. 梳理 `DeviceModel`、`DeviceGraph`、`DeviceDiagram` 和 Scene 的引用，决定合并或废弃（#015~#017）。
5. M2 预研：EdgeTask、EdgeTaskReceipt、EdgeCapability、EdgeRuntimeStatus 和 EdgeCollectionAssignment 设计（#020~#024），以及 `edge-node-v1`/`collection-config-v1`/`edge-task-v1` 契约草案（#027）。
6. M3 前置：semantic-core 收敛决策材料准备（#029）——盘点 `semantic-core.v1.schema.json`、CollectionTask 草稿 DTO 与 IoTEdge 本地采集模型的差异。
