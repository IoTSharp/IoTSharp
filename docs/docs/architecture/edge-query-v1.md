# Edge 查询 V1 范围冻结

## 目标范围

第一版只覆盖以下内容：

- EdgeNode 管理页
- 列表查询、筛选、排序、分页
- EdgeNode 详情展示
- Gateway 注册协议正式说明
- 配置下发 / 任务寻址模型草案
- 为执行端对接预留稳定 API 契约与状态机边界

第一版明确不包含：

- 配置下发实际执行
- 任务调度中心
- 远程升级执行
- 运行时写操作界面
- 指标聚合分析

## EdgeNode 查询模型

### 列表页字段

- Name
- RuntimeType
- RuntimeName
- Version
- Status
- Healthy
- Active
- LastHeartbeatDateTime
- LastActivityDateTime
- HostName
- IpAddress

### 列表筛选项

- 名称 name
- runtimeType
- status
- healthy
- active
- version
- platform

### 列表排序

第一版支持基础排序，字段范围：

- Name
- RuntimeType
- RuntimeName
- Version
- Status
- Healthy
- Active
- LastHeartbeatDateTime
- LastActivityDateTime
- HostName
- IpAddress

默认排序：

- LastHeartbeatDateTime desc

## EdgeRuntimeStatus 正式模型（#020）

`EdgeRuntimeStatus` 是 EdgeNode 的运行态快照模型，用于承载注册、心跳、版本、实例、主机、健康和低频指标。它不替代 EdgeCapability、EdgeTask 或 ReleaseTask，也不承载采集模板和长周期发布状态。

### 契约版本

- 当前状态快照版本：`edge-runtime-status-v1`
- DTO 名称：`EdgeRuntimeStatusDto`
- 所属包：`IoTSharp.Contracts`

### 只读接口

- `GET /api/Edge/{id}/RuntimeStatus`

`id` 使用平台侧 EdgeNode ID；当前第一版与承载接入凭证的 Gateway 设备 ID 保持一致。接口只允许已授权的普通用户读取，仍受租户和客户边界限制。

### 字段范围

- edgeNodeId
- gatewayId
- active
- lastActivityDateTime
- runtimeType
- runtimeName
- version
- instanceId
- platform
- hostName
- ipAddress
- status
- healthy
- uptimeSeconds
- lastHeartbeatDateTime
- lastRegistrationDateTime
- updatedAt
- metadata
- metrics

### 边界约束

- `metadata` 只保存非敏感部署上下文，不保存密钥和大体积日志。
- `metrics` 只保存心跳携带的低频运行指标，不作为时序指标聚合入口。
- 能力声明进入 #021 `EdgeCapability`，不塞入运行状态模型。
- 配置、软件、诊断和 OTA 等长周期执行结果进入 EdgeTask/ReleaseTask，不藏在运行状态模型里。
- EdgeNode 列表和详情短期保留既有扁平字段；新增消费者应优先读取 `runtimeStatus`。

## EdgeCapability 正式模型（#021）

`EdgeCapability` 是 EdgeNode 的能力快照模型，用于承载执行端支持的协议、点位类型、转换、上报触发、任务能力和合同版本兼容范围。它不保存运行健康状态，不承载采集模板实例，也不表达任务执行历史。

### 契约版本

- 当前能力快照版本：`edge-capability-v1`
- DTO 名称：`EdgeCapabilityDto`
- 所属包：`IoTSharp.Contracts`

### 上报与只读接口

- `POST /api/Edge/{access_token}/Capabilities`
- `GET /api/Edge/{id}/Capability`

`POST` 兼容旧执行端继续使用 `edge-v1` 或省略 `contractVersion`，平台会归一化为 `edge-capability-v1` 快照。`GET` 使用平台侧 EdgeNode ID；当前第一版与承载接入凭证的 Gateway 设备 ID 保持一致。

### 字段范围

- edgeNodeId
- gatewayId
- runtimeType
- runtimeName
- version
- instanceId
- reportedAt
- updatedAt
- protocols
- supportedProtocols
- supportedPointTypes
- supportedTransforms
- supportedReportTriggers
- features
- tasks
- taskCapabilities
- compatibleContracts
- metadata

### 任务能力

`taskCapabilities` 用于描述执行端对任务类型的结构化支持：

- taskType
- contractVersion
- supportsProgress
- supportsCancellation
- metadata

`tasks` 保留字符串数组形式，便于旧执行端和未来任务类型先以能力标识透传；正式执行语义仍以 EdgeTask 合同和状态机为准。

### 版本兼容

`compatibleContracts` 用于声明执行端兼容的云边合同：

- contractName，例如 `edge-runtime`、`edge-capability`、`collection-config`、`edge-task`
- contractVersion
- minPlatformVersion
- maxPlatformVersion
- deprecated
- metadata

平台在旧执行端未显式上报时会自动补齐当前已知合同版本，但该默认值只表示平台侧归一化能力，不替代执行端真实兼容矩阵。

### 边界约束

- `protocols` 保留执行端原始协议标识，`supportedProtocols` 用于与采集模板做静态匹配。
- `supportedPointTypes` 描述点位源类型，不替代 Product 的点位模板定义。
- `supportedTransforms` 和 `supportedReportTriggers` 描述执行端可执行能力，不保存具体采集任务配置。
- `taskCapabilities` 只声明可接收的任务类型和回执行为，不保存任务状态和执行结果。
- `metadata` 只保存非敏感能力上下文，不保存密钥、日志或高频指标。
- EdgeNode 列表和详情短期保留既有 `capabilities` JSON 字符串；新增消费者应优先读取 `capability` 或只读接口。

## EdgeCollectionAssignment 正式模型（#024）

`EdgeCollectionAssignment` 是平台侧采集配置分配快照，用于记录某个 `collection-config-v1` 配置版本当前分配给哪个 EdgeNode、Gateway runtime 或设备范围。它不替代 M3 的 Collection Template，也不承载执行端回执、长周期发布状态或规则链执行状态。

### 契约版本

- 当前配置分配快照版本：`collection-config-v1`
- DTO 名称：`EdgeCollectionAssignmentDto`
- 所属包：`IoTSharp.Contracts`

### 只读接口

- `GET /api/Edge/CollectionAssignments`
- `GET /api/Edge/{id}/CollectionAssignments`

`id` 使用平台侧 EdgeNode ID；当前第一版与承载接入凭证的 Gateway 设备 ID 保持一致。接口只允许已授权的普通用户读取，仍受租户和客户边界限制。

### 字段范围

- id
- contractVersion
- targetType
- gatewayId
- edgeNodeId
- targetKey
- runtimeType
- instanceId
- configurationVersion
- configurationHash
- taskCount
- status
- sourceType
- sourceId
- sourceVersion
- assignedAt
- lastPulledAt
- revokedAt
- createdAt
- updatedAt
- createdBy
- updatedBy
- metadata

### 状态枚举

- Pending = 0
- Active = 1
- Superseded = 2
- Revoked = 3

### 写入规则

- `PUT /api/Edge/{id}/CollectionConfig` 保存新配置版本时，同步创建一条 Active assignment。
- 同一 Gateway 旧的 Active assignment 会转为 Superseded。
- `GET /api/Edge/{access_token}/CollectionConfig` 被执行端拉取时，平台更新当前 Active assignment 的 `lastPulledAt`。
- `configurationHash` 根据规范化后的配置 JSON 计算，用于平台和执行端核对配置一致性。

### 边界约束

- assignment 只保存版本、目标、哈希、任务数量和状态，不保存完整采集配置大对象。
- 完整运行时配置正文由 `CollectionConfigurationVersion` 保存为平台侧版本快照；执行端仍通过 `EdgeCollectionConfigurationDto` 和 `collection-config-v1` 拉取当前配置。
- M3 的 Product Collection Template 落地前，`sourceType` 使用 `InlineCollectionConfig`，`sourceId` 可以为空。
- 执行成功、失败、超时和回滚结果归 EdgeTask/EdgeTaskReceipt 或后续 ReleaseTask，不写入 assignment。

## CollectionConfigurationVersion 正式模型（#040）

`CollectionConfigurationVersion` 是平台侧采集配置版本快照。它保存规范化后的 `collection-config-v1` 正文、Gateway 维度递增版本号、配置哈希、任务数量和 Product Collection Template 来源信息。该模型不替代 Product 侧 Collection Template，也不承担发布任务执行状态；目标分配仍由 `EdgeCollectionAssignment` 表示，执行回执归 EdgeTask/EdgeTaskReceipt 或后续 ReleaseTask。

### 关键字段

- id
- contractVersion
- gatewayId
- edgeNodeId
- version
- configurationHash
- taskCount
- sourceType
- sourceId
- sourceVersion
- sourceMetadata
- payload
- createdAt
- updatedAt
- createdBy
- updatedBy
- tenantId
- customerId

### 查询接口

- `GET /api/Edge/CollectionConfigVersions`：按租户/客户边界查询配置版本，可按 gateway、edgeNode、版本、哈希和来源过滤。
- `GET /api/Edge/{id}/CollectionConfigVersions`：查询某个 Gateway/EdgeNode 的配置版本历史。
- `GET /api/Edge/{id}/CollectionConfigVersions/{version}`：读取指定版本详情，并返回完整 `collection-config-v1` 正文。

### 写入规则

- `PUT /api/Edge/{id}/CollectionConfig` 会先生成 `CollectionConfigurationVersion`，再创建引用该快照的 Active assignment。
- 版本号仍按 Gateway 维度递增；旧 AttributeLatest 中的配置视图仅作为兼容读取路径。
- `configurationHash` 以规范化后的配置正文计算，assignment 和版本快照使用同一个哈希，便于发布和执行端核对。
- SQLite 迁移只落版本引用列和索引，不对已有 assignment 表追加外键约束；其他关系型 provider 创建外键。

### 查询返回结构

统一返回现有前端分页表格格式：

```json
{
  "code": 0,
  "msg": "OK",
  "data": {
    "total": 120,
    "rows": []
  }
}
```

## Edge 注册协议正式说明

### 协议版本

- 当前协议版本号：edge-v1
- 第一版要求 Gateway 按 edge-v1 执行注册与心跳

### 注册请求字段

注册接口：

- `POST /api/Edge/{access_token}/Register`

字段说明：

- RuntimeType: 必填
- RuntimeName: 可选，未传时默认使用 gateway name
- Version: 可选
- InstanceId: 可选，但正式接入时应提供
- Platform: 可选
- HostName: 可选
- IpAddress: 可选
- Metadata: 可选

### runtimeType 约束

第一版建议约束值：

- gateway

后续新增 runtimeType 只能扩展，不能重定义已有语义。

### instanceId 规则

- instanceId 表示运行时实例标识，不等于设备 ID
- 同一 access_token 下，instanceId 应尽量稳定
- 推荐由运行时安装实例或宿主机唯一标识派生
- 不允许使用高频变化值作为 instanceId

### 注册幂等与重复注册覆盖

- Register 接口按 access_token 对应的网关设备做幂等更新
- 同一 access_token 重复注册时，允许覆盖以下字段：
  - RuntimeType
  - RuntimeName
  - Version
  - InstanceId
  - Platform
  - HostName
  - IpAddress
  - Metadata
- 第一版不保留多实例并存模型，一个 access_token 当前只映射一个 EdgeNode 运行时视图

### 心跳超时

- 心跳接口：`POST /api/Edge/{access_token}/Heartbeat`
- 心跳超时沿用设备 Timeout 配置
- 若最近活动超过 Timeout，运行时可被视为非活跃
- 第一版只做查询展示，不在 EdgeNode 页面内引入复杂状态推导器

### capabilities 结构

Capabilities 接口：

- `POST /api/Edge/{access_token}/Capabilities`

第一版兼容结构：

```json
{
  "contractVersion": "edge-capability-v1",
  "protocols": ["modbus-tcp", "opcua"],
  "supportedProtocols": ["Modbus", "OpcUa"],
  "supportedPointTypes": ["coil", "holding-register", "opcua-node"],
  "supportedTransforms": ["Scale", "Offset", "Expression"],
  "supportedReportTriggers": ["OnChange", "Interval"],
  "features": ["heartbeat", "telemetry", "config-pull"],
  "tasks": ["ConfigPush", "HealthProbe"],
  "taskCapabilities": [
    {
      "taskType": "HealthProbe",
      "contractVersion": "edge-task-v1",
      "supportsProgress": false,
      "supportsCancellation": false
    }
  ],
  "compatibleContracts": [
    { "contractName": "edge-runtime", "contractVersion": "edge-v1" },
    { "contractName": "edge-capability", "contractVersion": "edge-capability-v1" },
    { "contractName": "collection-config", "contractVersion": "collection-config-v1" },
    { "contractName": "edge-task", "contractVersion": "edge-task-v1" }
  ]
}
```

约束：

- protocols 表示支持的协议接入能力
- features 表示通用平台能力
- tasks 表示可接收的任务类型
- supportedPointTypes、supportedTransforms 和 supportedReportTriggers 用于 Collection Template 与执行端能力匹配
- taskCapabilities 表示任务合同、进度和取消能力，不表示任务执行结果
- compatibleContracts 表示执行端兼容合同，不替代跨仓兼容矩阵文档
- 第一版不在 capabilities 中直接承载复杂嵌套状态机定义

### metadata 边界

metadata 用于承载部署上下文，不用于承载高频运行数据。

建议包含：

- site
- region
- clusterName
- environment
- labels

不建议包含：

- 高频 metrics
- 大体积日志
- 每秒变化状态
- 敏感密钥明文

## 配置下发 / 任务寻址模型草案

第一版只形成草案，不做执行闭环。

### 目标类型

- edge-node
- gateway-runtime
- device-scope

### 寻址键

- deviceId
- accessToken
- runtimeType
- instanceId

正式任务模型应至少支持 deviceId + runtimeType，后续再扩展 instanceId 精确寻址。

### 任务类型

- config-push
- config-pull-request
- package-download
- package-apply
- restart-runtime
- health-probe

### 生命周期

- Pending
- Sent
- Accepted
- Running
- Succeeded
- Failed
- TimedOut
- Cancelled

### 回执模型

建议统一包含：

- taskId
- targetType
- targetKey
- runtimeType
- instanceId
- status
- message
- reportedAt
- progress
- result

## 稳定 API 契约与状态机预留

第一版后续对接必须遵守以下边界：

- Edge 查询接口保持 extend-only，不删除既有字段
- runtimeType 作为稳定分流键使用
- Register / Heartbeat / Capabilities 路由保持兼容
- 任务状态机只允许增加新状态，不修改已有状态语义
- 回执模型字段遵循向后兼容扩展

## 第二步稳定契约正式接口文档

本节将前面的草案收敛成正式接口文档，用于 Gateway 执行端后续对接。

### 契约版本

- task 契约版本：edge-task-v1
- receipt 契约版本：edge-task-v1
- 后续版本演进遵循 extend-only 原则

### 设计原则

- 不修改既有字段语义，只新增字段
- 枚举值一旦发布，不允许重排或复用旧值
- DTO 字段保持前向和后向兼容扩展
- 任务请求与回执可独立演进，但 contractVersion 必须显式返回

### 任务请求 DTO

建议正式 DTO 名称：EdgeTaskRequestDto

字段：

- contractVersion: string，必填，当前固定为 edge-task-v1
- taskId: guid，必填，全局任务标识
- taskType: enum，必填，任务类型
- address: object，必填，目标寻址对象
- createdAt: datetime，必填，任务创建时间 UTC
- expireAt: datetime，可选，任务过期时间 UTC
- parameters: object，可选，任务参数
- metadata: object，可选，扩展元数据

示例：

```json
{
  "contractVersion": "edge-task-v1",
  "taskId": "8e0e8bd8-63b5-4d61-bec7-df1a45b42c39",
  "taskType": "ConfigPush",
  "address": {
    "targetType": "GatewayRuntime",
    "deviceId": "ab000000-0000-0000-0000-000000000001",
    "runtimeType": "gateway",
    "instanceId": "gateway-host-01",
    "targetKey": "ab000000-0000-0000-0000-000000000001:gateway"
  },
  "createdAt": "2026-04-19T08:00:00Z",
  "expireAt": "2026-04-19T08:10:00Z",
  "parameters": {
    "configVersion": "v2026.04.19.1",
    "mode": "merge"
  },
  "metadata": {
    "operator": "system",
    "source": "edge-console"
  }
}
```

### 寻址 DTO

建议正式 DTO 名称：EdgeTaskAddressDto

字段：

- targetType: enum，必填
- deviceId: guid，可选，但在平台内任务下发场景中推荐必填
- accessToken: string，可选，不建议作为长期主寻址键
- runtimeType: string，可选，推荐与 deviceId 组合使用
- instanceId: string，可选，用于多实例精确寻址
- targetKey: string，必填，落地后的标准寻址键

targetKey 规则：

- 单实例阶段：deviceId:runtimeType
- 多实例阶段：deviceId:runtimeType:instanceId

### 任务类型枚举

建议正式枚举名称：EdgeTaskType

枚举值：

- ConfigPush = 0
- ConfigPullRequest = 1
- PackageDownload = 2
- PackageApply = 3
- RestartRuntime = 4
- HealthProbe = 5

约束：

- 只允许新增任务类型，不允许修改已有值
- 执行端遇到未知任务类型时，必须返回 Failed 或 Rejected 风格回执，不允许静默吞掉

### 目标类型枚举

建议正式枚举名称：EdgeTaskTargetType

枚举值：

- EdgeNode = 0
- GatewayRuntime = 1
- DeviceScope = 3

约束：

- Gateway 对接使用 GatewayRuntime
- DeviceScope 预留给边缘节点下的设备范围型任务

### 任务状态机枚举

建议正式枚举名称：EdgeTaskStatus

枚举值：

- Pending = 0
- Sent = 1
- Accepted = 2
- Running = 3
- Succeeded = 4
- Failed = 5
- TimedOut = 6
- Cancelled = 7

### 任务状态机定义

状态含义：

- Pending: 任务已创建，尚未投递
- Sent: 任务已下发到目标通道
- Accepted: 执行端已接收并确认受理
- Running: 执行端已开始执行
- Succeeded: 执行完成且成功
- Failed: 执行完成但失败
- TimedOut: 在有效时间内未完成
- Cancelled: 平台或执行端主动取消

允许流转：

- Pending -> Sent
- Sent -> Accepted
- Sent -> TimedOut
- Accepted -> Running
- Accepted -> Cancelled
- Running -> Succeeded
- Running -> Failed
- Running -> TimedOut
- Running -> Cancelled

终态：

- Succeeded
- Failed
- TimedOut
- Cancelled

约束：

- 终态不可再次流转
- 不允许从 Pending 直接进入 Running
- 不允许从 Succeeded 回退到 Running
- 后续若新增状态，只能通过新增枚举值和新增迁移规则实现

### 回执 DTO

建议正式 DTO 名称：EdgeTaskReceiptDto

字段：

- contractVersion: string，必填，当前固定为 edge-task-v1
- taskId: guid，必填，对应请求任务 ID
- targetType: enum，必填
- targetKey: string，必填
- runtimeType: string，可选但推荐返回
- instanceId: string，可选
- status: enum，必填
- message: string，可选，人类可读消息
- reportedAt: datetime，必填，UTC
- progress: int，可选，范围 0-100
- result: object，可选，执行结果载荷
- metadata: object，可选，附加上下文

示例：

```json
{
  "contractVersion": "edge-task-v1",
  "taskId": "8e0e8bd8-63b5-4d61-bec7-df1a45b42c39",
  "targetType": "GatewayRuntime",
  "targetKey": "ab000000-0000-0000-0000-000000000001:gateway",
  "runtimeType": "gateway",
  "instanceId": "gateway-host-01",
  "status": "Running",
  "message": "Configuration package unpacked, applying runtime changes.",
  "reportedAt": "2026-04-19T08:01:30Z",
  "progress": 55,
  "result": {
    "step": "apply-config"
  },
  "metadata": {
    "hostName": "gateway-prod-01"
  }
}
```

### 回执约束

- 每个回执必须带 taskId
- reportedAt 使用 UTC
- progress 仅在 Running 或部分执行态下返回
- result 只承载任务相关结果，不承载大体量日志
- 执行端失败时，message 必须可读，便于控制台直接展示

### 推荐 HTTP / 消息接口边界

第一版正式接口边界建议如下：

- `POST /api/Edge/{access_token}/Register`
- `POST /api/Edge/{access_token}/Heartbeat`
- `POST /api/Edge/{access_token}/Capabilities`
- POST /api/EdgeTask/Receipt
- GET /api/EdgeTask/StateMachine

说明：

- Receipt 在第二步已定义为最小可用接收接口，用于执行端正式对接
- StateMachine 用于提供平台侧公开的任务状态机定义，减少执行端硬编码漂移
- 平台下发任务接口可以后续按控制平面演进，不要求与执行端回执接口在同一轮上线

### 当前已落地的最小接口能力

当前代码中已经补充：

- EdgeRuntimeStatusDto
  - 收敛注册、心跳、版本、实例、主机、健康和低频指标字段
  - EdgeNode 列表和详情内嵌 runtimeStatus
  - GET /api/Edge/{id}/RuntimeStatus 提供正式只读状态快照
- EdgeCapabilityDto
  - 收敛协议、点位类型、转换、上报触发、任务能力和合同版本兼容字段
  - EdgeNode 列表和详情内嵌 capability
  - POST /api/Edge/{access_token}/Capabilities 兼容旧结构并归一化为 edge-capability-v1
  - GET /api/Edge/{id}/Capability 提供正式只读能力快照
- EdgeCollectionAssignmentDto
  - 记录采集配置版本到 EdgeNode/Gateway 运行时的目标分配
  - 返回 `collectionConfigurationVersionId`，用于关联平台侧配置版本快照
  - PUT /api/Edge/{id}/CollectionConfig 保存配置时生成 Active assignment
  - GET /api/Edge/{id}/CollectionAssignments 提供节点内分配历史
  - GET /api/Edge/CollectionAssignments 提供租户边界内分配查询
- CollectionConfigurationVersionDto
  - 返回平台侧配置版本快照、哈希、来源和任务数量
  - GET /api/Edge/{id}/CollectionConfigVersions 提供节点内版本历史
  - GET /api/Edge/{id}/CollectionConfigVersions/{version} 提供指定版本正文
  - GET /api/Edge/CollectionConfigVersions 提供租户边界内版本查询
- EdgeTask / EdgeTaskReceipt 正式查询主路径（#025）
  - EdgeNode 列表和详情中的最近任务状态来自 `EdgeTasks`
  - GET /api/EdgeTask/Receipt/{deviceId} 从 `EdgeTaskReceipts` 或 `EdgeTasks.LastReceiptPayload` 返回最近回执
  - GET /api/EdgeTask/History/{deviceId} 从 `EdgeTasks` 和 `EdgeTaskReceipts` 组合任务历史
  - GET /api/EdgeTask/List 从正式任务和回执模型生成 timeline
  - Dispatch/Pull/Accept/Receipt 的状态流转不再写入或回退到 AttributeLatest/TelemetryData 主存储
- POST /api/EdgeTask/Receipt
  - 校验 contractVersion
  - 校验 taskId
  - 校验 targetKey
  - 校验 progress 范围
  - 校验 Running 状态必须带 progress
- GET /api/EdgeTask/StateMachine
  - 返回正式状态机枚举列表
  - 返回允许流转关系
  - 返回终态列表

当前还未落地：

- 任务执行审计查询页
- M3 Collection Template 与 assignment 的正式来源关联

### 兼容性规则

- contractVersion 必须显式保留
- 新字段只能追加，不删除旧字段
- 枚举值只能追加，不重排
- 执行端必须容忍新增字段
- 平台必须容忍旧版本执行端缺失非关键字段

## 第一版交付清单

- EdgeNode 列表页
- 后端分页、筛选、排序
- 统一分页返回结构
- Edge 详情抽屉
- 菜单入口
- Edge 查询范围文档
- Gateway 注册协议说明
- 配置下发 / 任务寻址草案
