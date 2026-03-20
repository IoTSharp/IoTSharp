---
title: 安装方式
---

# 安装方式

IoTSharp 推荐按以下优先级选择安装方式。

## 1. Docker 部署

适合：

- 快速试用
- 服务器部署
- 与 PostgreSQL、InfluxDB、RabbitMQ 等组合交付

优点：

- 环境一致性高
- 升级简单
- 便于 CI/CD

入口参考：

- `Deployments/` 下的组合部署目录
- [Docker 部署](../deployment/docker.md)

## 2. Windows MSI 安装

适合：

- Windows 服务器
- 需要系统服务注册
- 需要标准安装/卸载体验

特点：

- 安装后自动注册 `IoTSharp` Windows 服务
- 适合配合浏览器访问和后续 `IoTSharp.Agent` 托盘工具

## 3. 手工发布包

适合：

- 自定义安装目录
- 临时验证环境
- 对发布内容有二次封装需求

通常来自：

- GitHub Release
- 本地 `dotnet publish`

## 选择建议

- 首次体验：优先 Docker。
- Windows 正式交付：优先 MSI。
- Linux 正式交付：先用归档包或 Docker，后续再接 `.deb/.rpm`。
