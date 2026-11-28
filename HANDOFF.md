# IoTSharp 本次路线规划交接

## 2026-11-28（Asia/Shanghai）IoTSharp Client M1 会话

- 子模块升级至 `IoTSharp/Client main@d5f1ea1a2a696632f45228b9dc719303ff00428f`。客户端接入平台令牌续期，清理已过期或切换实例的会话，区分 401 和 403，兼容平台业务成功码 `10000`。
- Windows 目标构建退出码为 0，有 37 条警告，应用启动到登录页。演示站验证码端点返回 HTTP 200、业务码 `10000` 和两张验证码图。本轮启动的客户端进程已回收。
- C06 实现完成。C07 仍需人工完成演示账号滑块，并读回登录、选设备、最新遥测和退出。运行时续期、自动化测试和其他平台构建待执行。

## 2026-11-28（Asia/Shanghai）IoTSharp Client 接入与 .NET 11 起步

- IoTSharp 父仓通过 `Client/` Git 子模块跟踪 `IoTSharp/Client` 的 `main@6dca11e6d8e498028a5d7f62f6ee6d60771d11c5`，客户端路线图、MAUI 升级和首批源码随该提交交付。
- 客户端采用 .NET SDK `11.0.100` 系列和 MAUI Controls `11.0.10`，目标为 Windows、Android、iPhone 和 Mac Catalyst。子模块内沿用客户端自己的包版本配置。小程序首版按微信小程序规划，单独实现前端并复用 IoTSharp API。
- 已实现桌面并排和窄屏纵向布局、验证码等比例缩放、实例地址切换和登录请求修复，以及登录后首个设备的数据自动加载。`Client/ROADMAP.md` 记录 5 个里程碑及 18 个有序任务，当前下一项为 C06 的令牌续期与过期处理。
- Windows 目标构建通过，命令为 `dotnet build IoTSharp.Client/IoTSharp.Client.csproj -f net11.0-windows10.0.19041.0 -p:TargetFrameworks=net11.0-windows10.0.19041.0 --no-restore`，执行目录为 `Client/`。依赖通过本机代理还原，首次编译输出含 WinRT AOT 兼容性警告。真实业务联调、自动化测试和其他平台构建待执行。本轮启动的构建进程已退出，常规编译产物留在客户端的忽略目录。

## 2026-11-28（Asia/Shanghai）SonnetDB #425 配置操作租约候选

- 提交前，四个改动的 C# 文件通过 `dotnet format style` 和 `dotnet format analyzers` 的 error 级别检查。`.gitattributes` 对 C# 指定 LF，当前 `.editorconfig` 的换行检查要求 CRLF。综合格式命令触发 ENDOFLINE 并达到输出上限，源码和暂存区保留 Git 规范化的 LF。

- 最终二进制的完整 25 项以 `--logger "trx;LogFileName=iotsharp-lease-official-final-serial.trx"` 和 `-- xUnit.ParallelizeTestCollections=false xUnit.MaxParallelThreads=1` 串行复跑，25/25 PASS，零失败零跳过。使用相同的正式包和固定镜像摘要。运行 PID 74156，约 84 秒，退出 0，Cordonix 回收后的任务进程数为 0。TRX SHA-256 为 `02719D07C835875D304A9066B9CF407717820B871D012E7473F7D452B5D804D2`。按回执逐一查询 `42df1113e439`、`59b6ff77559b`、`6577cd72132b`，均已回收。

- 专用工作树 `D:\GitHub\IoTSharp-issue-425-cross-instance-lease`，交付分支 `codex/issue-425-cross-instance-lease`，基线 `a8a26743a523405ca37d41df792931f37b11db26`。按 gitlink 恢复 JsonDB `38c0ff1dfac8be1164fe4ffe15259f36dd16bd0a`。已获授权提交、回复 SonnetDB #425 并推送交付分支。生产 provider 准入等待上游发布验收。
- 测试宿主启用的远程 SonnetDB 候选要求服务端确认 Serializable 事务会话，使用带 Gateway 参数的 `SELECT ... FOR UPDATE NOWAIT` 持行锁至事务结束，只将 `sql_lock_not_available` 映射为忙碌。继续要求条件写入得到准确影响行数，并以 Gateway 和本次租约 ID 条件释放。提交尝试发出后保留原异常，禁止重复尝试同一个句柄。生产 DI 仍拒绝 SonnetDB，嵌入式也继续拒绝。正式 5.2.0 的事务开始取消、提交异常与存储恢复仍有上游缺口。
- 七个 5.2.0 包都从 nuget.org 读取，nuspec 均绑定 `cbbe5ed2e64879ea6fb7650d1d7b3ff4eee2909d`，与测试使用的镜像 `iotsharp/sonnetdb@sha256:bc0c473ba905d60c7d4db4f0d48b53dccb0069f31884a3d1ccadb5a2cc5da449` 匹配。Release 测试输出的 EF、ADO DLL SHA-256 分别为 `153154647EB2DFB820D60DE8F031266E239C0E48E0637253D478B746159DBEF5`、`8B4EB16919591857B845D01477DFD9DB0D15AE210D89C50BCD832AD07A0906E5`。相关依赖及目标框架维持现有版本。
- 在工作树执行 `dotnet restore IoTSharp.Test/IoTSharp.Test.csproj --configfile IoTSharp.Test/obj/issue425-official/NuGet.config --packages IoTSharp.Test/obj/issue425-official/packages --disable-parallel -m:1 -nr:false`，隔离官方 SonnetDB 源，已知本地缓存只补其它包。Restore PASS，NuGet 审计报告既有依赖告警。完整构建 FAIL，ClientApp 的 npm build 返回 MSB3073。使用 `dotnet build IoTSharp.Test/IoTSharp.Test.csproj -c Release --no-restore --disable-build-servers -m:1 -nr:false -p:UseSharedCompilation=false -p:GeneratePackageOnBuild=false -p:ShouldRunNpmInstall=false -p:ShouldRunBuildScript=false -p:StaticWebAssetsEnabled=false` 后端构建 PASS。首轮 62 个告警，隔离夹具后的最终增量构建 55 个告警，均为本轮未改动处，0 错误。
- 首轮以 `IOTSHARP_SONNETDB_LEASE_QUALIFICATION=1` 和 `SONNETDB_TEST_IMAGE` 固定镜像执行 `dotnet test IoTSharp.Test/IoTSharp.Test.csproj -c Release --no-build --no-restore --disable-build-servers -m:1 -nr:false --filter "FullyQualifiedName~ConfigurationOperationLeaseTests|FullyQualifiedName~AppWithSonnetDBTest" --logger "trx;LogFileName=iotsharp-lease-official.trx" --results-directory IoTSharp.Test/obj/issue425-official`。22 PASS、1 FAIL、0 跳过。5 个候选租约用例、SQLite 回归、原 SonnetDB 安全拒绝、EdgeRuntime 配置业务均 PASS。失败项为 `AppWithSonnetDBTest` 跨测试共享设备库的分页断言，租约用例移入专属容器后以相同命令复测，TRX 文件名改为 `iotsharp-lease-official-final.trx`，最终 24/24 PASS，零失败零跳过。6 个远程租约用例包含真实 HTTP 进入提交阶段时注入异常，500 返回后从独立 DbContext 确认旧配置版本、唯一 active assignment 和保留属性版本仍为 1，租约为空。
- 加入提交后的应用故障用例，最终后端构建 PASS。全组 25 项重跑到 300 秒时终止，TRX 未生成，结果 UNKNOWN。随后在同一最终二进制按 `FullyQualifiedName~CommitStageFailure_LeavesNoConfigurationVersionOrAssignment|FullyQualifiedName~CommittedBeforeHttpFailure_CanReconcileByCallerSourceId` 定向验证，两项均 PASS。第二项在数据库确认提交后使 HTTP 返回 500，新客户端使用原 `SourceId` 读回唯一配置版本，另一个 DbContext 确认唯一 active assignment 及空租约。SQL 会话的提交响应丢失由专门的传输探针验证。两组有效回执为 `iotsharp-lease-official-final.trx`（24/24）与 `iotsharp-lease-official-fault-pair-results.trx`（2/2）。
- 对账入口 `GET /api/Edge/{id}/CollectionConfigVersions?sourceId=...`。直接保存允许调用方在请求中填全局唯一 `SourceId` 并据此查找，但该字段可空可重用，API 不保证以它幂等，重复发送可能产生另一版本。模板与 Release 还可按请求中的 TaskId 或已知计划 ID 查询。直接保存和模板仍忽略保留属性 `AttributeLatest` 的逐键准备错误，该检查与五表故障验证分别记录。正式包跨真实 IoTSharp 进程故障验证和准入尚未完成。
- 构建均限制最多 2 个逻辑 CPU 与单 MSBuild 节点。Cordonix 已回收本轮任务进程。首轮容器 `183c9b859c80`、`c2cbf954ce3d` 与复测容器 `41892b6ad79a`、`c4b6a4f9bf27`、`e09290dc90dc`，以及故障用例容器 `a94b277c7d4f`、`1883dc165902` 已逐项查询确认不存在。25 项超时和故障定向测试的已记录进程也均已回收。CDX/TRX 回执和复现所需的隔离包目录保存在 `IoTSharp.Test/obj/issue425-official`。

## 2026-11-25（Asia/Shanghai）SonnetMQ 属性批处理与写入恢复

- 属性消费继续每条消息调用一次 `SaveChanges`，统一由 SonnetDB 服务端、ADO.NET 和 EF Core provider 协商有序事务 DML 批处理。现有公开包版本沿用配置。隔离候选联调通过，测试逐条核对两条 QoS 1 消息的入队和消费位点，并在确认后读取属性值。
- SonnetMQ 在存储明确拒绝且尚未写入时保留消费位点，以可取消的 1 至 30 秒退避重试。批次可能已有写入时，停止当前订阅以待核对。暂停状态保存在进程内，重启前先核实可能已写入的前缀。遵守死信写入成功后推进位点和正常批次成功后累计确认的既有流程。
- 工作树基线为 `master@8b96db8fe4f0d4bef3318eeade2257cf3a768a7b`，该提交包含消费开销优化与分段诊断。写入保护定向测试覆盖门禁退避、部分写入、批次回退、普通错误转死信及累计确认。候选镜像分别与既有服务端和支持稳定错误码的服务端完成本地配对启动。
- 194 已按现场回执切换到含写入门禁、时序封存与压缩和队列冷读修复的候选。最后记录的服务健康与属性消费已核验，遥测实时确认、积压和确认后业务字段仍为 UNKNOWN。详细发布及现场证据保存在本地 `output/release-194-sonnetmq-dml-batch-20261124/README.md`。
- 生产凭据轮换、MQTT 入队结果传播、真实规则负载以及历史死信的核对仍待专门处理。历史隔离联调已回收进程、容器及卷，工具审批拒绝递归删除的两处工作目录已在本地回执登记。

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
