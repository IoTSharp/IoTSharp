# IoTSharp 测试路线图

本文档记录 `IoTSharp.Test` 的集成测试路线。当前阶段只推进第一套容器化组合：关系型数据库使用 PostgreSQL，时序数据库使用 InfluxDB 2.0，消息总线使用 CAP，EventBusMQ 使用 RabbitMQ，EventBusStore 使用 MongoDB。

## 总体思路

测试代码采用“一套业务测试套件，多套测试配置”的方式组织：

- `IoTSharpBusinessTestSuite<TFixture>` 只描述 IoTSharp 业务行为，不关心具体基础设施。
- 每套基础设施由独立 fixture 负责启动容器、生成连接串、注入 `IoTSharpTestProfile`。
- 配置契约由 `TestProfileContractTests` 固化，先保证枚举、连接串键名、CAP Store/MQ 选择与 `Startup` 读取方式一致。
- 后续 SonnetDB 作为第二套 profile 接入同一业务套件，本阶段不让 SonnetDB 影响 PostgreSQL + InfluxDB + RabbitMQ + MongoDB 的验收。

## 第一阶段目标

基础设施全部使用 Testcontainers 启动：

- PostgreSQL：`DataBase=PostgreSql`，承载 IoTSharp 主业务库和 Identity 数据。
- InfluxDB 2.0：`TelemetryStorage=InfluxDB`，承载遥测时序数据。
- CAP：`EventBus=CAP`，保持与生产事件总线框架一致。
- RabbitMQ：`EventBusMQ=RabbitMQ`，承载 CAP 消息队列。
- MongoDB：`EventBusStore=MongoDB`，承载 CAP 持久化存储。

当前 `EventBusStore` 枚举支持 `PostgreSql`、`MongoDB`、`InMemory`、`LiteDB`、`MySql`、`SqlServer`，不支持 Redis；Redis 目前只适合作为 `EventBusMQ=RedisStreams` 的消息队列选项。因此第一阶段先选择 MongoDB 作为 CAP Store。

## 覆盖范围

第一阶段优先覆盖 IoTSharp 开源主平台的核心业务闭环：

- 安装与登录：初始化实例、创建根用户、登录、JWT 鉴权。
- 设备管理：设备创建、详情读取、更新、删除，校验访问令牌生成。
- 遥测写入与查询：HTTP 上报遥测，查询最新值和历史值，确认数据进入 InfluxDB。
- Edge Runtime：注册、心跳、能力上报、节点列表、采集配置拉取。
- API 合约：OpenAPI 暴露路由与 ASP.NET Core endpoint route 保持一致。

第二批补齐：

- 属性、事件、告警的上报与查询。
- 产品、设备模板、设备分组、客户与租户关系。
- 规则引擎触发、CAP 事件发布与消费结果验证。
- Edge Runtime 使用 access token 拉取采集配置的路由冲突修复：当前 32 位 hex token 可被 ASP.NET Core 识别为 `{id:guid}`，第一阶段测试先通过管理端 GUID 路由验证配置读写。
- Blob、证书、MQTT/CoAP/HTTP 多协议入口的关键路径。
- 健康检查、重试、并发写入、容器异常恢复。

## 测试方法

- 业务测试只通过公开 API 调用，不直接写数据库。
- 每个 fixture 独立启动依赖容器，测试数据使用随机名称或随机设备 ID，避免测试间污染。
- 对 CAP 和时序写入保留短轮询等待，避免异步处理造成偶发失败。
- 失败时优先定位为配置错误、容器启动错误、迁移/初始化错误、业务断言错误四类。
- 不在测试代码中写真实密钥；容器 token、JWT key、账号密码只使用测试占位值。

## 当前文件分工

- `IoTSharpTestProfile.cs`：描述测试 profile 并生成 Host 配置键值。
- `PostgreSqlInfluxRabbitMongoAppFixture.cs`：启动 PostgreSQL、InfluxDB、MongoDB、RabbitMQ 容器，并注入第一阶段配置。
- `IoTSharpBusinessTestSuite.cs`：跨 profile 复用的业务测试。
- `AppWithPostgreSqlInfluxRabbitMongoTest.cs`：把第一阶段 fixture 绑定到业务测试套件。
- `TestProfileContractTests.cs`：验证当前 profile 配置契约和枚举能力边界。

## 执行命令

优先执行第一阶段 profile：

```powershell
dotnet test external\IoTSharp\IoTSharp.Test\IoTSharp.Test.csproj --filter "FullyQualifiedName~AppWithPostgreSqlInfluxRabbitMongoTest|FullyQualifiedName~TestProfileContractTests"
```

如果只验证配置契约：

```powershell
dotnet test external\IoTSharp\IoTSharp.Test\IoTSharp.Test.csproj --filter "FullyQualifiedName~TestProfileContractTests"
```

## 验收标准

- 容器能从空环境启动并完成 IoTSharp 安装。
- PostgreSQL 中能完成身份、租户、客户、设备等主业务写入。
- InfluxDB 2.0 中能查询到 HTTP 上报的遥测最新值和历史值。
- CAP 能使用 RabbitMQ + MongoDB 组合正常初始化，业务测试不退回 InMemory。
- 测试代码不依赖本机预装数据库、消息队列或时序库。

## 后续 SonnetDB 阶段

SonnetDB 将作为第二套配置接入同一测试套件，目标是主业务库、时序、缓存、对象存储等环节尽可能使用 SonnetDB 实现。该阶段需要单独确认 CAP Store/MQ 的 SonnetDB 适配方式；在适配完成前不应把 SonnetDB profile 与第一阶段容器组合混跑。
