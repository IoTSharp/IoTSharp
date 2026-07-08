---
title: CoAP 接入
---

# CoAP 接入

IoTSharp 的 CoAP 接入遵守 CoAP/CoRE 标准术语，但平台业务层不把 `Resource` 作为业务概念暴露。

## 命名边界

| 层次 | 命名规则 |
| --- | --- |
| CoAP.NET 协议栈 | 保留 `Resource`、`IResource`、`ResourceAttributes`、`DiscoveryResource`、resource tree、resource discovery 等 RFC 术语，并提供 `CoAP.Server.Routing` route adapter。 |
| IoTSharp 平台接入层 | 只描述 Product、Device、Gateway 等平台入口约定和业务分发，不自行实现 CoAP.NET 应具备的 resource tree / route adapter 能力。 |
| IoTSharp API、DTO、文档业务概念 | 不使用裸 `Resource` 表示设备、产品、资产、网关、边缘节点或采集模板。 |

这样可以同时尊重 CoAP 标准术语，并避免和 .NET 的 `.resx`、`System.Resources`、`ResourceManager`、`EmbeddedResource` 混淆。

## 推荐 Uri-Path

新入口统一使用小写 Uri-Path，并以平台领域对象作为第一层：

| CoAP method | Uri-Path | 用途 |
| --- | --- | --- |
| `POST` | `devices/{device}/telemetry` | 直连设备遥测上报 |
| `POST` | `devices/{device}/attributes` | 直连设备属性上报 |
| `POST` | `devices/{device}/alarm` | 直连设备告警上报 |
| `POST` | `gateways/{gateway}/telemetry` | 网关遥测上报 |
| `POST` | `gateways/{gateway}/attributes` | 网关属性上报 |

`{device}` 和 `{gateway}` 是平台侧目标名称。当前业务分发支持通过 Uri-Query 携带 `access_token`、`accessToken` 或 `token`，并由平台校验令牌、目标类型和目标名称是否一致。

## 不兼容旧短路径

历史短路径不再作为新的兼容目标：

| 旧 Uri-Path | 当前含义 |
| --- | --- |
| `Telemetry` | 不再注册 |
| `Attributes` | 不再注册 |
| `Alarm` | 不再注册 |

新的 CoAP 接入只注册推荐 Uri-Path。实现、测试和文档都不再围绕旧短路径设计兼容层。

## 错误映射

| 场景 | CoAP response code |
| --- | --- |
| payload 或 Uri-Query 不合法 | `4.00 Bad Request` |
| 未携带或无法识别访问令牌 | `4.01 Unauthorized` |
| 访问令牌与 route 目标不匹配 | `4.03 Forbidden` |
| route 不存在 | `4.04 Not Found` |
| Content-Format 或 Accept 不支持 | `4.06 Not Acceptable` |
| 写入成功 | `2.01 Created` 或 `2.04 Changed` |

## 后续实现顺序

1. CoAP.NET `CoAP.Server.Routing` 接收 `Resource` 树分发后的请求。
2. CoAP.NET 把 Uri-Path、method、Content-Format、Accept 和 payload 转成 `CoapRouteContext`。
3. IoTSharp 将推荐 Uri-Path 模板映射为平台业务上下文，并复用现有设备、网关、事件总线、租户和 Product 能力解释。
4. 后续只围绕推荐 route 增加审计、运维说明和更完整的扩展格式支持。
