# IoTSharp.Benchmarks

本项目覆盖 `#09A CoAP 性能与压测` 的可复现入口。

## BenchmarkDotNet

```powershell
dotnet run -c Release --project tools/IoTSharp.Benchmarks -- --filter *Coap*
```

覆盖项：

- IoTSharp 推荐 CoAP path 约定匹配。
- CoAP.NET endpoint matcher 与 resource tree 构建。
- CoAP telemetry UTF-8 payload 解析。
- CoAP alarm payload 的 System.Text.Json source generation 解析与 DTO 映射。

## CoAP 压测 Runner

先启动 IoTSharp 并启用 `CoapServer`，再运行：

```powershell
dotnet run -c Release --project tools/IoTSharp.Benchmarks -- --coap-load --uri coap://127.0.0.1:5683/devices/device-001/telemetry --token "<device-access-token>" --requests 10000 --concurrency 32
```

说明：

- 压测 runner 通过真实 UDP CoAP POST 写入平台推荐 route。
- `--block-size` 可调小以覆盖 Blockwise 分片压力。
- payload 字典会交给事件总线，不做对象池复用；只池化或复用不会逃逸的临时结构，避免压测优化破坏业务所有权。
