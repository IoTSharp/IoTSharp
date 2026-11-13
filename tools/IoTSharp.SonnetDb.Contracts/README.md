# SonnetDB 配置租约消费合同验收

此程序针对 [SonnetDB #425](https://github.com/IoTSharp/SonnetDB/issues/425) 和
[IoTSharp #1269](https://github.com/IoTSharp/IoTSharp/issues/1269)，使用独立 NuGet 包消费者和临时服务端验证配置租约需要的数据库合同。
它不引用 IoTSharp 或 SonnetDB 源码项目，不读取业务连接串，不修改配置操作的 provider 白名单。

## 运行

需要 .NET 10 和已经准备好的正式 `iotsharp/sonnetdb:5.0.0` 镜像。项目固定消费正式
`SonnetDB 5.0.0`、`SonnetDB.EntityFrameworkCore 5.0.0` 与 EF Core `10.0.14`，禁用父目录构建及中央包配置继承。
使用新缓存和目录，防止同版本本地候选包污染验证；保留下载来源和 nuspec 中的 source commit。

在仓库根目录使用 PowerShell 7 执行：

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' restore tools/IoTSharp.SonnetDb.Contracts/IoTSharp.SonnetDb.Contracts.csproj --configfile tools/IoTSharp.SonnetDb.Contracts/nuget.config --packages D:\GitHub\IoTSharp\output\sonnetdb-example\packages --disable-parallel -m:1
& 'C:\Program Files\dotnet\dotnet.exe' build tools/IoTSharp.SonnetDb.Contracts/IoTSharp.SonnetDb.Contracts.csproj -c Release --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false
& 'C:\Program Files\dotnet\dotnet.exe' tools/IoTSharp.SonnetDb.Contracts/bin/Release/net10.0/IoTSharp.SonnetDb.Contracts.dll --output D:\GitHub\IoTSharp\output\sonnetdb-example\smoke --smoke
& 'C:\Program Files\dotnet\dotnet.exe' tools/IoTSharp.SonnetDb.Contracts/bin/Release/net10.0/IoTSharp.SonnetDb.Contracts.dll --output D:\GitHub\IoTSharp\output\sonnetdb-example\full
```

`--output` 必须是尚不存在的绝对路径。先执行 smoke，确认循环退出和资源回收，再执行完整套件。
`--docker` 可指定本机 Docker CLI 的绝对路径，`--image` 可指定已存在的明确镜像；程序不会拉取或发布镜像。

## 验证内容与结论边界

| 场景 | 检查 |
| --- | --- |
| 条件 UPDATE | nullable GUID、UTC DATETIME2、布尔条件、三个 setter、真实影响行数及新连接回读 |
| 真实事务 session | 实际捕获创建、SQL、提交、状态和回滚路径，拒绝把预演影响数视为所有权 |
| 五表原子性 | 配置版本、assignment、EdgeTask、receipt、audit 的合同切片；提交前外部不可见，提交后完整可见 |
| 五阶段回滚 | 各实际保存阶段回滚后，五表操作记录均为零且租约恢复 |
| 租约条件 | busy、过期接管、删除标志、错误 owner 及多 Gateway 部分获取回滚 |
| 竞争 | 固定交错两个独立 DbContext，分别检查 ReadCommitted 与 Serializable 的提交结果 |
| 冲突后重试 | 新事务重新获取租约、重新检查赢家在途任务，最多一次重试并拒绝重复下发 |
| 取消、超时、断连 | 核对实际请求、存储状态和清理结果，保留提交未知状态 |
| 提交响应丢失 | 必须读取原会话终态；不得重新创建事务或盲目重放写入 |
| 服务重启 | 分别对账未提交与已提交操作，核对业务操作编号对应的全部五表状态 |
| 旧服务端 | 不确认 Serializable session 时安全拒绝 |
| Frame 入口 | 镜像 HTTP/2 端口 `5081` 的严格 Frame 普通读，以及同一连接的真实事务读写与回滚 |

ReadCommitted 用例是安全要求的反例探针。如果两个操作都提交，结果保持 `FAIL`，整个程序返回 `1`。
这表示该隔离级别不满足本租约使用方式，不能推导为数据库不支持事务。
Serializable 也可能在不同 Gateway 之间发生表级冲突；调用方必须结束旧事务并重新检查完整业务动作，不能只重试最后一条写入。

这里的五张业务表是独立合同切片。它们不能代替 IoTSharp HTTP 权限、真实 Product/Asset、发布/回滚业务流或生产启用验收。
程序不自动解除白名单。即使合同切片通过，也需要上游合同、IoTSharp 最小适配和真实应用消费者回归。

## 回执和资源

`results.json` 记录每项 `PASS`、`FAIL`、`NOT_RUN`、错误码、业务操作编号、实际 HTTP 请求事件、加载程序集版本和 SHA-256。
`image.json` 保留镜像 digest/source commit，`processes.json` 保留本次 Docker 子进程的 PID、父 PID、启动时间、argv 和超时；`server.log` 保留受限尾部日志。

进程总上限为 6 分钟，每项 50 秒，启动最多 40 次检查且有 45 秒截止时间；Ctrl+C 可取消。
代理仅监听回环地址，最多 1000 个请求、单请求正文最多 1 MiB。容器限制为 1 CPU/1 GiB，只开放回环上的两个临时端口。
重启保持端口和容器可写层；结束时核对容器 ID 和唯一 ownership 标签后回收当前容器，不删除镜像、不操作共享卷。
预取消提交用例在 provider 无法清理 session 时，会额外通过捕获的精确 session ID 清理测试资源，并保留原始 `FAIL`。

## 实际 IoTSharp 存量升级验收

`IoTSharp.Test/SonnetDbExistingDatabaseUpgradeTests.cs` 使用真实 `ApplicationDbContext` 和仓库迁移：

1. 将空的临时数据库迁移至 `20261004010000_AddConfigurationOperationLease`。
2. 写入旧 DATETIME 刷新令牌和显式主键 `41` 的字典分组。
3. 执行 `20261112061658_AlignSonnetDb5Model`，核对旧数据、新 DATETIME2 精度、行数及约束。
4. 检查升级后的数据库生成主键是否继续大于 `41`。

两种输入分别保留正确自增元数据，以及精确移除旧迁移遗漏的八个 `AUTO_INCREMENT` 声明。
后一种输入用于验证真正存量库的升级，不能用修正后的初始建表脚本代替。

此专项默认跳过；显式启用后任何失败都会作为真实失败报告：

```powershell
$env:IOTSHARP_SONNETDB_UPGRADE_ACCEPTANCE = '1'
& 'C:\Program Files\dotnet\dotnet.exe' test IoTSharp.Test/IoTSharp.Test.csproj -c Release --no-restore --filter FullyQualifiedName~SonnetDbExistingDatabaseUpgradeTests --logger 'trx;LogFileName=sonnetdb-upgrade.trx' --disable-build-servers -m:1 -p:GeneratePackageOnBuild=false -p:ShouldRunNpmInstall=false -p:StaticWebAssetsEnabled=false
```

应用测试允许复用无关第三方依赖的既有缓存，但必须核对测试输出中的 SonnetDB DLL 与独立公开包消费者的 DLL 哈希完全一致。
