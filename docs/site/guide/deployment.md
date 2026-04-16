---
layout: default
title: 部署指南
description: IoTSharp 的容器、二进制压缩包与发布资产说明。
---

# 部署指南

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
