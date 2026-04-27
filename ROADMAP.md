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
| RD-05 | 🚧 | 组织与生态协同 | IoTSharp / Gateway / PiXiu 的职责边界已有方向，兼容矩阵、版本化契约和跨仓库节奏仍需落地。 |
| RD-06 | ⬜ | 平台高可用与多服务器扩展 | 横向扩展、状态外部化、故障转移、备份恢复和灾备演练尚未开始。 |
| RD-07 | 🚧 | AI 工作台与智能体横切能力 | MCP 入口和 AISettings 已具备，模型抽象、技能目录、Workbench、审计与成本视图仍需建设。 |
| RD-08 | ⬜ | 本地语音、治理与多智能体协作 | 语音 companion、本地模型协同、记忆、知识源、审批和自动运维边界尚未开始。 |
| RD-09 | 🚧 | 前端实时遥测订阅 | 后端最新值与设备详情 HTTP 查询已具备，浏览器侧订阅链路、权限和断线重连策略仍需补齐。 |

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
- ✅️ RD-05.7 保持 Gateway / PiXiu / 本地语音 / 本地模型运行时作为清晰边界内的独立部署能力。
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
- 与 Gateway、PiXiu 及未来采集代理的关系

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
- 🚧 RD-01.3 通过统一的平台契约管理 C# Gateway、PiXiu 客户端和未来代理
- 🚧 RD-01.4 暴露版本、能力、健康度、状态和诊断元数据

预期产出：
- 🚧 RD-01.5 边缘节点领域模型
- ✅️ RD-01.6 边缘注册协议
- ✅️ RD-01.7 心跳与能力上报机制
- 🚧 RD-01.8 统一的管理界面入口
- 🚧 RD-01.9 Gateway 与 PiXiu 集成的项目边界约定

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
- 这部分应优先沉淀到平台核心领域与 `IoTSharp.Contracts` 中，作为 IoTSharp、Gateway、PiXiu 和未来代理共享的版本化契约。

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
- DTO 使用 extend-only 思路演进，避免频繁破坏 Gateway/PiXiu 兼容性。
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
- Gateway/PiXiu 上报时只关心统一 `runtimeReport` 契约，不感知页面内部编辑字段。

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
- ⬜ RD-05.5 平台、Gateway、PiXiu、SDK 与代理的兼容矩阵
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

### Gateway
协议与南向采集运行时。

角色：
- 🚧 RD-02.4 执行采集任务
- 🚧 RD-02.2 适配工业协议
- 🚧 RD-01.4 向 IoTSharp 上报运行状态和采集结果
- 🚧 RD-02.8 接收 IoTSharp 下发的配置与软件任务

规则：
- ✅️ RD-05.8 Gateway 应通过稳定契约集成，而不是把平台逻辑重新嵌入采集器本体。

### PiXiu
轻量级边缘与现场侧运行时。

角色：
- 🚧 RD-01.3 在需要轻量部署或现场自治时作为边缘代理运行
- 🚧 RD-01.4 处理本地执行、缓冲、诊断和未来边缘侧协同
- 🚧 RD-01.3 通过统一边缘契约与 IoTSharp 同步

规则：
- ✅️ RD-05.9 PiXiu 应作为与 Gateway 同一边缘管理域下的受管边缘运行时，同时保留独立部署和生命周期。

### 组织内其他项目
相关仓库应通过清晰职责和显式契约协同。

规则：
- ✅️ RD-05.10 平台能力留在 IoTSharp
- ✅️ RD-05.11 协议运行时能力留在 Gateway 或其他采集器
- ✅️ RD-05.12 轻量边缘运行时能力留在 PiXiu
- ⬜ RD-05.13 SDK 与代理项目遵循平台契约和兼容矩阵
- ⬜ RD-05.14 跨项目变更应通过版本化接口驱动，而不是依赖未文档化耦合

## 执行原则

- ✅️ RD-00.1 不对现有平台做整体重写。
- ✅️ RD-00.6 保留当前设备接入、产品映射、资产层级和规则链投入。
- 🚧 RD-01/RD-02/RD-03 按层次新增领域：边缘管理、采集建模、发布编排。
- ⬜ RD-04.6 将高频脚本模式沉淀为标准节点或标准转换器。
- ⬜ RD-03.6 在需要时为长周期交付流程引入专用工作流或编排工具。

## 当前立即下一步

从 **RD-01：边缘与采集管理基础** 开始。

首轮实施应定义：
- 🚧 RD-01.5 边缘节点领域模型
- 🚧 RD-01.3 Gateway 与 PiXiu 注册契约
- ✅️ RD-01.7 心跳与能力上报契约
- 🚧 RD-01.4 运行时状态与诊断展示面
- 🚧 RD-05.5 相关仓库之间的兼容性与职责规则

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
