#nullable enable

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace IoTSharp.Test
{
    public sealed class AppWithSqliteTest : IClassFixture<SqliteAppFixture>
    {
        private readonly SqliteAppFixture _fixture;

        public AppWithSqliteTest(SqliteAppFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public Task AppIsInstalled() => _fixture.AssertAppIsInstalledAsync();

        [Fact]
        public Task AppAccountLogin() => _fixture.AssertAppAccountLoginAsync();

        [Fact]
        public Task AppDevicesCreate() => _fixture.AssertAppDevicesCreateAsync();

        [Fact]
        public async Task products_UpdatePersistsDefaultDeviceType()
        {
            using var client = _fixture.CreateClient();
            await _fixture.AuthorizeClientAsync(client);

            var productName = $"product-type-{Guid.NewGuid():N}";
            var ProductToken = $"pt-{Guid.NewGuid():N}";
            var saveProduct = await client.PostAsJsonAsync("/api/Products/Save", new ProductAddDto
            {
                Name = productName,
                Description = "product default type update test",
                ProductToken = ProductToken,
                DefaultDeviceType = DeviceType.Gateway,
                DefaultIdentityType = IdentityType.ProductToken,
                DefaultTimeout = 30,
                GatewayConfiguration = string.Empty
            });
            var saved = await ReadApiResultAsync<bool>(saveProduct);
            Assert.Equal((int)ApiCode.Success, saved.Code);

            var listed = await GetApiResultAsync<PagedData<ProductDto>>(client,
                $"/api/Products/List?offset=0&limit=10&name={Uri.EscapeDataString(productName)}");
            var product = Assert.Single(listed.Data!.rows, p => p.Name == productName);

            var update = await client.PutAsJsonAsync("/api/Products/Update", new ProductAddDto
            {
                Id = product.Id,
                Name = productName,
                Description = "product default type update test",
                ProductToken = ProductToken,
                DefaultDeviceType = DeviceType.Device,
                DefaultIdentityType = IdentityType.ProductToken,
                DefaultTimeout = 45,
                GatewayConfiguration = string.Empty
            });
            var updated = await ReadApiResultAsync<bool>(update);
            Assert.Equal((int)ApiCode.Success, updated.Code);
            Assert.True(updated.Data);

            var detail = await GetApiResultAsync<ProductAddDto>(client, $"/api/Products/Get?id={product.Id}");
            Assert.Equal(DeviceType.Device, detail.Data!.DefaultDeviceType);
            Assert.Equal(45, detail.Data.DefaultTimeout);
        }

        [Fact]
        public async Task AttributeLatest_SaveAsync_UpdatesExistingKeysAndAddsNewKeys()
        {
            using var client = _fixture.CreateClient();
            var device = await _fixture.CreateDeviceAsync(client);
            var deviceId = device.Data!.Id;
            var firstActivity = new DateTime(2026, 7, 2, 1, 0, 0, DateTimeKind.Utc);
            var lastConnect = new DateTime(2026, 7, 2, 1, 5, 0, DateTimeKind.Utc);
            var expectedKeys = new[] { Constants._Active, Constants._LastActivityDateTime, Constants._LastConnectDateTime };

            using (var scope = _fixture.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var result = await dbContext.SaveAsync<AttributeLatest>(new Dictionary<string, object>
                {
                    [Constants._Active] = true,
                    [Constants._LastActivityDateTime] = firstActivity
                }, deviceId, DataSide.ServerSide);

                Assert.Empty(result.exceptions);
            }

            using (var scope = _fixture.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var result = await dbContext.SaveAsync<AttributeLatest>(new Dictionary<string, object>
                {
                    [Constants._Active] = false,
                    [Constants._LastConnectDateTime] = lastConnect
                }, deviceId, DataSide.ServerSide);

                Assert.Empty(result.exceptions);
            }

            using (var scope = _fixture.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var rows = await dbContext.AttributeLatest
                    .Where(x => x.DeviceId == deviceId && expectedKeys.Contains(x.KeyName))
                    .AsNoTracking()
                    .ToListAsync();

                Assert.Equal(3, rows.Count);
                var active = Assert.Single(rows, x => x.KeyName == Constants._Active);
                Assert.Equal(DataCatalog.AttributeLatest, active.Catalog);
                Assert.Equal(DataSide.ServerSide, active.DataSide);
                Assert.False(active.Value_Boolean);

                var activity = Assert.Single(rows, x => x.KeyName == Constants._LastActivityDateTime);
                Assert.Equal(firstActivity, activity.Value_DateTime);

                var connect = Assert.Single(rows, x => x.KeyName == Constants._LastConnectDateTime);
                Assert.Equal(lastConnect, connect.Value_DateTime);
            }
        }

        [Fact]
        public async Task EdgeTask_FormalModelPersistsDispatchAndReceiptState()
        {
            using var client = _fixture.CreateClient();
            var created = await _fixture.CreateDeviceAsync(client, $"sqlite-edge-task-{Guid.NewGuid():N}", DeviceType.Gateway);
            var deviceId = created.Data!.Id;
            var token = await _fixture.GetDeviceAccessTokenAsync(client, deviceId);
            var taskId = Guid.NewGuid();

            await _fixture.AuthorizeClientAsync(client);
            var dispatch = await client.PostAsJsonAsync("/api/EdgeTask/Dispatch", new IoTSharp.Contracts.EdgeTaskRequestDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = taskId,
                TaskType = IoTSharp.Contracts.EdgeTaskType.HealthProbe,
                CreatedAt = DateTime.UtcNow,
                Address = new IoTSharp.Contracts.EdgeTaskAddressDto
                {
                    TargetType = IoTSharp.Contracts.EdgeTaskTargetType.EdgeNode,
                    DeviceId = deviceId,
                    RuntimeType = EdgeRuntimeTypes.Gateway,
                    TargetKey = deviceId.ToString()
                },
                Parameters = new Dictionary<string, object> { ["probe"] = "ping" },
                Metadata = new Dictionary<string, string> { ["source"] = "sqlite-test" }
            });
            var dispatchResult = await ReadApiResultAsync<IoTSharp.Contracts.EdgeTaskRequestDto>(dispatch);
            Assert.Equal((int)ApiCode.Success, dispatchResult.Code);

            await AssertStoredEdgeTaskAsync(taskId, IoTSharp.Contracts.EdgeTaskStatus.Pending, deviceId, progress: null);

            var pulled = await GetApiResultAsync<List<IoTSharp.Contracts.EdgeTaskRequestDto>>(client, $"/api/EdgeTask/Dispatch/{token}");
            Assert.Equal((int)ApiCode.Success, pulled.Code);
            Assert.Contains(pulled.Data!, task => task.TaskId == taskId);

            await AssertStoredEdgeTaskAsync(taskId, IoTSharp.Contracts.EdgeTaskStatus.Sent, deviceId, progress: null);

            var accepted = await client.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{token}/Accept", new IoTSharp.Contracts.EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = taskId,
                TargetType = IoTSharp.Contracts.EdgeTaskTargetType.EdgeNode,
                TargetKey = deviceId.ToString(),
                RuntimeType = EdgeRuntimeTypes.Gateway,
                Status = IoTSharp.Contracts.EdgeTaskStatus.Accepted,
                ReportedAt = DateTime.UtcNow
            });
            var acceptedResult = await ReadApiResultAsync<object>(accepted);
            Assert.Equal((int)ApiCode.Success, acceptedResult.Code);

            var running = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new IoTSharp.Contracts.EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = taskId,
                TargetType = IoTSharp.Contracts.EdgeTaskTargetType.EdgeNode,
                TargetKey = deviceId.ToString(),
                RuntimeType = EdgeRuntimeTypes.Gateway,
                Status = IoTSharp.Contracts.EdgeTaskStatus.Running,
                Progress = 25,
                ReportedAt = DateTime.UtcNow
            });
            var runningResult = await ReadApiResultAsync<IoTSharp.Contracts.EdgeTaskReceiptDto>(running);
            Assert.Equal((int)ApiCode.Success, runningResult.Code);

            await AssertStoredEdgeTaskAsync(taskId, IoTSharp.Contracts.EdgeTaskStatus.Running, deviceId, progress: 25);

            var duplicateDispatch = await client.PostAsJsonAsync("/api/EdgeTask/Dispatch", new IoTSharp.Contracts.EdgeTaskRequestDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = taskId,
                TaskType = IoTSharp.Contracts.EdgeTaskType.HealthProbe,
                CreatedAt = DateTime.UtcNow,
                Address = new IoTSharp.Contracts.EdgeTaskAddressDto
                {
                    TargetType = IoTSharp.Contracts.EdgeTaskTargetType.EdgeNode,
                    DeviceId = deviceId,
                    RuntimeType = EdgeRuntimeTypes.Gateway,
                    TargetKey = deviceId.ToString()
                },
                Parameters = new Dictionary<string, object> { ["probe"] = "retry" },
                Metadata = new Dictionary<string, string> { ["source"] = "sqlite-test" }
            });
            var duplicateDispatchResult = await ReadApiResultAsync<IoTSharp.Contracts.EdgeTaskRequestDto>(duplicateDispatch);
            Assert.Equal((int)ApiCode.Success, duplicateDispatchResult.Code);

            await AssertStoredEdgeTaskAsync(taskId, IoTSharp.Contracts.EdgeTaskStatus.Running, deviceId, progress: 25);
        }

        [Fact]
        public async Task CollectionConfigVersion_PersistsSnapshotAndAssignment()
        {
            using var client = _fixture.CreateClient();
            var created = await _fixture.CreateDeviceAsync(client, $"sqlite-edge-config-{Guid.NewGuid():N}", DeviceType.Gateway);
            var deviceId = created.Data!.Id;
            var templateId = Guid.NewGuid();

            await _fixture.AuthorizeClientAsync(client);
            var saveConfig = await client.PutAsJsonAsync($"/api/Edge/{deviceId}/CollectionConfig", new EdgeCollectionConfigurationUpdateDto
            {
                SourceType = "ProductCollectionTemplate",
                SourceId = templateId.ToString("D"),
                SourceVersion = "3",
                SourceMetadata = new Dictionary<string, object>
                {
                    ["templateKey"] = "sqlite-modbus-template"
                },
                Tasks =
                [
                    new CollectionTaskDto
                    {
                        TaskKey = "sqlite-modbus",
                        Protocol = CollectionProtocolType.Modbus,
                        Connection = new CollectionConnectionDto
                        {
                            ConnectionKey = "sqlite-plc",
                            ConnectionName = "SQLite PLC",
                            Protocol = CollectionProtocolType.Modbus,
                            Transport = "tcp",
                            Host = "127.0.0.1",
                            Port = 1502
                        },
                        Devices =
                        [
                            new CollectionDeviceDto
                            {
                                DeviceKey = "sqlite-device-01",
                                DeviceName = "SQLite Device 01",
                                Points =
                                [
                                    new CollectionPointDto
                                    {
                                        PointKey = "temperature",
                                        PointName = "Temperature",
                                        SourceType = "holding-register",
                                        Address = "40001",
                                        RawValueType = "Int16",
                                        Length = 1,
                                        Polling = new PollingPolicyDto { ReadPeriodMs = 1000 },
                                        Mapping = new PlatformMappingDto
                                        {
                                            TargetType = CollectionTargetType.Telemetry,
                                            TargetName = "temperature",
                                            ValueType = CollectionValueType.Double
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                ]
            });
            var savedConfig = await ReadApiResultAsync<EdgeCollectionConfigurationDto>(saveConfig);
            Assert.Equal((int)ApiCode.Success, savedConfig.Code);

            var versions = await GetApiResultAsync<PagedData<CollectionConfigurationVersionDto>>(client, $"/api/Edge/{deviceId}/CollectionConfigVersions?limit=10");
            Assert.Equal((int)ApiCode.Success, versions.Code);
            var versionRow = Assert.Single(versions.Data!.rows);
            Assert.Equal(savedConfig.Data!.Version, versionRow.Version);
            Assert.Equal("ProductCollectionTemplate", versionRow.SourceType);
            Assert.Equal(templateId.ToString("D"), versionRow.SourceId);
            Assert.False(string.IsNullOrWhiteSpace(versionRow.ConfigurationHash));
            Assert.Null(versionRow.Configuration);

            var versionDetail = await GetApiResultAsync<CollectionConfigurationVersionDto>(client, $"/api/Edge/{deviceId}/CollectionConfigVersions/{versionRow.Version}");
            Assert.Equal((int)ApiCode.Success, versionDetail.Code);
            Assert.NotNull(versionDetail.Data!.Configuration);
            Assert.Equal(deviceId, versionDetail.Data.Configuration!.EdgeNodeId);
            Assert.Equal("sqlite-modbus", Assert.Single(versionDetail.Data.Configuration.Tasks).TaskKey);

            var assignments = await GetApiResultAsync<PagedData<EdgeCollectionAssignmentDto>>(client, $"/api/Edge/{deviceId}/CollectionAssignments?limit=10");
            Assert.Equal((int)ApiCode.Success, assignments.Code);
            var activeAssignment = Assert.Single(assignments.Data!.rows, item => item.Status == EdgeCollectionAssignmentStatus.Active);
            Assert.Equal(versionRow.Id, activeAssignment.CollectionConfigurationVersionId);
            Assert.Equal(versionRow.ConfigurationHash, activeAssignment.ConfigurationHash);

            using var scope = _fixture.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedVersion = await dbContext.CollectionConfigurationVersions.AsNoTracking().SingleAsync(item => item.Id == versionRow.Id);
            Assert.Equal(deviceId, storedVersion.GatewayId);
            Assert.Equal(savedConfig.Data.Version, storedVersion.Version);
            Assert.Equal(versionRow.ConfigurationHash, storedVersion.ConfigurationHash);
        }

        private async Task AssertStoredEdgeTaskAsync(Guid taskId, IoTSharp.Contracts.EdgeTaskStatus status, Guid gatewayId, int? progress)
        {
            using var scope = _fixture.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedTask = await dbContext.EdgeTasks.AsNoTracking().SingleAsync(task => task.Id == taskId);

            Assert.Equal(gatewayId, storedTask.GatewayId);
            Assert.Equal(status, storedTask.Status);
            Assert.Equal(progress, storedTask.Progress);
        }

        private static async Task<ApiResult<T>> GetApiResultAsync<T>(HttpClient client, string requestUri)
        {
            var response = await client.GetAsync(requestUri);
            return await ReadApiResultAsync<T>(response);
        }

        private static async Task<ApiResult<T>> ReadApiResultAsync<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResult<T>>();
            Assert.NotNull(result);
            return result;
        }
    }
}
