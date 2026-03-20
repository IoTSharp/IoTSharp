---
title: Linux 部署
---

# Linux 部署

Linux 下目前推荐两种方式：

- Docker
- 手工发布包 + systemd 服务

## 现有 service 参考

代码库已经包含一个 Linux service 示例：

- `IoTSharp/iotsharp.service`

这个文件体现了几个关键部署约定：

- `WorkingDirectory=/var/lib/iotsharp`
- `ExecStart=/var/lib/iotsharp/IoTSharp`
- `Environment=ASPNETCORE_ENVIRONMENT=Production`

## Linux 手工部署建议

1. 准备发布目录，例如 `/var/lib/iotsharp`
2. 把发布包解压到该目录
3. 放置环境配置文件
4. 写入并启用 `systemd` 服务
5. 首次访问完成 Installer 初始化

## 后续交付方向

Linux 原生包后续建议按这个顺序推进：

1. `.deb`
2. `.rpm`

详细计划见 [发布分发计划](../operations/release-distribution-plan.md)。
