---
layout: default
title: 快速开始
description: IoTSharp 仓库的本地构建与验证说明。
---

# 快速开始

## 运行前提

- .NET SDK `10.0.x`
- Node.js 仅在构建前端或主站发布时需要
- Docker 仅在镜像验证或容器部署时需要

## 本地验证命令

```bash
cd /home/runner/work/IoTSharp/IoTSharp
dotnet restore ./IoTSharp.sln
dotnet format ./IoTSharp.sln --verify-no-changes --no-restore
dotnet build ./IoTSharp/IoTSharp.csproj -c Release --no-restore
dotnet test ./IoTSharp.Test/IoTSharp.Test.csproj -c Release --no-restore
dotnet test ./IoTSharp.Data.JsonDB.Tests/IoTSharp.Data.JsonDB.Tests.csproj -c Release --no-restore
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
