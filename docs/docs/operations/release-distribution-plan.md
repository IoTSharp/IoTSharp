---
title: 发布分发计划
---

# 发布分发计划

IoTSharp 当前的发布链路已经收敛为以 Git Tag 为中心的正式发布。

## 当前已支持

- Tag 触发 NuGet 发布
- Tag 触发 GitHub Release 二进制包
- Tag 触发容器镜像发布
- Docker 镜像同时推送到 GHCR 和 Docker Hub

## Windows 方向

Windows 侧已经开始建设 MSI 安装包，目标包括：

- 安装主程序
- 注册 Windows 服务
- 后续纳入 `IoTSharp.Agent`

## Linux 方向

Linux 建议按两个阶段推进：

1. 先稳定归档包和 Docker 交付
2. 再补 `.deb`，之后补 `.rpm`

## Pages 与文档发布

文档站点使用 Docusaurus，建议作为正式发布的一部分同步更新：

- GitHub Pages
- 自定义域名

## 下一阶段建议

1. 打通主程序正式 `win-x64 publish` 到 MSI 的全链路。
2. 把 `IoTSharp.Agent` 纳入安装包。
3. 为 Windows 安装包补自启动与升级说明。
4. 继续推进 Linux 原生包交付。
