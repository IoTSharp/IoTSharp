---
title: Docker Desktop 扩展
sidebar_position: 4
---

# Docker Desktop 扩展

IoTSharp 现在提供了一个 Docker Desktop 扩展镜像骨架，用于把“首次体验”收敛成单次安装动作。

## 这版解决什么问题

- 新用户不用先手动整理多份 `docker-compose.yml`
- Docker Desktop 安装扩展后即可自动拉起一个 SQLite 版 IoTSharp
- 扩展页直接给出 Web 入口、Installer 入口、端口与后续文档

## 当前扩展结构

仓库目录：

```text
docker-desktop-extension/
├─ Dockerfile
├─ metadata.json
├─ ui/
│  ├─ index.html
│  └─ styles.css
└─ vm/
   └─ extension-compose.yml
```

其中：

- `metadata.json` 定义 Docker Desktop 仪表板页
- `vm/extension-compose.yml` 定义安装扩展后自动启动的体验栈
- `Dockerfile` 生成扩展镜像，并把 IoTSharp 运行时与 UI 一起打包

## 本地构建与安装

```powershell
pwsh ./docker-desktop-extension/build-extension.ps1 -ImageName iotsharp/iotsharp-dd-extension:0.1.0
docker extension install iotsharp/iotsharp-dd-extension:0.1.0
```

安装后可以直接访问：

- Web UI: `http://localhost:2927`
- Installer: `http://localhost:2927/installer`
- MQTT: `localhost:1883`
- Secure MQTT: `localhost:8883`
- CoAP: `localhost:5683` / `5684`
- Modbus TCP: `localhost:1502`

## 交给 OpenClow 协助安装

如果你希望把本地 SQLite 体验实例的安装过程直接交给 AI 助手，可以把下面两样内容发给它：

- 文档链接：`https://iotsharp.net/docs/operations/openclow-sqlite-runbook`
- 提示词文件：`tools/prompts/openclow-sqlite-instance.txt`

这份运行手册已经明确约束了默认端口、优先路径、验证步骤，以及后续切换数据库时应使用 `appsettings.{Environment}.Installer.json` 覆盖文件，而不是直接修改模板文件。

## 数据持久化

扩展内置 Compose 会创建两个卷：

- `iotsharp-dd-data`
- `iotsharp-dd-security`

默认数据库模板为 `Sqlite`，因此 SQLite 数据文件会持久化到卷中。

## 发布建议

建议在 Tag 发布时自动完成以下动作：

1. 构建 IoTSharp 运行镜像
2. 构建 Docker Desktop 扩展镜像
3. 运行 `docker extension validate`
4. 同步推送到 Docker Hub 与 GHCR
5. 在发布说明中附上 `docker extension install <image>` 示例

## 下一轮建议

- 在扩展页增加容器状态检测与日志读取
- 支持扩展内切换 SQLite / PostgreSQL 体验模板
- 增加 PostgreSQL、RabbitMQ、Telemetry 组件的完整演示编排
