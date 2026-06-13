---
title: Docker 部署
---

# Docker 部署

Docker 是 IoTSharp 最推荐的部署方式。

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

如果希望关系库、时序库、缓存和对象桶都使用 SonnetDB，可以使用独立 Compose 文件：

```bash
docker compose -f docker-compose.sonnetdb.yml up -d
```

该入口设置 `ASPNETCORE_ENVIRONMENT=SonnetDB`，挂载 `appsettings.SonnetDB.json`，并通过环境变量把四类后端指向 `sonnetdb` 服务：

- `ConnectionStrings__IoTSharp`
- `ConnectionStrings__TelemetryStorage`
- `CachingUseSonnetDBConnectionString`
- `ConnectionStrings__BlobStorage`

默认 `docker-compose.yml` 仍保留 PostgreSQL / TimescaleDB 路线。需要并行验证或回滚时，停止 SonnetDB Profile，按原 `docker-compose.yml` 和原环境配置启动即可。

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
