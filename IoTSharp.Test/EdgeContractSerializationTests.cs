#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using IoTSharp.Contracts;
using Xunit;

namespace IoTSharp.Test;

public sealed class EdgeContractSerializationTests
{
    [Fact]
    public void EdgeRuntimeDtos_ArePublishedFromContractsAssembly()
    {
        Assert.Equal("IoTSharp.Contracts", typeof(EdgeRegistrationDto).Namespace);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeTaskRequestDto).Assembly);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeTaskDto).Assembly);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeTaskReceiptDto).Assembly);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeRuntimeStatusDto).Assembly);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeCapabilityDto).Assembly);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(EdgeCollectionAssignmentDto).Assembly);
        Assert.Equal(EdgeNodeContractVersions.EdgeRuntimeV1, new EdgeRegistrationDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.EdgeNodeV1, new EdgeNodeDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.EdgeRuntimeStatusV1, new EdgeRuntimeStatusDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.EdgeCapabilityV1, new EdgeCapabilityDto().ContractVersion);
        Assert.Equal("collection-config-v1", EdgeNodeContractVersions.CollectionConfigV1);
        Assert.Equal(EdgeNodeContractVersions.CollectionConfigV1, new EdgeCollectionConfigurationDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.CollectionConfigV1, new EdgeCollectionConfigurationPullResultDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.CollectionConfigV1, new CollectionConfigurationVersionDto().ContractVersion);
        Assert.Equal(EdgeNodeContractVersions.CollectionConfigV1, new EdgeCollectionAssignmentDto().ContractVersion);
        Assert.Equal("release-package-v1", EdgeNodeContractVersions.ReleasePackageV1);
        Assert.Equal(EdgeNodeContractVersions.ReleasePackageV1, new ReleasePackageDto().ContractVersion);
        Assert.Equal(typeof(EdgeRegistrationDto).Assembly, typeof(ReleasePlanDto).Assembly);
    }

    [Fact]
    public void EdgeContractSchemas_ExistAndAreParseable()
    {
        var contractsPath = Path.Combine(FindRepositoryRoot(), "IoTSharp.Contracts");
        var files = new[]
        {
            "edge-node.v1.schema.json",
            "collection-config.v1.schema.json",
            "edge-task.v1.schema.json",
            "release-package.v1.schema.json",
            Path.Combine("examples", "edge-node.v1.sample.json"),
            Path.Combine("examples", "collection-config.v1.sample.json"),
            Path.Combine("examples", "edge-task.v1.sample.json"),
            Path.Combine("examples", "edge-task.software-update.v1.sample.json"),
            Path.Combine("examples", "edge-task.device-script-ota.v1.sample.json"),
            Path.Combine("examples", "edge-task.firmware-ota.v1.sample.json"),
            Path.Combine("examples", "edge-task.firmware-ota.receipt.v1.sample.json"),
            Path.Combine("examples", "edge-task.firmware-ota.rollback-receipt.v1.sample.json"),
            Path.Combine("examples", "release-package.v1.sample.json"),
            Path.Combine("examples", "release-package.device-script.v1.sample.json"),
            Path.Combine("examples", "release-package.firmware.v1.sample.json")
        };

        foreach (var file in files)
        {
            var path = Path.Combine(contractsPath, file);
            Assert.True(File.Exists(path), $"Missing contract artifact: {file}");
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        }

        var collectionSample = File.ReadAllText(Path.Combine(contractsPath, "examples", "collection-config.v1.sample.json"));
        Assert.Contains("\"contractVersion\": \"collection-config-v1\"", collectionSample);
        var firmwareReceiptSample = File.ReadAllText(Path.Combine(contractsPath, "examples", "edge-task.firmware-ota.receipt.v1.sample.json"));
        Assert.Contains("\"bootloaderAccepted\": true", firmwareReceiptSample);
        var firmwareRollbackReceiptSample = File.ReadAllText(Path.Combine(contractsPath, "examples", "edge-task.firmware-ota.rollback-receipt.v1.sample.json"));
        Assert.Contains("\"rollbackConfirmed\": true", firmwareRollbackReceiptSample);
    }

    [Fact]
    public void EdgeRuntimeStatusDto_SerializesStructuredMetrics()
    {
        var dto = new EdgeRuntimeStatusDto
        {
            EdgeNodeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            GatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            RuntimeType = EdgeRuntimeTypes.Gateway,
            RuntimeName = "gateway-a",
            Version = "1.0.0",
            InstanceId = "gateway-a-01",
            HostName = "host-a",
            Status = EdgeNodeStatusNames.Running,
            Healthy = true,
            UptimeSeconds = 123,
            Metadata = new Dictionary<string, object> { ["site"] = "plant-a" },
            Metrics = new Dictionary<string, object> { ["cpu"] = 0.25, ["memoryMb"] = 128 }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<EdgeRuntimeStatusDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"edge-runtime-status-v1\"", json);
        Assert.NotNull(roundtrip);
        Assert.Equal(EdgeNodeStatusNames.Running, roundtrip!.Status);
        Assert.True(roundtrip.Healthy);
        Assert.True(roundtrip.Metadata.ContainsKey("site"));
        Assert.True(roundtrip.Metrics.ContainsKey("cpu"));
    }

    [Fact]
    public void EdgeCapabilityDto_SerializesStructuredTasksAndCompatibility()
    {
        var dto = new EdgeCapabilityDto
        {
            EdgeNodeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            GatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            RuntimeType = EdgeRuntimeTypes.Gateway,
            RuntimeName = "gateway-a",
            Version = "1.0.0",
            InstanceId = "gateway-a-01",
            Protocols = ["modbus-tcp"],
            SupportedProtocols = [CollectionProtocolType.Modbus],
            SupportedPointTypes = ["holding-register"],
            SupportedTransforms = [CollectionTransformType.Scale, CollectionTransformType.Offset],
            SupportedReportTriggers = [ReportTriggerType.OnChange],
            Features = ["collection"],
            Tasks = [nameof(EdgeTaskType.HealthProbe)],
            TaskCapabilities =
            [
                new EdgeTaskCapabilityDto
                {
                    TaskType = nameof(EdgeTaskType.HealthProbe),
                    ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                    SupportsProgress = false
                }
            ],
            CompatibleContracts =
            [
                new EdgeContractCompatibilityDto
                {
                    ContractName = "edge-task",
                    ContractVersion = EdgeNodeContractVersions.EdgeTaskV1
                }
            ],
            Metadata = new Dictionary<string, object> { ["driver"] = "test" }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<EdgeCapabilityDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"edge-capability-v1\"", json);
        Assert.Contains("\"supportedTransforms\":[\"Scale\",\"Offset\"]", json);
        Assert.NotNull(roundtrip);
        Assert.Contains(CollectionProtocolType.Modbus, roundtrip!.SupportedProtocols);
        Assert.Contains(roundtrip.TaskCapabilities, task => task.TaskType == nameof(EdgeTaskType.HealthProbe));
        Assert.Contains(roundtrip.CompatibleContracts, contract => contract.ContractVersion == EdgeNodeContractVersions.EdgeTaskV1);
    }

    [Fact]
    public void EdgeCollectionConfigurationDto_SerializesProductTemplateSource()
    {
        var dto = new EdgeCollectionConfigurationDto
        {
            EdgeNodeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Version = 3,
            UpdatedAt = DateTime.Parse("2026-07-05T08:10:00Z").ToUniversalTime(),
            UpdatedBy = "operator@example.com",
            SourceType = "ProductCollectionTemplate",
            SourceId = "11111111-1111-1111-1111-111111111111",
            SourceVersion = "3",
            SourceMetadata = new Dictionary<string, object>
            {
                ["productId"] = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                ["templateKey"] = "boiler-modbus-template"
            },
            Tasks = []
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<EdgeCollectionConfigurationDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"collection-config-v1\"", json);
        Assert.Contains("\"sourceType\":\"ProductCollectionTemplate\"", json);
        Assert.NotNull(roundtrip);
        Assert.Equal("ProductCollectionTemplate", roundtrip!.SourceType);
        Assert.Equal("11111111-1111-1111-1111-111111111111", roundtrip.SourceId);
        Assert.True(roundtrip.SourceMetadata.ContainsKey("templateKey"));
    }

    [Fact]
    public void CollectionConfigurationVersionDto_SerializesSnapshotAndConfiguration()
    {
        var versionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var gatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var dto = new CollectionConfigurationVersionDto
        {
            Id = versionId,
            GatewayId = gatewayId,
            EdgeNodeId = gatewayId,
            Version = 4,
            ConfigurationHash = "B90F05603F3343B2A0FAF1E1D3D48C85",
            TaskCount = 1,
            SourceType = "ProductCollectionTemplate",
            SourceId = "11111111-1111-1111-1111-111111111111",
            SourceVersion = "7",
            SourceMetadata = new Dictionary<string, object> { ["templateKey"] = "boiler-modbus-template" },
            CreatedAt = DateTime.Parse("2026-07-06T00:00:00Z").ToUniversalTime(),
            UpdatedAt = DateTime.Parse("2026-07-06T00:01:00Z").ToUniversalTime(),
            CreatedBy = "operator@example.com",
            UpdatedBy = "operator@example.com",
            Configuration = new EdgeCollectionConfigurationDto
            {
                EdgeNodeId = gatewayId,
                Version = 4,
                SourceType = "ProductCollectionTemplate",
                SourceId = "11111111-1111-1111-1111-111111111111"
            }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<CollectionConfigurationVersionDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"collection-config-v1\"", json);
        Assert.Contains("\"configurationHash\":\"B90F05603F3343B2A0FAF1E1D3D48C85\"", json);
        Assert.Contains("\"configuration\":", json);
        Assert.NotNull(roundtrip);
        Assert.Equal(versionId, roundtrip!.Id);
        Assert.Equal("ProductCollectionTemplate", roundtrip.Configuration!.SourceType);
    }

    [Fact]
    public void CollectionTemplateConfigurationPublishResultDto_SerializesReleaseAnchors()
    {
        var versionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var assignmentId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var taskId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var gatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var dto = new CollectionTemplateConfigurationPublishResultDto
        {
            ConfigurationVersion = new CollectionConfigurationVersionDto
            {
                Id = versionId,
                GatewayId = gatewayId,
                EdgeNodeId = gatewayId,
                Version = 8,
                ConfigurationHash = "B90F05603F3343B2A0FAF1E1D3D48C85",
                SourceType = "ProductCollectionTemplate",
                SourceId = "11111111-1111-1111-1111-111111111111"
            },
            Assignment = new EdgeCollectionAssignmentDto
            {
                Id = assignmentId,
                CollectionConfigurationVersionId = versionId,
                GatewayId = gatewayId,
                EdgeNodeId = gatewayId,
                TargetType = EdgeTaskTargetType.GatewayRuntime,
                TargetKey = $"{gatewayId}:gateway",
                RuntimeType = EdgeRuntimeTypes.Gateway,
                ConfigurationVersion = 8,
                ConfigurationHash = "B90F05603F3343B2A0FAF1E1D3D48C85",
                Status = EdgeCollectionAssignmentStatus.Active
            },
            Task = new EdgeTaskRequestDto
            {
                TaskId = taskId,
                TaskType = EdgeTaskType.ConfigPullRequest,
                CreatedAt = DateTime.Parse("2026-07-06T00:02:00Z").ToUniversalTime(),
                Address = new EdgeTaskAddressDto
                {
                    TargetType = EdgeTaskTargetType.GatewayRuntime,
                    DeviceId = gatewayId,
                    RuntimeType = EdgeRuntimeTypes.Gateway,
                    TargetKey = $"{gatewayId}:gateway"
                },
                Parameters = new Dictionary<string, object>
                {
                    ["configurationVersionId"] = versionId,
                    ["configurationVersion"] = 8,
                    ["configurationHash"] = "B90F05603F3343B2A0FAF1E1D3D48C85"
                }
            }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<CollectionTemplateConfigurationPublishResultDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"taskType\":\"ConfigPullRequest\"", json);
        Assert.Contains("\"collectionConfigurationVersionId\":\"33333333-3333-3333-3333-333333333333\"", json);
        Assert.Contains("\"configurationHash\":\"B90F05603F3343B2A0FAF1E1D3D48C85\"", json);
        Assert.NotNull(roundtrip);
        Assert.Equal(versionId, roundtrip!.Assignment.CollectionConfigurationVersionId);
        Assert.Equal(EdgeTaskType.ConfigPullRequest, roundtrip.Task.TaskType);
    }

    [Fact]
    public void EdgeCollectionConfigurationPullResultDto_SerializesTargetConfigurationEnvelope()
    {
        var versionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var assignmentId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var gatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var dto = new EdgeCollectionConfigurationPullResultDto
        {
            GatewayId = gatewayId,
            EdgeNodeId = gatewayId,
            ConfigurationVersionId = versionId,
            ConfigurationVersion = 8,
            ConfigurationHash = "B90F05603F3343B2A0FAF1E1D3D48C85",
            PulledAt = DateTime.Parse("2026-07-06T00:03:00Z").ToUniversalTime(),
            Assignment = new EdgeCollectionAssignmentDto
            {
                Id = assignmentId,
                CollectionConfigurationVersionId = versionId,
                GatewayId = gatewayId,
                EdgeNodeId = gatewayId,
                TargetType = EdgeTaskTargetType.GatewayRuntime,
                TargetKey = $"{gatewayId}:gateway",
                RuntimeType = EdgeRuntimeTypes.Gateway,
                ConfigurationVersion = 8,
                ConfigurationHash = "B90F05603F3343B2A0FAF1E1D3D48C85",
                Status = EdgeCollectionAssignmentStatus.Active
            },
            Configuration = new EdgeCollectionConfigurationDto
            {
                EdgeNodeId = gatewayId,
                Version = 8,
                SourceType = "ProductCollectionTemplate",
                SourceId = "11111111-1111-1111-1111-111111111111"
            }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<EdgeCollectionConfigurationPullResultDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"configurationVersionId\":\"33333333-3333-3333-3333-333333333333\"", json);
        Assert.Contains("\"configurationHash\":\"B90F05603F3343B2A0FAF1E1D3D48C85\"", json);
        Assert.Contains("\"assignment\":", json);
        Assert.Contains("\"configuration\":", json);
        Assert.NotNull(roundtrip);
        Assert.Equal(versionId, roundtrip!.ConfigurationVersionId);
        Assert.Equal(assignmentId, roundtrip.Assignment!.Id);
        Assert.Equal("ProductCollectionTemplate", roundtrip.Configuration.SourceType);
    }

    [Fact]
    public void EdgeTaskDto_SerializesEnumsAsStableStrings()
    {
        var dto = new EdgeTaskRequestDto
        {
            TaskId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TaskType = EdgeTaskType.HealthProbe,
            CreatedAt = DateTime.Parse("2026-07-05T00:00:00Z").ToUniversalTime(),
            Address = new EdgeTaskAddressDto
            {
                TargetType = EdgeTaskTargetType.EdgeNode,
                DeviceId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                RuntimeType = EdgeRuntimeTypes.Gateway,
                TargetKey = "22222222-2222-2222-2222-222222222222:gateway"
            }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"edge-task-v1\"", json);
        Assert.Contains("\"taskType\":\"HealthProbe\"", json);
        Assert.Contains("\"targetType\":\"EdgeNode\"", json);
    }

    [Fact]
    public void ReleasePackageDto_SerializesSoftwarePackageMetadata()
    {
        var packageId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var dto = new ReleasePackageDto
        {
            Id = packageId,
            PackageType = ReleasePackageType.Software,
            PackageKey = "iotedge-gateway",
            Name = "IoTEdge Gateway Runtime",
            Version = "1.4.0",
            TargetRuntimeType = EdgeRuntimeTypes.Gateway,
            TargetRuntimeVersion = ">=1.3.0",
            FileName = "iotedge-gateway-1.4.0-linux-x64.zip",
            ContentType = "application/zip",
            Size = 1024,
            Sha256 = "6D37795021E544D998C3A77A825EA542D52094C9AB63F6D0D42C79FE2E176FF8",
            Metadata = new Dictionary<string, object> { ["channel"] = "stable" },
            CreatedAt = DateTime.Parse("2026-07-06T08:12:00Z").ToUniversalTime(),
            UpdatedAt = DateTime.Parse("2026-07-06T08:12:00Z").ToUniversalTime(),
            CreatedBy = "operator@example.com",
            UpdatedBy = "operator@example.com"
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<ReleasePackageDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"release-package-v1\"", json);
        Assert.Contains("\"packageType\":\"Software\"", json);
        Assert.Contains("\"targetRuntimeType\":\"gateway\"", json);
        Assert.Contains("\"sha256\":\"6D37795021E544D998C3A77A825EA542D52094C9AB63F6D0D42C79FE2E176FF8\"", json);
        Assert.NotNull(roundtrip);
        Assert.Equal(packageId, roundtrip!.Id);
        Assert.Equal(ReleasePackageType.Software, roundtrip.PackageType);
        Assert.Equal("1.4.0", roundtrip.Version);
    }

    [Fact]
    public void SoftwareUpdateEdgeTask_SerializesReleasePackageParameters()
    {
        var packageId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var gatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var dto = new EdgeTaskRequestDto
        {
            TaskId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            TaskType = EdgeTaskType.SoftwareUpdate,
            CreatedAt = DateTime.Parse("2026-07-06T08:15:00Z").ToUniversalTime(),
            Address = new EdgeTaskAddressDto
            {
                TargetType = EdgeTaskTargetType.GatewayRuntime,
                DeviceId = gatewayId,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                InstanceId = "gateway-a-01",
                TargetKey = $"{gatewayId}:gateway:gateway-a-01"
            },
            Parameters = new Dictionary<string, object>
            {
                ["releasePackageContractVersion"] = EdgeNodeContractVersions.ReleasePackageV1,
                ["packageId"] = packageId,
                ["packageVersion"] = "1.4.0",
                ["sha256"] = "6D37795021E544D998C3A77A825EA542D52094C9AB63F6D0D42C79FE2E176FF8",
                ["downloadUrl"] = "https://iotsharp.example.com/api/ReleasePackages/33333333-3333-3333-3333-333333333333/Download"
            }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"taskType\":\"SoftwareUpdate\"", json);
        Assert.Contains("\"releasePackageContractVersion\":\"release-package-v1\"", json);
        Assert.Contains("\"packageId\":\"33333333-3333-3333-3333-333333333333\"", json);
        Assert.Contains("\"sha256\":\"6D37795021E544D998C3A77A825EA542D52094C9AB63F6D0D42C79FE2E176FF8\"", json);
    }

    [Fact]
    public void DeviceOtaEdgeTasks_SerializeScriptAndFirmwareContracts()
    {
        var packageId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var deviceId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var scriptTask = new EdgeTaskRequestDto
        {
            TaskId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            TaskType = EdgeTaskType.DeviceScriptOta,
            CreatedAt = DateTime.Parse("2026-07-06T08:30:00Z").ToUniversalTime(),
            Address = new EdgeTaskAddressDto
            {
                TargetType = EdgeTaskTargetType.Device,
                DeviceId = deviceId,
                RuntimeType = "iotembedded",
                InstanceId = "stm32-basic-01",
                TargetKey = $"{deviceId}:device:{deviceId}:iotembedded:stm32-basic-01"
            },
            Parameters = new Dictionary<string, object>
            {
                ["releasePackageContractVersion"] = EdgeNodeContractVersions.ReleasePackageV1,
                ["packageId"] = packageId,
                ["packageType"] = ReleasePackageType.DeviceScript.ToString(),
                ["packageVersion"] = "2026.7.6",
                ["sha256"] = "6D37795021E544D998C3A77A825EA542D52094C9AB63F6D0D42C79FE2E176FF8",
                ["targetDeviceId"] = deviceId,
                ["scriptCrc32"] = "A1B2C3D4",
                ["scriptSlot"] = "inactive"
            }
        };
        var firmwareTask = new EdgeTaskRequestDto
        {
            TaskId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            TaskType = EdgeTaskType.FirmwareOta,
            CreatedAt = DateTime.Parse("2026-07-06T08:35:00Z").ToUniversalTime(),
            Address = scriptTask.Address,
            Parameters = new Dictionary<string, object>
            {
                ["releasePackageContractVersion"] = EdgeNodeContractVersions.ReleasePackageV1,
                ["packageId"] = packageId,
                ["packageType"] = ReleasePackageType.Firmware.ToString(),
                ["packageVersion"] = "1.2.0",
                ["sha256"] = "6D37795021E544D998C3A77A825EA542D52094C9AB63F6D0D42C79FE2E176FF8",
                ["targetDeviceId"] = deviceId,
                ["firmwarePartition"] = "ota-b",
                ["bootloaderVersion"] = ">=1.0.0"
            }
        };

        var scriptJson = JsonSerializer.Serialize(scriptTask, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var firmwareJson = JsonSerializer.Serialize(firmwareTask, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"taskType\":\"DeviceScriptOta\"", scriptJson);
        Assert.Contains("\"targetType\":\"Device\"", scriptJson);
        Assert.Contains("\"scriptCrc32\":\"A1B2C3D4\"", scriptJson);
        Assert.Contains("\"taskType\":\"FirmwareOta\"", firmwareJson);
        Assert.Contains("\"firmwarePartition\":\"ota-b\"", firmwareJson);
    }

    [Fact]
    public void ReleaseCenterDtos_SerializeStablePlanTaskAndReceiptContracts()
    {
        var planId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var releaseTaskId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var edgeTaskId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var packageId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var gatewayId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var result = new ReleasePlanOperationResultDto
        {
            Plan = new ReleasePlanDto
            {
                Id = planId,
                Name = "Gateway runtime rollout",
                PlanType = ReleasePlanType.SoftwareUpdate,
                Status = ReleasePlanStatus.Running,
                PackageId = packageId,
                ConfirmationPolicy = ReleaseConfirmationPolicy.ManualBetweenBatches,
                BatchSize = 1,
                TotalTaskCount = 1,
                RunningTaskCount = 1,
                CurrentBatchNo = 1,
                Tasks =
                [
                    new ReleaseTaskDto
                    {
                        Id = releaseTaskId,
                        PlanId = planId,
                        PackageId = packageId,
                        TargetType = ReleaseTargetType.Gateway,
                        TargetId = gatewayId,
                        GatewayId = gatewayId,
                        TargetKey = $"{gatewayId}:gateway:gateway-a-01",
                        RuntimeType = EdgeRuntimeTypes.Gateway,
                        InstanceId = "gateway-a-01",
                        BatchNo = 1,
                        Status = ReleaseTaskStatus.Sent,
                        EdgeTaskId = edgeTaskId
                    }
                ]
            },
            EdgeTasks =
            [
                new EdgeTaskRequestDto
                {
                    TaskId = edgeTaskId,
                    TaskType = EdgeTaskType.SoftwareUpdate,
                    CreatedAt = DateTime.Parse("2026-07-06T08:15:00Z").ToUniversalTime(),
                    Address = new EdgeTaskAddressDto
                    {
                        TargetType = EdgeTaskTargetType.GatewayRuntime,
                        DeviceId = gatewayId,
                        RuntimeType = EdgeRuntimeTypes.Gateway,
                        InstanceId = "gateway-a-01",
                        TargetKey = $"{gatewayId}:gateway:gateway-a-01"
                    },
                    Parameters = new Dictionary<string, object>
                    {
                        ["releasePlanId"] = planId,
                        ["releaseTaskId"] = releaseTaskId,
                        ["packageId"] = packageId
                    }
                }
            ]
        };
        var receipt = new ReleaseReceiptDto
        {
            PlanId = planId,
            ReleaseTaskId = releaseTaskId,
            EdgeTaskId = edgeTaskId,
            Status = ReleaseTaskStatus.Succeeded,
            Progress = 100,
            Result = new Dictionary<string, object>
            {
                ["packageId"] = packageId,
                ["sha256"] = "6D37795021E544D998C3A77A825EA542D52094C9AB63F6D0D42C79FE2E176FF8"
            }
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var receiptJson = JsonSerializer.Serialize(receipt, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<ReleasePlanOperationResultDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"planType\":\"SoftwareUpdate\"", json);
        Assert.Contains("\"status\":\"Running\"", json);
        Assert.Contains("\"confirmationPolicy\":\"ManualBetweenBatches\"", json);
        Assert.Contains("\"targetType\":\"Gateway\"", json);
        Assert.Contains("\"taskType\":\"SoftwareUpdate\"", json);
        Assert.Contains("\"status\":\"Succeeded\"", receiptJson);
        Assert.NotNull(roundtrip);
        Assert.Equal(ReleasePlanStatus.Running, roundtrip!.Plan.Status);
        Assert.Equal(ReleaseTaskStatus.Sent, roundtrip.Plan.Tasks[0].Status);
        Assert.Equal(ReleaseTargetType.Gateway, roundtrip.Plan.Tasks[0].TargetType);
        Assert.Equal(EdgeTaskType.SoftwareUpdate, roundtrip.EdgeTasks[0].TaskType);
    }

    [Fact]
    public void EdgeTaskStateDto_SerializesCurrentStatusAsStableString()
    {
        var dto = new EdgeTaskDto
        {
            TaskId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TaskType = EdgeTaskType.HealthProbe,
            Status = EdgeTaskStatus.Running,
            Progress = 25,
            CreatedAt = DateTime.Parse("2026-07-05T00:00:00Z").ToUniversalTime(),
            Address = new EdgeTaskAddressDto
            {
                TargetType = EdgeTaskTargetType.EdgeNode,
                DeviceId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                RuntimeType = EdgeRuntimeTypes.Gateway,
                TargetKey = "22222222-2222-2222-2222-222222222222:gateway"
            }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"edge-task-v1\"", json);
        Assert.Contains("\"taskType\":\"HealthProbe\"", json);
        Assert.Contains("\"status\":\"Running\"", json);
        Assert.Contains("\"targetType\":\"EdgeNode\"", json);
    }

    [Fact]
    public void EdgeTaskReceiptDto_SerializesStatusAndResultAsStableContract()
    {
        var dto = new EdgeTaskReceiptDto
        {
            TaskId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TargetType = EdgeTaskTargetType.EdgeNode,
            TargetKey = "22222222-2222-2222-2222-222222222222:gateway",
            RuntimeType = EdgeRuntimeTypes.Gateway,
            InstanceId = "gateway-a-01",
            Status = EdgeTaskStatus.Running,
            Progress = 50,
            Message = "running health probe",
            ReportedAt = DateTime.Parse("2026-07-05T00:01:00Z").ToUniversalTime(),
            Result = new Dictionary<string, object> { ["step"] = "probe" },
            Metadata = new Dictionary<string, string> { ["worker"] = "edge-task" }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<EdgeTaskReceiptDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"edge-task-v1\"", json);
        Assert.Contains("\"targetType\":\"EdgeNode\"", json);
        Assert.Contains("\"status\":\"Running\"", json);
        Assert.Contains("\"progress\":50", json);
        Assert.NotNull(roundtrip);
        Assert.Equal(EdgeTaskStatus.Running, roundtrip!.Status);
        Assert.Equal("gateway-a-01", roundtrip.InstanceId);
        Assert.True(roundtrip.Result.ContainsKey("step"));
        Assert.Equal("edge-task", roundtrip.Metadata["worker"]);
    }

    [Fact]
    public void EdgeCollectionAssignmentDto_SerializesTargetAndStatusAsStableStrings()
    {
        var dto = new EdgeCollectionAssignmentDto
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            TargetType = EdgeTaskTargetType.EdgeNode,
            GatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            EdgeNodeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            TargetKey = "22222222-2222-2222-2222-222222222222:gateway",
            RuntimeType = EdgeRuntimeTypes.Gateway,
            ConfigurationVersion = 3,
            ConfigurationHash = "B90F05603F3343B2A0FAF1E1D3D48C85",
            TaskCount = 2,
            Status = EdgeCollectionAssignmentStatus.Active,
            SourceType = "InlineCollectionConfig",
            SourceVersion = "3",
            AssignedAt = DateTime.Parse("2026-07-05T00:02:00Z").ToUniversalTime(),
            LastExecutionTaskId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            LastExecutionStatus = EdgeTaskStatus.Succeeded,
            LastExecutionMessage = "applied",
            LastExecutionProgress = 100,
            LastExecutionAt = DateTime.Parse("2026-07-05T00:03:00Z").ToUniversalTime(),
            AppliedConfigurationVersion = 3,
            AppliedConfigurationHash = "B90F05603F3343B2A0FAF1E1D3D48C85",
            AppliedAt = DateTime.Parse("2026-07-05T00:04:00Z").ToUniversalTime(),
            Metadata = new Dictionary<string, object> { ["source"] = "unit-test" }
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<EdgeCollectionAssignmentDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"collection-config-v1\"", json);
        Assert.Contains("\"targetType\":\"EdgeNode\"", json);
        Assert.Contains("\"status\":\"Active\"", json);
        Assert.Contains("\"lastExecutionStatus\":\"Succeeded\"", json);
        Assert.Contains("\"appliedConfigurationVersion\":3", json);
        Assert.NotNull(roundtrip);
        Assert.Equal(EdgeCollectionAssignmentStatus.Active, roundtrip!.Status);
        Assert.Equal(EdgeTaskTargetType.EdgeNode, roundtrip.TargetType);
        Assert.Equal(EdgeTaskStatus.Succeeded, roundtrip.LastExecutionStatus);
        Assert.Equal(3, roundtrip.AppliedConfigurationVersion);
        Assert.True(roundtrip.Metadata.ContainsKey("source"));
    }

    [Fact]
    public void EdgeCollectionVersionStatusDto_SerializesCurrentAndTargetVersionState()
    {
        var dto = new EdgeCollectionVersionStatusDto
        {
            GatewayId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            EdgeNodeId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            AssignmentId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            TargetConfigurationVersionId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            TargetConfigurationVersion = 4,
            TargetConfigurationHash = "TARGET-HASH",
            TargetTaskCount = 2,
            TargetSourceType = "ProductCollectionTemplate",
            TargetSourceId = "template-01",
            TargetSourceVersion = "7",
            TargetAssignedAt = DateTime.Parse("2026-07-05T00:02:00Z").ToUniversalTime(),
            LastPulledAt = DateTime.Parse("2026-07-05T00:03:00Z").ToUniversalTime(),
            CurrentConfigurationVersion = 3,
            CurrentConfigurationHash = "CURRENT-HASH",
            CurrentAppliedAt = DateTime.Parse("2026-07-05T00:01:00Z").ToUniversalTime(),
            IsTargetApplied = false,
            HasDifference = true,
            VersionDelta = 1,
            DifferenceSummary = "当前版本落后目标版本 1 个版本",
            LastPublishTaskId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            LastPublishStatus = EdgeTaskStatus.Running,
            LastPublishMessage = "applying",
            LastPublishProgress = 50,
            LastPublishAt = DateTime.Parse("2026-07-05T00:04:00Z").ToUniversalTime()
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var roundtrip = JsonSerializer.Deserialize<EdgeCollectionVersionStatusDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"contractVersion\":\"collection-config-v1\"", json);
        Assert.Contains("\"targetConfigurationVersion\":4", json);
        Assert.Contains("\"currentConfigurationVersion\":3", json);
        Assert.Contains("\"lastPublishStatus\":\"Running\"", json);
        Assert.NotNull(roundtrip);
        Assert.True(roundtrip!.HasDifference);
        Assert.False(roundtrip.IsTargetApplied);
        Assert.Equal(1, roundtrip.VersionDelta);
        Assert.Equal(EdgeTaskStatus.Running, roundtrip.LastPublishStatus);
    }

    [Fact]
    public void EdgeHeartbeatDto_ToleratesFutureFields()
    {
        const string json = """
        {
          "contractVersion": "edge-v1",
          "status": "Running",
          "healthy": true,
          "futureField": "ignored"
        }
        """;

        var dto = JsonSerializer.Deserialize<EdgeHeartbeatDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(dto);
        Assert.Equal(EdgeNodeStatusNames.Running, dto!.Status);
        Assert.True(dto.Healthy);
    }

    [Fact]
    public void EdgeTaskStateMachine_RejectsInvalidRollback()
    {
        Assert.True(EdgeTaskStateMachine.IsTransitionAllowed(EdgeTaskStatus.Pending, EdgeTaskStatus.Sent));
        Assert.True(EdgeTaskStateMachine.IsTransitionAllowed(EdgeTaskStatus.Running, EdgeTaskStatus.Succeeded));
        Assert.False(EdgeTaskStateMachine.IsTransitionAllowed(EdgeTaskStatus.Succeeded, EdgeTaskStatus.Running));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var contractsProject = Path.Combine(directory.FullName, "IoTSharp.Contracts", "IoTSharp.Contracts.csproj");
            if (File.Exists(contractsProject))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Cannot locate IoTSharp repository root from test output path.");
    }
}
