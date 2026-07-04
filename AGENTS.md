# AGENTS.md

This file records the current execution plan for contributors and automation working in this repository.

## Current Mission

IoTSharp is moving from a device platform to a platform with three coordinated layers:

1. access and collection
2. real-time rules
3. operations and release

All planning and implementation should support that direction.

## Current Priority Order

1. Complete domain concept realignment before broad feature expansion
2. Build the edge and collection management foundation
3. Standardize collection modeling
4. Introduce release center for OTA, software updates, and configuration rollout
5. Upgrade rule-chain usability and observability without turning it into a long-running workflow engine
6. Align repository boundaries and contracts across organization projects

## Current Concept Realignment

The next wave of work must first make the domain language consistent.

- Product is the capability template.
- Device is the runtime instance.
- Asset is the managed business object.
- Gateway is the southbound protocol and child-device access runtime.
- EdgeNode is the managed edge runtime responsible for registration, heartbeat, capability reporting, task receipt and diagnostics.
- Collection templates define protocol, connection, point, transform, mapping and sampling behavior.
- Release tasks, configuration rollout, OTA and long-running operations must not be hidden inside the real-time rule-chain model.

Existing `Produce` code and API surfaces are compatibility implementation details. New documentation, UI text, DTOs and contracts should use Product language unless preserving an existing public API.

## AI Workbench Direction

AI is a cross-cutting capability for the three existing layers, not a separate rewrite track.

- Build AI on top of the IoTSharp control plane, permissions, audit, UI, and domain services.
- Prefer the latest Microsoft Agent Framework for agent orchestration.
- Prefer Microsoft.Extensions.AI for model/provider abstraction.
- Keep MCP as the primary tool boundary for exposing IoTSharp capabilities and integrating external tools.
- Treat Camel.NET integration as capability absorption for multi-model access, tool calling, memory, and multi-agent collaboration.
- Keep local model runtimes, speech services, and companion/desktop experiences deployable outside the main web host.

## Domain Boundary Rules

### IoTSharp
Owns:
- platform control plane
- tenants, products, assets, devices
- rule-chain runtime for real-time processing
- edge node management
- release center and lifecycle management
- APIs, UI, permissions, observability, and audit

### Gateway
Owns:
- southbound protocol execution
- collection runtime behavior
- protocol adapters and industrial connectivity
- reporting collection state and results back to IoTSharp

Gateway must integrate with IoTSharp through stable contracts for:
- registration
- heartbeat
- capability report
- configuration pull or push
- software and task delivery feedback

### Other organization projects
Rules:
- do not move unrelated responsibilities into IoTSharp
- do not couple projects through hidden internal assumptions
- introduce versioned contracts before introducing cross-repository features
- maintain compatibility matrices for platform, Gateway, SDKs, and related agents

## Core Domain Concept Definitions

IoTSharp uses Product, Device, Asset, Gateway, EdgeNode and Collection Template as separate core concepts. Contributors and automation must preserve these boundaries when designing models, APIs, UI, rules, release flows, and AI workbench capabilities.

### Product

Product defines the type and capability model for a class of devices or collection objects.

It should be used for:
- protocol and connection templates
- telemetry, attribute, command, event, and configuration definitions
- point templates, transform hints, and sample policies
- default credential policy, default rule chain, alarm templates, software and firmware compatibility
- model-level defaults shared by many device instances

Product should not represent a physical installation, business site, production line, runtime connection, device state, telemetry latest value, alarm instance, command execution record, or individual runtime instance.

### Device

Device represents a real access instance connected to IoTSharp or reported through Gateway.

It should be used for:
- identity, credentials, and access lifecycle
- connectivity, heartbeat, runtime status, and telemetry
- attributes, commands, events, and real-time rule inputs
- binding to a Product for capability interpretation
- binding to an Asset when the device serves a business object
- binding to a Gateway or EdgeNode when the device is collected through a managed runtime

Device should not be used as a broad business hierarchy, site tree, or release scope substitute when the real target is an Asset or EdgeNode.

### Asset

Asset represents a managed business object in the user's operational world.

It should be used for:
- sites, parks, buildings, floors, rooms, workshops, production lines, stations, facilities, equipment systems, and other field objects
- business ownership, spatial hierarchy, operational responsibility, alarm context, dashboard context, and permission scope
- linking one or more Devices to the business object they observe or control
- addressing configuration, release, diagnosis, and operational views by business scope when appropriate

Asset is not a synonym for Device, Product, or DeviceGroup. Asset also does not mean static project resources such as images, fonts, CSS, or bundled frontend files; those resources should use names such as `static`, `public`, `media`, `wwwroot`, or framework-specific asset folders outside the domain model.

### Gateway

Gateway is a special Device that owns southbound protocol access and child-device reporting.

It should be used for:
- protocol adapter execution
- child device discovery, creation and connectivity reporting
- uploading child-device telemetry, attributes, events and alarms
- receiving collection configuration, software tasks and diagnostics tasks
- reporting runtime capability, task receipt, task result and errors

Gateway should not store platform-side Product templates, bypass tenant permission checks, or depend on hidden IoTSharp database assumptions.

### EdgeNode

EdgeNode is a managed edge runtime instance.

It should be used for:
- registration, heartbeat, version, instance identity and health state
- capability reporting and runtime diagnostics
- receiving configuration, software, firmware and diagnostic tasks
- reporting task receipt, execution, completion, failure, timeout and rollback result

Gateway and EdgeNode may be deployed together, but Gateway describes protocol and child-device access while EdgeNode describes managed runtime lifecycle and task closure.

### Collection Template

Collection Template is the platform-side definition of collection behavior.

It should be used for:
- protocol templates
- connection templates
- point templates
- transform chains
- sampling policies
- mapping policies to telemetry, attributes, alarms or command feedback

Collection Template should belong to Product or a reusable template library, while runtime task delivery should target EdgeNode, Gateway or selected Devices.

## Planning Rules

When adding or reviewing work, prioritize the following outcomes:

### Phase 0: domain concept realignment
Deliver:
- Product, Device, Asset, Gateway, EdgeNode and Collection Template boundaries
- Product language for new UI, DTOs and docs while preserving old `Produce` compatibility
- Device workspace focused on runtime state, credentials, telemetry, commands, alarms and relations
- Product workspace focused on capability templates, point definitions, credential policy and default behavior

### Phase 1: edge foundation
Deliver:
- edge node model
- registration and heartbeat flow
- capability and status reporting
- unified management surface for Gateway
- AI-assisted edge diagnostics and operator guidance should plug into this phase through read-only skills first

### Phase 2: collection modeling
Deliver:
- protocol templates
- connection templates
- point templates
- sample policies and transform chains
- runtime persistence linked to designer configuration
- AI should assist with mapping suggestions, protocol configuration hints, and transform generation without bypassing review

### Phase 3: release center
Deliver:
- OTA package model
- collector software package model
- configuration template and version model
- rollout, gray release, rollback, and confirmation flow
- AI should help with release grouping, gray rollout planning, rollback suggestions, and operational Q&A

### Phase 4: rule-chain enhancement
Deliver:
- more standard nodes
- less script dependency
- better versioning, simulation, observability, and audit
- AI should explain rules, generate safe drafts, and support diagnostics without turning rule chains into long-running workflow engines

## Implementation Constraints

- Do not propose a full rewrite.
- Preserve existing access, product, asset, and FlowRule investments.
- Use layered upgrades.
- Do not mix Product template responsibilities into Device runtime code.
- Do not use Device as a business hierarchy, release scope substitute, or asset tree.
- Do not hide Gateway or EdgeNode contracts behind internal database assumptions.
- Keep real-time processing and long-running operations separated.
- Prefer explicit domain models over script-only behavior.
- Expose IoTSharp capabilities to AI through authorized skills and MCP tools, not direct database access.
- Require tenant isolation, audit trails, and human confirmation for higher-risk AI actions.

## Dependency and Component Removal Rules

- 清理某个依赖时，目标首先是 IoTSharp 自有项目中的直接引用：显式 `PackageReference`、`using`、类型/方法调用、序列化配置、生成代码配置和自有扩展方法。
- 第三方包的传递依赖不能自动等同于 IoTSharp 直接依赖；不能仅因为第三方内部使用某个库，就删除 IoTSharp 的数据库 provider、协议适配、规则链能力、API 文档/UI、SDK 生成、网关接入或运维发布能力。
- 如果必须替换一个第三方组件，应先证明替代方案覆盖原有入口、配置项、公开契约、运行时行为和回滚路径；无法完全覆盖时，要保留兼容层或明确列出兼容差异。
- 删除组件前必须单独说明影响面：用户可见能力、配置名称、数据库/迁移、API/SDK、文档、测试、部署脚本和跨项目契约；没有明确批准或废弃证据时，不做大范围删除。
- 依赖清理应尽量采用分层替换、适配器、功能开关或可选包隔离；不要把“去掉某个库”扩大成动摇平台根能力的重构。
- `NSwag.AspNetCore`、`RulesEngine`、`Jint` 是当前保留的基础组件；没有明确批准和等价替代验证时，不得删除、替换或弱化相关能力。
- 安全、许可证或供应链风险确需移除传递依赖时，应拆成独立变更，给出风险说明、迁移方案和验证清单，避免和无关功能调整混在一次提交中。

## Code Documentation Rules

- IoTSharp 代码注释优先使用中文；只有外部协议、标准术语、API 名称或引用资料需要保留英文时才使用英文。
- 新增或修改方法时，尽量补充 XML 文档注释，尤其是 public、internal、扩展方法和跨模块调用的方法。
- 方法注释要说明功能目的、关键参数、返回值，以及不容易从签名看出的副作用或兼容行为。
- 注释要简洁、语义清楚；不要重复方法名，也不要解释显而易见的赋值或语法。
- 只有在控制流、转换规则、兼容逻辑或领域决策不直观时，才添加简短行内注释。
- 不要添加只是复述代码的噪音注释。

## Immediate Execution Focus

The next implementation step starts with edge management.

Any first-wave design or code change should help define:
- how Gateway registers
- how IoTSharp tracks edge runtime state
- how configuration and release tasks are addressed to an edge runtime
- how cross-project responsibilities are kept clear inside the organization
- how AI workbench capabilities can consume these contracts through governed skills instead of hidden coupling
