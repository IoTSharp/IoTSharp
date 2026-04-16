---
layout: default
title: NuGet 包说明
description: IoTSharp NuGet 包发布范围、图标、README 与使用说明。
---

# NuGet 包说明

## 发布范围

当推送 `v*.*.*` tag 时，发布工作流会打包仓库中启用了 `GeneratePackageOnBuild` 的项目，并额外包含：

- `IoTSharp.Data.JsonDB`

发布目标：

1. `https://api.nuget.org/v3/index.json`
2. `https://nuget.pkg.github.com/<owner>/index.json`

## 包元数据规范

所有发布包统一补齐以下信息：

- `RepositoryUrl`
- `PackageProjectUrl`
- `RepositoryType`
- `PackageReadmeFile`
- 通用 README 内容

`IoTSharp.Data.JsonDB` 额外补齐了：

- NuGet 图标
- 包描述
- 使用标签
- 独立 README 打包

## 常用包

| 包名 | 用途 |
| --- | --- |
| `IoTSharp` | 主程序与平台交付包 |
| `IoTSharp.Data.JsonDB` | 对 JSON 执行 SQL 查询的 ADO.NET 提供程序 |
| `IoTSharp.Sdk.Http` | 通过 HTTP 访问 IoTSharp |
| `IoTSharp.Sdk.MQTT` | 通过 MQTT 对接设备与服务 |
| `IoTSharp.Extensions.*` | 平台扩展组件集合 |
