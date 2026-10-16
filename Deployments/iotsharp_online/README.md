# IoTSharp Online 持久服务迁移

日期：2026-10-16（Asia/Shanghai）。本轮部署与公开业务验收已完成，当前状态见 [最终发布回执](deployment-receipt.md)。以下保留初始盘点记录；其中“待验收/NOT_RUN”描述不覆盖最终回执。

用户目标：`iotsharp.online / 111.228.23.26` 承载 IoTSharp 与 SonnetDB 演示、注册及用户接入；两个产品各有 `demo@iotsharp.online`，30 分钟只恢复该演示账号的用户名与密码；管理员为 `admin@iotsharp.online`，其他账号及业务数据保留。密码只存在受保护的凭据存储，不写入本文件。

## 已核验

- SSH 使用本机 `iotsharp-online` 别名，专用私钥位于 `C:\Users\mysti\.ssh\iotsharp_saas_prod_ed25519`。主机名 `lavm-2mvzje6iag`，ED25519 指纹 `SHA256:ZzS3L/fa4kbpqQgBm25LN+8NvDnc7bmle9FI7f+3zEw`。
- 交付仓 `D:\OpenSource\IoTSharp.Platform`；原生 systemd 服务 `iotsharp-demo.service`、`sonnetdb-demo.service`；HTTP 分别在回环 `32927`、`35080`。
- 当前发布链接 `/opt/iotsharp-demo/current` 在盘点时指向 `20260826T055651Z-4a152706be22/demo`，IoTSharp 报告 3.5.0.0；尚未部署本轮代码。
- GitHub 最新正式 Release 重定向为 `v4.0.0`；2026-10-16 本次读取的 SonnetDB 主分支为 `67a5c8c8d64c36002aee5ebcfcb40634e7d5f482`，提交时间 14:37:44+08:00。本仓 SonnetDB 子模块已切到该版本，账户邮件名支持属于本轮本地补丁。
- 发现旧 reset 每 30 分钟重建所有数据，guardian 每 5 分钟检查全部用户哈希并可能触发整库 reset。已停用两个本机旧 timer，并给旧 reset 服务加持久模式条件，阻止旧脚本及 facade 远程入口清库；原应用服务仍 active。
- 防护脚本位于交付仓 `deploy/native/scripts/enable-online-persistent-mode.sh`，原状态在服务器 `/var/lib/iotsharp-deploy/online-persistent-mode/`，标记为 `/etc/iotsharp/online-persistent-mode`。不要删除该标记或复用旧 `activate-release.sh demo --start`。

## 当前开发与待验收

- 本机维护命令：通过 Identity 服务首次迁移旧管理员并创建独立演示租户，之后通过固定用户 ID 仅恢复演示用户名与密码。不得用全表更新或全库重建实现。
- 自助注册必须建立独立租户，已有租户用户邀请必须鉴权并验证范围；客户管理员不得创建系统管理员。
- 最新 SonnetDB 集成覆盖关系/EF、遥测、KV 缓存、对象、SonnetMQ、查询下推和需要的运维入口；尚需按能力逐项给出实测状态。
- SonnetDB 管理端计划使用 `/sonnetdb/`；演示账号只授权自己的 demo 数据库，不得读取 IoTSharp 内部库。
- 仍需完成构建、测试、数据备份、发布、新账号登录、周期恢复、注册隔离、真实数据写入回读、重启保留与回滚验证。

本机执行日志与进程回执位于 `artifacts/online-20261016/`；其中初始构建因 Testcontainers 版本降级失败，升级到 4.16.0 后正在继续，不能抹除失败证据。生产变更只实施了阻止旧整库复位的保护措施，账号修改、应用升级和新管理入口仍为 `NOT_RUN`。
