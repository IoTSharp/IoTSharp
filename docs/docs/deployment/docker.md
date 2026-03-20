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
