---
title: SonnetDB 容量与可靠性基准
---

# SonnetDB 容量与可靠性基准报告

日期：2026-07-06

范围：本报告对应路线图 #071，聚焦 IoTSharp 使用 SonnetDB Profile 时的遥测写入吞吐、容量口径和恢复能力。SonnetDB 仓库负责数据库引擎通用基准；IoTSharp 仓库负责把这些数据落到平台接入、运维指标、回滚边界和 CI 验收上。

## 结论摘要

| 能力 | 当前基线 | IoTSharp 侧验收 |
| --- | --- | --- |
| 遥测写入吞吐 | SonnetDB Core 100 万点写入 704.8 ms，约 141.9 万点/秒；SonnetDB Server LP 100 万点写入 1,651 ms，约 60.6 万点/秒。 | `TelemetryStorage=SonnetDB` 走 `SonnetDBStorage.StoreTelemetryBatchAsync`，持续用 SonnetDB storage tests 覆盖批量写入、schema 演进和类型提升。 |
| 容量口径 | 使用 `backup.TotalBytes / telemetryValueCount` 作为平台侧 bytes/value 指标，不把客户端内存分配误当成磁盘容量。 | `CapacitySmoke_WhenUsingEmbeddedSonnetDB_ReportsBytesPerTelemetryValue` 固定 20 设备、50 样本、4 字段的容量 smoke，并限制 bytes/value 上界。 |
| 恢复能力 | SonnetDB CrashTests 覆盖 kill -9、WAL torn tail、delete replay、compaction 中断和磁盘写失败；IoTSharp 覆盖备份、校验、dry-run、恢复和故障窗口 replay。 | `BackupRestoreAndReplay_WhenUsingEmbeddedSonnetDB_PreserveTelemetry` 确认 IoTSharp 遥测恢复后 latest、history 与字段列表保持一致。 |
| 可观测指标 | `/healthz` 与 `/metrics` 暴露服务存活、数据库数、SQL 请求/错误、插入行数、返回行数和 segment 数。 | SonnetDB Profile 运维报告必须采集 `sonnetdb_rows_inserted_total`、`sonnetdb_sql_errors_total`、`sonnetdb_segments{db=...}` 和数据目录大小。 |

## 基准边界

本报告只描述 SonnetDB 作为 IoTSharp 可选单依赖数据底座时的基线，不改变 Product、Device、Asset、Gateway、EdgeNode 和 Collection Template 的领域边界。

- 遥测写入吞吐来自 SonnetDB 可复现 BenchmarkDotNet 基准；IoTSharp 侧不把引擎 benchmark 包装成平台 API SLA。
- 容量指标以落盘备份体积和实际遥测值数量计算；客户端 `Allocated` 只用于观察内存分配，不用于容量规划。
- 恢复能力分两层验收：SonnetDB 引擎负责 WAL、segment、compaction 和 torn record；IoTSharp 负责 `SonnetDBStorage` 备份恢复 API、业务 latest 查询和故障窗口回放。
- 长稳、跨库迁移、双写比对和多实例高可用仍按 `sonnetdb-compat-matrix` 继续推进，不由 #071 一次性关闭。

## 遥测吞吐基线

以下数据来自 SonnetDB 侧 BenchmarkDotNet 报告，数据集为 1,000,000 点，固定随机种子，同机运行。嵌入式路径和服务端路径必须分开引用。

| 路径 | 平均耗时 | 吞吐 | 说明 |
| --- | ---: | ---: | --- |
| SonnetDB Core 写入 1M | 704.8 ms | 141.9 万点/秒 | 嵌入式引擎路径，覆盖 WAL、MemTable、Flush 与 Segment。 |
| SonnetDB Server LP 写入 1M | 1,651 ms | 60.6 万点/秒 | HTTP Line Protocol 快路径。 |
| SonnetDB Server Bulk VALUES 写入 1M | 1,691 ms | 59.1 万点/秒 | HTTP Bulk VALUES 快路径。 |
| SonnetDB Server JSON 写入 1M | 2,309 ms | 43.3 万点/秒 | HTTP JSON 批量路径。 |
| SonnetDB Server SQL Batch 写入 1M | 13,469 ms | 7.4 万点/秒 | HTTP + SQL parser，适合兼容验证，不作为高吞吐入口。 |

同口径服务端对比中，SonnetDB Server 与 Apache IoTDB Server 都走 HTTP 路径，1,000 设备、30 字段、12 时间点、AB BA AB BA 四轮平均结果如下：

| 数据库 | 平均耗时 | 平均吞吐 |
| --- | ---: | ---: |
| SonnetDB Server | 20,892 ms | 22,867 values/sec |
| Apache IoTDB Server | 33,050 ms | 11,541 values/sec |

对外表述时只允许说“在这组同机、同 HTTP 口径测试下，SonnetDB Server 平均吞吐约为 IoTDB Server 的 1.98 倍”。不要把嵌入式 SonnetDB Core 数据与对方服务端路径混用。

## 容量基线口径

IoTSharp 侧容量报告使用以下公式：

```text
bytes_per_telemetry_value = backup.TotalBytes / telemetry_value_count
```

其中：

- `backup.TotalBytes` 来自 `SonnetDBStorage.CreateBackup(...)` 生成的备份报告。
- `telemetry_value_count` 是消息数乘以每条消息写入的遥测字段数。
- 报告同时记录 `FileCount`、`MeasurementCount`、`SegmentCount`、数据目录大小和备份目录大小。

当前 CI smoke 使用 20 个设备、每设备 50 条消息、每条 4 个字段，共 1,000 条消息和 4,000 个遥测值。该测试不把具体 bytes/value 固化为宣传数据，只设置 4,096 bytes/value 的安全上界，目的是防止容量口径失效或备份体积异常膨胀。

生产或发布报告必须补充以下实测表：

| 场景 | 设备数 | 样本/设备 | 字段/样本 | 遥测值总数 | 备份体积 | bytes/value | segment 数 | 备注 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| smoke | 20 | 50 | 4 | 4,000 | 由 CI 输出 | 由 CI 输出 | 由 CI 输出 | 验证容量口径。 |
| standard | 1,000 | 1,000 | 4 | 4,000,000 | 待发布基准填入 | 待发布基准填入 | 待发布基准填入 | 发布前推荐档位。 |
| soak | 10,000 | 10,000 | 8 | 800,000,000 | 待长稳填入 | 待长稳填入 | 待长稳填入 | 需要独立长稳环境。 |

## 恢复能力基线

SonnetDB 引擎侧已覆盖以下故障注入：

| 故障 | 验收点 |
| --- | --- |
| kill -9 during fsync | 重启后只 replay 有效 WAL 记录。 |
| kill -9 mid compaction | 重启后 committed segment 不重复。 |
| kill -9 after delete | 已删除时间范围不复活。 |
| OS flushed writes process crash | 进程崩溃后已确认写入可恢复。 |
| disk full during WAL append | 已同步记录保留，失败写不污染恢复。 |
| torn WAL tail | 重启时忽略 torn tail，已提交数据保留。 |
| half renamed segment | pending replacement 被忽略，旧 segment 保持可读。 |

IoTSharp 平台侧恢复验收覆盖：

| API 或测试 | 验收点 |
| --- | --- |
| `CreateBackup` | 生成 manifest，记录文件数、总字节数、measurement 数和 segment 数。 |
| `VerifyBackup` | 校验文件大小和 SHA-256。 |
| `RestoreDryRun` | 在不写目标目录前验证 manifest 与目标目录可用性。 |
| `Restore` | 恢复到新目录后可重新打开并查询 latest。 |
| `ReplayFailureAsync` | 对设备、字段和时间窗口做故障回放，返回点数和字段列表。 |

## 运行命令

IoTSharp 侧 smoke：

```bash
dotnet test ./IoTSharp.Test/IoTSharp.Test.csproj -c Release --filter "FullyQualifiedName~SonnetDBStorageTests"
```

SonnetDB 侧吞吐基准：

```bash
dotnet run -c Release --project ./SonnetDB/tests/SonnetDB.Benchmarks/SonnetDB.Benchmarks.csproj -- --filter *Insert*
dotnet run -c Release --project ./SonnetDB/tests/SonnetDB.Benchmarks/SonnetDB.Benchmarks.csproj -- --filter *ServerInsert*
dotnet run -c Release --project ./SonnetDB/tests/SonnetDB.Benchmarks/SonnetDB.Benchmarks.csproj -- --comparison-server
```

SonnetDB Profile 指标采集：

```bash
docker compose -f docker-compose.sonnetdb.yml up -d
curl http://localhost:5080/healthz
curl http://localhost:5080/metrics
```

必须采集的最小指标：

- `sonnetdb_uptime_seconds`
- `sonnetdb_databases`
- `sonnetdb_sql_requests_total`
- `sonnetdb_sql_errors_total`
- `sonnetdb_rows_inserted_total`
- `sonnetdb_rows_returned_total`
- `sonnetdb_segments{db="..."}`

## 发布门槛

一次可对外引用的 SonnetDB Profile 基准报告必须包含：

1. 代码版本、commit、运行日期、CPU、内存、磁盘、OS、.NET SDK、Docker Desktop 或 Linux 容器运行时版本。
2. 数据模型：设备数、字段数、采样间隔、消息数、遥测值总数、写入批大小。
3. 写入结果：平均耗时、吞吐、错误数、`sonnetdb_rows_inserted_total` 增量。
4. 容量结果：数据目录大小、备份体积、bytes/value、measurement 数、segment 数。
5. 恢复结果：备份校验、dry-run、恢复耗时、恢复后 latest/range/replay 抽样结果。
6. 回滚说明：停止 SonnetDB Profile 后切回原 `docker-compose.yml` 和原环境配置，关系库、时序库、缓存、对象桶和事件总线不得混用半迁移状态。

## 后续缺口

- 长稳容量曲线仍需独立环境补充，包括 compaction、retention、latest 清理和大量 measurement 场景。
- 远端 SonnetDB Server 的备份恢复应接入服务端维护 API；当前 `SonnetDBStorage` 的 `CreateBackup`/`Restore` 只支持嵌入式 Data Source 路径。
- 缓存、对象桶和 SonnetMQ 的容量与恢复报告应拆成后续报告，不能混在遥测吞吐报告里一次性宣称完成。
