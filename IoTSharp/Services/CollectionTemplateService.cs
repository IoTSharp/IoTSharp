using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace IoTSharp.Services
{
    /// <summary>
    /// 采集模板领域转换和运行时配置生成服务。
    /// </summary>
    internal static class CollectionTemplateService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// 将采集模板实体转换为对外 DTO。
        /// </summary>
        /// <param name="template">采集模板实体。</param>
        /// <returns>采集模板 DTO。</returns>
        public static CollectionTemplateDto ToDto(CollectionTemplate template)
        {
            var reportPolicy = Deserialize<ReportPolicyDto>(template.ReportPolicy) ?? new ReportPolicyDto();

            return new CollectionTemplateDto
            {
                Id = template.Id,
                ProductId = template.ProductId,
                TemplateKey = template.TemplateKey ?? string.Empty,
                Name = template.Name ?? string.Empty,
                Description = template.Description ?? string.Empty,
                SemanticModelId = template.SemanticModelId ?? string.Empty,
                Version = template.Version,
                Status = template.Status,
                Enabled = template.Enabled,
                Protocol = ToProtocolDto(template.Protocol),
                Connections = (template.Connections ?? [])
                    .OrderBy(c => c.ConnectionKey)
                    .Select(ToConnectionDto)
                    .ToArray(),
                Points = (template.Points ?? [])
                    .OrderBy(c => c.ConnectionKey)
                    .ThenBy(c => c.PointKey)
                    .Select(ToPointDto)
                    .ToArray(),
                ReportPolicy = reportPolicy,
                Metadata = DeserializeObjectMap(template.Metadata),
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt,
                CreatedBy = template.CreatedBy ?? string.Empty,
                UpdatedBy = template.UpdatedBy ?? string.Empty
            };
        }

        /// <summary>
        /// 使用请求 DTO 填充采集模板实体。
        /// </summary>
        /// <param name="template">待填充的模板实体。</param>
        /// <param name="dto">请求 DTO。</param>
        /// <param name="product">所属 Product。</param>
        /// <param name="updatedBy">操作者显示名或账号。</param>
        /// <param name="now">本次更新时间。</param>
        /// <param name="isCreate">是否新增。</param>
        public static void ApplyUpsert(
            CollectionTemplate template,
            CollectionTemplateUpsertDto dto,
            Product product,
            string updatedBy,
            DateTime now,
            bool isCreate)
        {
            template.ProductId = product.Id;
            template.Product = product;
            template.TenantId = product.Tenant?.Id;
            template.CustomerId = product.Customer?.Id;
            template.TemplateKey = NormalizeKey(dto.TemplateKey, dto.Name, "collection-template");
            template.Name = (dto.Name ?? string.Empty).Trim();
            template.Description = dto.Description ?? string.Empty;
            template.SemanticModelId = dto.SemanticModelId ?? string.Empty;
            template.Version = ResolveVersion(template.Version, dto.Version, isCreate);
            template.Status = dto.Status;
            template.Enabled = dto.Enabled;
            template.ReportPolicy = Serialize(dto.ReportPolicy ?? new ReportPolicyDto());
            template.Metadata = SerializeObjectMap(dto.Metadata);
            template.UpdatedAt = now;
            template.UpdatedBy = updatedBy;

            if (isCreate)
            {
                template.Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
                template.CreatedAt = now;
                template.CreatedBy = updatedBy;
                template.Deleted = false;
            }

            template.Protocol = ToProtocolEntity(dto.Protocol ?? new ProtocolTemplateDto(), template.Id);
            template.Connections = (dto.Connections ?? [])
                .Select(connection => ToConnectionEntity(connection, template.Id))
                .ToList();
            template.Points = (dto.Points ?? [])
                .Select(point => ToPointEntity(point, template.Id, template.Connections.FirstOrDefault()?.ConnectionKey))
                .ToList();
        }

        /// <summary>
        /// 校验采集模板是否可以生成边缘运行时配置。
        /// </summary>
        /// <param name="template">采集模板实体。</param>
        /// <returns>诊断结果。</returns>
        public static CollectionTemplateValidationResultDto Validate(CollectionTemplate template)
        {
            var diagnostics = new List<CollectionTemplateDiagnosticDto>();

            if (template == null)
            {
                AddError(diagnostics, "template.required", "$", "采集模板不能为空。");
                return ToValidationResult(diagnostics);
            }

            if (string.IsNullOrWhiteSpace(template.TemplateKey))
            {
                AddError(diagnostics, "template.key.required", "$.templateKey", "模板键不能为空。");
            }

            if (string.IsNullOrWhiteSpace(template.Name))
            {
                AddError(diagnostics, "template.name.required", "$.name", "模板名称不能为空。");
            }

            if (!template.Enabled)
            {
                AddWarning(diagnostics, "template.disabled", "$.enabled", "模板已禁用，不能作为默认运行时配置。");
            }

            ValidateProtocol(template, diagnostics);
            ValidateConnections(template, diagnostics);
            ValidatePoints(template, diagnostics);
            ValidateNonSecretJson(template.ReportPolicy, "$.reportPolicy", diagnostics);
            ValidateNonSecretJson(template.Metadata, "$.metadata", diagnostics);

            return ToValidationResult(diagnostics);
        }

        /// <summary>
        /// 从模板生成边缘运行时配置。
        /// </summary>
        /// <param name="template">采集模板。</param>
        /// <param name="edgeNodeId">目标 EdgeNode 或 Gateway ID。</param>
        /// <param name="version">运行时配置版本。</param>
        /// <param name="updatedBy">操作者显示名或账号。</param>
        /// <param name="now">生成时间。</param>
        /// <returns>边缘采集运行时配置。</returns>
        public static EdgeCollectionConfigurationDto BuildRuntimeConfiguration(
            CollectionTemplate template,
            Guid edgeNodeId,
            int version,
            string updatedBy,
            DateTime now)
        {
            var reportPolicy = Deserialize<ReportPolicyDto>(template.ReportPolicy) ?? new ReportPolicyDto();
            var tasks = new List<CollectionTaskDto>();

            foreach (var connection in (template.Connections ?? []).OrderBy(c => c.ConnectionKey))
            {
                var points = (template.Points ?? [])
                    .Where(point => point.Enabled && string.Equals(point.ConnectionKey, connection.ConnectionKey, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(point => point.PointKey)
                    .Select(ToRuntimePoint)
                    .ToArray();

                if (points.Length == 0)
                {
                    continue;
                }

                tasks.Add(new CollectionTaskDto
                {
                    Id = Guid.NewGuid(),
                    TaskKey = $"{template.TemplateKey}:{connection.ConnectionKey}",
                    Protocol = template.Protocol?.Protocol ?? CollectionProtocolType.Unknown,
                    Version = version,
                    EdgeNodeId = edgeNodeId,
                    Connection = ToRuntimeConnection(connection, template.Protocol?.Protocol ?? CollectionProtocolType.Unknown),
                    Devices =
                    [
                        new CollectionDeviceDto
                        {
                            DeviceKey = template.TemplateKey ?? template.Id.ToString("N"),
                            DeviceName = template.Name ?? template.TemplateKey ?? string.Empty,
                            Enabled = true,
                            Points = points
                        }
                    ],
                    ReportPolicy = reportPolicy
                });
            }

            return new EdgeCollectionConfigurationDto
            {
                ContractVersion = EdgeNodeContractVersions.CollectionConfigV1,
                EdgeNodeId = edgeNodeId,
                Version = version,
                UpdatedAt = now,
                UpdatedBy = updatedBy,
                Tasks = tasks
            };
        }

        /// <summary>
        /// 从模板生成可提交给 Edge 配置保存接口的更新请求。
        /// </summary>
        /// <param name="template">采集模板。</param>
        /// <param name="configuration">运行时配置。</param>
        /// <returns>带来源信息的配置更新请求。</returns>
        public static EdgeCollectionConfigurationUpdateDto ToRuntimeConfigurationUpdate(
            CollectionTemplate template,
            EdgeCollectionConfigurationDto configuration)
        {
            return new EdgeCollectionConfigurationUpdateDto
            {
                Tasks = configuration.Tasks,
                SourceType = "ProductCollectionTemplate",
                SourceId = template.Id.ToString(),
                SourceVersion = template.Version.ToString(CultureInfo.InvariantCulture),
                SourceMetadata = new Dictionary<string, object>
                {
                    ["productId"] = template.ProductId,
                    ["templateKey"] = template.TemplateKey ?? string.Empty,
                    ["semanticModelId"] = template.SemanticModelId ?? string.Empty
                }
            };
        }

        /// <summary>
        /// 使用模板和可选原始值生成预览结果。
        /// </summary>
        /// <param name="template">采集模板。</param>
        /// <param name="request">预览请求。</param>
        /// <returns>预览结果。</returns>
        public static TaskPreviewResponseDto Preview(CollectionTemplate template, CollectionTemplatePreviewRequestDto request)
        {
            var point = (template.Points ?? [])
                .FirstOrDefault(c => string.Equals(c.PointKey, request?.PointKey, StringComparison.OrdinalIgnoreCase))
                ?? (template.Points ?? []).OrderBy(c => c.PointKey).FirstOrDefault();

            if (point == null)
            {
                return new TaskPreviewResponseDto
                {
                    Success = false,
                    ErrorCode = "point.notFound",
                    ErrorMessage = "模板中没有可预览的点位。"
                };
            }

            var rawValue = ReadPreviewRawValue(request?.RawValue, template.Protocol?.Protocol ?? CollectionProtocolType.Unknown);
            try
            {
                var transformed = ApplyTransforms(rawValue, point.Transforms ?? []);
                return new TaskPreviewResponseDto
                {
                    Success = true,
                    RawValue = rawValue,
                    TransformedValue = transformed,
                    ValueType = point.ValueType,
                    QualityStatus = QualityStatusType.Good
                };
            }
            catch (InvalidOperationException exception)
            {
                return new TaskPreviewResponseDto
                {
                    Success = false,
                    RawValue = rawValue,
                    ValueType = point.ValueType,
                    QualityStatus = QualityStatusType.InvalidData,
                    ErrorCode = "transform.failed",
                    ErrorMessage = exception.Message
                };
            }
        }

        private static ProtocolTemplateDto ToProtocolDto(CollectionProtocolTemplate protocol)
            => protocol == null
                ? new ProtocolTemplateDto()
                : new ProtocolTemplateDto
                {
                    Id = protocol.Id,
                    Protocol = protocol.Protocol,
                    ProtocolKind = protocol.ProtocolKind ?? string.Empty,
                    Parameters = ParseJsonElement(protocol.Parameters),
                    Metadata = DeserializeObjectMap(protocol.Metadata)
                };

        private static ConnectionTemplateDto ToConnectionDto(CollectionConnectionTemplate connection)
            => new()
            {
                Id = connection.Id,
                ConnectionKey = connection.ConnectionKey ?? string.Empty,
                ConnectionName = connection.ConnectionName ?? string.Empty,
                Transport = connection.Transport ?? string.Empty,
                EndpointRef = connection.EndpointRef ?? string.Empty,
                Host = connection.Host ?? string.Empty,
                Port = connection.Port,
                SerialPort = connection.SerialPort ?? string.Empty,
                AuthType = connection.AuthType ?? string.Empty,
                TimeoutMs = connection.TimeoutMs,
                RetryCount = connection.RetryCount,
                ProtocolOptions = ParseJsonElement(connection.ProtocolOptions),
                Metadata = DeserializeObjectMap(connection.Metadata)
            };

        private static PointTemplateDto ToPointDto(CollectionPointTemplate point)
            => new()
            {
                Id = point.Id,
                ConnectionKey = point.ConnectionKey ?? string.Empty,
                PointKey = point.PointKey ?? string.Empty,
                SemanticId = point.SemanticId ?? string.Empty,
                BindingId = point.BindingId ?? string.Empty,
                Name = point.Name ?? string.Empty,
                DisplayName = point.DisplayName ?? string.Empty,
                SourceType = point.SourceType ?? string.Empty,
                Address = point.Address ?? string.Empty,
                FieldPath = point.FieldPath ?? string.Empty,
                RawValueType = point.RawValueType ?? string.Empty,
                ValueType = point.ValueType,
                Access = point.Access,
                Length = point.Length,
                Quantity = point.Quantity ?? string.Empty,
                Unit = point.Unit ?? string.Empty,
                Enabled = point.Enabled,
                ProtocolOptions = ParseJsonElement(point.ProtocolOptions),
                QualityPolicy = ParseJsonElement(point.QualityPolicy),
                SamplingPolicy = ToSamplingDto(point.SamplingPolicy),
                Mapping = ToMappingDto(point.Mapping),
                Transforms = (point.Transforms ?? [])
                    .OrderBy(c => c.Order)
                    .Select(ToTransformDto)
                    .ToArray(),
                Metadata = DeserializeObjectMap(point.Metadata)
            };

        private static TransformTemplateDto ToTransformDto(CollectionTransformTemplate transform)
            => new()
            {
                Id = transform.Id,
                TransformType = transform.TransformType,
                Order = transform.Order,
                Parameters = ParseJsonElement(transform.Parameters)
            };

        private static SamplingPolicyTemplateDto ToSamplingDto(CollectionSamplingPolicy sampling)
            => sampling == null
                ? new SamplingPolicyTemplateDto()
                : new SamplingPolicyTemplateDto
                {
                    Id = sampling.Id,
                    ReadPeriodMs = sampling.ReadPeriodMs,
                    TimeoutMs = sampling.TimeoutMs,
                    Trigger = sampling.Trigger,
                    Deadband = sampling.Deadband,
                    ReportOnQualityChange = sampling.ReportOnQualityChange,
                    Subscription = sampling.Subscription,
                    AggregateHint = sampling.AggregateHint ?? string.Empty,
                    Group = sampling.Group ?? string.Empty
                };

        private static MappingPolicyTemplateDto ToMappingDto(CollectionMappingPolicy mapping)
            => mapping == null
                ? new MappingPolicyTemplateDto()
                : new MappingPolicyTemplateDto
                {
                    Id = mapping.Id,
                    TargetType = mapping.TargetType,
                    TargetName = mapping.TargetName ?? string.Empty,
                    ValueType = mapping.ValueType,
                    DisplayName = mapping.DisplayName ?? string.Empty,
                    Unit = mapping.Unit ?? string.Empty,
                    Group = mapping.Group ?? string.Empty
                };

        private static CollectionProtocolTemplate ToProtocolEntity(ProtocolTemplateDto dto, Guid templateId)
            => new()
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                CollectionTemplateId = templateId,
                Protocol = dto.Protocol,
                ProtocolKind = string.IsNullOrWhiteSpace(dto.ProtocolKind) ? NormalizeProtocolKind(dto.Protocol) : dto.ProtocolKind.Trim(),
                Parameters = SerializeElement(dto.Parameters),
                Metadata = SerializeObjectMap(dto.Metadata)
            };

        private static CollectionConnectionTemplate ToConnectionEntity(ConnectionTemplateDto dto, Guid templateId)
            => new()
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                CollectionTemplateId = templateId,
                ConnectionKey = NormalizeKey(dto.ConnectionKey, dto.ConnectionName, "connection"),
                ConnectionName = dto.ConnectionName ?? string.Empty,
                Transport = dto.Transport ?? string.Empty,
                EndpointRef = dto.EndpointRef ?? string.Empty,
                Host = dto.Host ?? string.Empty,
                Port = dto.Port,
                SerialPort = dto.SerialPort ?? string.Empty,
                AuthType = dto.AuthType ?? string.Empty,
                TimeoutMs = dto.TimeoutMs <= 0 ? 3000 : dto.TimeoutMs,
                RetryCount = dto.RetryCount < 0 ? 0 : dto.RetryCount,
                ProtocolOptions = SerializeElement(dto.ProtocolOptions),
                Metadata = SerializeObjectMap(dto.Metadata)
            };

        private static CollectionPointTemplate ToPointEntity(PointTemplateDto dto, Guid templateId, string defaultConnectionKey)
        {
            var pointId = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id;
            return new CollectionPointTemplate
            {
                Id = pointId,
                CollectionTemplateId = templateId,
                ConnectionKey = string.IsNullOrWhiteSpace(dto.ConnectionKey) ? defaultConnectionKey ?? string.Empty : dto.ConnectionKey.Trim(),
                PointKey = NormalizeKey(dto.PointKey, dto.Name, "point"),
                SemanticId = dto.SemanticId ?? string.Empty,
                BindingId = dto.BindingId ?? string.Empty,
                Name = dto.Name ?? string.Empty,
                DisplayName = dto.DisplayName ?? string.Empty,
                SourceType = dto.SourceType ?? string.Empty,
                Address = dto.Address ?? string.Empty,
                FieldPath = dto.FieldPath ?? string.Empty,
                RawValueType = dto.RawValueType ?? string.Empty,
                ValueType = dto.ValueType,
                Access = dto.Access,
                Length = dto.Length <= 0 ? 1 : dto.Length,
                Quantity = dto.Quantity ?? string.Empty,
                Unit = dto.Unit ?? string.Empty,
                Enabled = dto.Enabled,
                ProtocolOptions = SerializeElement(dto.ProtocolOptions),
                QualityPolicy = SerializeElement(dto.QualityPolicy),
                Metadata = SerializeObjectMap(dto.Metadata),
                SamplingPolicy = ToSamplingEntity(dto.SamplingPolicy ?? new SamplingPolicyTemplateDto(), pointId),
                Mapping = ToMappingEntity(dto.Mapping ?? new MappingPolicyTemplateDto(), pointId),
                Transforms = (dto.Transforms ?? [])
                    .Select(transform => ToTransformEntity(transform, pointId))
                    .ToList()
            };
        }

        private static CollectionTransformTemplate ToTransformEntity(TransformTemplateDto dto, Guid pointId)
            => new()
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                PointTemplateId = pointId,
                TransformType = dto.TransformType,
                Order = dto.Order,
                Parameters = SerializeElement(dto.Parameters)
            };

        private static CollectionSamplingPolicy ToSamplingEntity(SamplingPolicyTemplateDto dto, Guid pointId)
            => new()
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                PointTemplateId = pointId,
                ReadPeriodMs = dto.ReadPeriodMs <= 0 ? 5000 : dto.ReadPeriodMs,
                TimeoutMs = dto.TimeoutMs,
                Trigger = dto.Trigger,
                Deadband = dto.Deadband,
                ReportOnQualityChange = dto.ReportOnQualityChange,
                Subscription = dto.Subscription,
                AggregateHint = dto.AggregateHint ?? string.Empty,
                Group = dto.Group ?? string.Empty
            };

        private static CollectionMappingPolicy ToMappingEntity(MappingPolicyTemplateDto dto, Guid pointId)
            => new()
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                PointTemplateId = pointId,
                TargetType = dto.TargetType,
                TargetName = dto.TargetName ?? string.Empty,
                ValueType = dto.ValueType,
                DisplayName = dto.DisplayName ?? string.Empty,
                Unit = dto.Unit ?? string.Empty,
                Group = dto.Group ?? string.Empty
            };

        private static CollectionConnectionDto ToRuntimeConnection(CollectionConnectionTemplate connection, CollectionProtocolType protocol)
            => new()
            {
                ConnectionKey = connection.ConnectionKey ?? string.Empty,
                ConnectionName = connection.ConnectionName ?? string.Empty,
                Protocol = protocol,
                Transport = connection.Transport ?? string.Empty,
                Host = string.IsNullOrWhiteSpace(connection.Host) ? null : connection.Host,
                Port = connection.Port,
                SerialPort = string.IsNullOrWhiteSpace(connection.SerialPort) ? null : connection.SerialPort,
                TimeoutMs = connection.TimeoutMs,
                RetryCount = connection.RetryCount,
                ProtocolOptions = ParseJsonElement(connection.ProtocolOptions)
            };

        private static CollectionPointDto ToRuntimePoint(CollectionPointTemplate point)
            => new()
            {
                PointKey = point.PointKey ?? string.Empty,
                PointName = string.IsNullOrWhiteSpace(point.DisplayName) ? point.Name ?? string.Empty : point.DisplayName,
                SourceType = point.SourceType ?? string.Empty,
                Address = point.Address ?? string.Empty,
                RawValueType = point.RawValueType ?? string.Empty,
                Length = point.Length,
                Polling = new PollingPolicyDto
                {
                    ReadPeriodMs = point.SamplingPolicy?.ReadPeriodMs > 0 ? point.SamplingPolicy.ReadPeriodMs : 5000,
                    Group = string.IsNullOrWhiteSpace(point.SamplingPolicy?.Group) ? null : point.SamplingPolicy.Group
                },
                Transforms = (point.Transforms ?? [])
                    .OrderBy(transform => transform.Order)
                    .Select(transform => new ValueTransformDto
                    {
                        TransformType = transform.TransformType,
                        Order = transform.Order,
                        Parameters = ParseJsonElement(transform.Parameters)
                    })
                    .ToArray(),
                Mapping = new PlatformMappingDto
                {
                    TargetType = point.Mapping?.TargetType ?? CollectionTargetType.Telemetry,
                    TargetName = point.Mapping?.TargetName ?? point.PointKey ?? string.Empty,
                    ValueType = point.Mapping?.ValueType ?? point.ValueType,
                    DisplayName = string.IsNullOrWhiteSpace(point.Mapping?.DisplayName)
                        ? (string.IsNullOrWhiteSpace(point.DisplayName) ? point.Name : point.DisplayName)
                        : point.Mapping.DisplayName,
                    Unit = string.IsNullOrWhiteSpace(point.Mapping?.Unit) ? point.Unit : point.Mapping.Unit,
                    Group = string.IsNullOrWhiteSpace(point.Mapping?.Group) ? point.SamplingPolicy?.Group : point.Mapping.Group
                },
                ProtocolOptions = ParseJsonElement(point.ProtocolOptions)
            };

        private static void ValidateProtocol(CollectionTemplate template, List<CollectionTemplateDiagnosticDto> diagnostics)
        {
            if (template.Protocol == null)
            {
                AddError(diagnostics, "protocol.required", "$.protocol", "协议模板不能为空。");
                return;
            }

            if (template.Protocol.Protocol == CollectionProtocolType.Unknown)
            {
                AddError(diagnostics, "protocol.unknown", "$.protocol.protocol", "协议类型不能为 Unknown。");
            }

            ValidateNonSecretJson(template.Protocol.Parameters, "$.protocol.parameters", diagnostics);
            ValidateNonSecretJson(template.Protocol.Metadata, "$.protocol.metadata", diagnostics);
        }

        private static void ValidateConnections(CollectionTemplate template, List<CollectionTemplateDiagnosticDto> diagnostics)
        {
            var connections = template.Connections ?? [];
            if (connections.Count == 0)
            {
                AddError(diagnostics, "connection.required", "$.connections", "至少需要一个连接模板。");
                return;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < connections.Count; index++)
            {
                var connection = connections[index];
                var path = $"$.connections[{index}]";
                if (string.IsNullOrWhiteSpace(connection.ConnectionKey))
                {
                    AddError(diagnostics, "connection.key.required", $"{path}.connectionKey", "连接键不能为空。");
                }
                else if (!seen.Add(connection.ConnectionKey))
                {
                    AddError(diagnostics, "connection.key.duplicate", $"{path}.connectionKey", $"连接键 {connection.ConnectionKey} 重复。");
                }

                if (string.IsNullOrWhiteSpace(connection.ConnectionName))
                {
                    AddError(diagnostics, "connection.name.required", $"{path}.connectionName", "连接名称不能为空。");
                }

                if (connection.TimeoutMs <= 0)
                {
                    AddError(diagnostics, "connection.timeout.invalid", $"{path}.timeoutMs", "连接超时时间必须大于 0。");
                }

                if (connection.RetryCount < 0)
                {
                    AddError(diagnostics, "connection.retry.invalid", $"{path}.retryCount", "重试次数不能为负数。");
                }

                ValidateNonSecretString(connection.EndpointRef, $"{path}.endpointRef", diagnostics);
                ValidateNonSecretString(connection.Host, $"{path}.host", diagnostics);
                ValidateNonSecretJson(connection.ProtocolOptions, $"{path}.protocolOptions", diagnostics);
                ValidateNonSecretJson(connection.Metadata, $"{path}.metadata", diagnostics);
            }
        }

        private static void ValidatePoints(CollectionTemplate template, List<CollectionTemplateDiagnosticDto> diagnostics)
        {
            var points = template.Points ?? [];
            if (points.Count == 0)
            {
                AddError(diagnostics, "point.required", "$.points", "至少需要一个点位模板。");
                return;
            }

            var connectionKeys = (template.Connections ?? []).Select(c => c.ConnectionKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var pointKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var semanticIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var bindingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < points.Count; index++)
            {
                var point = points[index];
                var path = $"$.points[{index}]";
                if (string.IsNullOrWhiteSpace(point.PointKey))
                {
                    AddError(diagnostics, "point.key.required", $"{path}.pointKey", "点位键不能为空。");
                }
                else if (!pointKeys.Add(point.PointKey))
                {
                    AddError(diagnostics, "point.key.duplicate", $"{path}.pointKey", $"点位键 {point.PointKey} 重复。");
                }

                if (string.IsNullOrWhiteSpace(point.ConnectionKey) || !connectionKeys.Contains(point.ConnectionKey))
                {
                    AddError(diagnostics, "point.connection.invalid", $"{path}.connectionKey", "点位必须引用已存在的连接键。");
                }

                if (string.IsNullOrWhiteSpace(point.SemanticId))
                {
                    AddError(diagnostics, "point.semanticId.required", $"{path}.semanticId", "点位必须保留 semantic-core semanticId。");
                }
                else if (!semanticIds.Add(point.SemanticId))
                {
                    AddError(diagnostics, "point.semanticId.duplicate", $"{path}.semanticId", $"semanticId {point.SemanticId} 重复。");
                }

                if (string.IsNullOrWhiteSpace(point.BindingId))
                {
                    AddError(diagnostics, "point.bindingId.required", $"{path}.bindingId", "点位必须保留 semantic-core bindingId。");
                }
                else if (!bindingIds.Add(point.BindingId))
                {
                    AddError(diagnostics, "point.bindingId.duplicate", $"{path}.bindingId", $"bindingId {point.BindingId} 重复。");
                }

                if (string.IsNullOrWhiteSpace(point.Name))
                {
                    AddError(diagnostics, "point.name.required", $"{path}.name", "点位名称不能为空。");
                }

                if (string.IsNullOrWhiteSpace(point.Address))
                {
                    AddError(diagnostics, "point.address.required", $"{path}.address", "点位协议地址不能为空。");
                }

                if (point.Length <= 0)
                {
                    AddError(diagnostics, "point.length.invalid", $"{path}.length", "点位读取长度必须大于 0。");
                }

                ValidateSampling(point, path, diagnostics);
                ValidateMapping(point, path, diagnostics);
                ValidateTransforms(point, path, diagnostics);
                ValidateNonSecretJson(point.ProtocolOptions, $"{path}.protocolOptions", diagnostics);
                ValidateNonSecretJson(point.QualityPolicy, $"{path}.qualityPolicy", diagnostics);
                ValidateNonSecretJson(point.Metadata, $"{path}.metadata", diagnostics);
            }
        }

        private static void ValidateSampling(CollectionPointTemplate point, string path, List<CollectionTemplateDiagnosticDto> diagnostics)
        {
            if (point.SamplingPolicy == null)
            {
                AddError(diagnostics, "sampling.required", $"{path}.samplingPolicy", "点位必须包含采样策略。");
                return;
            }

            if (point.SamplingPolicy.ReadPeriodMs <= 0)
            {
                AddError(diagnostics, "sampling.period.invalid", $"{path}.samplingPolicy.readPeriodMs", "采样周期必须大于 0。");
            }

            if (point.SamplingPolicy.Deadband < 0)
            {
                AddError(diagnostics, "sampling.deadband.invalid", $"{path}.samplingPolicy.deadband", "死区不能为负数。");
            }
        }

        private static void ValidateMapping(CollectionPointTemplate point, string path, List<CollectionTemplateDiagnosticDto> diagnostics)
        {
            if (point.Mapping == null)
            {
                AddError(diagnostics, "mapping.required", $"{path}.mapping", "点位必须包含平台映射策略。");
                return;
            }

            if (string.IsNullOrWhiteSpace(point.Mapping.TargetName))
            {
                AddError(diagnostics, "mapping.targetName.required", $"{path}.mapping.targetName", "平台映射目标键名不能为空。");
            }
        }

        private static void ValidateTransforms(CollectionPointTemplate point, string path, List<CollectionTemplateDiagnosticDto> diagnostics)
        {
            var orders = new HashSet<int>();
            var transforms = point.Transforms ?? [];
            for (var index = 0; index < transforms.Count; index++)
            {
                var transform = transforms[index];
                if (!orders.Add(transform.Order))
                {
                    AddWarning(diagnostics, "transform.order.duplicate", $"{path}.transforms[{index}].order", "转换链存在重复顺序，运行时会按数据库返回顺序以外再按 order 排序。");
                }

                ValidateNonSecretJson(transform.Parameters, $"{path}.transforms[{index}].parameters", diagnostics);
            }
        }

        private static CollectionTemplateValidationResultDto ToValidationResult(List<CollectionTemplateDiagnosticDto> diagnostics)
            => new()
            {
                Success = diagnostics.All(c => c.Severity != CollectionTemplateDiagnosticSeverity.Error),
                Diagnostics = diagnostics
            };

        private static void AddError(List<CollectionTemplateDiagnosticDto> diagnostics, string code, string path, string message)
            => diagnostics.Add(new CollectionTemplateDiagnosticDto
            {
                Severity = CollectionTemplateDiagnosticSeverity.Error,
                Code = code,
                Path = path,
                Message = message
            });

        private static void AddWarning(List<CollectionTemplateDiagnosticDto> diagnostics, string code, string path, string message)
            => diagnostics.Add(new CollectionTemplateDiagnosticDto
            {
                Severity = CollectionTemplateDiagnosticSeverity.Warning,
                Code = code,
                Path = path,
                Message = message
            });

        private static object ReadPreviewRawValue(JsonElement? rawValue, CollectionProtocolType protocol)
        {
            if (rawValue.HasValue)
            {
                var element = rawValue.Value;
                return element.ValueKind switch
                {
                    JsonValueKind.Number when element.TryGetDouble(out var number) => number,
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => element.GetRawText()
                };
            }

            return protocol switch
            {
                CollectionProtocolType.Modbus => 1234d,
                CollectionProtocolType.OpcUa => 56.78d,
                CollectionProtocolType.Mqtt => 42d,
                _ => 0d
            };
        }

        private static object ApplyTransforms(object rawValue, IEnumerable<CollectionTransformTemplate> transforms)
        {
            object current = rawValue;
            foreach (var transform in transforms.OrderBy(c => c.Order))
            {
                current = transform.TransformType switch
                {
                    CollectionTransformType.Scale => ToDouble(current) * GetParameter(transform.Parameters, 1d, "factor", "scale", "value"),
                    CollectionTransformType.Offset => ToDouble(current) + GetParameter(transform.Parameters, 0d, "offset", "value"),
                    CollectionTransformType.Clamp => Clamp(
                        ToDouble(current),
                        GetParameter(transform.Parameters, double.NegativeInfinity, "min", "minimum"),
                        GetParameter(transform.Parameters, double.PositiveInfinity, "max", "maximum")),
                    CollectionTransformType.DefaultOnError => current,
                    CollectionTransformType.Expression => current,
                    CollectionTransformType.EnumMap => current,
                    CollectionTransformType.BitExtract => current,
                    CollectionTransformType.WordSwap => current,
                    CollectionTransformType.ByteSwap => current,
                    _ => current
                };
            }

            return current;
        }

        private static double ToDouble(object value)
        {
            if (value is double number)
            {
                return number;
            }

            if (value is float single)
            {
                return single;
            }

            if (value is int integer)
            {
                return integer;
            }

            if (value is long longValue)
            {
                return longValue;
            }

            if (value is decimal decimalValue)
            {
                return (double)decimalValue;
            }

            if (double.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            throw new InvalidOperationException("当前转换链需要数值型原始值。");
        }

        private static double Clamp(double value, double min, double max)
            => Math.Min(Math.Max(value, min), max);

        private static double GetParameter(string parametersJson, double fallback, params string[] names)
        {
            if (string.IsNullOrWhiteSpace(parametersJson))
            {
                return fallback;
            }

            try
            {
                using var document = JsonDocument.Parse(parametersJson);
                foreach (var name in names)
                {
                    if (document.RootElement.ValueKind == JsonValueKind.Object
                        && document.RootElement.TryGetProperty(name, out var property)
                        && property.TryGetDouble(out var value))
                    {
                        return value;
                    }
                }
            }
            catch (JsonException)
            {
                return fallback;
            }

            return fallback;
        }

        private static string NormalizeKey(string key, string name, string fallbackPrefix)
        {
            var source = string.IsNullOrWhiteSpace(key) ? name : key;
            source = string.IsNullOrWhiteSpace(source) ? $"{fallbackPrefix}-{Guid.NewGuid():N}" : source.Trim();
            var chars = source
                .Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.' ? char.ToLowerInvariant(c) : '-')
                .ToArray();
            var normalized = new string(chars).Trim('-');
            return string.IsNullOrWhiteSpace(normalized) ? $"{fallbackPrefix}-{Guid.NewGuid():N}" : normalized;
        }

        private static int ResolveVersion(int currentVersion, int requestedVersion, bool isCreate)
        {
            if (requestedVersion > 0)
            {
                return requestedVersion;
            }

            return isCreate ? 1 : Math.Max(1, currentVersion + 1);
        }

        private static string NormalizeProtocolKind(CollectionProtocolType protocol)
            => protocol switch
            {
                CollectionProtocolType.Modbus => "modbusTcp",
                CollectionProtocolType.OpcUa => "opcUa",
                CollectionProtocolType.Mqtt => "mqtt",
                CollectionProtocolType.Bacnet => "bacnet",
                CollectionProtocolType.IEC104 => "iec104",
                CollectionProtocolType.Custom => "custom",
                _ => string.Empty
            };

        private static T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        private static Dictionary<string, object> DeserializeObjectMap(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static JsonElement? ParseJsonElement(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                return document.RootElement.Clone();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string Serialize<T>(T value)
            => value == null ? "{}" : JsonSerializer.Serialize(value, JsonOptions);

        private static string SerializeObjectMap(Dictionary<string, object> map)
            => map == null || map.Count == 0 ? "{}" : JsonSerializer.Serialize(map, JsonOptions);

        private static string SerializeElement(JsonElement? element)
            => element.HasValue ? element.Value.GetRawText() : string.Empty;

        private static void ValidateNonSecretString(string value, string path, List<CollectionTemplateDiagnosticDto> diagnostics)
        {
            if (LooksSensitiveValue(value))
            {
                AddError(diagnostics, "secret.value.forbidden", path, "采集模板不能保存明文密码、Token、连接串或访问密钥。");
            }
        }

        private static void ValidateNonSecretJson(string json, string path, List<CollectionTemplateDiagnosticDto> diagnostics)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                ValidateNonSecretElement(document.RootElement, path, diagnostics);
            }
            catch (JsonException)
            {
                AddError(diagnostics, "json.invalid", path, "JSON 扩展参数格式不正确。");
            }
        }

        private static void ValidateNonSecretElement(JsonElement element, string path, List<CollectionTemplateDiagnosticDto> diagnostics)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var childPath = $"{path}.{property.Name}";
                        if (LooksSensitiveKey(property.Name))
                        {
                            AddError(diagnostics, "secret.key.forbidden", childPath, "采集模板扩展参数不能包含密码、Token 或访问密钥字段。");
                        }

                        ValidateNonSecretElement(property.Value, childPath, diagnostics);
                    }

                    break;
                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        ValidateNonSecretElement(item, $"{path}[{index++}]", diagnostics);
                    }

                    break;
                case JsonValueKind.String:
                    ValidateNonSecretString(element.GetString(), path, diagnostics);
                    break;
            }
        }

        private static bool LooksSensitiveKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var normalized = key.Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .ToLowerInvariant();
            return normalized.Contains("password", StringComparison.Ordinal)
                || normalized.Contains("passwd", StringComparison.Ordinal)
                || normalized.Contains("secret", StringComparison.Ordinal)
                || normalized.Contains("credential", StringComparison.Ordinal)
                || normalized.Contains("accesstoken", StringComparison.Ordinal)
                || normalized.Contains("privatekey", StringComparison.Ordinal)
                || normalized.Contains("apikey", StringComparison.Ordinal)
                || normalized.Contains("connectionstring", StringComparison.Ordinal);
        }

        private static bool LooksSensitiveValue(string value)
            => !string.IsNullOrWhiteSpace(value)
               && (value.Contains("password=", StringComparison.OrdinalIgnoreCase)
                   || value.Contains("pwd=", StringComparison.OrdinalIgnoreCase)
                   || value.Contains("access_token=", StringComparison.OrdinalIgnoreCase)
                   || value.Contains("secret=", StringComparison.OrdinalIgnoreCase));
    }
}
