---
title: Docker 部署
---

# Docker 部署

Docker 是 IoTSharp 最推荐的部署方式。首次体验默认使用 SonnetDB Profile，它把 IoTSharp 的关系库、遥测库、缓存、对象存储和平台事件队列收敛到 SonnetDB，便于 5 分钟内拉起完整平台。

## 5 分钟快速体验

在仓库根目录执行：

```bash
docker compose -f docker-compose.sonnetdb.yml up -d
```

启动后访问：

```text
http://localhost:2927
```

按页面提示完成 Installer 初始化即可。该入口只需要 IoTSharp 与 SonnetDB 两个服务，适合作为本地评估、演示和内网无外部依赖验证的默认路径。

## 代码库中的部署入口

重点查看：

- `Deployments/`
- 根目录 `docker-compose` 相关项目
- 服务端 `IoTSharp/Dockerfile`

## Docker 方式的优点

- 构建环境统一
- 与消息队列、数据库、时序库组合方便
- 更适合 CI 和云服务器部署

## 基本思路

1. 选择合适的数据库与消息组件组合。
2. 准备对应的 `appsettings.*.json` 环境文件。
3. 设置 `ASPNETCORE_ENVIRONMENT`。
4. 启动 IoTSharp 容器和依赖组件。
5. 首次访问时完成 Installer 初始化。

## SonnetDB Profile

SonnetDB Profile 使用独立 Compose 文件：

```bash
docker compose -f docker-compose.sonnetdb.yml up -d
```

该入口设置 `ASPNETCORE_ENVIRONMENT=SonnetDB`，挂载 `appsettings.SonnetDB.json`，并通过环境变量把四类后端指向 `sonnetdb` 服务：

- `ConnectionStrings__IoTSharp`
- `ConnectionStrings__TelemetryStorage`
- `CachingUseSonnetDBConnectionString`
- `ConnectionStrings__BlobStorage`
- `ConnectionStrings__EventBusMQ`

如果后续改用发布包或 SonnetDB 独立服务端二进制，仍应复用同一组 `appsettings.SonnetDB.json` 配置语义：IoTSharp 通过环境变量覆盖 SonnetDB 服务地址、Token、数据库名和 bucket，而不是把 SonnetDB 的内部目录结构写进平台代码。

默认 `docker-compose.yml` 仍保留 PostgreSQL / TimescaleDB 路线。需要并行验证或回滚时，停止 SonnetDB Profile，按原 `docker-compose.yml` 和原环境配置启动即可。

云边同底座的参考拓扑见 [云边同底座参考部署](../operations/cloud-edge-sonnetdb-reference.md)。该部署把云端 IoTSharp 与边缘 IoTEdge 都放在 SonnetDB 数据底座上，但云边之间仍通过 EdgeNode、CollectionConfig 和 EdgeTask 合同交互，不共享内部数据库结构。

## 需要重点关注的配置

- 数据库连接字符串
- 遥测存储配置
- `ASPNETCORE_URLS`
- 反向代理与 HTTPS
- 容器卷目录

## 升级建议

- 固定镜像标签，不直接依赖 `latest`
- 重要数据目录使用卷挂载
- 升级前备份数据库与配置文件
