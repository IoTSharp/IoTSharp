# Studio 配置与九模型更新回执

日期：2026-10-17（Asia/Shanghai）

## 发布

- 主机：`iotsharp.online / 111.228.23.26`
- 发布链接：`/opt/iotsharp-demo/current` → `/opt/iotsharp-releases/20261017-studio-config-v2/demo`
- SonnetDB：`d8d83c5b`，已推送 `codex/online-demo-service-20261016`
- 演示账号：`demo@iotsharp.online`，只允许 `demo` 数据库；实例配置端点返回 403
- v1 候选因漏传 `SONNETDB_WEB_BASE_PATH=/sonnetdb/` 未作为最终版本使用；v2 已修正并重新构建

## 变更

- 设置页新增数据库参数和整个实例配置。配置为只读快照，路径、端口、预算、WAL、KV、SQL、观测、协议和 AI 状态可见；凭据、Token、密码和连接秘密只显示“已配置/未配置”。
- 观测页新增授权数据库九模型目录、存储/WAL、自检和慢查询状态；普通账号不请求实例 Prometheus 指标。
- 演示 `demo` 库包含九模型合成数据：72 条时序、6 条关系、6 份文档、6 个 KV、全文和向量查询样例、3 个对象、8 条 MQ、7 个图节点和 6 条边。

## 验证

- SonnetDB 配置/认证/诊断：59/59
- Web 合同：469/469
- IoTSharp 定向测试：36/36
- SonnetDB Studio 浏览器：1440px 与 390px，设置/观测/工作台 PASS，无页面异常和横向溢出
- demo 设置页：数据库参数可见，实例级参数未泄露
- admin API：实例配置可见；demo API：实例配置 403、demo 数据库配置可见
- 服务：`iotsharp-demo.service`、`sonnetdb-demo.service`、`iotsharp-online-demo-passwords.timer` active；健康接口 200

未宣称完整全仓 CI、NativeAOT 发布、所有生产现场能力或真实业务数据质量验收。合成向量仅为界面和查询演示数据。
