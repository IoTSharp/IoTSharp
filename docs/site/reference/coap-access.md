---
layout: default
title: CoAP 接入
description: IoTSharp 平台 CoAP 接入路径、旧路径差异、组件版本、回滚和运维配置说明。
permalink: /reference/coap-access/
---

# CoAP 接入

IoTSharp 的 CoAP 接入用于受约束设备、UDP 上报和需要保留 CoAP/CoRE 协议语义的场景。平台业务层只暴露 Product、Device、Gateway 等 IoTSharp 领域入口，不把 CoAP `Resource` 当作平台业务概念。

## 范围边界

| 层次 | 负责内容 |
| --- | --- |
| IoTSharp 平台 | Device/Gateway 接入路径、访问令牌校验、目标类型校验、遥测/属性/告警业务分发、租户与审计扩展。 |
| CoAP.NET 基础组件 | CoAP server/client、Resource tree、route adapter、`.well-known/core` discovery、Content-Format、Accept、Observe、Blockwise、DTLS PSK 和 host-managed resource 映射。 |
| MQTT 接入 | 保持现状，不属于本轮 CoAP route 化轨道改造范围。 |

组件消费、宿主 API、版本和回滚策略也归属本页维护。CoAP.NET 组件文档只保留协议栈、Resource/MVC、路由、发现和传输层说明，不记录 IoTSharp 平台业务约定。

## 组件消费与版本

IoTSharp 源码开发态使用本地 `ProjectReference` 消费 CoAP.NET fork，便于平台 CoAP route adapter、DTLS、Observe 和业务分发一起验证。

| 项目 | 引用方式 |
| --- | --- |
| `IoTSharp/IoTSharp.csproj` | `CoAP.NET` 与 `CoAP.NET.AspNetCore` 本地 `ProjectReference` |
| `tools/IoTSharp.Benchmarks/IoTSharp.Benchmarks.csproj` | `CoAP.NET` 本地 `ProjectReference` |

根目录 `Directory.Packages.props` 不声明旧 `IoTSharp.CoAP.NET` 2.0.8 中央版本，避免旧包版本被误认为当前协议栈版本。只有平台项目切换为 NuGet `PackageReference` 时，才应在同一次变更中新增对应中央版本声明。

宿主入口保留 Resource 命名：

```csharp
builder.Services.AddCoapServer(options =>
{
    options.ListenAnyIP(5683);
});
builder.Services.AddCoapResources();

var app = builder.Build();
app.MapCoapResources();
app.Run();
```

约束：

- `AddCoapServer()` 只负责 CoAP server 配置、监听端点、传输、DTLS 和生命周期。
- `AddCoapResources()` 注册 CoAP resource/controller 能力。
- `app.MapCoapResources()` 映射 CoAP resource endpoint。
- 平台 DTO、授权、租户、审计和领域服务调用保留在 IoTSharp 宿主应用。

3.0.0 fork 发布三个包：

| 包 | 用途 |
| --- | --- |
| `IoTSharp.CoAP.NET` | 核心 CoAP client/server、Resource tree、Blockwise、Observe、DTLS PSK 和 routing。 |
| `IoTSharp.CoAP.NET.AspNetCore` | ASP.NET Core 宿主适配，提供 `app.MapCoapResources()`。 |
| `IoTSharp.CoAP.NET.SourceGeneration` | Resource/MVC endpoint factory source generator，供 AOT 或低反射宿主选择性引用。 |

外部项目按 NuGet 方式消费时应显式固定 `3.0.0` 或后续批准的 `3.0.x` 版本。

```powershell
dotnet add package IoTSharp.CoAP.NET --version 3.0.0
dotnet add package IoTSharp.CoAP.NET.AspNetCore --version 3.0.0
dotnet add package IoTSharp.CoAP.NET.SourceGeneration --version 3.0.0
```

回滚必须显式选择路径：

1. 如果问题来自 fork 源码改动，优先回滚 `SonnetDB/extensions/IoTSharp.CoAP.NET` 到上一个已验证提交，继续保留 `ProjectReference`。
2. 如果需要临时切回 NuGet 包，必须在同一个变更中把平台项目改为 `PackageReference`，并在 `Directory.Packages.props` 新增 `IoTSharp.CoAP.NET`、`IoTSharp.CoAP.NET.AspNetCore` 的已批准版本。
3. 不把旧 `2.0.8` 作为隐式回滚目标；它不代表当前 3.0.0 fork 的 Resource/MVC route adapter、DTLS PSK、source generation 与 host-managed resource 能力。

## 推荐 Uri-Path

| CoAP method | Uri-Path | 目标 | 用途 |
| --- | --- | --- | --- |
| `POST` | `devices/{device}/telemetry` | Device | 直连设备遥测上报 |
| `POST` | `devices/{device}/attributes` | Device | 直连设备属性上报 |
| `POST` | `devices/{device}/alarm` | Device | 直连设备告警上报 |
| `POST` | `gateways/{gateway}/telemetry` | Gateway | 网关遥测上报 |
| `POST` | `gateways/{gateway}/attributes` | Gateway | 网关属性上报 |

`{device}` 和 `{gateway}` 可以是平台侧设备/网关名称，也可以是对应 ID。请求必须通过 Uri-Query 携带访问令牌，支持的 query 名称为 `access_token`、`accessToken` 或 `token`。

示例：

```text
coap://127.0.0.1:5683/devices/device-001/telemetry?access_token=<device-access-token>
```

payload 当前要求是非空 JSON object。遥测和属性 payload 会进入现有事件总线；设备告警 payload 至少需要 `AlarmType`。

## 新旧路径差异

| 旧 Uri-Path | 新 Uri-Path | 兼容结论 |
| --- | --- | --- |
| `Telemetry` | `devices/{device}/telemetry` 或 `gateways/{gateway}/telemetry` | 不兼容，旧路径返回 `4.04 Not Found`。 |
| `Attributes` | `devices/{device}/attributes` 或 `gateways/{gateway}/attributes` | 不兼容，旧路径返回 `4.04 Not Found`。 |
| `Alarm` | `devices/{device}/alarm` | 不兼容，旧路径返回 `4.04 Not Found`；Gateway 不提供告警短路径替代。 |

迁移设备端时，应同时调整 Uri-Path、令牌 query 和目标名称。不要在 IoTSharp 平台侧重新增加旧短路径兼容层。

## 运维配置

CoAP 入口由 `CoapServer` 配置节控制。默认配置位于 `IoTSharp/appsettings.DefaultSettings.json`。

| 配置 | 默认值 | 说明 |
| --- | --- | --- |
| `CoapServer:Enabled` | `true` | 是否启用 UDP `coap://` 明文监听。 |
| `CoapServer:BindAddress` | `0.0.0.0` | 明文 CoAP 绑定地址。生产环境可收窄到内网地址或由容器端口映射控制暴露范围。 |
| `CoapServer:Port` | `5683` | 明文 CoAP UDP 端口；未设置时回退到 `DefaultPort`。 |
| `CoapServer:Dtls:Enabled` | `false` | 是否启用 PSK DTLS `coaps://`。 |
| `CoapServer:Dtls:BindAddress` | 空 | DTLS 绑定地址；为空时复用明文 CoAP 绑定地址。 |
| `CoapServer:Dtls:Port` | `5684` | DTLS UDP 端口；只有启用 DTLS PSK 时才监听。 |
| `CoapServer:Dtls:PskKeys` | `{}` | `identity -> key` 映射；启用 `coaps://` 时必须配置。 |
| `CoapServer:Dtls:SessionIdleSeconds` | `300` | DTLS session 空闲时间。 |
| `CoapServer:MaxMessageSize` | `1024` | 单消息大小上限。 |
| `CoapServer:DefaultBlockSize` | `512` | Blockwise 默认分片大小。 |
| `CoapServer:ChannelReceivePacketSize` | `2048` | UDP 接收包大小。 |

默认体验只开放 `5683/udp` 明文 CoAP。`5684/udp` 不是默认可用端口，必须同时满足以下条件才可对外说明或开放：

1. `CoapServer:Dtls:Enabled=true`
2. `CoapServer:Dtls:PskKeys` 至少配置一个 PSK identity
3. 容器、主机防火墙或负载均衡显式放行 `5684/udp`

容器部署默认只映射 `5683/udp`。如需变更端口，应优先通过 compose 环境变量或部署层端口映射调整，不直接修改模板文件。

## 错误映射

| 场景 | CoAP response code |
| --- | --- |
| payload 或 Uri-Query 不合法 | `4.00 Bad Request` |
| 未携带、无法识别或无效访问令牌 | `4.01 Unauthorized` |
| 访问令牌与 route 目标不匹配 | `4.03 Forbidden` |
| route 不存在，包括旧短路径 | `4.04 Not Found` |
| Content-Format 或 Accept 不支持 | `4.06 Not Acceptable` |
| 暂不支持的业务操作 | `5.01 Not Implemented` |
| 写入成功 | `2.04 Changed` |

缺失 token 和无效 token 统一返回 `4.01 Unauthorized`，避免通过错误信息枚举有效令牌。

## 验证入口

性能基准和真实 UDP 压测入口位于 `tools/IoTSharp.Benchmarks`：

```powershell
dotnet run -c Release --project tools/IoTSharp.Benchmarks -- --filter *Coap*
```

对运行中的 IoTSharp CoAP 入口发起真实 UDP 压测：

```powershell
dotnet run -c Release --project tools/IoTSharp.Benchmarks -- --coap-load --uri coap://127.0.0.1:5683/devices/device-001/telemetry --token "<device-access-token>" --requests 10000 --concurrency 32
```

可通过 `--payload` 调整 JSON 负载，通过 `--block-size` 覆盖 Blockwise 分片压力。
