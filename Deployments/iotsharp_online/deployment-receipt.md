# Online 发布回执

## 2026-10-17 Studio 配置与九模型更新

最终发布目录：`/opt/iotsharp-releases/20261017-studio-config-v2/demo`，当前链接已实测指向该目录。v1 候选 `20261017-studio-config` 因 SonnetDB Web 基路径遗漏而未作为最终版本保留；v2 使用 `SONNETDB_WEB_BASE_PATH=/sonnetdb/` 重新构建。

- SonnetDB 源提交：`d8d83c5b`（含上游 `4d6e4f98` 及本轮 Studio 配置、诊断、邮箱账号/诊断脱敏补丁）。IoTSharp 使用同一子模块指针构建；SonnetDB 与 IoTSharp Linux 自包含发布均成功。
- 新增设置页：数据库参数、管理员实例配置、连接检测、浏览器观测刷新偏好。配置端点只读，秘密/凭据脱敏；demo 账号访问 `/v1/system/configuration` 为 403，只能读取 `demo` 数据库配置。
- 新增观测页：授权数据库九模型数量、存储/WAL、自检和慢查询状态。当前 demo 浏览器回读为 measurement 3、table 1、document 1、KV 3、全文 1、向量 1、对象 1、MQ 1、图 1。
- 验证：SonnetDB 配置/认证/诊断 **59/59**；Web 合同 **469/469**；桌面/手机 SonnetDB 浏览器 **PASS**；demo 设置页与 admin API 配置权限 **PASS**；IoTSharp 定向测试 **36/36**。构建保留已有警告，不宣称全仓 CI、NativeAOT 发布或完整现场验收。
- 数据和账号边界：九模型合成数据只写 demo 库；注册用户、管理员及业务数据未清理；30 分钟演示密码 timer active；服务与 Nginx active；v2 更新前备份和原版本均保留。

详细证据：`D:\GitHub\IoTSharp\artifacts\demo-models-20261017\` 下的发布、浏览器、配置 API、测试和哈希回执；不要把本机凭据、Token 或管理密码写入仓库。

后续前端展示更新已发布为 `20261016-public-demo-access`，后端程序集未变；见 [演示信息公开回执](demo-access-publication.md)。本文以下保留 r2 后端和业务验收记录。

日期：2026-10-16（Asia/Shanghai）。本轮部署和公开业务验证通过。

## 入口和账号

| 产品 | 入口 | 演示账号 | 管理员 |
| --- | --- | --- | --- |
| IoTSharp | `https://iotsharp.online/#/login` | `demo@iotsharp.online` | `admin@iotsharp.online` |
| SonnetDB | `https://iotsharp.online/sonnetdb/admin/login` | `demo@iotsharp.online` | `admin@iotsharp.online` |

密码均按用户指定配置，本文不保存密码。IoTSharp 演示用户属于独立演示租户、没有 SystemAdmin；SonnetDB 演示用户不是超级用户，只能访问 `demo` 库，访问内部 `iotsharp` 库已实测 403。IoTSharp 后台使用独立服务身份。旧公开超级用户及旧 IoTSharp JWT 会话已退休。

当前发布目录：`/opt/iotsharp-releases/20261016-online-67a5c8c8-r2/demo`。SonnetDB 为本轮核实的 2026-10-16 主干 `67a5c8c8d64c36002aee5ebcfcb40634e7d5f482` 加邮箱用户名与密码 SQL 诊断脱敏补丁，**不是新的正式 Release**。IoTSharp 及其 SonnetDB 客户端采用同一子模块基线；原 `D:\source\SonnetDB` 的外部脏改动未修改。

## 验证结果

- IoTSharp 定向测试 **36/36**、SonnetDB 认证/权限/诊断 **53/53**、控制面解析 **52/52**，总计 141 项，0 失败/跳过。已有构建警告保留；未宣称全解决方案或远端 CI 通过。
- 两个产品的四个账号均通过真实 Chrome 登录。SonnetDB 静态模块曾因 API 连接限额返回 503，已分离 `/sonnetdb/assets/` 并通过复验。
- `iotsharp-online-demo-passwords.timer` 在 **16:30:04+08:00** 自动触发，16:30:07 退出 0。另在主动改动两个演示密码后于 16:41:48 执行恢复，16:41:52 退出 0，两个账号重新登录成功。
- 公开注册创建独立验收租户 `verification-20261016@iotsharp.online`。它在密码恢复后仍可用原密码登录；管理员、注册用户、SonnetDB 非演示账号的凭据指纹均未变。验收租户保留以复核持久性，随机密码仅在服务器 root-only 配置中。
- Product `6cc03f5a-d365-4d35-a478-9f67b29c7faa` 和设备 `bac93e5b-b3f4-4a78-930c-3576af94af85` 使用合成演示数据：温度 23.75、湿度 61。真实 HTTP 上报经 SonnetMQ 消费并从 SonnetDB 查询成功，其它租户无法读取该设备。
- 对象上传及下载内容一致，跨客户对象读取返回 403；列表按客户前缀查询。重启两个服务后，遥测、对象和注册用户仍保留。
- SonnetDB `demo.OnlineDemoTelemetry` 已由演示账号写入并查询合成数据；未向演示账号开放内部业务库。

脱敏回执：[实际进程与程序集](receipts/final-runtime.json)、[业务验收](receipts/business-acceptance.json)、[SonnetDB 浏览器](receipts/sonnet-browser.json)、[自动恢复后的凭据保留](receipts/scheduled-reset-preservation.json)、[SonnetDB 演示数据](receipts/sonnet-demo-data.json)。测试 TRX 和进程日志位于本机 `artifacts/online-20261016/`。

## 更新和恢复边界

交付仓为 `D:\OpenSource\IoTSharp.Platform`。首次配置使用 `deploy/native/scripts/activate-online-release.sh`；后续发布使用 `update-online-release.sh <release-root>`，不再次引导管理员或重建数据。本次 r2 已验证后续更新流程：停写备份、新目录切换、服务恢复、Nginx 验证、定时器恢复及账号数据保留。

升级前和 r2 前备份分别在 `/opt/iotsharp-backups/online-20261016-before-upgrade/`、`/opt/iotsharp-backups/20261016-online-67a5c8c8-r2/`，带 SHA-256。升级前副本已用新 SonnetDB 打开，并通过 IoTSharp 启动、迁移和 Identity 维护验证。

最新 SonnetDB 包含 v15/v16 目录升级屏障，不能让旧二进制直接读取新目录。跨版本降级必须使用兼容的升级前副本并处理期间新增数据；本轮没有在公网执行破坏性降级，不以目录链接存在冒称已完成生产降级演练。

本机技能为 `C:\Users\mysti\.codex\skills\iotsharp-online-release\SKILL.md`，SSH 别名 `iotsharp-online` 已验证。应用密码保存在 Windows DPAPI 加密凭据；一次性服务器管理员引导文件已删除，定时任务仅保留演示密码。所有源码、子模块补丁和技能改动尚未提交或推送。

三仓补丁及新增源文件已保存到本机 `artifacts/online-20261016/source-evidence.tgz`，哈希见 [源码证据](receipts/source-evidence.json)。29 个有界执行进程和 8 个浏览器会话检查均无本任务残留；服务器候选 systemd 服务已退出，任务专用数据副本和误基路径的前端输出已回收，正式服务、发布目录与备份保留。

本次是演示与注册运行环境验收；不提升真实现场 O1→O5、Gateway/EdgeNode、工业预测质量或所有数据库模型的平台业务完成状态。
