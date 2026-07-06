---
layout: default
title: 云边同底座参考部署
description: IoTSharp 与 IoTEdge 共同使用 SonnetDB 数据底座的参考拓扑。
permalink: /reference/cloud-edge-sonnetdb-reference/
---

# 云边同底座参考部署

本页对应路线图 #072。完整部署文件位于仓库 `Deployments/cloud_edge_sonnetdb/`。

## 拓扑

| 层级 | 组件 | SonnetDB 角色 |
| --- | --- | --- |
| 云端 | IoTSharp | 关系库、遥测库、缓存、对象桶、SonnetMQ 事件队列 |
| 边缘 | IoTEdge | 本地采集状态、上传缓冲、诊断数据 |

云端和边缘默认使用独立 SonnetDB 实例，统一镜像和运维方法，但不共享内部数据库结构。云边之间仍通过 EdgeNode 注册、心跳、能力上报、`collection-config-v1` 和 `edge-task-v1` 合同交互。

## 启动命令

```bash
cd Deployments/cloud_edge_sonnetdb
cp cloud.env.example cloud.env
docker compose --env-file cloud.env -f docker-compose.cloud.yml up -d
```

```bash
cp edge.env.example edge.env
docker compose --env-file edge.env -f docker-compose.edge.yml up -d edge-sonnetdb
docker compose --env-file edge.env -f docker-compose.edge.yml --profile runtime up -d
```

## 验收点

- IoTSharp 使用 SonnetDB Profile 完成初始化。
- IoTEdge 使用平台接入凭据完成注册、心跳和能力上报。
- Product Collection Template 发布后，边缘可拉取 `collection-config-v1`。
- EdgeTask 回执进入平台审计。
- 云端与边缘 SonnetDB 均可采集 `/healthz` 和 `/metrics`。

完整文档位于 `docs/docs/operations/cloud-edge-sonnetdb-reference.md`。
