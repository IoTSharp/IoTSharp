<p align="left">
  <a href="https://iotsharp.net/">
    <img src="docs/static/img/logo_white.svg" width="360px" alt="IoTSharp logo" />
  </a>
</p>

# IoTSharp

[English](README.md) | [Chinese](README.zh.md)

[![License](https://img.shields.io/github/license/IoTSharp/IoTSharp.svg)](LICENSE)
[![.NET build](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-build.yml)
[![Docs deploy](https://github.com/IoTSharp/IoTSharp/actions/workflows/docs-deploy.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/docs-deploy.yml)
[![Release binaries](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-publish.yml)
[![NuGet packages](https://github.com/IoTSharp/IoTSharp/actions/workflows/pack-nupkg.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/pack-nupkg.yml)
[![Docker images](https://github.com/IoTSharp/IoTSharp/actions/workflows/docker-release.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/docker-release.yml)
![Docker Pulls](https://img.shields.io/docker/pulls/maikebing/iotsharp)
![GitHub all releases](https://img.shields.io/github/downloads/IoTSharp/IoTSharp/total)

IoTSharp is an open-source industrial IoT platform for device access, telemetry collection, rule-chain processing, visualization, multi-tenant operations, and product delivery.

## 🚀 Overview

IoTSharp brings together the core building blocks needed to run an IoT platform in production:

- Device and gateway connectivity with HTTP, MQTT, CoAP, and extensible protocol integration.
- Telemetry, attributes, alarms, products, assets, and tenant-aware management models.
- Rule-chain driven processing for transformation, notification, automation, and business actions.
- Relational and time-series storage options for different deployment and scaling needs.
- Multiple delivery modes including Docker, Windows service, Linux service, installer flows, and release artifacts.
- The roadmap now also treats AI workbench, MCP tools, and agent-assisted operations as a cross-cutting capability for collection, rules, and release workflows.

The current main application targets `.NET 10`, and the web console is maintained as an IoTSharp-branded Vue 3 application.

## 🧱 Product Matrix (Cloud-Edge-Device)

IoTSharp is not a single platform application but an open-source product matrix covering three layers plus an AI foundation:

| Layer | Project | Description |
| --- | --- | --- |
| Platform | [IoTSharp](https://github.com/IoTSharp/IoTSharp) (this repo) | Control plane for device access, telemetry, rule chains, multi-tenancy, EdgeNode management, and release operations |
| Edge | [IoTEdge](https://github.com/IoTSharp/IoTEdge) | Edge gateway runtime: single-host executable with a local management UI, built-in Modbus, OPC UA, and mainstream PLC collection drivers plus script-based transformation; integrates with the platform for registration, heartbeat, capability reporting, collection-config rollout, and task receipts |
| Device | [IoTEmbedded](https://github.com/IoTSharp/IoTEmbedded) | Embedded device runtime for MCU/RTOS targets: firmware-level runtime with a built-in BASIC script engine, Modbus RTU, and MQTT access, supporting dual-slot script storage with failure rollback |
| AI foundation | [Tomur](https://github.com/IoTSharp/Tomur) | Local model runtime: hosts the platform's AI capabilities in offline/intranet environments, with GGUF LLM, speech, image, and OCR multimodal inference behind OpenAI-compatible APIs, deployable as a single file |

All layers are open source and independently usable, and together they form an end-to-end industrial collection and operations solution; in offline or intranet scenarios, combining SonnetDB and Tomur enables a fully functional deployment with zero external dependencies.

## 🧭 Quick Start

### 🖥️ Run IoTSharp locally

The recommended documentation entry points are:

- Product docs: <https://iotsharp.net/docs/intro>
- Installation options: <https://iotsharp.net/docs/getting-started/installation-options>
- Installer guide: <https://iotsharp.net/docs/getting-started/installer>
- Docker Desktop extension: <https://iotsharp.net/docs/deployment/docker-desktop-extension>

For frontend development, the current local dev server default is:

- Frontend: `http://localhost:27915`
 

## 📚 Documentation

- Chinese README: [README.zh.md](README.zh.md)
- Roadmap: [ROADMAP.md](ROADMAP.md)
- Changelog: [CHANGELOG.md](CHANGELOG.md)

## 🤝 Contributing

Contributions are welcome through issues and pull requests:

- Pull requests: <https://github.com/IoTSharp/IoTSharp/pulls>
- Issues: <https://github.com/IoTSharp/IoTSharp/issues>

Before contributing, please review the codebase structure, related documentation, and the current release/distribution direction in the docs site.

## 💬 Community Support

If you need help using or deploying IoTSharp, community channels are available in the docs and community materials:

- GitHub: <https://github.com/IoTSharp/IoTSharp>
- Gitee: <https://gitee.com/IoTSharp/IoTSharp>
- Official site: <https://iotsharp.net>

![微信群](docs/static/img/qyqun.jpg)


## ❤️ Donation

IoTSharp is released under the Apache 2.0 license. If you would like to support the project, you can back it through:

- OpenCollective: <https://opencollective.com/IoTSharp>
- Afdian: <https://afdian.net/a/maikebing>
- Backers list: [BACKERS.md](BACKERS.md)

## ✨ Blessing

- May you do good and not evil.
- May you find forgiveness for yourself and forgive others.
- May you share freely, never taking more than you give.
