---
title: SonnetDB 兼容矩阵
---

# SonnetDB 兼容矩阵与基线套件

日期：2026-06-28

范围：本文件是 IoTSharp 侧维护的 SonnetDB 数据底座兼容矩阵。SonnetDB 仓库负责数据库通用能力；IoTSharp 仓库负责决定如何把 SonnetDB 作为关系库、时序库、缓存、对象桶、向量搜索与全文搜索的可选后端接入、验证、灰度和回滚。

容量、吞吐和恢复演练的当前基线见 [SonnetDB 容量与可靠性基准](./sonnetdb-capacity-reliability-baseline.md)。

## 状态图例

| 状态 | 含义 |
| --- | --- |
| 已接入 | 当前代码已有配置枚举、依赖注入路径或实现类。 |
| 需验证 | 有插件或配置入口，但生产选择 SonnetDB 前必须补迁移、方言、事务或长稳验证。 |
| 规划接入 | 当前未以该形态消费 SonnetDB，需要后续增加 provider、API、profile 或业务入口。 |
| 不适用 | 该后端不是当前能力域的直接对照对象。 |

## 关系数据库矩阵

IoTSharp 通过 `DataBaseType` 和 `ApplicationDbContext` 选择主数据存储，承载 Identity、租户、客户、设备、资产、规则、告警等平台主数据。

| 后端 | IoTSharp 当前入口 | 主要能力 | 风险点 | SonnetDB 基线要求 |
| --- | --- | --- | --- | --- |
| PostgreSQL | `DataBase=PostgreSql`，`ConfigureNpgsql` | EF Core、迁移、健康检查、分片遥测默认路径 | 生产常用基线，查询/事务/迁移覆盖面最大 | EF Core provider 必须先对齐 PostgreSQL 常用 DDL、Identity、分页、事务和迁移回滚。 |
| MySQL | `DataBase=MySql`，`ConfigureMySql` | EF Core、迁移、健康检查 | 字符集、大小写、日期精度、分页方言 | provider 需验证字符串、时间、唯一索引、分页和迁移差异。 |
| SQLServer | `DataBase=SqlServer`，`ConfigureSqlServer` | EF Core、迁移、健康检查 | Identity schema、datetime/rowversion、事务语义 | provider 需覆盖 Identity 登录、并发列、事务提交/回滚。 |
| SQLite | `DataBase=Sqlite`，`ConfigureSqlite` | 本地体验、安装向导、轻量部署 | 并发写、大小写搜索、迁移切换 | SonnetDB Profile 应至少达到 SQLite 本地体验易用性。 |
| Oracle | `DataBase=Oracle`，`ConfigureOracle` | EF Core、迁移、健康检查 | 标识符长度、序列、分页、大小写 | provider 需记录不支持清单，不以 Oracle 完整兼容作为首批门槛。 |
| ClickHouse | `DataBase=ClickHouse`，`ConfigureClickHouse` | 有 EF 插件和健康检查入口 | 分析型数据库，事务和更新语义差异大 | 仅作为读多写少/分析场景对照；不作为 OLTP provider 首批等价目标。 |
| SonnetDB | `DataBase=SonnetDB`，`ConfigureSonnetDB` | 已有 EF provider 接线、迁移 assembly、健康检查和 Profile 入口 | 仍需完整业务回归、迁移/回滚、长稳和不支持清单 | 以 `ApplicationDbContext` 真实业务套件作为生产门槛。 |

关系验收用例：

- `ApplicationDbContext` schema 创建、迁移升级、迁移回滚和空库初始化。
- Identity 用户、角色、登录、JWT 相关查询。
- 租户、客户、设备、资产、规则、告警 CRUD。
- 常用 `Include`、分页、排序、条件过滤、唯一索引冲突和事务回滚。
- 迁移前后行数、主键、外键、索引和关键业务查询结果一致。
- 设备批量删除和重建：覆盖 `Device`、`DeviceIdentities`、设备规则、端口、图谱、资产关系、告警等设备域主数据清理。SonnetDB 路径不得依赖长时间阻塞的逐行前台删除；应支持逻辑删除/删除标记、索引可见性即时更新，以及后台 vacuum/compaction/shrink 在机器空闲时逐步回收空间。

## 时序数据库矩阵

IoTSharp 通过 `TelemetryStorage` 和 `IStorage` 承载遥测写入、最新值、历史查询与聚合。

| 后端 | IoTSharp 当前入口 | 主要能力 | 风险点 | SonnetDB 基线要求 |
| --- | --- | --- | --- | --- |
| SingleTable | `TelemetryStorage=SingleTable` | EF 单表遥测存储 | 大规模性能有限 | SonnetDB 应覆盖同等写入、最新值和历史查询语义。 |
| Sharding | `TelemetryStorage=Sharding` | EF 分表，支持 PostgreSQL/MySQL/SQLServer/SQLite/Oracle；SonnetDB 已有最小 ShardingCore 分表 CRUD 基线 | 路由、分片周期、跨分片聚合、生产迁移 | 继续验证按时间范围查询、聚合、分片迁移和长稳一致性。 |
| InfluxDB | `TelemetryStorage=InfluxDB` | 原生时序写入、查询、健康检查 | token/bucket 配置、Flux/聚合差异 | SonnetDB 需覆盖写入、latest、range、聚合和 Influx 迁移校验。 |
| TimescaleDB | `TelemetryStorage=TimescaleDB` | hypertable、time_bucket 聚合 | PostgreSQL 扩展依赖、聚合 SQL 方言 | SonnetDB 需覆盖 `Mean/Max/Min/Sum/First/Last/Median` 对应能力或不支持清单。 |
| Taos / TDengine | `TelemetryStorage=Taos` | 超级表、tag、last_row、范围查询 | SQL 拼接、类型映射、聚合差异 | SonnetDB 需验证 tag、latest、批量写和中文/特殊 key 迁移。 |
| IoTDB | `TelemetryStorage=IoTDB` | storage group、设备路径、聚合查询 | path 编码、类型映射、时间格式 | SonnetDB 需验证设备维度映射、点位类型和聚合结果。 |
| SonnetDB | `TelemetryStorage=SonnetDB`，`SonnetDBStorage` | 已覆盖 auto-create measurement、写入、range、批量写入、schema cache、健康检查，以及委托 SonnetDB `BackupService` 的嵌入式一致性备份恢复 | latest 当前仍有平台侧全量扫描，时间桶聚合仍是逐桶逐字段查询；普通范围查询曾被误称为故障回放；按路线图 #073~#076 整改，并继续补连接复用、大量 measurement 长稳、跨库回归基准和迁移双写报告 | 继续作为 RD-10 的重点生产化路径，数据库执行与维护能力必须留在 SonnetDB。 |

时序验收用例：

- Boolean/String/Long/Double/Json/XML/Binary/DateTime 输入映射。
- 单设备多 key、多设备同 key、空 key、重复 key、保留列名冲突。
- 最新值查询、指定 key 最新值查询、时间范围查询。
- `None/Mean/Median/Last/First/Max/Min/Sum` 聚合语义。
- 批量写入、断线恢复、重复写入、时间边界和 UTC 转换。
- 大量 measurement、海量小 segment、随机重启和后台 flush/compaction/retention 并发。
- 设备重新接入前的 latest 清理：覆盖属性最新值、遥测最新值和设备重连后的重新上报。前台清理动作应快速完成并保证新设备重连不会读到已标记删除的旧 latest；物理删除、段合并和空间收缩由后台任务按 CPU、IO、内存和活跃查询压力限速推进，并暴露清理进度和节流原因。

## 缓存与 KV 矩阵

IoTSharp 通过 `CachingUseIn` 和 EasyCaching provider 选择缓存后端。

| 后端 | IoTSharp 当前入口 | 主要能力 | 风险点 | SonnetDB 基线要求 |
| --- | --- | --- | --- | --- |
| InMemory | `CachingUseIn=InMemory` | 进程内缓存，默认轻量路径 | 重启丢失，多实例不共享 | SonnetDB provider 需提供同等 API，并明确持久化/共享语义。 |
| Redis | `CachingUseIn=Redis` | 分布式缓存、健康检查 | TTL、连接池、网络故障、集群差异 | SonnetDB 需补 TTL、惰性过期、后台清理、批量操作和故障语义。 |
| LiteDB | `CachingUseIn=LiteDB` | 本地持久化缓存 | 文件锁、TTL 行为、并发 | SonnetDB 需验证本地目录型持久化缓存体验和重启恢复。 |
| SQLite | `CachingUseIn=SQlite` 枚举存在 | 当前启动逻辑未显式注册 SQLite provider | 入口不完整 | 作为不支持项记录，不纳入首批 SonnetDB 缓存选项目标。 |
| SonnetDB | `CachingUseIn=SonnetDB`，EasyCaching `options.UseSonnetDB(...)` | 已有 EasyCaching provider 接线、连接串、keyspace、namespace 配置；`IDistributedCache` 由独立包提供 | 仍需 TTL、并发、重启、健康检查和降级行为的业务回归 | 不把普通 KV 直接等同 Redis；以 IoTSharp 缓存调用路径验收。 |

缓存验收用例：

- `Set/Get/Remove/Exists`、批量读写、前缀隔离。
- 绝对过期、滑动过期、过期后不可读、重启后过期仍生效。
- 并发写同 key、并发读写、删除后读一致性。
- provider 名称、配置错误、健康检查和降级行为。

## 对象桶矩阵

IoTSharp 通过 `StorageFactory.Blobs.FromConnectionString(ConnectionStrings:BlobStorage)` 或显式 `sonnetdb://` 连接串使用 `IBlobStorage`。默认路径仍可回退到用户目录下的 `disk://`。

| 后端 | IoTSharp 当前入口 | 主要能力 | 风险点 | SonnetDB 基线要求 |
| --- | --- | --- | --- | --- |
| BlobStorage / disk | `ConnectionStrings:BlobStorage` 或默认 `disk://.../IoTSharp/` | list、upload、download、modify、delete | 本地磁盘容量、备份、权限 | SonnetDB 对象桶需覆盖现有 BlobStorageController 行为。 |
| S3-compatible | Storage.Net 连接串可承载 S3 类后端 | 对象上传下载、外部对象存储 | multipart、etag、range、presigned URL、权限 | 外部 S3 保留为并列回滚后端。 |
| SonnetDB bucket | `ConnectionStrings:BlobStorage=sonnetdb://...`，`SonnetDbBlobStorage` | bucket/object metadata、content、etag/sha256、range、multipart、presigned URL、版本、delete marker、生命周期执行、审计列表 | 大对象压测、跨后端迁移/双写、quota 与管理面 | 已可进入 IoTSharp Profile；迁移、长稳和回滚继续跟随 RD-10。 |

对象桶验收用例：

- bucket 创建/列举、对象上传、覆盖写、下载、删除。
- content-type、etag、sha256、大小、最后修改时间。
- range read、multipart upload、copy object、presigned URL。
- 对象 metadata 与 content 迁移一致性，删除/回滚后引用不悬挂。

## EventBus 与 SonnetMQ 矩阵

IoTSharp 通过 `EventBus`、`EventBusStore`、`EventBusMQ` 和 `IoTSharp.EventBus.*` provider 选择事件总线路径。

| 后端 | IoTSharp 当前入口 | 主要能力 | 风险点 | SonnetMQ 基线要求 |
| --- | --- | --- | --- | --- |
| CAP + InMemory | `EventBus=CAP`，`EventBusStore=InMemory`，`EventBusMQ=InMemory` | 默认轻量事件路径 | 进程内/单机语义，重启和多实例边界有限 | SonnetMQ Profile 必须保持可切回默认路径。 |
| CAP + 外部 MQ | `EventBus=CAP`，外部 `EventBusMQ` | 可接 RabbitMQ/Kafka 等 | 部署复杂、连接和运维成本 | SonnetMQ 只作为轻量可选路径，不替代高可用外部 MQ。 |
| SonnetMQ | `EventBus=SonnetMQ`，`EventBusMQ=SonnetMQ`，`IoTSharp.EventBus.SonnetMQ` | 已有 publish/pull/ack provider、后台 worker 和 topic 映射入口 | 仍需 offset 恢复、失败重试、lag 指标、回放、长稳和多实例边界验证 | 验证 IoTSharp 事件主题、payload、ack、replay、故障恢复和切回 CAP/InMemory。 |

SonnetMQ 验收用例：

- 设备创建/删除、属性、遥测、告警、连接状态和活跃状态事件均能发布和消费。
- 服务重启后未 ack 消息可恢复，已 ack 消息不重复派发。
- 消费失败有明确重试、错误日志和可观测指标。
- topic lag、offset、consumer group 状态可诊断。
- 切回 CAP/InMemory 后核心业务事件路径仍可用。

## 向量搜索矩阵

IoTSharp 当前主平台未发现独立向量搜索后端入口；SonnetDB 已具备 `VECTOR(N)`、KNN 和向量索引能力。该能力只能作为后续增强能力，不应把 IoTSharp 现有功能误标为已支持。

| 能力 | IoTSharp 当前状态 | SonnetDB 当前状态 | 基线要求 |
| --- | --- | --- | --- |
| 向量字段 | 未作为主平台通用字段使用 | `VECTOR(N)` measurement field | 支持设备/资产/文档 embedding 存储，不污染遥测业务数据边界。 |
| KNN 查询 | 未接入 | `knn(...)` 或等价查询能力 | 支持 cosine、L2、inner product，返回稳定 topK 与 distance。 |
| 向量索引 | 未接入 | HNSW 等索引能力 | 验证索引创建、重建、备份恢复和召回率基线。 |
| 混合检索 | 未接入 | Hybrid Search 能力 | 可将设备/资产/知识文档语义搜索与全文 BM25 融合。 |

向量搜索验收用例：

- 向量维度校验、非法向量拒绝、空集合查询。
- topK 顺序、distance 单调性、metric 差异。
- 索引创建、重建、备份恢复后查询结果一致。
- 与时间、tag、关系维表过滤组合时结果不越权、不串租户。

## 全文搜索矩阵

IoTSharp 当前主要是普通字段过滤和 SQLite 大小写搜索配置，未发现独立全文索引服务入口；SonnetDB 已通过内置全文引擎支持文档集合全文索引、匹配查询、BM25 排序和 explain 访问路径。

| 能力 | IoTSharp 当前状态 | SonnetDB 当前状态 | 基线要求 |
| --- | --- | --- | --- |
| 普通搜索 | 控制器/EF 查询中的字段过滤 | 关系表/文档集合查询 | 保持现有名称、标签、属性筛选能力。 |
| 全文索引 | 未作为独立后端接入 | 全文索引与匹配查询 | 支持设备/资产/规则/知识文档搜索，索引可重建。 |
| 中文分词 | 未统一抽象 | SonnetDB CJK/Jieba tokenizer | 验证中文、英文、混合 token 和大小写。 |
| BM25 排序 | 未接入 | BM25 排序能力 | 结果排序稳定，支持分页和 explain。 |

全文搜索验收用例：

- 创建/删除/展示全文索引。
- 中文、英文、数字、混合符号查询。
- 命中、BM25 排序、分页稳定性。
- 索引重建、备份恢复、删除文档后索引同步。

## 迁移、双写与回滚清单

关系迁移：

- 从 PostgreSQL/MySQL/SQLServer/SQLite 导出 schema、索引、约束和数据。
- 导入 SonnetDB 后执行行数、主键、唯一索引、外键候选关系和核心查询校验。
- 迁移失败时保留原库只读可用，SonnetDB 目标库可丢弃重建。
- 回滚时关闭 SonnetDB 写入，恢复原连接串并验证 Identity 登录和核心 CRUD。

时序迁移：

- 按设备、key、时间窗口分批迁移，保留原始时间戳和数据类型。
- 支持双写窗口，比较 latest、range、聚合和抽样原始点。
- 回滚时以原时序库为准，SonnetDB 写入可停止并保留校验报告。

缓存迁移：

- 缓存不作为强一致主数据迁移；仅迁移需要持久化的命名空间。
- 切换前清理易失缓存，切换后验证 TTL 和热点 key。
- 回滚时允许缓存冷启动，但不得影响关系/时序主数据。

对象桶迁移：

- 先迁 metadata，再迁 content，校验 size、etag/sha256、content-type。
- multipart 未完成会话不得迁为完成对象。
- 回滚时保留原 bucket 只读，切回原 BlobStorage/S3 连接串并抽样下载校验。

向量/全文迁移：

- embedding 原始向量、索引定义和全文索引定义分开迁移。
- 索引可重建，不作为唯一事实来源；重建后执行 topK/BM25 抽样校验。
- 回滚时保留原文档/向量主数据，删除或停用 SonnetDB 派生索引。
