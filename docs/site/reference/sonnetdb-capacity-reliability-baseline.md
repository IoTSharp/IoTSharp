---
layout: default
title: SonnetDB 容量与可靠性基准
description: IoTSharp SonnetDB Profile 的吞吐、容量和恢复基线说明。
permalink: /reference/sonnetdb-capacity-reliability-baseline/
---

# SonnetDB 容量与可靠性基准

本页对应路线图 #071，记录 IoTSharp 使用 SonnetDB Profile 时的最小基准口径。

## 当前基线

| 能力 | 基线 |
| --- | --- |
| 遥测写入吞吐 | SonnetDB Core 100 万点写入 704.8 ms，约 141.9 万点/秒；SonnetDB Server LP 100 万点写入 1,651 ms，约 60.6 万点/秒。 |
| 容量口径 | 使用 `backup.TotalBytes / telemetry_value_count` 计算 bytes/value；IoTSharp CI smoke 固定 20 设备、50 样本、4 字段。 |
| 恢复能力 | SonnetDB 引擎覆盖 kill -9、WAL torn tail、compaction 中断等故障；IoTSharp 覆盖备份、校验、dry-run、恢复和 replay。 |
| 可观测指标 | `/healthz` 与 `/metrics` 暴露 `sonnetdb_rows_inserted_total`、`sonnetdb_sql_errors_total`、`sonnetdb_segments{db=...}` 等指标。 |

## 验证命令

```bash
dotnet test ./IoTSharp.Test/IoTSharp.Test.csproj -c Release --filter "FullyQualifiedName~SonnetDBStorageTests"
```

```bash
docker compose -f docker-compose.sonnetdb.yml up -d
curl http://localhost:5080/healthz
curl http://localhost:5080/metrics
```

完整报告位于仓库文档 `docs/docs/operations/sonnetdb-capacity-reliability-baseline.md`。
