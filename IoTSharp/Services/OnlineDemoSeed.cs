#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using IoTSharp.Contracts;
using IoTSharp.Data;

namespace IoTSharp.Services;

internal static class OnlineDemoSeed
{
    private const string Synthetic = "[Demo] Synthetic cold-chain data";
    private const string Metadata = """{"synthetic":true,"scenario":"cold-chain"}""";
    internal const int DevicesPerCustomer = 4;

    /// <summary>按租户生成固定账号 ID，按重置批次生成可安全重试的设备 ID。</summary>
    internal static Guid StableId(Guid scope, string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"{scope:N}/{key}")).AsSpan(0, 16));

    internal static Guid DeviceId(Guid batch, int site, int device) => StableId(batch, $"device/{site}/{device}");

    internal static IEnumerable<PlayloadData> History(Guid batch, DateTime now)
    {
        for (var site = 0; site < 10; site++)
        for (var device = 1; device < DevicesPerCustomer; device++)
        for (var slot = 17; slot >= 0; slot--)
        {
            yield return new PlayloadData
            {
                DeviceId = DeviceId(batch, site, device), ts = now.AddMinutes(-slot * 10),
                DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData,
                MsgBody = new() { ["temperature"] = Temperature(site, device, slot), ["humidity"] = 62d + site + slot / 4d }
            };
        }
    }

    /// <summary>每个客户生成一套仓库、采集、告警和发布记录，直接持久化合成历史。</summary>
    internal static void AddSite(ApplicationDbContext db, Tenant tenant, OnlineDemoAccounts.Site site, Guid batch, int index, DateTime now)
    {
        var customer = site.Customer;
        var label = $"[Demo] {customer.Name}";
        var batchLabel = $"{now:MM-dd HH:mm} UTC";
        var gatewayProduct = new Product
        {
            Name = $"{label} Cold-chain gateway", Description = Synthetic, Tenant = tenant, Customer = customer,
            DefaultDeviceType = DeviceType.Gateway, DefaultIdentityType = IdentityType.X509Certificate
        };
        var sensorProduct = new Product
        {
            Name = $"{label} Temperature and humidity sensor", Description = Synthetic, Tenant = tenant, Customer = customer,
            DefaultDeviceType = DeviceType.Device, DefaultIdentityType = IdentityType.X509Certificate
        };
        var gateway = new Gateway
        {
            Id = DeviceId(batch, index, 0), Name = $"{label} Warehouse gateway", Tenant = tenant, Customer = customer,
            Product = gatewayProduct, DeviceType = DeviceType.Gateway, Timeout = 300
        };
        var asset = new Asset
        {
            Name = $"{label} Cold storage building", Description = Synthetic, AssetType = "Cold storage",
            Tenant = tenant, Customer = customer
        };
        db.AddRange(gateway, asset);
        AddDeviceState(db, gateway, now, online: true);
        for (var number = 1; number < DevicesPerCustomer; number++)
        {
            var device = new Device
            {
                Id = DeviceId(batch, index, number), Name = $"{label} Cold room {number} sensor",
                Tenant = tenant, Customer = customer, Product = sensorProduct, Owner = gateway, Timeout = 300
            };
            db.Add(device);
            AddDeviceState(db, device, now, online: number != 3);
            asset.OwnedAssets.Add(new AssetRelation
            {
                DeviceId = device.Id, Name = $"Room {number} temperature", Description = Synthetic,
                KeyName = "temperature", DataCatalog = DataCatalog.TelemetryLatest
            });
            db.AddRange(
                new TelemetryLatest { DeviceId = device.Id, KeyName = "temperature", Type = DataType.Double, DateTime = now,
                    DataSide = DataSide.ClientSide, Value_Double = Temperature(index, number, 0) },
                new TelemetryLatest { DeviceId = device.Id, KeyName = "humidity", Type = DataType.Double, DateTime = now,
                    DataSide = DataSide.ClientSide, Value_Double = 62d + index },
                new AttributeLatest { DeviceId = device.Id, KeyName = "room", Type = DataType.String, DateTime = now,
                    DataSide = DataSide.ServerSide, Value_String = $"Cold room {number}" },
                new Alarm
                {
                    Id = Guid.NewGuid(), Tenant = tenant, Customer = customer, OriginatorId = device.Id,
                    OriginatorType = OriginatorType.Device, AlarmType = $"[Demo] {(number == 3 ? "Sensor offline" : "Temperature drift")}",
                    AlarmDetail = Metadata, AlarmStatus = number == 1 ? AlarmStatus.Cleared_Act : AlarmStatus.Active_UnAck,
                    Serverity = number == 3 ? ServerityLevel.Major : ServerityLevel.Warning, StartDateTime = now.AddMinutes(-25),
                    EndDateTime = now.AddMinutes(-20), AckDateTime = number == 1 ? now.AddMinutes(-19) : default,
                    ClearDateTime = number == 1 ? now.AddMinutes(-16) : default
                });
        }
        sensorProduct.Dictionaries.AddRange([
            new ProductDictionary { KeyName = "temperature", DisplayName = "Temperature", Unit = "°C", Display = true },
            new ProductDictionary { KeyName = "humidity", DisplayName = "Humidity", Unit = "%RH", Display = true }]);

        var node = new EdgeNode
        {
            Gateway = gateway, GatewayId = gateway.Id, Name = $"{label} Cold-chain edge", Tenant = tenant, Customer = customer,
            RuntimeType = "DemoSimulator", RuntimeName = "Synthetic demonstration", Version = "1.4.2",
            InstanceId = $"demo-{batch:N}-{index}", Platform = "Synthetic", Status = "Online", Healthy = true,
            Capabilities = """{"protocols":["Modbus"],"synthetic":true}""", Metadata = Metadata,
            LastHeartbeatDateTime = now, LastRegistrationDateTime = now.AddHours(-5), CreatedAt = now.AddHours(-5), UpdatedAt = now
        };
        db.Add(node);
        AddCollection(db, tenant, customer, gatewayProduct, gateway, node, now);
        AddRelease(db, tenant, customer, gateway, node, now);

        var rule = new FlowRule
        {
            RuleId = Guid.NewGuid(), Tenant = tenant, Customer = customer, Name = $"{label} Cold-room temperature threshold",
            Describes = Synthetic, RuleDesc = "Draft example of a sustained temperature threshold", RuleStatus = 0,
            Version = 1, CreatTime = now, Creator = site.AdminUserId, CreateId = Guid.Parse(site.AdminUserId)
        };
        db.Add(rule);
        foreach (var userId in new[] { site.AdminUserId, site.NormalUserId })
            db.Add(new InboxMessage
            {
                TenantId = tenant.Id, CustomerId = customer.Id, UserId = userId, Title = "[Demo] Cold storage summary",
                Body = $"{Synthetic}. Sample batch {batchLabel}.", Category = "system", Sender = "Demo", CreatedAt = now
            });
    }

    private static void AddDeviceState(ApplicationDbContext db, Device device, DateTime now, bool online)
    {
        // 合成设备没有与此指纹对应的证书，也没有可用于接入的 token。
        db.Add(new DeviceIdentity
        {
            Device = device, DeviceId = device.Id, IdentityType = IdentityType.X509Certificate,
            IdentityId = $"synthetic-{device.Id:N}", IdentityValue = ""
        });
        db.AddRange(
            new AttributeLatest { DeviceId = device.Id, KeyName = Constants._Active, Type = DataType.Boolean, DateTime = now,
                DataSide = DataSide.ServerSide, Value_Boolean = online },
            new AttributeLatest { DeviceId = device.Id, KeyName = Constants._LastActivityDateTime, Type = DataType.DateTime, DateTime = now,
                DataSide = DataSide.ServerSide, Value_DateTime = online ? now : now.AddMinutes(-35) });
    }

    private static double Temperature(int site, int device, int slot) =>
        Math.Round(1.8 + site * 0.18 + device * 0.35 + Math.Sin((slot + site) / 3d) * 1.1, 2);

    private static void AddCollection(ApplicationDbContext db, Tenant tenant, Customer customer,
        Product product, Gateway gateway, EdgeNode node, DateTime now)
    {
        var template = new CollectionTemplate
        {
            Product = product, Tenant = tenant, Customer = customer, TemplateKey = "cold-chain",
            Name = "[Demo] Cold room Modbus collection", Description = Synthetic, Metadata = Metadata,
            Status = CollectionTemplateStatus.Active, CreatedAt = now, UpdatedAt = now,
            Protocol = new() { Protocol = CollectionProtocolType.Modbus, ProtocolKind = "ModbusTcp", Metadata = Metadata },
            Connections = [new() { ConnectionKey = "simulator", ConnectionName = "Cold-room simulator", Transport = "tcp",
                EndpointRef = "synthetic.invalid", Host = "synthetic.invalid", Port = 502, Metadata = Metadata }],
            Points = [new() { ConnectionKey = "simulator", PointKey = "temperature", Name = "Temperature", DisplayName = "Temperature",
                Address = "40001", Unit = "°C", Metadata = Metadata, SamplingPolicy = new() { ReadPeriodMs = 10000 },
                Mapping = new() { TargetName = "temperature", DisplayName = "Temperature", Unit = "°C" },
                Transforms = [new() { Order = 1, Parameters = """{"factor":0.1}""" }] }]
        };
        var payload = """{"synthetic":true,"tasks":[]}""";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        var version = new CollectionConfigurationVersion
        {
            Tenant = tenant, Customer = customer, Gateway = gateway, GatewayId = gateway.Id, EdgeNodeId = node.Id, Version = 1,
            ConfigurationHash = hash, SourceId = template.Id.ToString(), SourceType = "CollectionTemplate", SourceVersion = "1",
            Payload = payload, SourceMetadata = Metadata, CreatedAt = now, UpdatedAt = now
        };
        db.AddRange(template, version, new EdgeCollectionAssignment
        {
            Tenant = tenant, Customer = customer, Gateway = gateway, GatewayId = gateway.Id, EdgeNodeId = node.Id,
            RuntimeType = node.RuntimeType, InstanceId = node.InstanceId, TargetKey = node.InstanceId,
            CollectionConfigurationVersion = version, ConfigurationVersion = 1, ConfigurationHash = hash,
            AppliedConfigurationVersion = 1, AppliedConfigurationHash = hash, AppliedAt = now.AddMinutes(-10),
            Metadata = Metadata, AssignedAt = now.AddMinutes(-15), LastPulledAt = now.AddMinutes(-10), CreatedAt = now, UpdatedAt = now
        });
    }

    private static void AddRelease(ApplicationDbContext db, Tenant tenant, Customer customer,
        Gateway gateway, EdgeNode node, DateTime now)
    {
        var package = new ReleasePackage
        {
            Tenant = tenant, Customer = customer, Name = "[Demo] Cold-chain edge software", PackageKey = "cold-chain-edge",
            Version = "1.4.2", TargetRuntimeType = node.RuntimeType, FileName = "synthetic-release.json",
            ContentType = "application/json", Metadata = Metadata, CreatedAt = now.AddHours(-2), UpdatedAt = now
        };
        var edgeTask = new EdgeTask
        {
            Tenant = tenant, Customer = customer, Gateway = gateway, GatewayId = gateway.Id, EdgeNodeId = node.Id,
            TaskType = EdgeTaskType.SoftwareUpdate, TargetType = EdgeTaskTargetType.EdgeNode, TargetKey = node.InstanceId,
            RuntimeType = node.RuntimeType, InstanceId = node.InstanceId, Status = EdgeTaskStatus.Succeeded,
            Progress = 100, Metadata = Metadata, CreatedAt = now.AddMinutes(-20), CompletedAt = now.AddMinutes(-18), UpdatedAt = now
        };
        var plan = new ReleasePlan
        {
            Tenant = tenant, Customer = customer, Name = "[Demo] Canary rollout", Description = Synthetic, Package = package,
            Status = ReleasePlanStatus.Succeeded, BatchSize = 1, CurrentBatchNo = 1, TotalTaskCount = 1, SucceededTaskCount = 1,
            StartedAt = now.AddMinutes(-20), CompletedAt = now.AddMinutes(-18), Metadata = Metadata,
            CreatedAt = now.AddMinutes(-25), UpdatedAt = now
        };
        var task = new ReleaseTask
        {
            Tenant = tenant, Customer = customer, Plan = plan, Package = package, EdgeTask = edgeTask,
            TargetType = ReleaseTargetType.EdgeNode, TargetId = node.Id, TargetKey = node.InstanceId,
            GatewayId = gateway.Id, EdgeNodeId = node.Id, Status = ReleaseTaskStatus.Succeeded, RuntimeType = node.RuntimeType,
            InstanceId = node.InstanceId, BatchNo = 1, Progress = 100, Metadata = Metadata, CreatedAt = now.AddMinutes(-20),
            CompletedAt = now.AddMinutes(-18), UpdatedAt = now
        };
        db.AddRange(new EdgeTaskReceipt
        {
            Tenant = tenant, Customer = customer, Gateway = gateway, GatewayId = gateway.Id, EdgeNodeId = node.Id,
            Task = edgeTask, TargetType = EdgeTaskTargetType.EdgeNode, TargetKey = node.InstanceId, RuntimeType = node.RuntimeType,
            InstanceId = node.InstanceId, Status = EdgeTaskStatus.Succeeded, Message = Synthetic,
            Metadata = Metadata, Progress = 100, ReportedAt = now.AddMinutes(-18), ReceivedAt = now.AddMinutes(-18)
        }, new ReleaseReceipt
        {
            Tenant = tenant, Customer = customer, Plan = plan, ReleaseTask = task, EdgeTaskId = edgeTask.Id,
            TargetType = task.TargetType, TargetId = task.TargetId, TargetKey = task.TargetKey, GatewayId = gateway.Id,
            EdgeNodeId = node.Id, RuntimeType = node.RuntimeType, InstanceId = node.InstanceId, Status = ReleaseTaskStatus.Succeeded,
            Progress = 100, Message = Synthetic, Metadata = Metadata, ReportedAt = now.AddMinutes(-18), ReceivedAt = now.AddMinutes(-18)
        });
    }
}
