# 演示信息公开回执

2026-10-16（Asia/Shanghai）。用户明确授权公开受限演示凭据，并要求正常注册账号的长期保留政策写清楚。

## 已上线的演示首页

| 产品 | 首页 | 演示账号 | 公共密码 |
| --- | --- | --- | --- |
| IoTSharp | `https://iotsharp.online/` | `demo@iotsharp.online` | `P@ssw0rd` |
| SonnetDB | `https://iotsharp.online/sonnetdb/` | `demo@iotsharp.online` | `P@ssw0rd` |

两页均展示正确的登录链接，并明确：

> 仅公共演示账号的用户名和密码每半小时恢复一次。正常注册账号可用于测试服务，账号、密码和业务数据均不参与演示重置，长期保留。

活动发布为 `/opt/iotsharp-releases/20261016-public-demo-access/demo`。本轮只发布前端资源，IoTSharp/SonnetDB 后端程序集哈希与 r2 相同；正常账号和数据通过保留数据的更新流程继续使用。SonnetDB 演示密码已恢复为公开值并验证登录，管理员和注册用户的凭据基线未变。

## 验证与其它门面

- IoTSharp Vite 构建、SonnetDB TypeScript/Vite 构建通过。
- 1365px 桌面与 390px 手机宽度下，两页均显示账号、密码、长期保留政策和正确链接，没有横向溢出；截图已人工检查。
- 383 个静态文本资源检查未发现管理员私密密码。公开信息中不含管理员或后台服务凭据。
- IoTSharp 中英文 README、SonnetDB 当前子模块与 canonical 仓的中英文 README 已更新；GitHub/Gitee 尚未提交推送，不能称远端 README 已更新。
- `iotsharp.net` 的两个产品介绍页已准备同样内容并通过 Next.js 构建；`www.sonnetdb.com` 对应 Hero 已准备源码。115.29.197.222 的 SSH/HTTPS 连接超时，独立官网更新 **NOT_DEPLOYED**；没有重启或改写门面机业务服务。

浏览器回执见 [demo-access-browser.json](receipts/demo-access-browser.json)。构建、上传和过程证据保存在本机 `artifacts/demo-access-20261016/`；本轮不重复声称后端全量或现场业务测试，前一轮业务验证仍见 [后端部署回执](deployment-receipt.md)。所有原有工作树改动保留。
