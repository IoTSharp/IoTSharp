# IoTSharp 本次路线规划交接

## 2026-10-17 SonnetDB Studio 配置与九模型演示更新

- SonnetDB 子模块已指向 `d8d83c5b`，其配置/Studio变更已推送到 `codex/online-demo-service-20261016`；IoTSharp 主仓仍保留本轮账号隔离、租户注册、SonnetDB 适配和对象路径改动。
- Studio 新增“设置 → 数据库参数”和管理员可见的“整个实例的配置”。启动参数只读展示并脱敏；数据库参数按现有数据库授权读取。普通 `demo@iotsharp.online` 不能读取实例配置或其它数据库。
- 观测页继续展示授权数据库的九模型目录、存储/WAL、自检和慢查询状态；设置、观测、工作台在 1440px/390px 浏览器复验通过。前端合同 469 项、SonnetDB 配置/认证/诊断测试 59 项通过。
- 演示主站已更新到 `/opt/iotsharp-releases/20261017-studio-config-v2/demo`。第一次发布因遗漏 `SONNETDB_WEB_BASE_PATH=/sonnetdb/` 仅保留为失败候选；v2 已用正确子路径重新构建。数据、账号、定时器和旧版本备份保留，公网浏览器与 API 权限复验通过。
- 九模型合成数据回执保留：时序 72、关系 6、文档 6、KV 6、全文查询 1、向量查询 3、对象 3、MQ 8、属性图 7 节点/6 边。合成向量仅用于演示，不代表模型质量评估。

## 2026-10-16 演示入口公开说明

两套演示首页发布到 `20261016-public-demo-access`，显示公共演示地址、账号和密码，并明确“正常注册账号可用于测试服务，账号、密码和业务数据均不参与演示重置，长期保留”。IoTSharp 中英文 README、SonnetDB 子模块及 canonical 仓的中英文 README 同步补充，旧 `host.iotsharp.net` 链接已替换。本轮仅调整前端展示和文档，不修改后端账号重置逻辑；SonnetDB 演示密码已通过产品 API 恢复并验证。独立官网源码已准备，但门面机连接超时尚未上线；仓库文件仍未提交推送。

## 2026-10-16 续接：Online 演示与持久 SaaS（本轮已上线验收）

本轮用户已授权将 `111.228.23.26 / iotsharp.online` 用作 IoTSharp 与 SonnetDB 演示、注册和对接服务，采用最新 SonnetDB；两个产品的 `demo@iotsharp.online` 每半小时只恢复用户名和密码，管理员 `admin@iotsharp.online` 与其它用户不参与周期重置。具体密码只保存在本机 DPAPI 凭据文件，不能进入文档。

当前进度、真实现场变化与未完成验收见 [Online 迁移记录](Deployments/iotsharp_online/README.md)。本仓开始时干净；`D:\source\SonnetDB` 的大量外部脏改动保持不动。本轮仅更新本仓 SonnetDB 子模块和自有改动，并在 `D:\OpenSource\IoTSharp.Platform` 维护原生部署脚本。已有专用 SSH 密钥与 `iotsharp-online` 别名；不要再要求用户重复提供登录入口。

当前服务已在 `20261016-online-67a5c8c8-r2` 上线；四个账号浏览器登录、注册隔离、实际遥测/MQ/对象读写、演示密码恢复及重启保留通过。16:30 定时执行与管理员/注册用户密码不变已有现场证据。最终状态、源码未提交边界、备份及降级限制见 [发布回执](Deployments/iotsharp_online/deployment-receipt.md)；不提升 O1→O5 现场状态。

日期：2026-10-14，Asia/Shanghai。

## 本次范围与起点

- 用户要求把 Ontology Engineering 应用规划纳入 IoTSharp 路线图，并提交、推送。
- 起点为 `master` / `bbc7e6a8f3c1dce54a92af66b8955b7b1d29d4e0`；开始时工作区与暂存区干净，已独立查询 `origin/master`，远端 SHA 与起点相同。
- 开始时仓库缺少根 `HANDOFF.md`，本文件补充本次文档任务的交接，不替代 `ROADMAP.md` 的唯一计划职责。
- 当前授权范围是路线规划与对应文档提交；应用实现、生产访问、数据导入、部署和模型训练未在本次执行。

## 当前进展

- 已核对领域边界、设备监测专项、O1→O5 生产启用顺序和 AI 跨仓分工。
- 已登记应用轨道 K 和 #180~#185：本体定义、版本化映射、Sezika 类型化适配、审核入库、追溯查询与独立评测；首个试点为设备—告警—维修工单，所有实现项保持待开始。
- 已同步中英文 README 的规划入口及 CHANGELOG 的文档变更记录。
- 本次归属文件限定为 `ROADMAP.md`、`README.md`、`README.zh.md`、`CHANGELOG.md`、`HANDOFF.md`；不修改源码、子模块和现有生产状态。

## 验证与后续

- 文档差异、唯一锚点、README 链接、#180~#185 唯一编号与待开始状态、UTF-8 内容检查通过；提交前复核最终暂存树并执行 `git diff --cached --check`。
- 编译、测试、模型推理、领域质量、现场及部署验收：`NOT_RUN`，本次没有对应实现变更。
- Git 交付分支为 `master`，提交消息为 `docs: plan ontology engineering application with Sezika`，目标为 `origin/master`；准确提交和远端同步结果由本次 Git 执行回执及独立 `git ls-remote` 核验，不以规划文档代替远端证据。
- 当前生产主线仍从 O1 业务盘点与评审继续，后续依次遵守 O2→O3→O4→O5；规划记录不提升任何生产或模型能力的完成状态。
- 下一应用工作从 #180 开始：选择并确认业务范围、列出待回答问题、盘点术语/关系、建立有来源的人工金标；#181~#185 按路线依赖接续。
- 本任务没有启动服务、训练、下载或后台作业，没有留下任务临时文件；有界远端 Git 查询已退出，直接子进程检查为 0，后续 Git 操作也须使用有界等待并核对退出与回收结果。
