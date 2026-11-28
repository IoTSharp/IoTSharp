---
layout: default
title: 发布自动化
description: IoTSharp 基础测试、手动验证和 Tag 发布流程。
permalink: /reference/release-automation/
---

# 发布自动化

## 分支行为

普通分支推送和 Pull Request 自动构建后端并运行基础测试。IoTSharp 测试覆盖 SQLite API 契约、配置租约、MQTT 路由、Edge 契约和测试配置，另执行 JsonDB 测试。Pull Request 上继续运行依赖差异检查。

## 手动验证

在 GitHub Actions 中选择工作流并点击 **Run workflow**，可执行以下任务。

- **Full .NET CI** 运行完整平台测试、容器集成测试、JsonDB 测试和解决方案格式检查。勾选 `sonnetdb_acceptance` 可额外启用 SonnetDB 租约资格验证与已有数据库升级验收。
- **Frontend build matrix** 使用 Node.js 22 和 24 构建前端。
- **CodeQL** 扫描 C# 和 JavaScript。
- **Build and Deploy Docs** 构建文档。从默认分支运行时部署 GitHub Pages。

Gitee Go 的 OpenSCA 和容器构建也通过流水线页面手动运行。

## Tag 行为

向 GitHub 推送 `v1.0.0` 或 `v1.0.0.1` 格式的版本 Tag 会触发正式发布。

1. 打包并发布全部 NuGet 包到 NuGet.org
2. 再发布同一批包到 GitHub Packages
3. 构建并推送 Docker 镜像到 GHCR 与 Docker Hub
4. 生成 Windows ZIP 和 MSI、Linux RPM 和 DEB、macOS tar.gz 并上传到 GitHub Release

## Secrets

正式发布使用以下仓库 secrets。

- `NUGET_API_KEY`
- `DOCKERHUB_USERNAME`
- `DOCKERHUB_TOKEN`

GitHub Packages 与 GHCR 使用 `GITHUB_TOKEN`。
