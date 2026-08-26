# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

IoTSharp 面向负责物联网接入、设备与资产管理、边缘运行时、采集配置、实时规则、发布和平台治理的运维人员、实施人员与平台管理员。用户通常在桌面工作台中连续工作，也需要在较窄屏幕上完成查询、确认和应急操作。

## Product Purpose

IoTSharp 提供统一的物联网控制面，把接入与采集、实时规则、运维与发布三个层次组织在同一套租户权限、审计、可观测性和领域服务之上。成功意味着用户能准确识别当前业务对象、运行状态、版本和任务闭环，并从问题定位到受控操作保持上下文连续。

## Positioning

IoTSharp 的核心机制是以 Product、Device、Asset、Gateway、EdgeNode 和 Collection Template 的明确边界组织平台能力，同时把实时规则处理与发布、回滚等长时间操作分离。AI 能力复用同一控制面、权限和审计，通过授权技能与 MCP 暴露能力，不直接绕过领域服务访问数据。

## Operating Context

- 用户围绕 Product 能力模板、Device 运行实例和 Asset 业务对象建立生产模型。
- Gateway 负责南向协议与子设备接入，EdgeNode 负责边缘运行时生命周期和任务闭环。
- Collection Template 定义协议、连接、点位、转换、映射和采样行为，并以不可变配置版本交付到运行时。
- Release Center 负责软件、固件和配置的灰度发布、确认与回滚。
- RuleChain 面向实时处理，不承担发布任务或其他长时间工作流。

## Capabilities and Constraints

- 保留现有 Vue 3、Vite、Element Plus、Fast Crud、图形设计器和可视化能力，不做全量重写。
- 保留现有路由、权限、API 契约、URL 和核心工作流，界面重构不得破坏租户隔离和审计。
- 当前优先级是生产业务建模与真实运行闭环，不以演示数据或本地健康状态替代生产证据。
- Product 是能力模板，Device 是运行实例，Asset 是业务对象。界面语言不得混用这些概念。
- NSwag.AspNetCore、RulesEngine 和 Jint 是保留组件，界面重构不得弱化相关能力。
- 生产 HTTP 入口目标端口为 80；应用内部端口可保持独立，由部署映射提供稳定入口。

## Brand Commitments

- 产品名称和现有 IoTSharp 标志保持不变。
- 新界面以 XJETC Ops 控制台为设计参考，采用 12 套可切换主题。每套主题由一个深色锚点和一个浅色陪衬组成，共 24 个核心颜色。
- 主题选择器显示两枚配色样本，控件名称统一为“主题”。
- 允许使用接近 Microsoft 365 和 Windows 11 的克制渐变、毛玻璃与亚克力材质，但数据表格、表单、状态和长时间阅读区域保持实色与高对比度。

## Evidence on Hand

- 领域边界、实施顺序和生产闭环要求记录在 `AGENTS.md`。
- 当前前端实现位于 `ClientApp`，包含入口、登录、仪表盘、Product、Device、Asset、Gateway、EdgeNode、规则链和系统管理页面。
- XJETC 主题目录位于 `D:\XJETC\skills\theme-system\theme-catalog.json`，实现参考位于 `D:\XJETC\xjetc-ops\src\Xjetc.Ops.Web`。
- 当前生产实例 `10.165.83.194` 的 HTTP 服务位于 2927 端口，80 端口尚未开放。
- 当前没有可用于界面展示的已批准客户、性能指标或生产闭环证明，界面不得虚构这些内容。

## Product Principles

1. 业务对象、运行状态和操作结果优先于装饰。
2. 导航围绕接入采集、实时规则、运维发布和平台治理组织。
3. 主题改变品牌表达，不改变状态语义和数据可读性。
4. 高风险操作必须保留权限、确认、审计和回滚上下文。
5. 界面展示真实能力与真实状态，不用模板内容制造完成感。

## Accessibility & Inclusion

界面需要支持键盘导航、清晰焦点、可辨识状态、足够对比度、减少动态效果偏好和桌面/移动端响应式布局。颜色不得成为传达成功、告警、失败、离线或审批状态的唯一方式。
