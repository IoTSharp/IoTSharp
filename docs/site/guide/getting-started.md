---
layout: default
title: 快速开始
description: IoTSharp 仓库的本地构建与验证说明。
permalink: /guide/getting-started/
---

# 快速开始

## 5 分钟 SonnetDB Profile

首次体验默认使用 SonnetDB Profile。它只启动 IoTSharp 与 SonnetDB 两个服务，并把关系库、遥测库、缓存、对象存储和平台事件队列都指向 SonnetDB。

```bash
docker compose -f docker-compose.sonnetdb.yml up -d
```

启动后访问 `http://localhost:2927`，按页面提示完成 Installer 初始化。

传统 PostgreSQL / TimescaleDB Compose 仍保留用于兼容性和生产拓扑验证；快速试用、演示和内网无外部依赖验证优先使用 SonnetDB Profile。

## 运行前提

- .NET SDK `10.0.x`
- Node.js 仅在构建前端或主站发布时需要
- Docker 是 SonnetDB Profile 快速体验入口

## 本地验证命令

```bash
cd /home/runner/work/IoTSharp/IoTSharp
dotnet restore ./IoTSharp.sln
dotnet format ./IoTSharp.sln --verify-no-changes --severity error --no-restore
dotnet build ./IoTSharp/IoTSharp.csproj -c Release --no-restore
dotnet test ./IoTSharp.Test/IoTSharp.Test.csproj -c Release --no-restore
dotnet test ./IoTSharp.Data.JsonDB/tests/IoTSharp.Data.JsonDB.Tests/IoTSharp.Data.JsonDB.Tests.csproj -c Release --no-restore
```

## 文档站验证

```bash
cd /home/runner/work/IoTSharp/IoTSharp
dotnet tool restore
dotnet jekyllnet build --source ./docs/site --destination ./docs/site/_site
```

## 推荐阅读

- [部署指南](/guide/deployment/)
- [NuGet 包说明](/reference/nuget-packages/)
- [发布自动化](/reference/release-automation/)
