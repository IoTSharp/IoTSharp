using IoTSharp.Contracts;
using IoTSharp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace IoTSharp.Controllers;

[Route("api/[controller]/[action]")]
[Authorize]
[ApiController]
public class CollectionTaskController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CollectionTaskController> _logger;

    public CollectionTaskController(ApplicationDbContext context, ILogger<CollectionTaskController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.NormalUser))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public ApiResult<CollectionTaskDto> GetDraft(CollectionProtocolType protocol)
    {
        var draft = CreateDraft(protocol);
        return new ApiResult<CollectionTaskDto>(ApiCode.Success, "OK", draft);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.NormalUser))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public ActionResult<ApiResult<CollectionTaskDto>> ValidateDraft([FromBody] CollectionTaskDto request)
    {
        if (request == null)
        {
            return Ok(new ApiResult<CollectionTaskDto>(ApiCode.InValidData, "Task payload is required", null));
        }

        if (string.IsNullOrWhiteSpace(request.TaskKey))
        {
            return Ok(new ApiResult<CollectionTaskDto>(ApiCode.InValidData, "taskKey is required", null));
        }

        if (request.Connection == null || string.IsNullOrWhiteSpace(request.Connection.ConnectionName))
        {
            return Ok(new ApiResult<CollectionTaskDto>(ApiCode.InValidData, "connection.connectionName is required", null));
        }

        if (request.Devices == null || request.Devices.Count == 0)
        {
            return Ok(new ApiResult<CollectionTaskDto>(ApiCode.InValidData, "At least one device is required", null));
        }

        foreach (var device in request.Devices)
        {
            if (string.IsNullOrWhiteSpace(device.DeviceKey))
            {
                return Ok(new ApiResult<CollectionTaskDto>(ApiCode.InValidData, "device.deviceKey is required", null));
            }

            foreach (var point in device.Points)
            {
                if (string.IsNullOrWhiteSpace(point.PointKey) || string.IsNullOrWhiteSpace(point.PointName))
                {
                    return Ok(new ApiResult<CollectionTaskDto>(ApiCode.InValidData, "point.pointKey and point.pointName are required", null));
                }

                if (point.Mapping == null || string.IsNullOrWhiteSpace(point.Mapping.TargetName))
                {
                    return Ok(new ApiResult<CollectionTaskDto>(ApiCode.InValidData, "point.mapping.targetName is required", null));
                }
            }
        }

        return Ok(new ApiResult<CollectionTaskDto>(ApiCode.Success, "OK", request));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.NormalUser))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    public ActionResult<ApiResult<TaskPreviewResponseDto>> Preview([FromBody] TaskPreviewRequestDto request)
    {
        if (request == null)
        {
            return Ok(new ApiResult<TaskPreviewResponseDto>(ApiCode.InValidData, "Preview payload is required", null));
        }

        var response = new TaskPreviewResponseDto
        {
            Success = true,
            RawValue = request.Protocol switch
            {
                CollectionProtocolType.Modbus => 1234,
                CollectionProtocolType.OpcUa => 56.78,
                _ => "preview-not-supported"
            },
            TransformedValue = request.Protocol switch
            {
                CollectionProtocolType.Modbus => 123.4,
                CollectionProtocolType.OpcUa => 56.78,
                _ => "preview-not-supported"
            },
            ValueType = CollectionValueType.Double,
            QualityStatus = QualityStatusType.Good,
        };

        _logger.LogInformation("Previewed collection point {PointKey} for protocol {Protocol}", request.Point?.PointKey, request.Protocol);
        return Ok(new ApiResult<TaskPreviewResponseDto>(ApiCode.Success, "OK", response));
    }

    private static CollectionTaskDto CreateDraft(CollectionProtocolType protocol)
    {
        return new CollectionTaskDto
        {
            Id = Guid.NewGuid(),
            TaskKey = $"{protocol.ToString().ToLowerInvariant()}-draft",
            Protocol = protocol,
            Version = 1,
            Connection = new CollectionConnectionDto
            {
                ConnectionKey = "default-connection",
                ConnectionName = "默认连接",
                Protocol = protocol,
                Transport = protocol == CollectionProtocolType.Modbus ? "Tcp" : "Tcp",
                Host = "127.0.0.1",
                Port = protocol == CollectionProtocolType.Modbus ? 502 : 4840,
                TimeoutMs = 3000,
                RetryCount = 3,
            },
            Devices =
            [
                new CollectionDeviceDto
                {
                    DeviceKey = "device-1",
                    DeviceName = "示例设备",
                    Enabled = true,
                    Points =
                    [
                        new CollectionPointDto
                        {
                            PointKey = "point-1",
                            PointName = "示例点位",
                            SourceType = protocol == CollectionProtocolType.Modbus ? "HoldingRegister" : "Variable",
                            Address = protocol == CollectionProtocolType.Modbus ? "40001" : "ns=2;s=Demo.Dynamic.Scalar.Double",
                            RawValueType = "Double",
                            Length = 1,
                            Polling = new PollingPolicyDto { ReadPeriodMs = 5000, Group = "default" },
                            Mapping = new PlatformMappingDto
                            {
                                TargetType = CollectionTargetType.Telemetry,
                                TargetName = "demoValue",
                                ValueType = CollectionValueType.Double,
                                DisplayName = "示例值",
                                Unit = protocol == CollectionProtocolType.Modbus ? "°C" : null,
                                Group = "default"
                            }
                        }
                    ]
                }
            ],
            ReportPolicy = new ReportPolicyDto
            {
                DefaultTrigger = ReportTriggerType.OnChange,
                IncludeQuality = true,
                IncludeTimestamp = true,
            }
        };
    }
}