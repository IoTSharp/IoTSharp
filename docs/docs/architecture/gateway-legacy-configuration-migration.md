---
title: Gateway 旧配置迁移方案
---

# Gateway 旧配置迁移方案

本文对应路线图 #018，目标是把 Product 上的旧 Gateway 配置字段从活动建模路径中移出，迁移到后续 Collection Template 体系。当前阶段只完成迁移方案与边界确认，不提前创建 M3 的正式模板表。

## 背景

旧模型把网关采集配置挂在 Product 上：

- `Product.GatewayType`
- `Product.GatewayConfiguration`

这两个字段混合了承载模板、连接配置、命名配置文件和运行时接入习惯，容易让 Product 变成 Gateway 运行态配置容器。新的领域边界要求：

- Product 只保存能力模板和采集模板定义。
- Gateway 只承担南向协议执行和子设备上报。
- EdgeNode 只承担受管运行时生命周期和任务闭环。
- 运行时配置由 Collection Template 生成，并发布到 EdgeNode、Gateway 或选定 Device。

## 迁移目标

旧字段不再作为新增采集配置的主入口。它们迁入 Collection Template 后，对应关系如下：

| 旧字段 | 新归属 | 处理方式 |
| --- | --- | --- |
| `GatewayType` | `ProtocolTemplate.Protocol` | 映射为 Modbus、OPC UA、BACnet、Custom 等协议类型。 |
| `GatewayConfiguration` 命名配置 | `ConnectionTemplate` 或 `ProtocolTemplate.ProtocolOptions` | 保留命名来源，生成待确认模板，不直接发布。 |
| `GatewayConfiguration` 自定义 JSON | `ConnectionTemplate`、`PointTemplate`、`SamplingPolicy`、`MappingPolicy` | 尽量结构化解析，无法识别字段原样放入 `ProtocolOptions` 并标记需人工确认。 |
| 空配置或 `Unknow` | 无迁移目标 | 跳过迁移，只记录无动作结果。 |

## 中间形态

M3 尚未落地 `CollectionTemplate` 实体前，现有 `CollectionTaskDto` 和 `EdgeCollectionConfigurationDto` 只作为运行时配置草稿和边缘拉取契约使用，不能替代平台侧模板持久化。

因此 #018 的中间形态是：

1. 盘点 Product 上仍有旧 Gateway 配置的记录。
2. 对每条记录生成迁移干跑结果。
3. 干跑结果包含未来 Collection Template 的目标结构和可选的 `CollectionTaskDto` 预览。
4. 不自动写入 Edge 运行时配置，不自动发布到 Gateway 或 EdgeNode。
5. 等 #030 完成后，把干跑结果导入正式 Collection Template。

## 协议映射

| `GatewayType` | 目标 `CollectionProtocolType` | 说明 |
| --- | --- | --- |
| `Unknow` | `Unknown` | 不生成模板。 |
| `Customize` | `Custom` | 自定义 JSON 尽量解析；无法识别部分进入 `ProtocolOptions.legacy`。 |
| `Modbus` | `Modbus` | 连接、站号、寄存器、采样周期、映射字段优先结构化。 |
| `Bacnet` | `Bacnet` | 对象类型、实例号、属性、采样周期进入点位模板。 |
| `OPC_UA` | `OpcUa` | Endpoint、Namespace、NodeId、认证参数进入协议和连接模板。 |
| `CanOpen` | `Custom` | 当前 `CollectionProtocolType` 未含 CanOpen，先以 Custom 保留 `legacyGatewayType=CanOpen`。 |

## 迁移流程

### 1. 盘点

筛选条件：

- `Product.Deleted = false`
- `GatewayType != Unknow` 或 `GatewayConfiguration` 非空
- 按 Tenant、Customer 隔离执行

输出字段：

- ProductId、ProductName、TenantId、CustomerId
- GatewayType、GatewayConfiguration 摘要和配置哈希
- 是否可自动解析
- 是否需要人工确认

### 2. 干跑

干跑阶段不写业务表，只生成迁移计划：

```json
{
  "source": "ProductGatewayConfiguration",
  "productId": "00000000-0000-0000-0000-000000000000",
  "legacyGatewayType": "Modbus",
  "configHash": "sha256:...",
  "target": {
    "collectionTemplateKey": "legacy-modbus-default",
    "protocol": "Modbus",
    "reviewStatus": "NeedsReview"
  },
  "warnings": [
    "GatewayConfiguration is a named profile; connection detail requires manual confirmation."
  ]
}
```

### 3. 人工确认

旧 Gateway 配置不能无审查地变成可执行配置。确认界面应至少展示：

- 源 Product 和旧字段值。
- 生成的协议、连接、点位、采样、转换和映射。
- 未识别字段列表。
- 对应 Gateway 或 EdgeNode 的候选发布目标。
- 是否创建模板、跳过、或标记为手工重建。

### 4. 写入模板

#030 完成后，迁移写入正式 Collection Template：

- `CollectionTemplate.ProductId = Product.Id`
- `CollectionTemplate.Source = LegacyProductGatewayConfiguration`
- `CollectionTemplate.SourceHash = sha256(GatewayType + GatewayConfiguration)`
- `CollectionTemplate.Status = Draft` 或 `NeedsReview`
- `ProtocolTemplate`、`ConnectionTemplate`、`PointTemplate` 等字段按映射写入。

写入必须幂等。同一 Product、同一旧配置哈希重复执行时，应更新同一份迁移草稿，而不是创建重复模板。

### 5. 发布与关闭旧入口

发布闭环归 M4：

1. 从 Collection Template 生成配置版本。
2. 创建配置发布任务，目标为 EdgeNode、Gateway 或选定 Device。
3. 执行端接收、执行并回执。
4. 平台记录当前版本、目标版本和执行结果。
5. 发布成功并完成审计后，旧字段进入只读或清空流程。

## 校验规则

- 不能因为 `GatewayConfiguration` 是合法 JSON 就自动发布。
- 未识别字段必须保留在 `ProtocolOptions.legacy`，不得静默丢弃。
- 密钥、密码、证书等敏感字段必须进入受保护配置，不在模板明文展示。
- 点位映射必须明确目标：Telemetry、Attribute、AlarmInput 或 CommandFeedback。
- 采样策略必须有默认周期或触发方式。
- 租户、客户和权限边界按 Product 所属范围执行。
- 迁移动作必须有审计记录和操作者。

## 回滚策略

迁移分为三个层次，回滚也分层处理：

| 层次 | 回滚方式 |
| --- | --- |
| 干跑结果 | 删除迁移计划即可，不影响现有 Product 和 Gateway。 |
| 模板草稿 | 删除或标记废弃 Collection Template，不改 Edge 运行态。 |
| 已发布配置 | 通过 M4 Release/EdgeTask 回滚到上一配置版本，旧字段不作为运行时回滚来源。 |

## UI 调整

M1 收口后，Product 表单中的旧 Gateway 配置区域应转为迁移提示或只读历史信息。新增采集配置入口应进入 Product 的 Collection Template 工作区。

在 #030 未完成前，前端可以保留旧字段编辑能力用于兼容存量数据，但文案必须明确：

- 这是旧 Gateway 配置。
- 新增采集建模应使用后续 Collection Template。
- 保存 Product 不等于发布采集配置。

## API 调整

短期保留 `ProductAddDto.GatewayType` 和 `ProductAddDto.GatewayConfiguration` 作为历史字段，避免升级时丢失存量配置。后续步骤：

1. M2 前不新增依赖旧字段的新业务逻辑。
2. M3 提供 Collection Template API 后，Product API 只读返回旧字段或移入单独历史视图。
3. M4 发布闭环完成后，禁止通过 Product Save/Update 修改旧 Gateway 配置。
4. 数据库字段最终删除需另开迁移事项，必须先确认模板、发布记录和回滚路径覆盖完毕。

## 验收标准

#018 视为完成时满足：

- 旧字段到 Collection Template 的映射关系明确。
- 干跑、人工确认、写入模板、发布和关闭旧入口的顺序明确。
- 不把旧 Gateway 配置直接写入 Edge 运行态。
- 不把 Gateway 或 EdgeNode 契约隐藏在数据库字段里。
- 后续 #030、#040 可以按本文继续实施，不需要重新讨论领域边界。
