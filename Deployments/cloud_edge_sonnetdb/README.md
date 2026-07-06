# 云边同底座参考部署

本目录对应路线图 #072，提供 `IoTSharp + SonnetDB` 云端控制面和 `IoTEdge + SonnetDB` 边缘运行时的参考部署骨架。

目标是展示“平台和边缘共用 SonnetDB 这一数据底座”的交付方式，而不是让边缘运行时直接读写 IoTSharp 的内部业务库。云边之间的正式边界仍然是 EdgeNode 注册、心跳、能力上报、CollectionConfig 拉取和 EdgeTask 回执合同。

## 拓扑

```text
Cloud host
  IoTSharp
    - control plane
    - Product / Device / Asset / Gateway / EdgeNode
    - Collection Template / Release Center / audit
  SonnetDB
    - IoTSharp relational store
    - telemetry store
    - cache
    - blob bucket
    - SonnetMQ event queue

Edge host
  IoTEdge
    - southbound protocol runtime
    - local collection execution
    - offline cache and retry
    - EdgeTask receipt reporter
  SonnetDB
    - local edge store
    - optional upload buffer
    - edge-side diagnostic data
```

云端 SonnetDB 和边缘 SonnetDB 可以使用同一种镜像、备份和观测方法，但默认是两个独立实例。边缘侧不持有云端 SonnetDB 管理令牌，也不依赖 IoTSharp 数据表结构。

## 文件

| 文件 | 说明 |
| --- | --- |
| `docker-compose.cloud.yml` | 云端 IoTSharp + SonnetDB 参考部署。 |
| `docker-compose.edge.yml` | 边缘 SonnetDB + IoTEdge 对接模板。 |
| `cloud.env.example` | 云端环境变量示例。 |
| `edge.env.example` | 边缘环境变量示例。 |

## 云端启动

复制并修改云端环境变量：

```bash
cp cloud.env.example cloud.env
```

至少修改：

- `CLOUD_SONNETDB_TOKEN`
- `JWT_KEY`
- 对外端口和镜像版本

启动云端：

```bash
docker compose --env-file cloud.env -f docker-compose.cloud.yml up -d
```

验证：

```bash
curl http://localhost:2927/healthz
curl http://localhost:5080/healthz
```

首次访问 `http://localhost:2927` 完成 Installer 初始化，然后在 IoTSharp 中创建设备或 Gateway/EdgeNode 接入凭据。

## 边缘启动

复制并修改边缘环境变量：

```bash
cp edge.env.example edge.env
```

至少修改：

- `CLOUD_IOTSHARP_BASE_URL`
- `EDGE_ACCESS_TOKEN`
- `EDGE_NODE_ID`
- `EDGE_SONNETDB_TOKEN`
- `IOTEDGE_IMAGE`

启动边缘侧 SonnetDB：

```bash
docker compose --env-file edge.env -f docker-compose.edge.yml up -d edge-sonnetdb
```

启动 IoTEdge 参考服务：

```bash
docker compose --env-file edge.env -f docker-compose.edge.yml --profile runtime up -d
```

`docker-compose.edge.yml` 中的 IoTEdge 环境变量是跨仓部署模板。若 IoTEdge 当前版本使用不同配置键，请在 IoTEdge 仓库按等价配置项映射以下语义：

- 平台地址：`CLOUD_IOTSHARP_BASE_URL`
- Edge/Gateway 接入令牌：`EDGE_ACCESS_TOKEN`
- 本地 SonnetDB 连接串：`EDGE_LOCAL_SONNETDB_CONNECTION`
- 上传缓冲或诊断库连接串：`EDGE_BUFFER_SONNETDB_CONNECTION`
- 合同版本：`edge-node-v1`、`collection-config-v1`、`edge-task-v1`

## 验收流程

1. 云端 IoTSharp 使用 SonnetDB Profile 完成初始化。
2. 在 IoTSharp 中创建 Gateway 或 EdgeNode 接入凭据。
3. 边缘 IoTEdge 使用该凭据完成注册、心跳和能力上报。
4. 在 Product 下维护 Collection Template，并发布到目标 EdgeNode 或 Gateway runtime。
5. 边缘拉取 `collection-config-v1`，执行采集并上报任务回执。
6. 断网后边缘继续写本地 SonnetDB；网络恢复后按 IoTEdge 上传通道补传。
7. 云端 Release Center 对 EdgeNode/Gateway runtime 发布软件或配置任务，回执仍走 `edge-task-v1`。

## 安全边界

- 云端 SonnetDB token 只给 IoTSharp 使用，不下发到边缘。
- 边缘 SonnetDB token 只在边缘主机内使用。
- IoTEdge 只通过 IoTSharp 授权接口拿配置和任务，不直接查询平台数据库。
- 生产环境应在反向代理或 VPN 后暴露 IoTSharp HTTP/MQTT 入口，并启用 TLS。
- SonnetDB `/metrics` 可接入 Prometheus，但不要暴露管理 token。

## 回滚

云端回滚到传统 PostgreSQL / TimescaleDB profile 时，停止本目录云端 compose，按根目录 `docker-compose.yml` 和原环境配置启动即可。不要让一个 IoTSharp 实例同时把关系库、遥测库、缓存、对象桶和事件队列拆到半迁移状态。

边缘回滚时，停止 `iotedge` 服务，保留本地 SonnetDB 数据目录用于补传或故障分析，再切回 IoTEdge 当前生产配置。
