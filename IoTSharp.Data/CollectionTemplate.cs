using IoTSharp.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IoTSharp.Data
{
    /// <summary>
    /// Product 下的采集模板根模型，负责把协议、连接、点位、转换、采样和映射组织成可生成运行时配置的能力模板。
    /// </summary>
    public class CollectionTemplate : IJustMy
    {
        /// <summary>
        /// 模板 ID。
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 所属 Product ID。
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Product 内稳定模板键。
        /// </summary>
        public string TemplateKey { get; set; }

        /// <summary>
        /// 模板名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 模板描述。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// semantic-core 模型批次或导入来源标识。
        /// </summary>
        public string SemanticModelId { get; set; }

        /// <summary>
        /// 模板版本号。正式配置版本会在 M4 独立建模。
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// 模板状态。
        /// </summary>
        public CollectionTemplateStatus Status { get; set; } = CollectionTemplateStatus.Draft;

        /// <summary>
        /// 是否允许生成运行时配置。
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 默认上报策略 JSON。
        /// </summary>
        public string ReportPolicy { get; set; }

        /// <summary>
        /// 非敏感扩展信息 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 是否删除。
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间。
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 创建人显示名或账号。
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 最近更新人显示名或账号。
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// 租户 ID。
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 客户 ID。
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// 所属 Product。
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// 租户导航属性。
        /// </summary>
        public Tenant Tenant { get; set; }

        /// <summary>
        /// 客户导航属性。
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// 协议模板。
        /// </summary>
        public CollectionProtocolTemplate Protocol { get; set; }

        /// <summary>
        /// 连接模板集合。
        /// </summary>
        public List<CollectionConnectionTemplate> Connections { get; set; } = new();

        /// <summary>
        /// 点位模板集合。
        /// </summary>
        public List<CollectionPointTemplate> Points { get; set; } = new();
    }

    /// <summary>
    /// 协议模板，保存协议族和协议级非敏感参数。
    /// </summary>
    public class CollectionProtocolTemplate
    {
        /// <summary>
        /// 协议模板 ID。
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 所属采集模板 ID。
        /// </summary>
        public Guid CollectionTemplateId { get; set; }

        /// <summary>
        /// 归一化协议类型。
        /// </summary>
        public CollectionProtocolType Protocol { get; set; } = CollectionProtocolType.Unknown;

        /// <summary>
        /// semantic-core 的 protocolKind 或自定义协议标识。
        /// </summary>
        public string ProtocolKind { get; set; }

        /// <summary>
        /// 协议级非敏感参数 JSON。
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// 非敏感扩展信息 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 所属采集模板。
        /// </summary>
        public CollectionTemplate CollectionTemplate { get; set; }
    }

    /// <summary>
    /// 连接模板，描述地址、串口、认证类型、超时和重试。
    /// </summary>
    public class CollectionConnectionTemplate
    {
        /// <summary>
        /// 连接模板 ID。
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 所属采集模板 ID。
        /// </summary>
        public Guid CollectionTemplateId { get; set; }

        /// <summary>
        /// 模板内连接键。
        /// </summary>
        public string ConnectionKey { get; set; }

        /// <summary>
        /// 连接名称。
        /// </summary>
        public string ConnectionName { get; set; }

        /// <summary>
        /// 传输方式，例如 Tcp、Rtu 或 Serial。
        /// </summary>
        public string Transport { get; set; }

        /// <summary>
        /// 非敏感端点引用，真实凭据由安全配置或发布流程注入。
        /// </summary>
        public string EndpointRef { get; set; }

        /// <summary>
        /// 主机地址。
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// TCP/UDP 端口。
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// 串口名称。
        /// </summary>
        public string SerialPort { get; set; }

        /// <summary>
        /// 认证方式标识，不保存明文凭据。
        /// </summary>
        public string AuthType { get; set; }

        /// <summary>
        /// 连接超时时间，毫秒。
        /// </summary>
        public int TimeoutMs { get; set; } = 3000;

        /// <summary>
        /// 重试次数。
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// 协议连接扩展参数 JSON。
        /// </summary>
        public string ProtocolOptions { get; set; }

        /// <summary>
        /// 非敏感扩展信息 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 所属采集模板。
        /// </summary>
        public CollectionTemplate CollectionTemplate { get; set; }
    }

    /// <summary>
    /// 点位模板，保存点位语义、协议绑定、采样、转换和平台映射。
    /// </summary>
    public class CollectionPointTemplate
    {
        /// <summary>
        /// 点位模板 ID。
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 所属采集模板 ID。
        /// </summary>
        public Guid CollectionTemplateId { get; set; }

        /// <summary>
        /// 关联连接键。
        /// </summary>
        public string ConnectionKey { get; set; }

        /// <summary>
        /// 模板内点位键。
        /// </summary>
        public string PointKey { get; set; }

        /// <summary>
        /// semantic-core 的 semanticId。
        /// </summary>
        public string SemanticId { get; set; }

        /// <summary>
        /// semantic-core 的 bindingId。
        /// </summary>
        public string BindingId { get; set; }

        /// <summary>
        /// 点位名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// UI 显示名称。
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 协议源类型，例如 holding-register 或 opcua-variable。
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// 协议原生地址或工程地址。
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 载荷字段路径。
        /// </summary>
        public string FieldPath { get; set; }

        /// <summary>
        /// 采集源值类型。
        /// </summary>
        public string RawValueType { get; set; }

        /// <summary>
        /// 平台值类型。
        /// </summary>
        public CollectionValueType ValueType { get; set; } = CollectionValueType.Double;

        /// <summary>
        /// 点位访问语义。
        /// </summary>
        public CollectionPointAccess Access { get; set; } = CollectionPointAccess.Read;

        /// <summary>
        /// 读取长度。
        /// </summary>
        public int Length { get; set; } = 1;

        /// <summary>
        /// 业务量纲。
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// 工程单位。
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 协议点位扩展参数 JSON。
        /// </summary>
        public string ProtocolOptions { get; set; }

        /// <summary>
        /// 质量策略 JSON。
        /// </summary>
        public string QualityPolicy { get; set; }

        /// <summary>
        /// 非敏感扩展信息 JSON。
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 所属采集模板。
        /// </summary>
        public CollectionTemplate CollectionTemplate { get; set; }

        /// <summary>
        /// 采样策略。
        /// </summary>
        public CollectionSamplingPolicy SamplingPolicy { get; set; }

        /// <summary>
        /// 平台映射策略。
        /// </summary>
        public CollectionMappingPolicy Mapping { get; set; }

        /// <summary>
        /// 值转换链。
        /// </summary>
        public List<CollectionTransformTemplate> Transforms { get; set; } = new();
    }

    /// <summary>
    /// 值转换模板。
    /// </summary>
    public class CollectionTransformTemplate
    {
        /// <summary>
        /// 转换模板 ID。
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 所属点位模板 ID。
        /// </summary>
        public Guid PointTemplateId { get; set; }

        /// <summary>
        /// 转换类型。
        /// </summary>
        public CollectionTransformType TransformType { get; set; } = CollectionTransformType.Scale;

        /// <summary>
        /// 转换顺序。
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 转换参数 JSON。
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// 所属点位模板。
        /// </summary>
        public CollectionPointTemplate PointTemplate { get; set; }
    }

    /// <summary>
    /// 采样策略模板。
    /// </summary>
    public class CollectionSamplingPolicy
    {
        /// <summary>
        /// 采样策略 ID。
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 所属点位模板 ID。
        /// </summary>
        public Guid PointTemplateId { get; set; }

        /// <summary>
        /// 读取周期，毫秒。
        /// </summary>
        public int ReadPeriodMs { get; set; } = 5000;

        /// <summary>
        /// 点位级超时时间，毫秒。
        /// </summary>
        public int? TimeoutMs { get; set; }

        /// <summary>
        /// 上报触发方式。
        /// </summary>
        public ReportTriggerType Trigger { get; set; } = ReportTriggerType.OnChange;

        /// <summary>
        /// 死区阈值。
        /// </summary>
        public double? Deadband { get; set; }

        /// <summary>
        /// 质量变化时是否上报。
        /// </summary>
        public bool ReportOnQualityChange { get; set; } = true;

        /// <summary>
        /// 是否为订阅型采集。
        /// </summary>
        public bool Subscription { get; set; }

        /// <summary>
        /// 聚合提示。
        /// </summary>
        public string AggregateHint { get; set; }

        /// <summary>
        /// 采样分组。
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 所属点位模板。
        /// </summary>
        public CollectionPointTemplate PointTemplate { get; set; }
    }

    /// <summary>
    /// 平台映射策略模板。
    /// </summary>
    public class CollectionMappingPolicy
    {
        /// <summary>
        /// 映射策略 ID。
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 所属点位模板 ID。
        /// </summary>
        public Guid PointTemplateId { get; set; }

        /// <summary>
        /// 映射目标类型。
        /// </summary>
        public CollectionTargetType TargetType { get; set; } = CollectionTargetType.Telemetry;

        /// <summary>
        /// 平台目标键名。
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// 平台值类型。
        /// </summary>
        public CollectionValueType ValueType { get; set; } = CollectionValueType.Double;

        /// <summary>
        /// 显示名称。
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 工程单位。
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 映射分组。
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 所属点位模板。
        /// </summary>
        public CollectionPointTemplate PointTemplate { get; set; }
    }
}
