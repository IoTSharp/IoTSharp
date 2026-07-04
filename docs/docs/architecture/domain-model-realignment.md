---
title: 领域概念收口与改造路线
---

# 领域概念收口与改造路线

本文档是 `ROADMAP.md` 的落地细则，目标是把 IoTSharp 从设备平台收束为工业采集与运营平台。当前阶段不追求大而全，而是先把概念、模型、页面和契约理顺。

## 核心原则

- 不做全量重写。
- 新代码、新 DTO、新 UI、新文档和新 API 使用 Product，不再新增 Produce 命名。
- Product 是模板，Device 是实例，Asset 是业务对象，Gateway 是南向协议和子设备接入运行时，EdgeNode 是受管边缘运行时，Collection Template 是平台侧采集行为定义。
- 实时规则链只处理实时数据、告警和短动作，不承载 OTA、灰度、回滚、审批等长周期运营任务。
- AI 是横切能力，只能通过授权领域服务、技能和 MCP 工具使用平台能力。
- 旧概念如果与新领域模型重叠，优先删除、合并或标记废弃，不再为了概念名保留兼容路线。

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

旧 `Produce` 概念应改为 Product。`ProduceToken` 改为 `ProductToken`。

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

## 旧概念处理

### 直接改名或迁移

- `Produce` 改为 Product。
- `ProduceToken` 改为 `ProductToken`。
- 前端 `produce` 目录和 API 模块改为 `product`。
- 文档、菜单、按钮、DTO 和新 API 统一使用 Product。

### 合并

- `DeviceModel` 合并到 Product 的能力、命令和配置定义。
- Product/Produce 上的旧 Gateway 配置合并到 Collection Template。
- Scene 如果只是规则链或资产视图包装，合并到 RuleChain View 或 Asset View。

### 废弃

- 旧 `DeviceGraph`、`DeviceDiagram`、`DeviceGraphToolBox` 如果只是历史设计器数据，标记废弃。
- 把 Device 当业务层级、模板容器或发布范围的用法停止扩展。

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

### P0 - 领域概念清理

目标：

- Product 正式替代 Produce。
- 删除、合并或废弃重复旧概念。
- 统一菜单、DTO、文档和接口命名。

交付：

- Product 命名迁移清单。
- DeviceModel 合并方案。
- 旧 DeviceGraph/DeviceDiagram/Scene 处理方案。
- Product、Device、Asset、Gateway、EdgeNode、Collection Template 页面职责说明。

验收：

- 新增代码不再出现 Produce 命名。
- 新文档不再混用产品、设备、资产和网关职责。
- Product 和 Device 的责任边界在页面上可见。

### P1 - EdgeNode 正式模型

目标：

- 把现有 Edge 第一版能力从属性键沉淀为正式模型。
- 稳定注册、心跳、能力、任务、回执和历史契约。

交付：

- EdgeRuntimeStatus。
- EdgeCapability。
- EdgeTask。
- EdgeTaskReceipt。
- EdgeCollectionAssignment。
- Edge 管理 API 和前端详情页改造。

验收：

- 平台可查询 EdgeNode 当前状态、能力和最近任务。
- 任务状态流转不依赖 AttributeLatest 作为主存储。
- Edge 任务历史可按任务、节点和时间查询。

### P2 - Collection Template 产品化

目标：

- 把采集配置从草稿 DTO 升级为 Product 下的一等模板。
- 支持从模板生成运行时采集任务。

交付：

- CollectionTemplate。
- ProtocolTemplate。
- ConnectionTemplate。
- PointTemplate。
- TransformTemplate。
- SamplingPolicy。
- MappingPolicy。
- 模板校验和预览。

验收：

- Product 可以维护采集模板。
- 模板可以映射到 Telemetry、Attribute、AlarmInput 或 CommandFeedback。
- 模板能生成发布到 EdgeNode 或 Gateway 的运行时配置。

### P3 - 配置发布闭环

目标：

- 从 Product 采集模板生成配置版本。
- 将配置发布到 EdgeNode、Gateway 或选定 Device。
- 形成下发、接收、执行、成功、失败和回滚结果闭环。

交付：

- Collection Configuration Version。
- Edge/Gateway 配置发布 API。
- 配置发布任务和回执。
- 配置当前版本、目标版本和差异展示。

验收：

- 平台知道哪个采集配置发布到了哪个 EdgeNode 或 Gateway。
- 执行端能拉取或接收配置。
- 发布结果可审计、可查询、可失败重试。

### P4 - Release Center

目标：

- 建立 OTA、采集器软件包、配置模板和版本发布中心。

交付：

- ReleasePackage。
- ReleasePlan。
- ReleaseTask。
- ReleaseReceipt。
- 灰度、回滚、暂停、继续和确认流程。

验收：

- 发布任务不进入 RuleChain。
- 发布范围可以指向 Product、Asset、EdgeNode、Gateway 或筛选后的 Device 集合。
- 每次发布有审计、状态、结果和回滚记录。

### P5 - RuleChain 增强

目标：

- 保留 FlowRule 的实时处理定位。
- 增强标准节点、版本、模拟、观测和审计。

交付：

- 标准节点库。
- 规则版本。
- 仿真输入和输出记录。
- 运行观测和错误定位。

验收：

- RuleChain 仍只处理实时链路。
- 长周期发布、审批、灰度和回滚不进入规则链。

## 后续池

以下方向暂不进入 P0-P3：

- AI Workbench、模型路由、技能目录、成本和审计。
- 本地语音、桌面 companion、本地模型协同、多智能体协作。
- 平台高可用、多服务器、灾备和备份恢复演练。
- SonnetDB 全面生产化和跨存储兼容矩阵。
- 前端实时遥测订阅增强。

## 当前第一批任务

1. 制定 Product 命名迁移清单。
2. 将前端产品工作台从 `produce` 目录迁移到 `product`。
3. 新增 Product API 命名，替换 Produce API 命名。
4. 梳理 `DeviceModel`、`DeviceGraph`、`DeviceDiagram` 和 Scene 的引用，决定合并或废弃。
5. 新增 EdgeTask、EdgeTaskReceipt、EdgeCapability、EdgeRuntimeStatus 和 EdgeCollectionAssignment 设计。
6. 将 Edge 任务回执与设备详情、边缘详情关联展示。
