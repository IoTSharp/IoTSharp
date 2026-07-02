# IoTSharp 路线图

这份路线图用于记录 IoTSharp 从设备平台演进为完整工业采集与运营平台的当前产品方向。

## 产品方向

IoTSharp 将围绕三层架构持续演进：

1. **接入与采集层**
   - 设备接入
   - 网关与边缘连接
   - 采集建模与协议模板
2. **实时规则层**
   - 遥测处理
   - 告警与动作规则
   - 数据转换与分发
3. **运营与发布层**
   - OTA 固件下发
   - 采集器软件包下发
   - 配置版本管理与发布
   - 批量发布、灰度发布、回滚与结果确认

AI 工作台与智能体能力应作为**横切层**并入以上三层，而不是独立重写第四个平台：

- 在接入与采集层提供 AI 辅助建模、协议配置建议与边缘诊断。
- 在实时规则层提供规则解释、调试建议、标准节点生成与运行态分析。
- 在运营与发布层提供发布分组、灰度策略、风险提示、回滚建议与运维问答。

## 关键判断

- 当前方向是正确的，但平台目前仍处于“可用基础”阶段，还不是一个完全产品化的工业平台。
- 现有的接入、产品、资产和规则链能力应当保留并升级，而不是整体重写。
- 下一阶段必须把网关、边缘代理、采集配置和发布管理提升为平台的一等能力。
- AI 工作台应构建在现有 IoTSharp 控制平面之上，通过权限、审计、租户隔离和领域服务接入，而不是绕过业务层直接操作数据。
- 技术栈优先采用最新 **Microsoft Agent Framework** 与 **Microsoft.Extensions.AI**，并保留、扩展现有 **MCP** 接口作为工具边界。
- Camel.NET 的并入方向是吸收多模型接入、多智能体协作、工具调用、记忆和任务编排能力，而不是替换 IoTSharp 主体架构。

## 业务顺序任务索引

状态说明：✅️ 已完成，🚧 进行中，⬜ 未开始。

下面的编号是路线图任务的业务推进顺序；详细设计章节可以保留更多背景、草案和示例，但任务状态以此索引为准。

| 编号 | 状态 | 业务任务 | 当前判断 |
| --- | --- | --- | --- |
| RD-00 | ✅️ | 保留现有平台基础 | 设备接入、产品、资产、FlowRule、API、UI、权限、审计和现有 MCP/AISettings 基础继续保留。 |
| RD-01 | 🚧 | 边缘与采集管理基础 | Edge 注册、心跳、能力上报、任务回执和管理入口已有雏形，仍需完善统一领域模型、诊断面和跨项目契约。 |
| RD-02 | 🚧 | 采集建模与可视化设计 | `CollectionTaskDto`、采集配置保存/拉取和 Gateway 配置同步已有基础，协议模板、设计器和运行时闭环仍需产品化。 |
| RD-03 | ⬜ | 运营与发布中心 | OTA、采集器软件包、配置版本、灰度、回滚和确认流程尚未开始。 |
| RD-04 | 🚧 | 实时规则增强 | FlowRule 的实时处理定位已成立，标准节点、版本化、仿真、观测和审计能力仍需增强。 |
| RD-05 | 🚧 | 组织与生态协同 | IoTSharp / Gateway / SDK / 采集代理的职责边界已有方向，兼容矩阵、版本化契约和跨仓库节奏仍需落地。 |
| RD-06 | ⬜ | 平台高可用与多服务器扩展 | 横向扩展、状态外部化、故障转移、备份恢复和灾备演练尚未开始。 |
| RD-07 | 🚧 | AI 工作台与智能体横切能力 | MCP 入口和 AISettings 已具备，模型抽象、技能目录、Workbench、审计与成本视图仍需建设。 |
| RD-08 | ⬜ | 本地语音、治理与多智能体协作 | 语音 companion、本地模型协同、记忆、知识源、审批和自动运维边界尚未开始。 |
| RD-09 | 🚧 | 前端实时遥测订阅 | 后端最新值与设备详情 HTTP 查询已具备，浏览器侧订阅链路、权限和断线重连策略仍需补齐。 |
| RD-10 | 🚧 | SonnetDB 可选数据底座生产化 | 已从 SonnetDB 路线图迁入 IoTSharp 侧规划：覆盖关系库、时序库、缓存、对象桶、迁移/双写/回滚、长稳和兼容矩阵；`TelemetryStorage=SonnetDB` 已具备写入、latest、历史、聚合、批量写入、健康检查、备份恢复和故障回放基础。 |

## AI 工作台与智能体路线

### 当前基础

- ✅️ RD-07.1 主应用已暴露 MCP 接口，可作为 AI 工具平台的现有入口。
- ✅️ RD-07.2 已存在 `AISettings` 与最小化 MCP 设备工具，可作为租户级 AI 配置和工具体系的起点。
- ⬜ RD-07.3 还没有统一的模型接入抽象、会话记忆、推理编排与技能目录。
- ⬜ RD-07.4 还没有统一 AI Workbench 前端入口来承接设备、资产、产品、边缘、规则链和发布中心上下文。
- ⬜ RD-08.1 本仓库当前没有实际的 `IoTSharp.Agent` 项目目录，因此本地语音与桌面 companion 应作为后续独立运行时规划，而不是先耦合进主 Web 应用。

### AI 架构原则

- ✅️ RD-00.1 不整体重写 IoTSharp。
- ✅️ RD-00.2 保持 IoTSharp 继续承担控制平面、权限、审计、UI 与运营编排职责。
- ✅️ RD-05.7 保持 Gateway / 本地语音 / 本地模型运行时作为清晰边界内的独立部署能力。
- ⬜ RD-07.5 以 Microsoft Agent Framework 作为主智能体框架。
- ⬜ RD-07.6 以 Microsoft.Extensions.AI 作为模型抽象与提供方接入层。
- 🚧 RD-07.7 以 MCP 作为工具暴露与外部智能体集成边界。
- ⬜ RD-07.8 以领域服务与授权技能方式暴露 IoTSharp 能力，而不是让 AI 直接访问数据库。

### AI 平台分阶段路线

#### AI-P0 - 技术选型与领域模型
目标：
- ⬜ RD-07.P0.1 定义 AI 平台边界：模型、技能、会话、审计、成本、提示词、知识源。
- ⬜ RD-07.P0.2 把 `AISettings` 从“开关 + API Key”升级为租户/客户级 AI 配置中心。
- ⬜ RD-07.P0.3 明确本地模型、远程模型、MCP 工具、外部工具的统一接入方式。

预期产出：
- ⬜ RD-07.P0.4 AI 领域模型
- ⬜ RD-07.P0.5 AI 配置中心
- ⬜ RD-07.P0.6 AI 安全与权限模型
- ⬜ RD-07.P0.7 AI 审计与成本模型

#### AI-P1 - 统一模型接入层
目标：
- ⬜ RD-07.P1.1 支持 OpenAI 风格本地模型接口接入。
- ⬜ RD-07.P1.2 支持 Azure OpenAI / OpenAI / 其他兼容远程模型接口。
- ⬜ RD-07.P1.3 建立统一 Provider Router、模型路由、重试、降级、限流和审计机制。

预期产出：
- ⬜ RD-07.P1.4 模型提供方注册
- ⬜ RD-07.P1.5 模型配置档案
- ⬜ RD-07.P1.6 推理策略
- ⬜ RD-07.P1.7 提供方路由与回退机制

#### AI-P2 - IoTSharp 技能注册中心与 MCP 扩展
目标：
- ⬜ RD-07.P2.1 把 IoTSharp 现有能力沉淀为可授权技能。
- ⬜ RD-07.P2.2 扩展当前 MCP，形成覆盖设备、属性、遥测、资产、产品、边缘、规则链和发布中心的工具目录。
- ⬜ RD-07.P2.3 支持 IoTSharp 智能体调用外部 MCP 工具。

预期产出：
- ⬜ RD-07.P2.4 技能/工具注册中心
- ⬜ RD-07.P2.5 技能风险分级：只读、需确认、需审批
- ⬜ RD-07.P2.6 MCP 工具目录
- ⬜ RD-07.P2.7 工具调用审计与人工确认机制

#### AI-P3 - AI Workbench MVP
目标：
- ⬜ RD-07.P3.1 新增统一 AI Workbench，而不是在每个页面散落独立聊天入口。
- ⬜ RD-07.P3.2 提供对话与推理面板、工具执行记录、上下文对象栏、会话历史、计划/执行步骤、模型与技能选择、审计与成本视图。
- ⬜ RD-07.P3.3 支持从设备、资产、产品、边缘节点、规则链、发布任务页面带上下文进入 AI 工作台。

预期产出：
- ⬜ RD-07.P3.4 AI Workbench 统一入口
- ⬜ RD-07.P3.5 上下文感知会话
- ⬜ RD-07.P3.6 工具执行可视化
- ⬜ RD-07.P3.7 成本与审计可视化

#### AI-P4 - 首批高价值业务助手
目标：
- ⬜ RD-07.P4.1 先上线设备、遥测、边缘三个高价值助手场景。
- ⬜ RD-07.P4.2 支持设备诊断、属性解释、遥测聚合分析、边缘状态解读与操作建议。
- ⬜ RD-07.P4.3 所有高风险动作默认采用人工确认式执行。

预期产出：
- ⬜ RD-07.P4.4 设备诊断助手
- ⬜ RD-07.P4.5 遥测分析助手
- ⬜ RD-07.P4.6 边缘运维助手

#### AI-P5 - 扩展到产品、资产、规则链与发布中心
目标：
- ⬜ RD-07.P5.1 辅助产品模板、物模型、命令模板和兼容矩阵建设。
- ⬜ RD-07.P5.2 辅助资产层级理解、拓扑说明和影响范围分析。
- ⬜ RD-07.P5.3 辅助规则链解释、调试与优化，但不把规则链变成长周期工作流引擎。
- ⬜ RD-07.P5.4 辅助升级中心/发布中心进行版本选择、发布分组、灰度和回滚建议。

预期产出：
- ⬜ RD-07.P5.5 产品助手
- ⬜ RD-07.P5.6 资产助手
- ⬜ RD-07.P5.7 规则链助手
- ⬜ RD-07.P5.8 发布中心助手

#### AI-P6 - 本地部署语音识别与 TTS
目标：
- ⬜ RD-08.P6.1 为本地部署场景规划语音入口，但保持与主站解耦。
- ⬜ RD-08.P6.2 优先面向边缘伴随服务、桌面 companion、本地运维终端等独立运行时。
- ⬜ RD-08.P6.3 支持语音转文本、文本转语音、唤醒后的受控技能调用，以及本地模型优先、离线可降级。

预期产出：
- ⬜ RD-08.P6.4 语音 companion 运行时方案
- ⬜ RD-08.P6.5 本地语音与本地模型协同方案
- ⬜ RD-08.P6.6 语音技能调用安全边界

#### AI-P7 - 治理、安全与多智能体协作
目标：
- ⬜ RD-08.P7.1 建立提示词模板治理、模型白名单、敏感数据脱敏、审批流和可信度展示。
- ⬜ RD-08.P7.2 引入记忆、知识源、多智能体协作与自动运维闭环能力。
- ⬜ RD-08.P7.3 让 AI 输出始终附带证据来源与可审计轨迹，避免“AI 直接写平台状态”。

预期产出：
- ⬜ RD-08.P7.4 AI 治理体系
- ⬜ RD-08.P7.5 记忆与知识源体系
- ⬜ RD-08.P7.6 多智能体协作框架
- ⬜ RD-08.P7.7 自动运维闭环边界与安全策略

### AI MVP 范围

第一版不追求全自动自治平台，优先交付：

- ⬜ RD-07.MVP.1 统一模型接入
- ⬜ RD-07.MVP.2 AI Workbench
- ⬜ RD-07.MVP.3 统一技能目录
- ⬜ RD-07.MVP.4 MCP 增强
- ⬜ RD-07.MVP.5 设备 / 遥测 / 边缘诊断助手
- ⬜ RD-07.MVP.6 人工确认式执行

后续再逐步进入发布编排、语音值守、多智能体协作与自动运维闭环。

## 领域职责

### 产品
产品是**技术模板**。

应负责：
- 设备类型定义
- 协议与采集模板
- 点位模型
- 默认属性
- 命令模板
- 软件与固件兼容矩阵

### 资产
资产是**业务对象与拓扑容器**。

应负责：
- 站点
- 车间
- 产线
- 区域
- 客户部署拓扑
- 业务分组与层级结构

### 设备
设备是**运行时实例**。

应负责：
- 标识
- 在线状态
- 遥测与属性
- 告警与命令
- 与产品、资产、边缘节点的关系

### 边缘节点
边缘节点是**受管的采集运行时**。

应负责：
- 注册
- 心跳
- 版本
- 能力
- 运行状态
- 日志与诊断
- 与 Gateway 及未来采集代理的关系

能力边界补充：
- 边缘节点负责**执行采集任务**，但不负责承载平台侧的产品建模、资产管理、权限、审计与发布编排。
- 边缘节点负责协议连接、轮询调度、本地重试、基础缓存、数据预处理与结果上报。
- 边缘节点可以执行寄存器解析、字节序调整、数值换算、质量位附加和时间戳补全，但这些规则应来自平台下发的标准任务模型。
- 边缘节点应声明自身支持的协议、连接方式、数据类型、字节序处理能力、换算能力、批量读优化能力与诊断能力，作为 capability 的一部分。
- 平台负责保存任务定义、版本、目标属性模型、任务审计与任务下发；边缘节点负责接收、执行、回执与上报。

针对 Modbus 采集的边缘节点责任：
- 管理 Modbus TCP、Modbus RTU over TCP、串口 RTU 等连接实例。
- 按从站、功能码、地址段和轮询周期组织批量读取任务，避免平台直接下发零散寄存器指令。
- 根据任务定义解析 Coil、Discrete Input、Holding Register、Input Register 等不同数据区。
- 根据数据类型和字节序规则，将 16 位寄存器组合为 bool、int16、uint16、int32、uint32、int64、uint64、float32、float64、string、bitfield 等值。
- 在本地执行缩放系数、偏移量、表达式换算、枚举映射、位提取和异常值处理后，再上传到平台遥测或属性。
- 上报采集质量、原始值摘要、错误码、最后采集时间与连接诊断信息，便于平台排障。

### 规则链
规则链仍然是**实时数据处理引擎**。

应聚焦：
- 入站时转换
- 路由
- 富化
- 告警触发
- 遥测与属性发布

它不应成为长周期 OTA、审批、回滚或大批量运营操作的主引擎。

### 发布中心
发布中心是**运营交付领域**。

应负责：
- OTA 包
- 采集器软件包
- 配置模板与版本
- 分阶段发布计划
- 发布执行记录
- 回滚与确认流程

## 规划中的能力区域

### RD-01 🚧 第一阶段 - 边缘与采集管理基础
优先级：最高

目标：
- 🚧 RD-01.1 让网关和采集客户端成为平台中的一等实体
- ✅️ RD-01.2 引入统一的边缘节点注册与心跳模型
- 🚧 RD-01.3 通过统一的平台契约管理 C# Gateway 和未来代理
- 🚧 RD-01.4 暴露版本、能力、健康度、状态和诊断元数据

预期产出：
- 🚧 RD-01.5 边缘节点领域模型
- ✅️ RD-01.6 边缘注册协议
- ✅️ RD-01.7 心跳与能力上报机制
- 🚧 RD-01.8 统一的管理界面入口
- 🚧 RD-01.9 Gateway 与未来采集代理集成的项目边界约定

### RD-02 🚧 第二阶段 - 采集建模与可视化设计
目标：
- 🚧 RD-02.1 将采集配置从脚本和约定升级为标准模型
- 🚧 RD-02.2 构建协议模板、连接模板、点位模板、示例策略与转换链
- 🚧 RD-02.3 支持大小端、缩放、公式、单位、死区、降采样等工业映射能力
- 🚧 RD-02.4 打通设计器界面与运行时持久化、执行模型
- ⬜ RD-02.5 在可视化设计方向规划 FUXA 集成，用于组态/HMI/SCADA 场景

预期产出：
- 🚧 RD-02.6 采集模板模型
- 🚧 RD-02.7 点位模型与转换模型
- 🚧 RD-02.8 运行时配置持久化
- 🚧 RD-02.9 面向 Modbus、OPC UA 与网关场景的设计器到运行时集成
- ⬜ RD-02.10 FUXA 与 IoTSharp 可视化设计的集成方案与边界定义

其中 Modbus 可视化任务设计应优先细化为以下结构：

1. 任务分层
- 连接层：定义 TCP/串口、主机、端口、串口参数、超时、重试、并发限制。
- 设备层：定义从站地址、设备别名、设备模板、轮询分组、启停状态。
- 采集点层：定义具体读取哪个地址、采用哪个功能码、寄存器长度、数据类型、字节序和上传目标。
- 转换层：定义缩放、偏移、表达式、枚举映射、位拆分、质量判断与异常值处理。
- 上报层：定义写入平台的属性名称、属性标题、属性类型、数据分类、上报模式与变化阈值。

2. Modbus 点位模型建议字段
- pointKey：点位稳定标识，用于版本比较与任务增量下发。
- pointName：采集项名称，用于设计器展示。
- slaveId：从站编号。
- area / functionCode：线圈、离散输入、输入寄存器、保持寄存器。
- address：起始地址。
- registerCount：寄存器数量或位长度。
- rawDataType：原始数据类型，如 bool、int16、uint16、int32、uint32、float32、float64、string、bitfield。
- byteOrder：字节顺序，如 ABCD、BADC、CDAB、DCBA。
- wordOrder：字顺序，用于多寄存器组合。
- bitOrder：位序，面向 coil/bitfield 或寄存器内位提取。
- stringEncoding：字符串编码，如 ASCII、UTF8、GBK。
- scale：缩放系数。
- offset：偏移量。
- expression：表达式换算，例如 `(x * 0.1) + 5`。
- readPeriodMs：轮询周期。
- reportPolicy：全量上报、变化上报、死区上报、周期上报。
- qualityPolicy：超时、越界、非法值、通讯失败时的质量标记策略。
- targetType：上传到平台的目标类型，至少区分 telemetry、attribute、alarm-input。
- targetName：平台中的属性或遥测名称。
- targetValueType：平台侧值类型，如 boolean、integer、number、string、json。
- unit：工程单位。
- description：点位说明、现场备注。

3. Modbus 可视化编辑器交互要求
- 以“连接配置 + 从站设备 + 点位表格 + 转换配置 + 平台映射”五段式编辑，避免把所有字段堆在单一表格中。
- 点位表格应允许批量新增连续地址，并自动推导默认寄存器长度与数据类型。
- 对于 32 位/64 位数值，必须显式提供字节序和字序选择，不能只保留一个模糊的 `dataFormat` 文本字段。
- 提供常用模板：01/02 布尔量采集、03/04 单寄存器整数、双寄存器浮点、寄存器字符串、bitfield 位拆解。
- 提供“原始值预览/试读”能力，让用户在设计时看到寄存器原始结果、颠倒后的字节序和最终换算结果。
- 平台映射区域应直接选择或创建目标属性，明确属性名称、显示名称、值类型、单位和数据分类。
- 对于一个寄存器拆多个 bit 的场景，编辑器应支持“一源多目标”的位映射配置。
- 对于多个寄存器组合一个值的场景，编辑器应支持“地址块组合预览”和长度校验。

4. 平台属性映射规则
- 采集任务中的 targetName 应与平台产品模型或设备模型中的标准属性建立绑定，而不是只保存自由文本。
- 属性至少区分：遥测、只读属性、可写属性、计算属性四类，避免后续控制与展示语义混乱。
- 属性类型应标准化为 boolean、int、long、double、decimal、string、enum、json。
- 当采集值与平台属性类型不一致时，应在设计阶段阻断或提示显式转换。
- 属性映射应支持单位、精度、小数位和展示分组，方便前端组态与告警复用。

5. 设计器与运行时边界
- 设计器输出的是标准化任务 JSON/DTO，而不是直接拼接脚本。
- 运行时只消费标准任务模型，不依赖前端页面私有字段命名。
- 设计器允许保留协议扩展字段，但核心字段必须可被平台统一校验、版本化和审计。
- Modbus、OPC UA、未来 BACnet/IEC104 等协议应共享统一的“连接-点位-转换-映射-上报”骨架，只在协议特有字段上分叉。

6. 第一轮落地建议
- 🚧 RD-02.11 先定义 EdgeNode capability 中与采集相关的标准声明，如 supportedProtocols、supportedPointTypes、supportedTransforms、supportedReportPolicies。
- 🚧 RD-02.12 在平台侧先沉淀 ModbusConnection、ModbusDevice、ModbusPoint、ValueTransform、PlatformMapping 五个核心模型。
- ⬜ RD-02.13 将现有前端中的 Modbus mapping 表升级为结构化编辑器，补齐 byteOrder、wordOrder、expression、targetType、targetName、targetValueType 等字段。
- ⬜ RD-02.14 先支持最常见场景：03/04 读寄存器、bool/int16/uint16/int32/uint32/float32/string、缩放/偏移/表达式、遥测/属性两类上报。
- ⬜ RD-02.15 试读与预览优先于复杂编排，先保证“配得清楚、读得正确、映射明确”。

### 第二阶段补充 - C# 领域模型与 DTO 草案

当前判断：
- 现有仓库中已有 Edge 基础常量和通用遥测 DTO，但尚未形成统一的采集任务领域模型与协议 DTO。
- 这部分应优先沉淀到平台核心领域与 `IoTSharp.Contracts` 中，作为 IoTSharp、Gateway 和未来代理共享的版本化契约。

1. 建议的领域模型分层
- `EdgeNode`：受管边缘运行时聚合根，关注注册、版本、能力、状态、诊断和已部署任务版本。
- `CollectionProfile`：采集模板聚合根，描述某一协议采集方案的设计时模型，可绑定产品或设备类型。
- `CollectionTask`：下发给边缘运行时的可执行任务聚合根，描述连接、设备、点位、转换、映射和上报策略。
- `ConnectionDefinition`：协议连接定义，如 Modbus TCP、串口 RTU、OPC UA Endpoint。
- `CollectionDevice`：协议内的逻辑采集对象，如 Modbus 从站、OPC UA Server/Namespace 分组。
- `CollectionPoint`：单个采集点定义，描述源地址、源类型、转换规则和平台映射。
- `ValueTransform`：标准值转换链，描述缩放、偏移、表达式、位拆分、枚举映射、质量修正。
- `PlatformMapping`：采集点到 IoTSharp 平台属性/遥测/告警输入的映射定义。
- `TaskDispatchRecord` / `TaskReceiptRecord`：任务下发与回执记录，用于审计、重试和对账。

2. 建议的 C# 领域对象草案

```csharp
public sealed class EdgeNode
{
      public Guid Id { get; init; }
      public string RuntimeType { get; init; } = default!;
      public string RuntimeName { get; init; } = default!;
      public string InstanceId { get; init; } = default!;
      public string Version { get; init; } = default!;
      public EdgeCapability Capability { get; init; } = new();
      public EdgeRuntimeStatus Status { get; init; } = new();
}

public sealed class CollectionTask
{
      public Guid Id { get; init; }
      public string TaskKey { get; init; } = default!;
      public string Protocol { get; init; } = default!;
      public int Version { get; init; }
      public Guid EdgeNodeId { get; init; }
      public ConnectionDefinition Connection { get; init; } = default!;
      public IReadOnlyList<CollectionDevice> Devices { get; init; } = [];
      public ReportPolicyDefinition ReportPolicy { get; init; } = new();
}

public sealed class CollectionDevice
{
      public string DeviceKey { get; init; } = default!;
      public string DeviceName { get; init; } = default!;
      public bool Enabled { get; init; } = true;
      public string? ExternalKey { get; init; }
      public IReadOnlyList<CollectionPoint> Points { get; init; } = [];
      public JsonObject ProtocolOptions { get; init; } = new();
}

public sealed class CollectionPoint
{
      public string PointKey { get; init; } = default!;
      public string PointName { get; init; } = default!;
      public string SourceType { get; init; } = default!;
      public string Address { get; init; } = default!;
      public string RawValueType { get; init; } = default!;
      public int Length { get; init; }
      public PollingPolicyDefinition Polling { get; init; } = new();
      public IReadOnlyList<ValueTransform> Transforms { get; init; } = [];
      public PlatformMapping Mapping { get; init; } = new();
      public JsonObject ProtocolOptions { get; init; } = new();
}

public sealed class ValueTransform
{
      public string TransformType { get; init; } = default!;
      public int Order { get; init; }
      public JsonObject Parameters { get; init; } = new();
}

public sealed class PlatformMapping
{
      public string TargetType { get; init; } = default!;
      public string TargetName { get; init; } = default!;
      public string ValueType { get; init; } = default!;
      public string? Unit { get; init; }
      public string? DisplayName { get; init; }
      public string? Group { get; init; }
}
```

3. 建议的 Contracts DTO 草案
- `EdgeCapabilityDto`：运行时支持能力声明。
- `CollectionTaskDto`：平台下发给边缘的统一任务 DTO。
- `CollectionConnectionDto`：统一连接 DTO，保留 `protocolOptions` 承载协议特有字段。
- `CollectionDeviceDto`：统一设备 DTO。
- `CollectionPointDto`：统一点位 DTO。
- `ValueTransformDto`：转换规则 DTO。
- `PlatformMappingDto`：平台属性映射 DTO。
- `TaskPreviewRequestDto` / `TaskPreviewResponseDto`：设计器试读和预览 DTO。

建议的 DTO 风格：
- DTO 只承载契约，不包含领域行为。
- 公共字段显式建模，协议特有字段通过 `protocolOptions` 或协议子 DTO 扩展。
- DTO 使用 extend-only 思路演进，避免频繁破坏 Gateway 兼容性。
- `IoTSharp.Contracts` 中优先增加 `CollectionTaskDto` 及其子对象，作为 HTTP/API/MQTT 下发的统一负载。

4. 建议的枚举或代码表
- `CollectionProtocolType`：Modbus、OpcUa、Bacnet、IEC104、MQTT、Custom。
- `CollectionTargetType`：Telemetry、Attribute、AlarmInput、CommandFeedback。
- `CollectionValueType`：Boolean、Int32、Int64、Double、Decimal、String、Enum、Json。
- `TransformType`：Scale、Offset、Expression、EnumMap、BitExtract、WordSwap、ByteSwap、Clamp、DefaultOnError。
- `ReportTriggerType`：Always、OnChange、Deadband、Interval、QualityChange。
- `QualityStatusType`：Good、Uncertain、Bad、CommError、InvalidData。

### 第二阶段补充 - 前端 Modbus Designer 字段升级方案

当前前端字段模型：

```ts
export interface modbusmapping {
   _id?: string;
   id?: string;
   code?: number;
   dataName?: string;
   dataType?: string;
   dataCatalog?: string;
   funCode?: string;
   address?: number;
   length?: number;
   dataFormat?: string;
   codePage?: number;
}
```

问题判断：
- 现有模型把“协议源信息”“转换规则”“平台映射”混在一起，无法承载复杂配置。
- `dataFormat` 语义过于模糊，既可能表示大小端，也可能表示显示格式，不适合作为核心字段。
- 缺失平台映射、轮询策略、质量策略和预览字段，无法支撑设计器升级。

1. 建议保留字段
- `_id`：保留，用于前端临时行标识。
- `id`：保留，但应转为稳定业务标识，例如 `pointKey`。
- `dataName`：保留，重命名为 `pointName`。
- `address`：保留，仍作为 Modbus 起始地址。
- `length`：保留，重命名为 `registerCount`。

2. 建议替换字段
- `code` 替换为 `slaveId`，明确表示从站编号。
- `funCode` 替换为 `area` 和 `functionCode`，避免业务层长期只依赖数字码值。
- `dataType` 替换为 `rawDataType`，明确它表示原始采集类型，而不是平台属性类型。
- `dataCatalog` 替换为 `targetType`，当前“数据分类”应收敛为平台语义目标，如 telemetry/attribute。
- `dataFormat` 替换为 `byteOrder`、`wordOrder`、`bitOrder`、`displayFormat` 四类字段，不再复用单字段。
- `codePage` 替换为 `stringEncoding`，仅在字符串场景生效。

3. 建议新增字段
- `pointKey`
- `sourceType`
- `pollingGroup`
- `readPeriodMs`
- `byteOrder`
- `wordOrder`
- `bitOffset`
- `bitLength`
- `scale`
- `offset`
- `expression`
- `enumMapping`
- `qualityPolicy`
- `reportPolicy`
- `targetName`
- `targetValueType`
- `displayName`
- `unit`
- `precision`
- `description`
- `previewRawValue`
- `previewTransformedValue`

4. 前端推荐的新模型分拆
- `ProtocolConnectionModel`：连接信息。
- `ProtocolDeviceModel`：从站或逻辑设备信息。
- `CollectionPointModel`：通用点位骨架。
- `ModbusPointOptionsModel`：Modbus 特有字段，如 slaveId、functionCode、byteOrder。
- `ValueTransformModel`：转换链配置。
- `PlatformMappingModel`：平台目标映射。

建议结果：
- 不再把所有字段都堆进一个 `modbusmapping` 接口。
- 改为“通用点位骨架 + Modbus 扩展字段”的结构，让 OPC UA 和其他协议复用同一套设计器骨架。

### 第二阶段补充 - 通用协议设计器规划

原则：
- 边缘设计器必须通用，但不能为了通用而退化成最低公共抽象。
- 设计器应分为“协议无关骨架层”和“协议特化面板层”。
- 平台任务模型必须统一；协议特化只体现在连接参数、点位源信息和预览方式上。

1. 通用骨架层
- 连接配置：连接名称、协议类型、超时、重试、启停、标签。
- 设备分组：逻辑设备、现场设备、命名空间分组、轮询分组。
- 点位清单：点位名称、启停、采集周期、转换链、平台映射。
- 转换配置：缩放、偏移、表达式、枚举、质量策略、死区策略。
- 平台映射：目标类型、属性名、值类型、单位、显示名、告警输入绑定。
- 调试与预览：连接测试、试读、原始值、转换后值、上报预览。

2. 协议特化层
- Modbus：从站、功能码、寄存器地址、寄存器长度、字节序、位拆分、批量地址段优化。
- OPC UA：Endpoint、SecurityPolicy、Namespace、NodeId、BrowsePath、Subscription、SamplingInterval、Deadband。
- BACnet：DeviceId、ObjectType、ObjectInstance、PropertyIdentifier、Priority。
- IEC104：CommonAddress、InformationObjectAddress、TypeId、CauseOfTransmission。
- MQTT/自定义：Topic、QoS、PayloadPath、JSONPath、Schema、时间戳字段提取。

3. 通用与特化的边界要求
- 通用层定义“点位要采什么、如何转、上报到哪里”。
- 特化层定义“这个协议从哪里取值、如何编码、如何批量优化、如何试读”。
- 所有协议最终都输出统一的 `CollectionTaskDto`。
- 设计器内部可以有 `ModbusTaskDraft`、`OpcUaTaskDraft` 等特化草稿对象，但提交时必须归一化。

4. 第一轮协议覆盖建议
- 第一轮先把通用骨架稳定下来。
- 第一批协议优先支持 Modbus 和 OPC UA。
- Modbus 优先覆盖批量轮询、地址块、字节序和寄存器组合。
- OPC UA 优先覆盖 NodeId 浏览、订阅/轮询二选一、采样周期、死区与数据质量。
- 未来协议接入时，不允许绕过通用映射模型直接写私有脚本任务。

### 第二阶段补充 - Modbus 任务 JSON 示例

```json
{
   "taskKey": "modbus-boiler-room-a",
   "protocol": "Modbus",
   "version": 3,
   "edgeNodeId": "1f6df5e0-6af8-4b57-a7a5-8efef17f1001",
   "connection": {
      "connectionKey": "modbus-tcp-line-1",
      "connectionName": "锅炉房 1 号线",
      "protocol": "Modbus",
      "transport": "Tcp",
      "host": "192.168.10.15",
      "port": 502,
      "timeoutMs": 2000,
      "retryCount": 3,
      "protocolOptions": {
         "maxBatchRegisters": 64,
         "maxConcurrentRequests": 1
      }
   },
   "devices": [
      {
         "deviceKey": "slave-1",
         "deviceName": "锅炉控制器 1",
         "enabled": true,
         "protocolOptions": {
            "slaveId": 1
         },
         "points": [
            {
               "pointKey": "supply-temp",
               "pointName": "供水温度",
               "sourceType": "HoldingRegister",
               "address": "40001",
               "rawValueType": "UInt16",
               "length": 1,
               "polling": {
                  "readPeriodMs": 5000,
                  "group": "temperature"
               },
               "transforms": [
                  {
                     "transformType": "Scale",
                     "order": 1,
                     "parameters": {
                        "factor": 0.1
                     }
                  }
               ],
               "mapping": {
                  "targetType": "Telemetry",
                  "targetName": "supplyTemperature",
                  "valueType": "Double",
                  "displayName": "供水温度",
                  "unit": "°C",
                  "group": "boiler"
               },
               "protocolOptions": {
                  "slaveId": 1,
                  "functionCode": 3,
                  "byteOrder": "AB",
                  "wordOrder": "AB"
               }
            },
            {
               "pointKey": "pump-running",
               "pointName": "循环泵运行",
               "sourceType": "Coil",
               "address": "00017",
               "rawValueType": "Boolean",
               "length": 1,
               "polling": {
                  "readPeriodMs": 2000,
                  "group": "status"
               },
               "transforms": [],
               "mapping": {
                  "targetType": "Attribute",
                  "targetName": "pumpRunning",
                  "valueType": "Boolean",
                  "displayName": "循环泵运行状态",
                  "unit": null,
                  "group": "pump"
               },
               "protocolOptions": {
                  "slaveId": 1,
                  "functionCode": 1
               }
            },
            {
               "pointKey": "energy-total",
               "pointName": "累计热量",
               "sourceType": "HoldingRegister",
               "address": "40021",
               "rawValueType": "Float32",
               "length": 2,
               "polling": {
                  "readPeriodMs": 10000,
                  "group": "energy"
               },
               "transforms": [
                  {
                     "transformType": "WordSwap",
                     "order": 1,
                     "parameters": {
                        "mode": "CDAB"
                     }
                  },
                  {
                     "transformType": "Expression",
                     "order": 2,
                     "parameters": {
                        "expression": "x * 1000"
                     }
                  }
               ],
               "mapping": {
                  "targetType": "Telemetry",
                  "targetName": "energyTotal",
                  "valueType": "Double",
                  "displayName": "累计热量",
                  "unit": "kJ",
                  "group": "energy"
               },
               "protocolOptions": {
                  "slaveId": 1,
                  "functionCode": 3,
                  "byteOrder": "ABCD",
                  "wordOrder": "CDAB"
               }
            }
         ]
      }
   ],
   "reportPolicy": {
      "defaultTrigger": "OnChange",
      "deadband": 0.1,
      "includeQuality": true,
      "includeTimestamp": true
   }
}
```

### 第二阶段补充 - 属性映射示例

```json
{
   "mapping": {
      "targetType": "Telemetry",
      "targetName": "supplyTemperature",
      "valueType": "Double",
      "displayName": "供水温度",
      "unit": "°C",
      "group": "boiler"
   },
   "productProperty": {
      "key": "supplyTemperature",
      "name": "供水温度",
      "category": "Telemetry",
      "dataType": "Double",
      "unit": "°C",
      "precision": 1
   },
   "runtimeReport": {
      "keyName": "supplyTemperature",
      "value": 68.4,
      "valueType": "Double",
      "quality": "Good",
      "timestamp": "2026-04-19T10:15:23Z"
   }
}
```

对齐要求：
- 后端按 `targetName` 和产品属性模型建立绑定关系。
- 前端设计器在保存前校验 `mapping.valueType` 与 `productProperty.dataType` 是否兼容。
- Gateway 上报时只关心统一 `runtimeReport` 契约，不感知页面内部编辑字段。

---

## SaaS 协作路线图

> 本节描述 IoTSharp（开源主平台）与上层 `IoTSharp.SaaS`（商业叠加层）之间的协作面与稳定性公约。  
> 状态：✅ 已完成 ｜ 🚧 进行中 ｜ ⏳ 计划中 ｜ ⬜ 未开始 ｜ 🔁 持续维护
> 顺序：`[串行]` ｜ `[并行]` ｜ `[依赖: X]`

### S0 协作原则

- 本仓库始终保持**开源属性**，不接收任何商业逻辑（多租户、计费、License、Copilot 编排、付费模板）。
- 所有商业能力位于 `IoTSharp.SaaS` 仓库 `src/IoTSharp.SaaS.*` 系列模块，通过本仓库的公开 API / Webhook / 扩展点叠加。
- 任何破坏性变更需在 SaaS 仓库 issue 中先行公示，至少 6 个月废弃期。

### S1 协作面清单

| 编号 | 状态 | 顺序 | 协作面 | 说明 |
| --- | --- | --- | --- | --- |
| S1-1 | ✅ | 🔁 | 设备 / 遥测 / 属性 REST API | SaaS **只读消费**这些 API 取得脚本生成所需的设备/产品元数据；不在 SaaS 侧落库业务数据 |
| S1-2 | 🚧 | [并行] | OpenTelemetry trace / metrics 输出 | 仅用于 SaaS 端 AI 调用与生成服务的用量计量与审计；不采集业务数据指标 |
| S1-3 | 🚧 | [并行] | Webhooks / 事件总线 | SaaS 仅订阅与"脚本工件部署/运行结果"相关的元数据事件，用于改进生成质量；新增事件保持向后兼容 |
| S1-4 | ⏳ | [依赖: S1-1] | SaaS 消费契约（只读） | SaaS 仅消费本仓库的公开 API 取得脚本生成所需的设备/产品/点位元数据；**不实现数据面网关、不参与设备管理与下发链路** |
| S1-5 | ⏳ | [并行] | Edge / Gateway 元数据引用 | SaaS 在生成脚本时引用 `external/IoTEdge` 的目标元数据（架构/能力/接口）；**注册、心跳、控制下发由本仓库独家承担**，SaaS 不发起 |
| S1-6 | ⏳ | [并行] | 规则引擎扩展点 | 本仓库提供扩展点；SaaS 仅生成扩展实现的源码工件，由用户在本仓库侧装载 |
| S1-7 | ⏳ | [并行] | BASIC 脚本签名格式 | 与 `external/IoTEmbedded` 对齐签名格式；**脚本下发的传输面由本仓库承担**，SaaS 只产出已签名工件 |

### S2 稳定性公约

- 公开 REST API、事件 schema、OpenTelemetry 属性键属于公开契约。
- 内部数据库 schema、内部服务接口不属于公开契约，SaaS 不得直接依赖。
- 与 SaaS 的协作变更通过 PR 描述中的 `[saas-impact]` 标记声明影响面。

### S3 与子模块的接口对齐

- BasicRuntime 接口签名表与 `external/IoTEdge`、`external/IoTEmbedded` 共同维护，双方一致。
- Edge 上报通道 schema 与 `external/IoTEdge` 对齐。
- 详见各子模块仓库内的 `ROADMAP.md`。

### RD-03 ⬜ 第三阶段 - 运营与发布中心
目标：
- ⬜ RD-03.1 增加 OTA 固件包、采集器软件包和配置发布管理
- ⬜ RD-03.2 支持分阶段发布、灰度发布、回滚和结果确认
- ⬜ RD-03.3 将长周期运营编排从现有规则引擎中分离出来

预期产出：
- ⬜ RD-03.4 发布中心领域模型
- ⬜ RD-03.5 包与配置版本管理
- ⬜ RD-03.6 发布任务编排
- ⬜ RD-03.7 交付状态与审计轨迹

### RD-04 🚧 第四阶段 - 实时规则增强
目标：
- ✅️ RD-04.1 保持当前 FlowRule 面向实时处理的方向
- ⬜ RD-04.2 增加更多标准节点和工业转换能力，降低脚本依赖
- ⬜ RD-04.3 提升版本化、可观测性、仿真与可审计能力
- ⬜ RD-04.4 视需要集成 RulesEngine 或 NRules 以支持更丰富的决策逻辑

预期产出：
- ⬜ RD-04.5 更丰富的标准节点库
- ⬜ RD-04.6 更低代码化的转换能力
- ⬜ RD-04.7 更强的规则链版本化与可观测性

### RD-05 🚧 第五阶段 - 组织与生态协同
目标：
- 🚧 RD-05.1 明确组织内部的仓库边界和产品边界
- ✅️ RD-05.2 保持 IoTSharp 作为平台中心，而不是承载所有边缘实现的容器
- ⬜ RD-05.3 协调相关项目之间的发布节奏、API 契约和职责归属

预期产出：
- 🚧 RD-05.4 IoTSharp 及相关仓库的清晰职责图谱
- ⬜ RD-05.5 平台、Gateway、SDK 与代理的兼容矩阵
- 🚧 RD-05.6 面向注册、配置、遥测和发布任务的版本化契约

### RD-06 ⬜ 第六阶段 - 平台高可用与多服务器扩展
目标：
- ⬜ RD-06.1 支持 IoTSharp 控制平面的多服务器部署与横向扩展
- ⬜ RD-06.2 规划关键状态、缓存、任务与消息的外部化，降低单机故障影响
- ⬜ RD-06.3 引入故障转移、主备切换或集群协调机制，保障核心服务连续性
- ⬜ RD-06.4 补齐数据备份、恢复与容灾策略，提升服务器故障场景下的数据安全性

预期产出：
- ⬜ RD-06.5 多节点部署参考架构与基础兼容性约束
- ⬜ RD-06.6 面向会话、任务、消息与配置状态的高可用设计方案
- ⬜ RD-06.7 故障转移与恢复演练流程
- ⬜ RD-06.8 数据备份、恢复与灾备运行手册

### RD-10 🚧 SonnetDB 可选数据底座生产化
> 本节承接原 SonnetDB 仓库中“Milestone 19 - IoTSharp 生态数据底座选项”的 IoTSharp 专属规划。以后 IoTSharp 如何消费 SonnetDB、如何灰度切换、如何验证和回滚，归 IoTSharp 路线图维护；SonnetDB 仓库只维护数据库自身的通用能力和兼容边界。

目标：
- 🚧 RD-10.1 把 SonnetDB 作为 IoTSharp 的显式可选数据底座，而不是替换现有 PostgreSQL/MySQL/SQLServer/SQLite/Oracle/Cassandra/ClickHouse、InfluxDB/TimescaleDB/Taos/IoTDB、Redis/LiteDB/InMemory、BlobStorage/S3 路线。
- ✅ RD-10.2 将 `TelemetryStorage=SonnetDB` 从功能适配推进到生产化适配，覆盖写入、最新值、历史查询、聚合、健康检查、备份恢复和故障回放；定向测试 `SonnetDBStorageTests` 已通过。
- 🚧 RD-10.3 在保持现有每设备 measurement 路线的前提下，已补批量写入和可配置 schema cache 容量控制；后续补连接复用和大量物理分表容量边界。
- 🚧 RD-10.4 已将 latest 查询和时间桶聚合尽量下推到 SonnetDB SQL / ADO.NET 服务端路径，避免先拉全量原始点再在 IoTSharp 进程内聚合；后续补跨库回归基准。
- 🚧 RD-10.5 `DataBase=SonnetDB`、`CachingUseIn=SonnetDB`、`ConnectionStrings:BlobStorage=sonnetdb://...` 已有入口和 Profile 雏形；还需要以兼容矩阵、端到端测试、长稳和回滚报告证明可生产选择。
- ⬜ RD-10.6 暂不采用“共享 measurement + `deviceId` TAG”作为默认优化路线；如未来重议，必须先给出兼容性、迁移、性能基准和用户确认方案。

预期产出：
- 🚧 RD-10.7 IoTSharp SonnetDB Profile：`appsettings.SonnetDB.json`、`docker-compose.sonnetdb.yml`、健康检查、配置说明和一键切回 PostgreSQL/Redis/S3 等既有 Profile 的流程。
- ⬜ RD-10.8 `docs/docs/operations/sonnetdb-compat-matrix.md` 作为 IoTSharp 侧权威兼容矩阵，覆盖关系库、时序库、缓存、对象桶、向量搜索、全文搜索、不支持项、迁移和回滚清单。
- ⬜ RD-10.9 关系库回归：`ApplicationDbContext` schema 创建、迁移升级/回滚、重复迁移幂等、Identity 登录、租户/客户/设备/资产/规则 CRUD、`Include`、分页、常用查询、LIKE 字符串模式和 `SaveChanges` 事务。
- 🚧 RD-10.10 时序回归：latest / aggregate / range query 的服务端下推、批量遥测写入、schema cache 容量控制已完成；后续补连接复用、跨库回归基准和大量 measurement 长稳。
- ⬜ RD-10.11 缓存回归：通过 IoTSharp 现有 EasyCaching 调用路径验证 `Set/Get/Remove/Exists`、批量读写、前缀隔离、TTL、重启恢复、并发读写、配置错误和健康检查。
- ⬜ RD-10.12 对象桶回归：通过 IoTSharp BlobStorage 行为验证 bucket 创建/列举、对象上传/覆盖/下载/删除、content-type、etag、sha256、range read、multipart、presigned URL、metadata/content 一致性。
- ⬜ RD-10.13 迁移、双写与一致性校验：支持关系库、时序库、缓存 key、对象桶 metadata/content 的迁移清单、采样校验、双写窗口、一致性报告和失败回滚；迁移工具可以调用 SonnetDB 提供的通用能力，但 IoTSharp 负责业务语义和验收。
- ⬜ RD-10.14 SonnetDB Profile 长稳、断电恢复、备份恢复、升级回滚和容量边界报告，明确适用规模、边缘部署边界，以及仍建议使用外部 PostgreSQL/Redis/S3 的场景。
- ⬜ RD-10.15 向量搜索与全文搜索只作为后续 IoTSharp 增强能力规划：当前主平台未独立消费这些后端，不得误标为既有能力；若接入，必须通过租户隔离、权限过滤、索引重建和备份恢复回归。
- ⬜ RD-10.16 SonnetDB 外部能力依赖清单：分层文件布局、compaction manifest、增量索引、正则 SQL 函数、对象生命周期/配额等属于 SonnetDB 通用数据库能力；IoTSharp 只记录版本要求、兼容矩阵和验收门槛。
- 🚧 RD-10.17 SonnetMQ EventBus 路径：`EventBus=SonnetMQ`、`EventBusMQ=SonnetMQ`、`IoTSharp.EventBus.SonnetMQ` 已有入口；后续补发布/消费/ack/replay、topic lag、失败重试、指标、配置说明和回滚到 CAP/InMemory 的验证。
- ⬜ RD-10.18 IoTSharp + SonnetDB 联合边缘样例：设备接入、SonnetDB 本地存储、Studio 查看、Copilot 诊断、备份恢复和私有化部署说明归 IoTSharp 维护；SonnetDB 仓库只提供通用数据库、Studio 和 Agent 能力。
- ⬜ RD-10.19 设备域批量删除与重建验收：针对“清空设备数据后让设备重新连接、重新上报”的运维场景，IoTSharp 侧需要验证 SonnetDB 的逻辑删除、后台清理/收缩和整表清空原语。前台操作应快速返回并保证查询可见性，物理空间回收交给 SonnetDB 在机器空闲时渐进执行；必须暴露待清理数据量、清理进度、节流原因和失败告警。

迁入任务拆分：

| 编号 | 状态 | 主题 | IoTSharp 侧验收 |
| --- | --- | --- | --- |
| RD-10.M19-109 | ⬜ | 兼容矩阵与基线套件 | 固定关系、时序、缓存、对象桶、向量搜索、全文搜索的后端清单、验收用例、不支持项和迁移/回滚测试清单。 |
| RD-10.M19-115 | 🚧 | `DataBase=SonnetDB` 与 `ApplicationDbContext` 兼容 | 在 IoTSharp 侧跑通迁移历史表、Identity、主数据 CRUD、常用查询、事务和不支持清单。 |
| RD-10.M19-116 | 🚧 | `CachingUseIn=SonnetDB` 缓存路径 | 通过 EasyCaching provider 验证 TTL、命名空间、批量操作、前缀删除、过期统计和健康检查。 |
| RD-10.M19-117 | 🚧 | `ConnectionStrings:BlobStorage=sonnetdb://...` 对象桶路径 | 验证 BlobStorageController 行为、对象 metadata/content、range、multipart、删除和回滚。 |
| RD-10.M19-119 | 🚧 | IoTSharp SonnetDB Profile | Profile 可显式开启、可并行验证、可停止并切回既有 Profile；默认部署不被强制替换。 |
| RD-10.M19-120 | ⬜ | 迁移、双写与一致性校验 | 形成 IoTSharp 业务语义下的 migrate/verify/rollback 清单和报告格式。 |
| RD-10.M19-121 | ⬜ | 长稳、压测和故障恢复报告 | 覆盖 EF Core CRUD、遥测批量写入、缓存 TTL、对象 multipart、备份恢复、断电恢复和升级回滚。 |
| RD-10.M19-125 | ⬜ | 大量 measurement / 物理分表长稳 | 验证百万级 series、万级 measurement、海量小 segment、随机重启、后台 flush/compaction/retention 并发和恢复时间。 |
| RD-10.M19-126 | ⬜ | SQL 模式匹配验收 | 需要 SonnetDB 提供 `LIKE`/正则相关能力后，IoTSharp 再验证查询翻译、超时、模式长度和 scan filter 边界。 |
| RD-10.M19-126.1 | ⬜ | 设备域批量删除与后台收缩验收 | 验证 `Device`、`DeviceIdentities`、`DataStorage` latest、告警和设备关联数据的大批量清理不会阻塞前台请求；删除采用逻辑标记后由 SonnetDB 根据资源空闲度后台清理和收缩。 |
| RD-10.SMQ-01 | 🚧 | SonnetMQ EventBus Profile | 验证 IoTSharp 事件主题、publish/pull/ack、offset 恢复、失败重试、指标和切回 CAP/InMemory。 |
| RD-10.M27-188 | ⬜ | IoTSharp + SonnetDB 联合边缘样例 | 样例由 IoTSharp 维护，覆盖边缘接入、SonnetDB 存储、诊断、备份恢复和私有化部署。 |

推进顺序：

```text
RD-10.M19-109（兼容矩阵）
  -> RD-10.M19-115（ApplicationDbContext 兼容）
  -> RD-10.M19-116（缓存 Provider）
  -> RD-10.M19-117（对象桶 Provider）
  -> RD-10.M19-119（Profile 与回滚入口）
  -> RD-10.M19-120（迁移 / 双写 / 回滚）
  -> RD-10.M19-121（长稳 / 压测 / 报告）
  -> RD-10.M19-125（大量 measurement 长稳）
  -> RD-10.M19-126（SQL 模式匹配验收）
  -> RD-10.M19-126.1（设备域批量删除 / 后台收缩验收）
```

验收原则：
- SonnetDB 接入必须显式可选、可灰度、可双写、可校验、可回滚；不能要求用户一次性不可逆迁移，也不能移除既有数据库选择。
- 不把 SonnetDB 普通 KV keyspace 直接冒充 Redis；必须通过 IoTSharp 缓存路径验证 TTL、过期清理、并发语义和 provider 行为。
- 不把对象桶能力直接等同外部 S3；IoTSharp 以 `IBlobStorage` 行为、迁移一致性和回滚能力作为验收边界。
- 不把 IoTSharp 每设备 measurement 默认改为共享 measurement + `deviceId` TAG；SonnetDB 优化应优先兼容现有物理分表/多 measurement 模式。
- 设备清空、租户迁移、设备重建这类大范围删除不能依赖慢 SQL 逐行删除；IoTSharp 只发起受审计的领域操作，数据库侧用逻辑删除和后台清理/收缩保证前台可用性与最终空间回收。
- 向量搜索、全文搜索和 AI 检索只能作为 IoTSharp 后续增强能力，接入前必须经过权限、租户隔离、审计和索引生命周期评审。

## 项目关系策略

### IoTSharp
平台中心。

负责：
- ✅️ RD-00.3 租户与设备平台能力
- ✅️ RD-00.4 产品与资产
- ✅️ RD-04.1 规则链
- 🚧 RD-01.1 边缘管理控制平面
- ⬜ RD-03.1 发布中心
- ✅️ RD-00.5 API、UI、权限、审计与生命周期管理
- 🚧 RD-10.1 SonnetDB 已可作为显式选择的时序数据底座；既有 PostgreSQL/MySQL/Redis/S3 等后端和完整回滚 Profile 仍需保留并补齐验证

### Gateway
协议与南向采集运行时。

角色：
- 🚧 RD-02.4 执行采集任务
- 🚧 RD-02.2 适配工业协议
- 🚧 RD-01.4 向 IoTSharp 上报运行状态和采集结果
- 🚧 RD-02.8 接收 IoTSharp 下发的配置与软件任务

规则：
- ✅️ RD-05.8 Gateway 应通过稳定契约集成，而不是把平台逻辑重新嵌入采集器本体。

### 组织内其他项目
相关仓库应通过清晰职责和显式契约协同。

规则：
- ✅️ RD-05.10 平台能力留在 IoTSharp
- ✅️ RD-05.11 协议运行时能力留在 Gateway 或其他采集器
- ✅️ RD-05.12 轻量边缘运行时能力留在 IoTEmbedded 或其他采集器
- ⬜ RD-05.13 SDK 与代理项目遵循平台契约和兼容矩阵
- ⬜ RD-05.14 跨项目变更应通过版本化接口驱动，而不是依赖未文档化耦合
- ⬜ RD-10.4 SonnetDB 相关接入必须以 IoTSharp 兼容矩阵、SonnetDB 版本要求、文件布局、compaction 恢复和长稳报告作为显式验收边界

## 执行原则

- ✅️ RD-00.1 不对现有平台做整体重写。
- ✅️ RD-00.6 保留当前设备接入、产品映射、资产层级和规则链投入。
- 🚧 RD-01/RD-02/RD-03 按层次新增领域：边缘管理、采集建模、发布编排。
- ⬜ RD-04.6 将高频脚本模式沉淀为标准节点或标准转换器。
- ⬜ RD-03.6 在需要时为长周期交付流程引入专用工作流或编排工具。
- ⬜ RD-10.5 SonnetDB 时序适配优化不得默认改变现有 measurement 归属模型；任何 schema 路线调整必须单独评审。

## 当前立即下一步

从 **RD-01：边缘与采集管理基础** 开始。

首轮实施应定义：
- 🚧 RD-01.5 边缘节点领域模型
- 🚧 RD-01.3 Gateway 注册契约
- ✅️ RD-01.7 心跳与能力上报契约
- 🚧 RD-01.4 运行时状态与诊断展示面
- 🚧 RD-05.5 相关仓库之间的兼容性与职责规则
- 🚧 RD-10.6 SonnetDB 时序适配层已完成 latest / aggregate 下推、批量写入和 schema cache 容量控制；长稳验证、连接复用和跨库回归基准继续保留在 J11/RD-10 后续项。

## 专项评估：前端通过 MQTT 获取最新遥测

当前实现状态：
- ✅️ RD-09.1 设备侧 MQTT 遥测上报入口已存在，平台可以通过 `TelemetryController` / `GatewayController` 接收设备与网关上报。
- ✅️ RD-09.2 后端已提供 `/api/Devices/{deviceId}/TelemetryLatest` 及按 key 查询接口，支持读取设备最新遥测。
- ✅️ RD-09.3 当前前端设备详情页中的“遥测”区域，实际通过 `ClientApp/src/api/devices/index.ts` 的 HTTP 接口拉取最新遥测。
- ⬜ RD-09.4 当前前端代码中还没有浏览器侧 MQTT 订阅实现，也没有发现“前端直接通过 MQTT 刷新最新遥测”的现成链路。

纳入路线图的下一步：
- ⬜ RD-09.5 明确前端实时遥测订阅方案：浏览器直连 MQTT，或由平台提供 WebSocket / SSE / SignalR 转发层，避免把设备接入鉴权直接暴露给前端。
- ⬜ RD-09.6 为设备详情页的最新遥测视图补齐订阅式增量刷新能力，并保留现有 HTTP 最新值查询作为初始化与降级路径。
- ⬜ RD-09.7 统一前端实时订阅的主题、租户隔离、设备级权限控制与断线重连策略。
- ⬜ RD-09.8 为“最新遥测”与“历史遥测”定义清晰边界：MQTT/订阅链路负责推送最新值，历史查询继续走时序存储与 HTTP API。
