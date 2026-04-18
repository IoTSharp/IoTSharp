# Edge 查询 V1 范围冻结

## 目标范围

第一版只覆盖以下内容：

- EdgeNode 管理页
- 列表查询、筛选、排序、分页
- EdgeNode 详情展示
- Gateway / PiXiu 注册协议正式说明
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
- 第一版要求 Gateway 与 PiXiu 都按 edge-v1 执行注册与心跳

### 注册请求字段

注册接口：

- POST /api/Edge/{access_token}/Register

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
- pixiu

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

- 心跳接口：POST /api/Edge/{access_token}/Heartbeat
- 心跳超时沿用设备 Timeout 配置
- 若最近活动超过 Timeout，运行时可被视为非活跃
- 第一版只做查询展示，不在 EdgeNode 页面内引入复杂状态推导器

### capabilities 结构

Capabilities 接口：

- POST /api/Edge/{access_token}/Capabilities

第一版标准结构：

```json
{
  "protocols": ["modbus-tcp", "opcua"],
  "features": ["heartbeat", "telemetry", "config-pull"],
  "tasks": ["config-apply", "package-download"]
}
```

约束：

- protocols 表示支持的协议接入能力
- features 表示通用平台能力
- tasks 表示可接收的任务类型
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
- pixiu-runtime
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

本节将前面的草案收敛成正式接口文档，用于 Gateway / PiXiu 执行端后续对接。

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
- PixiuRuntime = 2
- DeviceScope = 3

约束：

- Gateway 对接使用 GatewayRuntime
- PiXiu 对接使用 PixiuRuntime
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

- POST /api/Edge/{access_token}/Register
- POST /api/Edge/{access_token}/Heartbeat
- POST /api/Edge/{access_token}/Capabilities
- POST /api/Edge/Tasks/Receipt

说明：

- Tasks/Receipt 在第二步作为正式保留接口文档先定义
- 平台下发任务接口可以后续按控制平面演进，不要求与执行端回执接口在同一轮上线

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
- Gateway / PiXiu 注册协议说明
- 配置下发 / 任务寻址草案