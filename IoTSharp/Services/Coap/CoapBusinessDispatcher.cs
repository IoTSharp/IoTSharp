using CoAP.Server.Routing;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IoTSharp.Services.Coap
{
    /// <summary>
    /// CoAP 平台入口命中后的业务分发服务。
    /// </summary>
    public interface ICoapBusinessDispatcher
    {
        /// <summary>
        /// 将 CoAP 平台入口上下文映射到现有设备、网关、遥测、属性和告警业务入口。
        /// </summary>
        /// <param name="context">CoAP 平台入口业务上下文。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>业务分发结果，供协议层映射响应码。</returns>
        ValueTask<CoapBusinessDispatchResult> DispatchAsync(CoapBusinessDispatchContext context, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// CoAP 平台入口业务上下文。
    /// </summary>
    public sealed class CoapBusinessDispatchContext
    {
        /// <summary>
        /// 创建 CoAP 平台入口业务上下文。
        /// </summary>
        /// <param name="platformRoute">平台入口匹配信息。</param>
        /// <param name="protocolContext">CoAP.NET 已匹配的协议 route 上下文。</param>
        public CoapBusinessDispatchContext(CoapPlatformRouteMatch platformRoute, CoapRouteContext protocolContext)
        {
            PlatformRoute = platformRoute;
            ProtocolContext = protocolContext ?? throw new ArgumentNullException(nameof(protocolContext));
        }

        /// <summary>
        /// 平台入口匹配信息。
        /// </summary>
        public CoapPlatformRouteMatch PlatformRoute { get; }

        /// <summary>
        /// CoAP.NET 已匹配的协议 route 上下文。
        /// </summary>
        public CoapRouteContext ProtocolContext { get; }

        /// <summary>
        /// Uri-Query 拆分后的查询片段。
        /// </summary>
        public IReadOnlyList<string> Queries => ProtocolContext.Queries;

        /// <summary>
        /// 请求负载，不强制转换为字符串。
        /// </summary>
        public ReadOnlyMemory<byte> Payload => ProtocolContext.Payload;

        /// <summary>
        /// CoAP Content-Format option 值。
        /// </summary>
        public int ContentFormat => ProtocolContext.ContentFormat;

        /// <summary>
        /// CoAP Accept option 值。
        /// </summary>
        public int Accept => ProtocolContext.Accept;
    }

    /// <summary>
    /// CoAP 业务分发结果类型。
    /// </summary>
    public enum CoapBusinessDispatchStatus
    {
        Success,
        BadRequest,
        Unauthorized,
        Forbidden,
        NotFound,
        NotAcceptable,
        UnsupportedOperation,
        Error
    }

    /// <summary>
    /// CoAP 业务分发结果。
    /// </summary>
    public sealed class CoapBusinessDispatchResult
    {
        private CoapBusinessDispatchResult(CoapBusinessDispatchStatus status, string message)
        {
            Status = status;
            Message = message;
        }

        /// <summary>
        /// 结果状态。
        /// </summary>
        public CoapBusinessDispatchStatus Status { get; }

        /// <summary>
        /// 面向设备端的简短结果说明。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 创建成功结果。
        /// </summary>
        /// <param name="message">结果说明。</param>
        /// <returns>成功结果。</returns>
        public static CoapBusinessDispatchResult Success(string message = "OK") => new(CoapBusinessDispatchStatus.Success, message);

        /// <summary>
        /// 创建失败结果。
        /// </summary>
        /// <param name="status">失败状态。</param>
        /// <param name="message">失败说明。</param>
        /// <returns>失败结果。</returns>
        public static CoapBusinessDispatchResult Fail(CoapBusinessDispatchStatus status, string message) => new(status, message);
    }

    /// <summary>
    /// 默认 CoAP 业务分发服务，复用现有设备身份和事件总线入口。
    /// </summary>
    public sealed class CoapBusinessDispatcher : ICoapBusinessDispatcher
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CoapBusinessDispatcher> _logger;

        /// <summary>
        /// 创建 CoAP 业务分发服务。
        /// </summary>
        /// <param name="scopeFactory">用于在 HostedService 请求处理中创建业务作用域。</param>
        /// <param name="logger">日志记录器。</param>
        public CoapBusinessDispatcher(IServiceScopeFactory scopeFactory, ILogger<CoapBusinessDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <inheritdoc />
        public async ValueTask<CoapBusinessDispatchResult> DispatchAsync(CoapBusinessDispatchContext context, CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.BadRequest, "CoAP business context is required.");
            }

            try
            {
                return await DispatchCoreAsync(context, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CoAP business dispatch failed. TargetKind={TargetKind}, TargetName={TargetName}, Operation={Operation}.",
                    context.PlatformRoute.TargetKind,
                    context.PlatformRoute.TargetName,
                    context.PlatformRoute.Operation);
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.Error, "CoAP business dispatch failed.");
            }
        }

        private async ValueTask<CoapBusinessDispatchResult> DispatchCoreAsync(
            CoapBusinessDispatchContext context,
            CancellationToken cancellationToken)
        {
            if (!TryGetAccessToken(context.ProtocolContext, out var accessToken))
            {
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.Unauthorized, "Access token is missing or invalid.");
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var device = await FindDeviceByAccessTokenAsync(dbContext, accessToken, cancellationToken);
            if (device == null)
            {
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.Unauthorized, "Access token is missing or invalid.");
            }

            if (!TargetMatches(context.PlatformRoute, device))
            {
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.Forbidden, "Access token does not match route target.");
            }

            if (context.Payload.IsEmpty)
            {
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.BadRequest, "Payload is required.");
            }

            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
            switch (context.PlatformRoute.Operation)
            {
                case CoapPlatformOperation.Telemetry:
                    {
                        var parseResult = TryParsePayload(context, out var payload);
                        if (parseResult != null)
                        {
                            return parseResult;
                        }

                        await publisher.PublishActive(device.Id, ActivityStatus.Activity);
                        return await PublishTelemetryAsync(publisher, device, payload);
                    }

                case CoapPlatformOperation.Attributes:
                    {
                        var parseResult = TryParsePayload(context, out var payload);
                        if (parseResult != null)
                        {
                            return parseResult;
                        }

                        await publisher.PublishActive(device.Id, ActivityStatus.Activity);
                        return await PublishAttributesAsync(publisher, device, payload);
                    }

                case CoapPlatformOperation.Alarm when context.PlatformRoute.TargetKind == CoapPlatformTargetKind.Device:
                    {
                        var parseResult = TryParseAlarm(context, out var alarm);
                        if (parseResult != null)
                        {
                            return parseResult;
                        }

                        await publisher.PublishActive(device.Id, ActivityStatus.Activity);
                        return await PublishAlarmAsync(publisher, device, alarm);
                    }

                default:
                    return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.UnsupportedOperation, "CoAP business operation is not supported.");
            }
        }

        private static async Task<Device> FindDeviceByAccessTokenAsync(ApplicationDbContext dbContext, string accessToken, CancellationToken cancellationToken)
        {
            var identity = await dbContext.DeviceIdentities
                .AsNoTracking()
                .Where(c => c.IdentityType == IdentityType.AccessToken && c.IdentityId == accessToken)
                .Select(c => new { c.DeviceId })
                .FirstOrDefaultAsync(cancellationToken);

            if (identity == null)
            {
                return null;
            }

            return await dbContext.Device
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == identity.DeviceId && !c.Deleted, cancellationToken);
        }

        private static bool TargetMatches(CoapPlatformRouteMatch route, Device device)
        {
            if (route.TargetKind == CoapPlatformTargetKind.Gateway && device.DeviceType != DeviceType.Gateway)
            {
                return false;
            }

            if (route.TargetKind == CoapPlatformTargetKind.Device && device.DeviceType != DeviceType.Device)
            {
                return false;
            }

            return string.Equals(route.TargetName, device.Name, StringComparison.Ordinal)
                || string.Equals(route.TargetName, device.Id.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetAccessToken(CoapRouteContext context, out string accessToken)
        {
            accessToken = null;
            if (context == null)
            {
                return false;
            }

            foreach (var name in TokenQueryNames)
            {
                context.TryGetQueryValue(name, StringComparison.OrdinalIgnoreCase, out var value);
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                accessToken = value;
                return true;
            }

            return false;
        }

        private static readonly string[] TokenQueryNames = { "access_token", "accessToken", "token" };

        private CoapBusinessDispatchResult TryParsePayload(
            CoapBusinessDispatchContext context,
            out Dictionary<string, object> payload)
        {
            try
            {
                payload = CoapPayloadParser.ParseObject(context.Payload);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    "CoAP payload is not valid JSON. TargetKind={TargetKind}, TargetName={TargetName}, Operation={Operation}.",
                    context.PlatformRoute.TargetKind,
                    context.PlatformRoute.TargetName,
                    context.PlatformRoute.Operation);
                payload = null;
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.BadRequest, "Payload must be a JSON object.");
            }

            if (payload.Count == 0)
            {
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.BadRequest, "Payload must contain at least one property.");
            }

            return null;
        }

        private CoapBusinessDispatchResult TryParseAlarm(
            CoapBusinessDispatchContext context,
            out CreateAlarmDto alarm)
        {
            try
            {
                alarm = CoapPayloadParser.ParseAlarm(context.Payload);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    "CoAP alarm payload is not valid JSON. TargetKind={TargetKind}, TargetName={TargetName}, Operation={Operation}.",
                    context.PlatformRoute.TargetKind,
                    context.PlatformRoute.TargetName,
                    context.PlatformRoute.Operation);
                alarm = null;
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.BadRequest, "Payload must be a JSON object.");
            }

            if (alarm == null || string.IsNullOrWhiteSpace(alarm.AlarmType))
            {
                return CoapBusinessDispatchResult.Fail(CoapBusinessDispatchStatus.BadRequest, "AlarmType is required.");
            }

            return null;
        }

        private static async ValueTask<CoapBusinessDispatchResult> PublishTelemetryAsync(
            IPublisher publisher,
            Device device,
            Dictionary<string, object> payload)
        {
            await publisher.PublishTelemetryData(new PlayloadData
            {
                DeviceId = device.Id,
                MsgBody = payload,
                DataSide = DataSide.ClientSide,
                DataCatalog = DataCatalog.TelemetryData
            });
            return CoapBusinessDispatchResult.Success();
        }

        private static async ValueTask<CoapBusinessDispatchResult> PublishAttributesAsync(
            IPublisher publisher,
            Device device,
            Dictionary<string, object> payload)
        {
            await publisher.PublishAttributeData(new PlayloadData
            {
                DeviceId = device.Id,
                MsgBody = payload,
                DataSide = DataSide.ClientSide,
                DataCatalog = DataCatalog.AttributeData
            });
            return CoapBusinessDispatchResult.Success();
        }

        private static async ValueTask<CoapBusinessDispatchResult> PublishAlarmAsync(
            IPublisher publisher,
            Device device,
            CreateAlarmDto alarm)
        {
            alarm.OriginatorName = string.IsNullOrWhiteSpace(alarm.OriginatorName) ? device.Name : alarm.OriginatorName;
            alarm.OriginatorType = device.DeviceType == DeviceType.Gateway ? OriginatorType.Gateway : OriginatorType.Device;
            await publisher.PublishDeviceAlarm(alarm);
            return CoapBusinessDispatchResult.Success();
        }
    }
}
