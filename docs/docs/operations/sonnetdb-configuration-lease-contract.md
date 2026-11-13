---
title: SonnetDB 配置租约正式包验证
---

# SonnetDB 配置租约正式包验证

验证日期：2026-11-13（Asia/Shanghai）。跟踪 [SonnetDB #425](https://github.com/IoTSharp/SonnetDB/issues/425)
和 [IoTSharp #1269](https://github.com/IoTSharp/IoTSharp/issues/1269)。两个 issue 均保持 OPEN；本次读取时 #425 尚无回复。

当前结论：SonnetDB 5.0.0 已证明若干需要的事务能力，但 IoTSharp 的启用门槛尚未完成。
`ConfigurationOperationLeaseService` 继续在打开 SonnetDB 连接前安全拒绝，不将其加入 provider 白名单。

## 已核对的发布物

独立消费者位于 `tools/IoTSharp.SonnetDb.Contracts`，只引用固定版本 NuGet 包，不引用 SonnetDB 或 IoTSharp 源码项目。

| 发布物 | 验证结果 |
| --- | --- |
| SonnetDB / EF Provider | NuGet.org 新缓存的 `5.0.0`，程序集 informational version 为 `5.0.0+84433f9edcceb84605520ebecc4b2879e3c4aa79` |
| EF Core | `10.0.14` |
| 镜像 | `iotsharp/sonnetdb:5.0.0`，Linux/amd64 |
| 镜像 digest | `sha256:f35b6493c9cbc41032ba7f825b02780a24be8c6c7072ee0442544cf3a4fbd325` |
| 镜像源码 | OCI revision `84433f9edcceb84605520ebecc4b2879e3c4aa79` |
| SonnetDB.Data.dll SHA-256 | `67036787d6679930f3267e527866473bc8fe6343848afb31b5c6add4b524feb2` |
| SonnetDB.EntityFrameworkCore.dll SHA-256 | `f9a59625a3e1f13c735bff09653712254affa7bfe554d593d359f592721bafd9` |

普通全局缓存中的同版本 `SonnetDB 5.0.0` nuspec 标注 `455bf3b5...`，与本次从 NuGet.org 取得的包不同。
本次没有改写全局缓存；独立消费者使用新缓存。应用测试可复用无关依赖缓存，但已逐一比较其
`SonnetDB.Core`、`SonnetDB.Data`、`SonnetDB.EntityFrameworkCore`、`SonnetDB.Protocol` 四个 DLL，均与独立公开包消费者哈希完全相同。

## 配置租约合同切片

最终独立消费者共 **14 PASS、2 FAIL、0 NOT_RUN**，退出码为 `1`。

| 检查 | 结果 | 证据含义 |
| --- | --- | --- |
| 条件 UPDATE、真实影响数 | PASS | nullable GUID、UTC DATETIME2、布尔条件、多 setter；取得、busy、释放分别返回 `1/0/1` |
| 真实远程事务 session | PASS | 捕获实际 session 路径；提交前其它连接读不到五表记录，提交后全部可见 |
| 五张关系表原子提交/回滚 | PASS | 配置版本、assignment、任务、receipt、audit 合同切片，各保存阶段回滚均无部分记录 |
| owner、过期和多目标 | PASS | 错误 owner 无法释放；过期可接管；第二目标 busy 时首个目标和业务写入回滚 |
| ReadCommitted 竞争 | FAIL | 固定交错的两个独立上下文都得到 `affected=1`，释放后两笔事务均成功提交 |
| Serializable 竞争 | PASS | 第二笔提交返回 `table_serialization_conflict`，其五表记录均未持久化 |
| 冲突后的完整业务重查 | PASS | 新事务重试一次，观察到赢家在途任务后拒绝重复下发 |
| 提交前取消与清理 | FAIL | 没有发出 commit 请求，服务端 session 仍 active，但本地 transaction 已结束，Rollback 失败 |
| SQL 响应超时/断连 | PASS | 在真实服务器执行后注入故障，显式回滚后五表记录均为零 |
| 提交响应丢失 | PASS | 能读取终态时确认提交；终态不可读时保持 IOException 提交未知，不新建事务重放 |
| 服务重启对账 | PASS | 未提交操作五表均无记录；已提交操作五表各一条，服务端 session 丢失不改变持久化结果 |
| 旧服务端回退 | PASS | 无法创建 Serializable session 时返回 NotSupportedException，不降级取得所有权 |
| Frame/HTTP2 入口 | PASS | 镜像 `5081` 端口的严格 Frame 普通读和真实事务回滚；事务控制/DML 使用 REST session 路由承载于 HTTP/2 |

ReadCommitted 反例说明当前默认事务方式不足以支撑这套租约用法，不意味着数据库缺少 UPDATE 或事务能力。
Serializable 的提交冲突需要从新事务重新读取和判断整个业务动作。它采用表级乐观校验，不能解释为持有悲观行锁。

## 取消边界

正式包上可稳定复现以下序列：

1. 在 Serializable session 内取得租约，并逐阶段保存五张关系表。
2. 在调用 CommitAsync 前取消令牌。
3. 客户端没有向服务器发送 commit，却返回“提交结果未知”的 IOException。
4. 查询原 session 返回 `{"state":"active"}`；新连接看到五表记录均为零，租约仍为原状态。
5. 同一 transaction 的 RollbackAsync 返回 `InvalidOperationException: 轻事务已结束`。

上游 `SndbConnection.CommitTransactionAsync` 的失败路径会清除当前事务并将本地 transaction 标记完成。
IoTSharp 当前的失败释放流程依赖 Rollback，直接开放白名单可能让清理异常覆盖原来的提交未知异常。
测试程序仅为回收自身资源，通过捕获的精确 session ID 执行额外回滚；这一动作不计为 provider 通过。

后续需要明确预取消、已发送提交、提交未知和原会话对账的可调用合同，再实现 IoTSharp 的隔离级别、错误处理和对账适配。
不能只添加 provider 名称或仅重试最后一条 SQL。

## 实际 IoTSharp 存量升级

`SonnetDbExistingDatabaseUpgradeTests` 使用真实 `ApplicationDbContext`、历史迁移和正式镜像，显式启用专项验收。
当前应用定向回归为 **17 PASS、1 FAIL、0 SKIP**，共 18 项。

| 存量输入 | 结果 |
| --- | --- |
| 旧库已有正确自增元数据 | PASS：旧 DATETIME 数据、行数、主键保留；新 DATETIME2 精确到 UTC ticks；重复主键、唯一索引、外键违规均被拒绝；新主键由 `41` 继续到 `42` |
| 旧库缺少历史遗漏的八个 AUTO_INCREMENT 声明 | FAIL：日期和上述约束通过，但新建字典分组仍报 `列 'DictionaryGroupId' 不允许为 NULL` |
| 现有模型日期与迁移快照 | PASS |
| 租约 provider 安全拒绝与既有 SQLite 竞争回归 | PASS；SQLite 结果仅作为原有行为回归 |
| 六个 provider 的租约迁移脚本生成 | PASS；不代表其它数据库已实际执行迁移 |

修正初始建表迁移不能改变已创建的存量 schema。`AlignSonnetDb5Model` 不会补回缺失的自增属性。
正式 EF Provider 明确拒绝通过 ALTER COLUMN 新增或修改 AUTO_INCREMENT，提示需要重建表。
因此存量主键修复还需单独验证保留行、索引、外键和回退的升级路径，本次未重建生产表，也未编写未经验证的自动重建迁移。

## 回执与后续门槛

本机回执位于被 Git 忽略的 `output/sonnetdb-contract-20261113/`：

- `final-verified/results.json`：最终合同结果、错误、实际请求路径、程序集版本和哈希，含五个保存阶段异常展开时的自动回滚。
- `final-verified/image.json`：镜像 digest、架构与 OCI 源码 revision。
- `final-verified/processes.json`：任务子进程身份和有界执行记录。
- `application/upgrade-and-lease-verified.trx`：实际应用回归及存量升级失败详情，唯一索引和外键使用稳定错误码核验。
- `application/provenance.json`：应用输出与独立公开包消费者的四个 SonnetDB DLL 哈希比对。

应用当前源码的数据层、SonnetDB Provider、主项目及测试程序集均已重新编译。
旧 Release Provider 缺少最新迁移的第一次运行已被排除，判断以最终回执为准。
前端构建、全套 CI、实际 SonnetDB 配置发布/回滚业务流、生产迁移、部署和现场验收均为 `NOT_RUN`。
五表合同切片通过不能代替后续这些门槛，也不能推进仓库的 O1→O5 生产启用状态。
