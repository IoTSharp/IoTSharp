---
layout: default
title: 部署指南
description: IoTSharp 的容器、二进制压缩包与发布资产说明。
permalink: /guide/deployment/
---

# 部署指南

## 默认快速体验

IoTSharp 的 5 分钟体验入口默认使用 SonnetDB Profile：

```bash
docker compose -f docker-compose.sonnetdb.yml up -d
```

该入口设置 `ASPNETCORE_ENVIRONMENT=SonnetDB`，挂载 `appsettings.SonnetDB.json`，并让关系库、遥测库、缓存、对象存储和平台事件队列都使用 SonnetDB。启动后访问 `http://localhost:2927` 完成 Installer 初始化。

如果后续改用发布包或 SonnetDB 独立服务端二进制，仍复用同一 Profile 语义，通过环境变量覆盖 SonnetDB 服务地址、Token、数据库名和 bucket。传统 PostgreSQL / TimescaleDB Compose 保留用于兼容性、迁移和生产拓扑验证。

云边同底座参考部署见 [云边同底座](/reference/cloud-edge-sonnetdb-reference/)。该拓扑让 IoTSharp 云端与 IoTEdge 边缘都使用 SonnetDB，但云边交互仍通过 EdgeNode、CollectionConfig 和 EdgeTask 合同完成。

## Docker 镜像

Semver tag（例如 `v1.0.0`）会构建 IoTSharp Docker 镜像，并同时推送到：

- `ghcr.io/<owner>/iotsharp`
- `docker.io/<dockerhub-user>/iotsharp`

镜像标签包括：

- 完整版本号
- `major.minor`
- 非预发布版本的 `latest`

## GitHub Release 二进制资产

Release 工作流会为以下运行时生成压缩包并附加到 GitHub Release：

- `win-x64`
- `win-x86`
- `linux-x64`
- 现有的 ARM / ARM64 / LoongArch64 / macOS x64 资产仍继续保留

## 主程序发布注意事项

- `dotnet publish` 使用 Release 配置
- Release 版本号来自 tag 去掉前缀 `v`
- 资产文件名使用 `IoTSharp-<version>-<rid>` 规则
