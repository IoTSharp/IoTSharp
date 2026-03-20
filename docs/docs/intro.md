---
sidebar_position: 1
title: 使用手册总览
---

# IoTSharp 使用手册

这套文档按照 IoTSharp 当前产品菜单重新整理，目标是让你从安装到上线都能沿着界面路径找到对应说明。

## 文档结构

- 平台概览：了解 IoTSharp 的定位、能力边界和系统组成。
- 快速开始：适合第一次部署，涵盖安装方式、Installer 初始化和首次登录。
- 部署与配置：聚焦 Docker、Windows 服务、Linux 和 `appsettings` 配置。
- 用户手册：直接对应产品菜单，包括仪表盘、设备、规则链、告警、产品、资产和系统管理。
- 集成接口：总结协议接入方式与 Web API 使用方式。
- 运维发布：面向发布、排障与交付。

## 与前端菜单的对应关系

当前后端菜单主要分为以下区域：

- 仪表盘
- 数字孪生
  - 设备管理
  - 设备告警
  - 规则链设计
  - 规则链审计
  - 场景
- 产品管理
  - 产品列表
- 资产管理
  - 资产列表
- 系统管理
  - 证书管理
  - 租户列表
  - 客户列表
  - 用户列表

此外还有登录页、Installer 初始化页、个人中心，以及设计器这类隐藏页面。

## 推荐阅读顺序

1. 先看 [安装方式](./getting-started/installation-options.md)。
2. 完成 [Installer 初始化](./getting-started/installer.md)。
3. 再按业务路径阅读 [仪表盘](./user-guide/dashboard.md)、[设备与网关](./user-guide/devices-and-gateways.md)、[规则链与场景](./user-guide/rules-and-scenarios.md)。
4. 上线前补看 [Docker 部署](./deployment/docker.md)、[Windows 服务](./deployment/windows-service.md) 和 [发布分发计划](./operations/release-distribution-plan.md)。
