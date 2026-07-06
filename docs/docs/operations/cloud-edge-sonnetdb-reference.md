---
title: 云边同底座参考部署
---

# 云边同底座参考部署

日期：2026-07-06

范围：本文件对应路线图 #072，说明 `IoTSharp + SonnetDB` 云端控制面与 `IoTEdge + SonnetDB` 边缘运行时的参考部署。交付物位于仓库 `Deployments/cloud_edge_sonnetdb/`。

## 目标

“云边同底座”指平台和边缘运行时都可以使用 SonnetDB 作为数据底座：

- 云端 IoTSharp 使用 SonnetDB 承载关系数据、遥测、缓存、对象桶和 SonnetMQ 事件队列。
- 边缘 IoTEdge 使用本地 SonnetDB 承载离线采集结果、上传缓冲和诊断数据。
- 云边控制面仍通过正式合同交互，不通过共享数据库表或隐藏内部字段耦合。

这一定义保护 Product、Device、Asset、Gateway、EdgeNode 和 Collection Template 的边界。SonnetDB 是部署底座，不是跨项目领域合同。

## 参考拓扑

```text
Cloud host
  IoTSharp
    |-- EdgeNode registration / heartbeat / capability
    |-- CollectionConfig publish and pull target
    |-- EdgeTask dispatch and receipt projection
    `-- Release Center and audit
  SonnetDB
    |-- iotsharp
    |-- telemetry
    |-- cache
    |-- objects
    `-- events

Edge host
  IoTEdge
    |-- protocol adapters and collection runtime
    |-- local execution state
    |-- offline cache and retry
    `-- task receipt reporter
  SonnetDB
    |-- iotedge
    |-- upload-buffer
    `-- diagnostics
```

第一版推荐云端和边缘分别运行独立 SonnetDB 实例。这样可以统一镜像、备份、指标和运维经验，同时避免边缘主机持有云端数据库管理 token。

## 部署文件

| 路径 | 用途 |
| --- | --- |
| `Deployments/cloud_edge_sonnetdb/docker-compose.cloud.yml` | 云端 IoTSharp + SonnetDB。 |
| `Deployments/cloud_edge_sonnetdb/cloud.env.example` | 云端环境变量模板。 |
| `Deployments/cloud_edge_sonnetdb/docker-compose.edge.yml` | 边缘 SonnetDB + IoTEdge 对接模板。 |
| `Deployments/cloud_edge_sonnetdb/edge.env.example` | 边缘环境变量模板。 |
| `Deployments/cloud_edge_sonnetdb/README.md` | 可执行命令、验收流程和回滚说明。 |

## 云端步骤

```bash
cd Deployments/cloud_edge_sonnetdb
cp cloud.env.example cloud.env
docker compose --env-file cloud.env -f docker-compose.cloud.yml up -d
```

必须修改默认值：

- `CLOUD_SONNETDB_TOKEN`
- `JWT_KEY`
- 镜像标签，生产环境不要依赖 `latest`
- 对外暴露端口和反向代理地址

验证：

```bash
curl http://localhost:2927/healthz
curl http://localhost:5080/healthz
curl http://localhost:5080/metrics
```

完成 Installer 后，在平台创建 Gateway 或 EdgeNode 接入凭据，后续边缘只使用该接入凭据和平台 API。

## 边缘步骤

```bash
cd Deployments/cloud_edge_sonnetdb
cp edge.env.example edge.env
docker compose --env-file edge.env -f docker-compose.edge.yml up -d edge-sonnetdb
docker compose --env-file edge.env -f docker-compose.edge.yml --profile runtime up -d
```

必须修改默认值：

- `CLOUD_IOTSHARP_BASE_URL`
- `EDGE_ACCESS_TOKEN`
- `EDGE_NODE_ID`
- `EDGE_SONNETDB_TOKEN`
- `IOTEDGE_IMAGE`

`docker-compose.edge.yml` 固定的是参考语义，不替代 IoTEdge 仓库自己的配置文档。IoTEdge 当前版本如使用不同配置键，应映射到以下语义：

| 语义 | 示例变量 |
| --- | --- |
| 平台地址 | `CLOUD_IOTSHARP_BASE_URL` |
| Edge/Gateway 接入令牌 | `EDGE_ACCESS_TOKEN` |
| 本地 SonnetDB 数据库 | `EDGE_LOCAL_SONNETDB_CONNECTION` |
| 上传缓冲或诊断数据库 | `EDGE_BUFFER_SONNETDB_CONNECTION` |
| 合同版本 | `edge-node-v1`、`collection-config-v1`、`edge-task-v1` |

## 验收清单

| 项目 | 验收点 |
| --- | --- |
| 云端底座 | IoTSharp 使用 `DataBase=SonnetDB`、`TelemetryStorage=SonnetDB`、`CachingUseIn=SonnetDB`、`EventBusMQ=SonnetMQ` 启动。 |
| 边缘底座 | IoTEdge 可以访问本地 `edge-sonnetdb`，断网时采集状态或上传缓冲仍能落本地。 |
| 注册心跳 | IoTEdge 通过平台接入凭据完成注册、心跳和能力上报。 |
| 配置发布 | Product Collection Template 发布后生成 `collection-config-v1`，目标 EdgeNode/Gateway runtime 可拉取。 |
| 任务闭环 | ConfigPullRequest、SoftwareUpdate 或诊断任务通过 `edge-task-v1` 分发，Accepted/Running/Succeeded/Failed 回执进入平台审计。 |
| 观测指标 | 云端和边缘 SonnetDB 均可采集 `/healthz` 与 `/metrics`，至少关注 `sonnetdb_rows_inserted_total`、`sonnetdb_sql_errors_total` 和 `sonnetdb_segments{db=...}`。 |

## 边界规则

- 不把 Product 模板写入边缘数据库作为平台事实来源。
- 不让 IoTEdge 直接查询或修改 IoTSharp 的云端业务库。
- 不把 ReleaseTask、OTA、配置灰度等长周期状态塞进实时 RuleChain。
- 不复用云端 SonnetDB 管理 token 到边缘。
- 不把边缘本地数据目录路径写进 IoTSharp 平台代码。

## 安全与运维

生产环境建议：

- IoTSharp HTTP/MQTT 入口放在反向代理、VPN 或专线后，并启用 TLS。
- SonnetDB 管理端口只在内网或容器网络暴露。
- 云端和边缘使用不同 token、不同数据库名、不同备份策略。
- 备份报告分别记录云端控制面数据和边缘本地缓存数据。
- 发布任务、配置下发和 AI 辅助诊断继续走平台权限、审计和人工确认策略。

## 回滚

云端回滚到 PostgreSQL / TimescaleDB 时，停止 `docker-compose.cloud.yml`，恢复原 `docker-compose.yml` 和原环境配置。关系库、遥测库、缓存、对象桶和事件队列必须成组切换，不能出现半迁移状态。

边缘回滚时，停止 IoTEdge 参考服务并保留 `edge_sonnetdb_data` 卷，确认补传或故障分析完成后再清理。切回 IoTEdge 当前生产配置时，仍应保留 EdgeNode 接入凭据和合同版本兼容检查。
