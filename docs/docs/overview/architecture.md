---
title: 系统架构
---

# 系统架构

IoTSharp 当前由几个关键层组成。

## 前端层

- `ClientApp` 提供 Vue 3 管理端。
- 菜单由后端 `/api/Menu/GetProfile` 动态下发。
- 隐藏页面包括个人中心、规则链设计器、规则链模拟器、网关设计器、资产设计器和 Installer。

## 应用层

- `IoTSharp` 是 ASP.NET Core Web 应用，也是 Windows Service / Linux Service 的主宿主。
- 服务端包含设备、规则链、租户、告警、资产、证书、产品等控制器。
- 应用启动后会暴露 Web 页面、MQTT、健康检查与 MCP 接口。

## 数据层

基础数据和遥测数据可以按环境配置不同存储方案：

- PostgreSQL
- MySQL
- Oracle
- SQL Server
- Sqlite
- Cassandra
- ClickHouse
- InMemory

## 消息与规则处理

- 规则链负责把接入数据转成业务动作。
- 执行器支持脚本、告警发布、属性写入、遥测发布等行为。
- 规则链审计页可回溯执行记录。

## 运维交付层

当前代码库中已经存在或正在建设这些交付方式：

- Docker 镜像
- GitHub Release 二进制包
- Windows MSI 安装包
- `IoTSharp.Agent` 托盘辅助程序

这也是文档将“部署与配置”和“运维发布”单独拆开的原因。
