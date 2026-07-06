---
title: appsettings 配置
---

# appsettings 配置

IoTSharp 通过 `ASPNETCORE_ENVIRONMENT` 选择不同环境配置文件。

## 常见配置文件

- `appsettings.json`
- `appsettings.Production.json`
- `appsettings.Sqlite.json`
- `appsettings.PostgreSql.json`
- `appsettings.MySql.json`
- `appsettings.SQLServer.json`
- `appsettings.InfluxDB.json`
- `appsettings.SonnetDB.json`

## 关键配置项

### 数据库

通过 `DataBase` 和 `ConnectionStrings` 控制基础数据存储。

例如 Sqlite：

```json
{
  "DataBase": "Sqlite",
  "ConnectionStrings": {
    "IoTSharp": "Data Source=.data/IoTSharp.db",
    "TelemetryStorage": "Data Source=.data/TelemetryStorage.db"
  }
}
```

例如 SonnetDB Profile：

```json
{
  "DataBase": "SonnetDB",
  "TelemetryStorage": "SonnetDB",
  "CachingUseIn": "SonnetDB",
  "ConnectionStrings": {
    "IoTSharp": "Data Source=.data/sonnetdb/iotsharp",
    "TelemetryStorage": "Data Source=.data/sonnetdb/telemetry;Measurement=TelemetryData;AutoCreate=true",
    "EventBusMQ": "Data Source=.data/sonnetdb/events",
    "BlobStorage": "sonnetdb://blob?bucket=iotsharp-blob-storage&connectionString=Data%20Source%3D.data%2Fsonnetdb%2Fobjects"
  },
  "CachingUseSonnetDBConnectionString": "Data Source=.data/sonnetdb/cache",
  "EventBusMQ": "SonnetMQ",
  "EventBus": "SonnetMQ"
}
```

SonnetDB Profile 会同时把关系库、时序库、缓存、对象桶和平台事件队列切到 SonnetDB。生产部署可以通过环境变量覆盖为远端 SonnetDB 服务连接串；默认 PostgreSQL、Redis、S3 或磁盘对象存储 Profile 不受影响，回滚时切回原环境名和原 Compose 文件即可。

### 遥测存储

遥测数据可落到关系库或时序库，部署前需要先明确：

- 是否追求快速试用
- 是否需要高吞吐时序写入
- 是否要求统一存储

### JWT

登录认证依赖：

- `JwtKey`
- `JwtIssuer`
- `JwtAudience`
- `JwtExpireHours`

### ACME / 证书

如果启用 ACME 或证书通信，需要重点检查：

- `IOTSHARP_ACME`
- `security` 目录可写性
- HTTPS 证书配置

## 配置建议

- 开发环境和生产环境分离。
- 不要把真实密钥直接提交到仓库。
- Windows 服务和 Linux 服务都应显式指定环境变量。
