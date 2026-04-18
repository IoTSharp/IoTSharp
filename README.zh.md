<p align="left">
  <a href="https://iotsharp.net/">
    <img src="docs/static/img/logo_white.svg" width="360px" alt="IoTSharp logo" />
  </a>
</p>

# IoTSharp

[英文](README.md) | [中文](README.zh.md)

[![License](https://img.shields.io/github/license/IoTSharp/IoTSharp.svg)](LICENSE)
[![.NET build](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-build.yml)
[![Docs deploy](https://github.com/IoTSharp/IoTSharp/actions/workflows/docs-deploy.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/docs-deploy.yml)
[![Release binaries](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-publish.yml)
[![NuGet packages](https://github.com/IoTSharp/IoTSharp/actions/workflows/pack-nupkg.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/pack-nupkg.yml)
[![Docker images](https://github.com/IoTSharp/IoTSharp/actions/workflows/docker-release.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/docker-release.yml)
![Docker Pulls](https://img.shields.io/docker/pulls/maikebing/iotsharp)
![GitHub all releases](https://img.shields.io/github/downloads/IoTSharp/IoTSharp/total)

IoTSharp 是一个面向工业与企业场景的开源 IoT 平台，覆盖设备接入、遥测采集、规则链处理、可视化管理、多租户运营与产品化交付。

## 🚀 项目概览

IoTSharp 将构建生产级 IoT 平台所需的关键能力组合在一起：

- 支持 HTTP、MQTT、CoAP 等协议的设备与网关接入能力。
- 围绕遥测、属性、告警、产品、资产和租户形成统一的数据与管理模型。
- 通过规则链完成数据转换、通知下发、自动化动作和业务处理。
- 支持关系型数据库与时序数据库，以适配不同部署和扩展场景。
- 提供 Docker、Windows 服务、Linux 服务、安装向导与发布包等多种交付方式。

当前主应用基于 `.NET 10`，Web 控制台基于 Vue 3，并已全面统一为 IoTSharp 品牌界面。

## 🧭 快速开始

### 🖥️ 本地运行 IoTSharp

建议先从以下文档入口开始：

- 产品文档首页：<https://iotsharp.net/docs/intro>
- 安装方式：<https://iotsharp.net/docs/getting-started/installation-options>
- 安装向导初始化：<https://iotsharp.net/docs/getting-started/installer>
- Docker Desktop 扩展：<https://iotsharp.net/docs/deployment/docker-desktop-extension>

前端本地开发默认端口为：

- 前端：`http://localhost:27915`

### 🤖 使用 OpenClaw 协助搭建 SQLite 体验实例

如果你希望让 OpenClaw 引导你完成本地 SQLite 版 IoTSharp 的安装，建议直接给它下面这两样内容：

- <https://iotsharp.net/docs/operations/openclaw-sqlite-runbook>
- 提示词模板：[`tools/prompts/openclaw-sqlite-instance.txt`](tools/prompts/openclaw-sqlite-instance.txt)

运行手册里已经定义了 SQLite 引导流程、安装向导初始化路径、Docker Desktop 扩展的回退方案，以及后续通过 `appsettings.{Environment}.Installer.json` 安全切换数据库的规则。

## 🧩 支持的组件

| 领域 | 概览 |
| --- | --- |
| 接入能力 | 面向工业场景的设备接入、网关接入与协议扩展能力 |
| 平台模型 | 遥测、属性、告警、产品、资产、租户与用户等核心模型 |
| 数据基础 | 关系型存储、时序存储，以及安装阶段可选的配置模板体系 |
| 集成处理 | 规则链、脚本、通知、事件传递与自动化动作 |
| 交付方式 | Docker、系统服务、安装向导与 Docker Desktop 扩展等形态 |

如果你需要更细的数据库矩阵、时序引擎、消息中间件支持和配置示例，请直接查看文档：

- <https://iotsharp.net/docs/overview/product-overview>
- <https://iotsharp.net/docs/configuration/appsettings>
- <https://iotsharp.net/docs/integrations/protocols>

## 📦 部署方式

- Docker：<https://iotsharp.net/docs/deployment/docker>
- Docker Desktop 扩展：<https://iotsharp.net/docs/deployment/docker-desktop-extension>
- Windows 服务：<https://iotsharp.net/docs/deployment/windows-service>
- Linux 服务：<https://iotsharp.net/docs/deployment/linux-service>
- 应用配置：<https://iotsharp.net/docs/configuration/appsettings>

在线演示：

- <https://host.iotsharp.net>

## 🗂️ 仓库结构

仓库中的关键目录如下：

- [`IoTSharp`](IoTSharp)：主 ASP.NET Core 应用。
- [`ClientApp`](ClientApp)：Vue 3 前端控制台。
- [`docs`](docs)：Docusaurus 帮助手册站点。
- [`docker-desktop-extension`](docker-desktop-extension)：Docker Desktop 扩展体验包。
- [`IoTSharp.Installer.Windows`](IoTSharp.Installer.Windows)：Windows 安装工程。
- [`IoTSharp.Agent`](IoTSharp.Agent)：桌面托盘代理项目。
- [`IoTSharp.SDKs`](IoTSharp.SDKs)：SDK 与面向客户端的相关项目。

## 🌐 生态与相关项目

NuGet 包与生态库包括：

- `IoTSharp.Sdk.Http`
- `IoTSharp.Sdk.MQTT`
- `IoTSharp.Extensions`
- `IoTSharp.Extensions.AspNetCore`
- `IoTSharp.Extensions.EFCore`
- `IoTSharp.Extensions.QuartzJobScheduler`
- `IoTSharp.HealthChecks.*`
- `IoTSharp.X509Extensions`

相关仓库包括：

- IoTSharp MQTT C SDK
- IoTSharp Edge paho.mqtt.c
- IoTSharp Edge nanoFramework
- IoTSharp RT-Thread package

## 📚 文档

- 英文说明： [README.md](README.md)
- 文档首页：<https://iotsharp.net/docs/intro>
- 路线图： [ROADMAP.md](ROADMAP.md)
- 变更记录： [CHANGELOG.md](CHANGELOG.md)

## 🤝 参与贡献

欢迎通过 Issue 与 Pull Request 参与 IoTSharp：

- Pull Requests：<https://github.com/IoTSharp/IoTSharp/pulls>
- Issues：<https://github.com/IoTSharp/IoTSharp/issues>

在提交改动前，建议先阅读当前的文档结构、仓库模块划分以及发布与分发方向。

## 💬 社区支持

如果你在使用或部署 IoTSharp 时遇到问题，可以通过以下渠道获取帮助：

- GitHub：<https://github.com/IoTSharp/IoTSharp>
- Gitee：<https://gitee.com/IoTSharp/IoTSharp>
- 官网：<https://iotsharp.net>

## ❤️ 捐赠

IoTSharp 采用 Apache 2.0 协议发布。如果你希望支持项目持续发展，可以通过以下方式：

- OpenCollective：<https://opencollective.com/IoTSharp>
- 爱发电：<https://afdian.net/a/maikebing>
- 支持者列表： [BACKERS.md](BACKERS.md)

## ✨ 祝福

- 愿你行善而不作恶。
- 愿你学会宽恕自己，也宽恕他人。
- 愿你乐于分享，不取多于所予。
