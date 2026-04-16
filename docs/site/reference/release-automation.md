---
layout: default
title: 发布自动化
description: IoTSharp semver tag 发布流程说明。
---

# 发布自动化

## 分支行为

普通分支与 Pull Request 只执行验证型 CI：

- `dotnet restore`
- `dotnet format --verify-no-changes`
- 主程序构建
- `IoTSharp.Data.JsonDB` 构建
- 单元测试

## Tag 行为

当推送 `v1.0.0` 这类 semver tag 时，工作流会执行：

1. 打包并发布全部 NuGet 包到 NuGet.org
2. 再发布同一批包到 GitHub Packages
3. 构建并推送 Docker 镜像到 GHCR 与 Docker Hub
4. 生成多个 RID 的压缩包并写入 GitHub Release

## Secrets

以下 secrets 需要在仓库中配置：

- `NUGET_API_KEY`
- `DOCKERHUB_USERNAME`
- `DOCKERHUB_TOKEN`

GitHub Packages 与 GHCR 使用 `GITHUB_TOKEN`。
