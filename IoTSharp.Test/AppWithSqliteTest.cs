#nullable enable

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
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
using System.Text.Json;

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
        public async Task ReleasePackage_UploadPublishDownloadAndReceiptFlow()
        {
            using var client = _fixture.CreateClient();
            var created = await _fixture.CreateDeviceAsync(client, $"sqlite-release-{Guid.NewGuid():N}", DeviceType.Gateway);
            var deviceId = created.Data!.Id;
            var token = await _fixture.GetDeviceAccessTokenAsync(client, deviceId);
            await _fixture.AuthorizeClientAsync(client);

            var packageBytes = Encoding.UTF8.GetBytes("iotedge-gateway-package-for-sqlite-test");
            var expectedSha256 = Convert.ToHexString(SHA256.HashData(packageBytes));
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(nameof(ReleasePackageType.Software)), "PackageType");
            form.Add(new StringContent("iotedge-gateway"), "PackageKey");
            form.Add(new StringContent("IoTEdge Gateway Runtime"), "Name");
            form.Add(new StringContent("1.4.0-test"), "Version");
            form.Add(new StringContent(EdgeRuntimeTypes.Gateway), "TargetRuntimeType");
            form.Add(new StringContent(">=1.3.0"), "TargetRuntimeVersion");
            form.Add(new StringContent("""{"channel":"sqlite-test"}"""), "Metadata");
            var fileContent = new ByteArrayContent(packageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            form.Add(fileContent, "File", "iotedge-gateway-1.4.0-test.zip");

            var upload = await client.PostAsync("/api/ReleasePackages/Upload", form);
            var uploadResult = await ReadApiResultAsync<ReleasePackageDto>(upload);
            Assert.Equal((int)ApiCode.Success, uploadResult.Code);
            Assert.NotNull(uploadResult.Data);
            Assert.Equal(ReleasePackageType.Software, uploadResult.Data!.PackageType);
            Assert.Equal("iotedge-gateway", uploadResult.Data.PackageKey);
            Assert.Equal("1.4.0-test", uploadResult.Data.Version);
            Assert.Equal(EdgeRuntimeTypes.Gateway, uploadResult.Data.TargetRuntimeType);
            Assert.Equal(expectedSha256, uploadResult.Data.Sha256);
            Assert.Equal(packageBytes.LongLength, uploadResult.Data.Size);

            var publish = await client.PostAsJsonAsync($"/api/ReleasePackages/{uploadResult.Data.Id}/Publish", new ReleasePackagePublishRequestDto
            {
                EdgeNodeId = deviceId,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                InstanceId = "sqlite-release-gateway",
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = "sqlite-test"
                }
            });
            var publishResult = await ReadApiResultAsync<ReleasePackagePublishResultDto>(publish);
            Assert.Equal((int)ApiCode.Success, publishResult.Code);
            Assert.NotNull(publishResult.Data);
            Assert.Equal(EdgeTaskType.SoftwareUpdate, publishResult.Data!.Task.TaskType);
            Assert.Equal(uploadResult.Data.Id.ToString("D"), GetParameterString(publishResult.Data.Task.Parameters, "packageId"));
            Assert.Equal(uploadResult.Data.Version, GetParameterString(publishResult.Data.Task.Parameters, "packageVersion"));
            Assert.Equal(uploadResult.Data.Sha256, GetParameterString(publishResult.Data.Task.Parameters, "sha256"));
            var downloadUrl = GetParameterString(publishResult.Data.Task.Parameters, "downloadUrl");
            Assert.False(string.IsNullOrWhiteSpace(downloadUrl));

            var pulledTasks = await GetApiResultAsync<List<EdgeTaskRequestDto>>(client, $"/api/EdgeTask/Dispatch/{token}");
            Assert.Equal((int)ApiCode.Success, pulledTasks.Code);
            Assert.Contains(pulledTasks.Data!, task => task.TaskId == publishResult.Data.Task.TaskId && task.TaskType == EdgeTaskType.SoftwareUpdate);

            var accepted = await client.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{token}/Accept", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                ReportedAt = DateTime.UtcNow
            });
            var acceptedResult = await ReadApiResultAsync<object>(accepted);
            Assert.Equal((int)ApiCode.Success, acceptedResult.Code);

            var wrongRunning = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                TargetType = publishResult.Data.Task.Address.TargetType,
                TargetKey = publishResult.Data.Task.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                InstanceId = "sqlite-release-gateway",
                Status = EdgeTaskStatus.Running,
                Progress = 20,
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["packageId"] = uploadResult.Data.Id,
                    ["packageVersion"] = uploadResult.Data.Version,
                    ["sha256"] = "0000000000000000000000000000000000000000000000000000000000000000"
                }
            });
            var wrongRunningResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(wrongRunning);
            Assert.Equal((int)ApiCode.InValidData, wrongRunningResult.Code);

            var running = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                TargetType = publishResult.Data.Task.Address.TargetType,
                TargetKey = publishResult.Data.Task.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                InstanceId = "sqlite-release-gateway",
                Status = EdgeTaskStatus.Running,
                Progress = 50,
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["packageId"] = uploadResult.Data.Id,
                    ["packageVersion"] = uploadResult.Data.Version,
                    ["sha256"] = uploadResult.Data.Sha256
                }
            });
            var runningResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(running);
            Assert.Equal((int)ApiCode.Success, runningResult.Code);

            var metadataOnlySucceeded = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                TargetType = publishResult.Data.Task.Address.TargetType,
                TargetKey = publishResult.Data.Task.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                InstanceId = "sqlite-release-gateway",
                Status = EdgeTaskStatus.Succeeded,
                Progress = 100,
                ReportedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["packageId"] = uploadResult.Data.Id.ToString("D"),
                    ["packageVersion"] = uploadResult.Data.Version,
                    ["sha256"] = uploadResult.Data.Sha256
                }
            });
            var metadataOnlySucceededResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(metadataOnlySucceeded);
            Assert.Equal((int)ApiCode.InValidData, metadataOnlySucceededResult.Code);

            var succeeded = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                TargetType = publishResult.Data.Task.Address.TargetType,
                TargetKey = publishResult.Data.Task.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                InstanceId = "sqlite-release-gateway",
                Status = EdgeTaskStatus.Succeeded,
                Progress = 100,
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["packageId"] = uploadResult.Data.Id,
                    ["packageVersion"] = uploadResult.Data.Version,
                    ["sha256"] = uploadResult.Data.Sha256
                }
            });
            var succeededResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(succeeded);
            Assert.Equal((int)ApiCode.Success, succeededResult.Code);

            var download = await client.GetAsync(downloadUrl);
            download.EnsureSuccessStatusCode();
            Assert.Equal(packageBytes, await download.Content.ReadAsByteArrayAsync());

            using var scope = _fixture.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedPackage = await dbContext.ReleasePackages.AsNoTracking().SingleAsync(package => package.Id == uploadResult.Data.Id);
            Assert.Equal(expectedSha256, storedPackage.Sha256);
            Assert.Equal(EdgeRuntimeTypes.Gateway, storedPackage.TargetRuntimeType);
            var storedTask = await dbContext.EdgeTasks.AsNoTracking().SingleAsync(task => task.Id == publishResult.Data.Task.TaskId);
            Assert.Equal(EdgeTaskType.SoftwareUpdate, storedTask.TaskType);
            Assert.Equal(EdgeTaskStatus.Succeeded, storedTask.Status);
            using var parameters = JsonDocument.Parse(storedTask.Parameters);
            Assert.Equal(uploadResult.Data.Id, parameters.RootElement.GetProperty("packageId").GetGuid());
            Assert.Equal(expectedSha256, parameters.RootElement.GetProperty("sha256").GetString());
        }

        [Fact]
        public async Task ReleaseCenter_CreatePlanDispatchesGatewaySoftwareTaskAndTracksReceipts()
        {
            using var client = _fixture.CreateClient();
            var created = await _fixture.CreateDeviceAsync(client, $"sqlite-release-center-{Guid.NewGuid():N}", DeviceType.Gateway);
            var deviceId = created.Data!.Id;
            var token = await _fixture.GetDeviceAccessTokenAsync(client, deviceId);
            await _fixture.AuthorizeClientAsync(client);

            var packageKey = $"iotedge-gateway-{Guid.NewGuid():N}";
            var packageVersion = $"1.5.0-{Guid.NewGuid():N}";
            var packageBytes = Encoding.UTF8.GetBytes($"release-center-package-{packageKey}");
            var expectedSha256 = Convert.ToHexString(SHA256.HashData(packageBytes));
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(nameof(ReleasePackageType.CollectorSoftware)), "PackageType");
            form.Add(new StringContent(packageKey), "PackageKey");
            form.Add(new StringContent("IoTEdge Gateway Runtime"), "Name");
            form.Add(new StringContent(packageVersion), "Version");
            form.Add(new StringContent(EdgeRuntimeTypes.Gateway), "TargetRuntimeType");
            form.Add(new StringContent(">=1.4.0"), "TargetRuntimeVersion");
            form.Add(new StringContent("""{"channel":"release-center-test"}"""), "Metadata");
            var fileContent = new ByteArrayContent(packageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            form.Add(fileContent, "File", $"{packageKey}.zip");

            var upload = await client.PostAsync("/api/ReleasePackages/Upload", form);
            var uploadResult = await ReadApiResultAsync<ReleasePackageDto>(upload);
            Assert.Equal((int)ApiCode.Success, uploadResult.Code);
            Assert.NotNull(uploadResult.Data);
            Assert.Equal(ReleasePackageType.CollectorSoftware, uploadResult.Data!.PackageType);
            Assert.Equal(expectedSha256, uploadResult.Data.Sha256);

            var create = await client.PostAsJsonAsync("/api/ReleaseCenter/Plans", new ReleasePlanCreateRequestDto
            {
                Name = $"sqlite-release-center-{Guid.NewGuid():N}",
                Description = "Release Center SQLite end-to-end test",
                PlanType = ReleasePlanType.SoftwareUpdate,
                PackageId = uploadResult.Data.Id,
                ConfirmationPolicy = ReleaseConfirmationPolicy.None,
                AutoStart = true,
                Strategy = new ReleaseRolloutStrategyDto
                {
                    BatchSize = 0,
                    ContinueOnFailure = false
                },
                Targets =
                [
                    new ReleaseTargetDto
                    {
                        TargetType = ReleaseTargetType.Gateway,
                        TargetId = deviceId,
                        RuntimeType = EdgeRuntimeTypes.Gateway,
                        InstanceId = "sqlite-release-center-gateway",
                        Metadata = new Dictionary<string, string>
                        {
                            ["scope"] = "sqlite"
                        }
                    }
                ],
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = "sqlite-test"
                }
            });
            var createResult = await ReadApiResultAsync<ReleasePlanOperationResultDto>(create);
            Assert.Equal((int)ApiCode.Success, createResult.Code);
            Assert.NotNull(createResult.Data);
            Assert.Equal(ReleasePlanStatus.Running, createResult.Data!.Plan.Status);
            Assert.Equal(1, createResult.Data.Plan.TotalTaskCount);
            Assert.Equal(1, createResult.Data.Plan.PendingTaskCount);
            var createdTask = Assert.Single(createResult.Data.Plan.Tasks);
            Assert.Equal(ReleaseTaskStatus.Pending, createdTask.Status);
            Assert.Equal(deviceId, createdTask.GatewayId.GetValueOrDefault());
            Assert.True(createdTask.EdgeTaskId.HasValue);
            var edgeTask = Assert.Single(createResult.Data.EdgeTasks);
            Assert.Equal(EdgeTaskType.SoftwareUpdate, edgeTask.TaskType);
            Assert.Equal(createdTask.EdgeTaskId.GetValueOrDefault(), edgeTask.TaskId);
            Assert.Equal(uploadResult.Data.Id.ToString("D"), GetParameterString(edgeTask.Parameters, "packageId"));
            Assert.Equal(createResult.Data.Plan.Id.ToString("D"), GetParameterString(edgeTask.Parameters, "releasePlanId"));
            Assert.Equal(createdTask.Id.ToString("D"), GetParameterString(edgeTask.Parameters, "releaseTaskId"));

            var pulledTasks = await GetApiResultAsync<List<EdgeTaskRequestDto>>(client, $"/api/EdgeTask/Dispatch/{token}");
            Assert.Equal((int)ApiCode.Success, pulledTasks.Code);
            var pulledTask = Assert.Single(pulledTasks.Data!, task => task.TaskId == edgeTask.TaskId);
            Assert.Equal(EdgeTaskType.SoftwareUpdate, pulledTask.TaskType);

            var sentPlan = await GetApiResultAsync<ReleasePlanDto>(client, $"/api/ReleaseCenter/Plans/{createResult.Data.Plan.Id}");
            Assert.Equal((int)ApiCode.Success, sentPlan.Code);
            var sentReleaseTask = Assert.Single(sentPlan.Data!.Tasks);
            Assert.Equal(ReleaseTaskStatus.Sent, sentReleaseTask.Status);
            Assert.Equal(ReleasePlanStatus.Running, sentPlan.Data.Status);

            var accepted = await client.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{token}/Accept", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                ReportedAt = DateTime.UtcNow
            });
            var acceptedResult = await ReadApiResultAsync<object>(accepted);
            Assert.Equal((int)ApiCode.Success, acceptedResult.Code);

            var running = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                TargetType = pulledTask.Address.TargetType,
                TargetKey = pulledTask.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                InstanceId = "sqlite-release-center-gateway",
                Status = EdgeTaskStatus.Running,
                Progress = 40,
                Message = "installing",
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["packageId"] = uploadResult.Data.Id,
                    ["packageVersion"] = uploadResult.Data.Version,
                    ["sha256"] = uploadResult.Data.Sha256
                }
            });
            var runningResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(running);
            Assert.Equal((int)ApiCode.Success, runningResult.Code);

            var runningPlan = await GetApiResultAsync<ReleasePlanDto>(client, $"/api/ReleaseCenter/Plans/{createResult.Data.Plan.Id}");
            Assert.Equal((int)ApiCode.Success, runningPlan.Code);
            var runningReleaseTask = Assert.Single(runningPlan.Data!.Tasks);
            Assert.Equal(ReleaseTaskStatus.Running, runningReleaseTask.Status);
            Assert.Equal(40, runningReleaseTask.Progress);

            var succeeded = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                TargetType = pulledTask.Address.TargetType,
                TargetKey = pulledTask.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                InstanceId = "sqlite-release-center-gateway",
                Status = EdgeTaskStatus.Succeeded,
                Progress = 100,
                Message = "installed",
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["packageId"] = uploadResult.Data.Id,
                    ["packageVersion"] = uploadResult.Data.Version,
                    ["sha256"] = uploadResult.Data.Sha256
                }
            });
            var succeededResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(succeeded);
            Assert.Equal((int)ApiCode.Success, succeededResult.Code);

            var completedPlan = await GetApiResultAsync<ReleasePlanDto>(client, $"/api/ReleaseCenter/Plans/{createResult.Data.Plan.Id}");
            Assert.Equal((int)ApiCode.Success, completedPlan.Code);
            Assert.Equal(ReleasePlanStatus.Succeeded, completedPlan.Data!.Status);
            Assert.Equal(1, completedPlan.Data.SucceededTaskCount);
            Assert.Equal(0, completedPlan.Data.PendingTaskCount);
            Assert.Equal(0, completedPlan.Data.RunningTaskCount);
            var completedReleaseTask = Assert.Single(completedPlan.Data.Tasks);
            Assert.Equal(ReleaseTaskStatus.Succeeded, completedReleaseTask.Status);
            Assert.Equal(100, completedReleaseTask.Progress);
            Assert.NotNull(completedReleaseTask.CompletedAt);
            Assert.NotNull(completedPlan.Data.CompletedAt);

            var receipts = await GetApiResultAsync<List<ReleaseReceiptDto>>(client, $"/api/ReleaseCenter/Plans/{completedPlan.Data.Id}/Receipts");
            Assert.Equal((int)ApiCode.Success, receipts.Code);
            Assert.Contains(receipts.Data!, receipt => receipt.Status == ReleaseTaskStatus.Accepted);
            Assert.Contains(receipts.Data!, receipt => receipt.Status == ReleaseTaskStatus.Running && receipt.Progress == 40);
            Assert.Contains(receipts.Data!, receipt => receipt.Status == ReleaseTaskStatus.Succeeded && receipt.Progress == 100);

            using var scope = _fixture.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedPlan = await dbContext.ReleasePlans.AsNoTracking().SingleAsync(plan => plan.Id == completedPlan.Data.Id);
            Assert.Equal(ReleasePlanStatus.Succeeded, storedPlan.Status);
            var storedReleaseTask = await dbContext.ReleaseTasks.AsNoTracking().SingleAsync(task => task.Id == completedReleaseTask.Id);
            Assert.Equal(ReleaseTaskStatus.Succeeded, storedReleaseTask.Status);
            Assert.Equal(edgeTask.TaskId, storedReleaseTask.EdgeTaskId.GetValueOrDefault());
            Assert.Equal(3, await dbContext.ReleaseReceipts.AsNoTracking().CountAsync(receipt => receipt.PlanId == completedPlan.Data.Id));
            Assert.True(await dbContext.AuditLog.AsNoTracking().AnyAsync(log =>
                log.ObjectID == completedPlan.Data.Id &&
                log.ObjectType == ObjectType.ReleasePlan &&
                log.ActionName == "ReleasePlanCreate"));
        }

        [Fact]
        public async Task ReleaseCenter_AssetScopeDispatchesDeviceScriptOtaAndTracksReceipts()
        {
            using var client = _fixture.CreateClient();
            var created = await _fixture.CreateDeviceAsync(client, $"sqlite-device-script-{Guid.NewGuid():N}", DeviceType.Device);
            var deviceId = created.Data!.Id;
            var token = await _fixture.GetDeviceAccessTokenAsync(client, deviceId);
            Guid assetId;

            using (var scope = _fixture.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var storedDevice = await dbContext.Device
                    .Include(device => device.Tenant)
                    .Include(device => device.Customer)
                    .SingleAsync(device => device.Id == deviceId);
                assetId = Guid.NewGuid();
                dbContext.Assets.Add(new Asset
                {
                    Id = assetId,
                    Name = $"sqlite-asset-scope-{Guid.NewGuid():N}",
                    Description = "Release Center AssetScope device OTA test",
                    AssetType = "equipment",
                    Tenant = storedDevice.Tenant,
                    Customer = storedDevice.Customer,
                    OwnedAssets =
                    [
                        new AssetRelation
                        {
                            DeviceId = deviceId,
                            DataCatalog = DataCatalog.TelemetryLatest,
                            KeyName = "temperature",
                            Name = "temperature",
                            Description = "temperature"
                        }
                    ]
                });
                await dbContext.SaveChangesAsync();
            }

            await _fixture.AuthorizeClientAsync(client);

            var packageKey = $"iotembedded-script-{Guid.NewGuid():N}";
            var packageVersion = $"2026.7.6-{Guid.NewGuid():N}";
            var scriptBytes = Encoding.UTF8.GetBytes($"10 PRINT \"{packageKey}\"");
            var expectedSha256 = Convert.ToHexString(SHA256.HashData(scriptBytes));
            const string expectedCrc32 = "A1B2C3D4";
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(nameof(ReleasePackageType.DeviceScript)), "PackageType");
            form.Add(new StringContent(packageKey), "PackageKey");
            form.Add(new StringContent("IoTEmbedded BASIC script"), "Name");
            form.Add(new StringContent(packageVersion), "Version");
            form.Add(new StringContent("iotembedded"), "TargetRuntimeType");
            form.Add(new StringContent(">=0.8.0"), "TargetRuntimeVersion");
            form.Add(new StringContent($$"""{"scriptCrc32":"{{expectedCrc32}}","scriptSlot":"inactive"}"""), "Metadata");
            var fileContent = new ByteArrayContent(scriptBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            form.Add(fileContent, "File", $"{packageKey}.bas");

            var upload = await client.PostAsync("/api/ReleasePackages/Upload", form);
            var uploadResult = await ReadApiResultAsync<ReleasePackageDto>(upload);
            Assert.Equal((int)ApiCode.Success, uploadResult.Code);
            Assert.NotNull(uploadResult.Data);
            Assert.Equal(ReleasePackageType.DeviceScript, uploadResult.Data!.PackageType);
            Assert.Equal(expectedSha256, uploadResult.Data.Sha256);

            var create = await client.PostAsJsonAsync("/api/ReleaseCenter/Plans", new ReleasePlanCreateRequestDto
            {
                Name = $"sqlite-device-script-ota-{Guid.NewGuid():N}",
                Description = "Release Center AssetScope device script OTA test",
                PlanType = ReleasePlanType.DeviceScriptOta,
                PackageId = uploadResult.Data.Id,
                ConfirmationPolicy = ReleaseConfirmationPolicy.None,
                AutoStart = true,
                Strategy = new ReleaseRolloutStrategyDto
                {
                    BatchSize = 1,
                    ContinueOnFailure = false
                },
                Targets =
                [
                    new ReleaseTargetDto
                    {
                        TargetType = ReleaseTargetType.AssetScope,
                        TargetId = assetId,
                        RuntimeType = "iotembedded",
                        InstanceId = "stm32-basic-01",
                        Metadata = new Dictionary<string, string>
                        {
                            ["scope"] = "asset"
                        }
                    }
                ]
            });
            var createResult = await ReadApiResultAsync<ReleasePlanOperationResultDto>(create);
            Assert.Equal((int)ApiCode.Success, createResult.Code);
            Assert.NotNull(createResult.Data);
            Assert.Equal(ReleasePlanType.DeviceScriptOta, createResult.Data!.Plan.PlanType);
            var releaseTask = Assert.Single(createResult.Data.Plan.Tasks);
            Assert.Equal(ReleaseTargetType.Device, releaseTask.TargetType);
            Assert.Equal(deviceId, releaseTask.TargetId);
            Assert.Equal(deviceId, releaseTask.GatewayId);
            var edgeTask = Assert.Single(createResult.Data.EdgeTasks);
            Assert.Equal(EdgeTaskType.DeviceScriptOta, edgeTask.TaskType);
            Assert.Equal(EdgeTaskTargetType.Device, edgeTask.Address.TargetType);
            Assert.Equal(deviceId.ToString("D"), GetParameterString(edgeTask.Parameters, "targetDeviceId"));
            Assert.Equal(expectedCrc32, GetParameterString(edgeTask.Parameters, "scriptCrc32"));

            var pulledTasks = await GetApiResultAsync<List<EdgeTaskRequestDto>>(client, $"/api/EdgeTask/Dispatch/{token}");
            Assert.Equal((int)ApiCode.Success, pulledTasks.Code);
            var pulledTask = Assert.Single(pulledTasks.Data!, task => task.TaskId == edgeTask.TaskId);
            Assert.Equal(EdgeTaskType.DeviceScriptOta, pulledTask.TaskType);

            var accepted = await client.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{token}/Accept", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                ReportedAt = DateTime.UtcNow
            });
            var acceptedResult = await ReadApiResultAsync<object>(accepted);
            Assert.Equal((int)ApiCode.Success, acceptedResult.Code);

            var running = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                TargetType = pulledTask.Address.TargetType,
                TargetKey = pulledTask.Address.TargetKey,
                RuntimeType = "iotembedded",
                InstanceId = "stm32-basic-01",
                Status = EdgeTaskStatus.Running,
                Progress = 50,
                Message = "script staging",
                ReportedAt = DateTime.UtcNow
            });
            var runningResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(running);
            Assert.Equal((int)ApiCode.Success, runningResult.Code);

            var succeeded = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                TargetType = pulledTask.Address.TargetType,
                TargetKey = pulledTask.Address.TargetKey,
                RuntimeType = "iotembedded",
                InstanceId = "stm32-basic-01",
                Status = EdgeTaskStatus.Succeeded,
                Progress = 100,
                Message = "script switched",
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["packageId"] = uploadResult.Data.Id,
                    ["packageVersion"] = uploadResult.Data.Version,
                    ["sha256"] = uploadResult.Data.Sha256,
                    ["targetDeviceId"] = deviceId,
                    ["scriptCrc32"] = expectedCrc32,
                    ["activeSlot"] = "b"
                }
            });
            var succeededResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(succeeded);
            Assert.Equal((int)ApiCode.Success, succeededResult.Code);

            var completedPlan = await GetApiResultAsync<ReleasePlanDto>(client, $"/api/ReleaseCenter/Plans/{createResult.Data.Plan.Id}");
            Assert.Equal((int)ApiCode.Success, completedPlan.Code);
            Assert.Equal(ReleasePlanStatus.Succeeded, completedPlan.Data!.Status);
            Assert.Equal(1, completedPlan.Data.SucceededTaskCount);
            var completedTask = Assert.Single(completedPlan.Data.Tasks);
            Assert.Equal(ReleaseTaskStatus.Succeeded, completedTask.Status);
            Assert.Equal(100, completedTask.Progress);
            Assert.Equal("AssetScope", completedTask.Metadata["sourceTargetType"]);
            Assert.Equal(assetId.ToString("D"), completedTask.Metadata["sourceTargetId"]);

            using var verifyScope = _fixture.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedEdgeTask = await verifyDbContext.EdgeTasks.AsNoTracking().SingleAsync(task => task.Id == edgeTask.TaskId);
            Assert.Equal(EdgeTaskType.DeviceScriptOta, storedEdgeTask.TaskType);
            Assert.Equal(EdgeTaskStatus.Succeeded, storedEdgeTask.Status);
            Assert.Equal(3, await verifyDbContext.ReleaseReceipts.AsNoTracking().CountAsync(receipt => receipt.PlanId == completedPlan.Data.Id));
        }

        [Fact]
        public async Task ReleaseCenter_DeviceScopeDeviceScriptOta_UsesGatewayChannelAndRequiresScriptCrc()
        {
            using var client = _fixture.CreateClient();
            var gateway = await _fixture.CreateDeviceAsync(client, $"sqlite-script-gateway-{Guid.NewGuid():N}", DeviceType.Gateway);
            var child = await _fixture.CreateDeviceAsync(client, $"sqlite-script-child-{Guid.NewGuid():N}", DeviceType.Device);
            var gatewayId = gateway.Data!.Id;
            var childId = child.Data!.Id;
            var gatewayToken = await _fixture.GetDeviceAccessTokenAsync(client, gatewayId);

            using (var scope = _fixture.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var storedGateway = await dbContext.Device.OfType<Gateway>().SingleAsync(device => device.Id == gatewayId);
                var storedChild = await dbContext.Device.SingleAsync(device => device.Id == childId);
                storedChild.Owner = storedGateway;
                await dbContext.SaveChangesAsync();
            }

            await _fixture.AuthorizeClientAsync(client);

            var packageKey = $"iotembedded-scope-script-{Guid.NewGuid():N}";
            var packageVersion = $"2026.7.6-{Guid.NewGuid():N}";
            var scriptBytes = Encoding.UTF8.GetBytes($"20 PRINT \"{packageKey}\"");
            var expectedSha256 = Convert.ToHexString(SHA256.HashData(scriptBytes));
            const string expectedCrc32 = "B2C3D4E5";
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(nameof(ReleasePackageType.DeviceScript)), "PackageType");
            form.Add(new StringContent(packageKey), "PackageKey");
            form.Add(new StringContent("IoTEmbedded scoped script"), "Name");
            form.Add(new StringContent(packageVersion), "Version");
            form.Add(new StringContent("iotembedded"), "TargetRuntimeType");
            form.Add(new StringContent(">=0.8.0"), "TargetRuntimeVersion");
            form.Add(new StringContent($$"""{"scriptCrc32":"{{expectedCrc32}}","scriptSlot":"inactive","scriptLanguage":"basic"}"""), "Metadata");
            var fileContent = new ByteArrayContent(scriptBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            form.Add(fileContent, "File", $"{packageKey}.bas");

            var upload = await client.PostAsync("/api/ReleasePackages/Upload", form);
            var uploadResult = await ReadApiResultAsync<ReleasePackageDto>(upload);
            Assert.Equal((int)ApiCode.Success, uploadResult.Code);
            Assert.NotNull(uploadResult.Data);
            Assert.Equal(expectedSha256, uploadResult.Data!.Sha256);

            var create = await client.PostAsJsonAsync("/api/ReleaseCenter/Plans", new ReleasePlanCreateRequestDto
            {
                Name = $"sqlite-device-scope-script-ota-{Guid.NewGuid():N}",
                Description = "Release Center DeviceScope device script OTA gateway channel test",
                PlanType = ReleasePlanType.DeviceScriptOta,
                PackageId = uploadResult.Data.Id,
                ConfirmationPolicy = ReleaseConfirmationPolicy.None,
                AutoStart = true,
                Strategy = new ReleaseRolloutStrategyDto
                {
                    BatchSize = 1,
                    ContinueOnFailure = false
                },
                Targets =
                [
                    new ReleaseTargetDto
                    {
                        TargetType = ReleaseTargetType.DeviceScope,
                        RuntimeType = "iotembedded",
                        InstanceId = "stm32-gateway-child-01",
                        Metadata = new Dictionary<string, string>
                        {
                            ["deviceIds"] = childId.ToString("D"),
                            ["scope"] = "device-scope"
                        }
                    }
                ]
            });
            var createResult = await ReadApiResultAsync<ReleasePlanOperationResultDto>(create);
            Assert.Equal((int)ApiCode.Success, createResult.Code);
            Assert.NotNull(createResult.Data);
            var releaseTask = Assert.Single(createResult.Data!.Plan.Tasks);
            Assert.Equal(ReleaseTargetType.Device, releaseTask.TargetType);
            Assert.Equal(childId, releaseTask.TargetId);
            Assert.Equal(gatewayId, releaseTask.GatewayId);
            Assert.Equal(gatewayId, releaseTask.EdgeNodeId);
            Assert.Equal("DeviceScope", releaseTask.Metadata["sourceTargetType"]);
            Assert.Equal(childId.ToString("D"), releaseTask.Metadata["targetDeviceId"]);

            var edgeTask = Assert.Single(createResult.Data.EdgeTasks);
            Assert.Equal(EdgeTaskType.DeviceScriptOta, edgeTask.TaskType);
            Assert.Equal(EdgeTaskTargetType.Device, edgeTask.Address.TargetType);
            Assert.Equal(gatewayId, edgeTask.Address.DeviceId);
            Assert.Contains($":device:{childId:D}:iotembedded", edgeTask.Address.TargetKey);
            Assert.Equal(gatewayId.ToString("D"), GetParameterString(edgeTask.Parameters, "deliveryChannelDeviceId"));
            Assert.Equal(childId.ToString("D"), GetParameterString(edgeTask.Parameters, "targetDeviceId"));
            Assert.Equal(expectedCrc32, GetParameterString(edgeTask.Parameters, "scriptCrc32"));

            var pulledTasks = await GetApiResultAsync<List<EdgeTaskRequestDto>>(client, $"/api/EdgeTask/Dispatch/{gatewayToken}");
            Assert.Equal((int)ApiCode.Success, pulledTasks.Code);
            var pulledTask = Assert.Single(pulledTasks.Data!, task => task.TaskId == edgeTask.TaskId);
            Assert.Equal(gatewayId, pulledTask.Address.DeviceId);

            var accepted = await client.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{gatewayToken}/Accept", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                ReportedAt = DateTime.UtcNow
            });
            var acceptedResult = await ReadApiResultAsync<object>(accepted);
            Assert.Equal((int)ApiCode.Success, acceptedResult.Code);

            var running = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                TargetType = pulledTask.Address.TargetType,
                TargetKey = pulledTask.Address.TargetKey,
                RuntimeType = "iotembedded",
                InstanceId = "stm32-gateway-child-01",
                Status = EdgeTaskStatus.Running,
                Progress = 60,
                Message = "script staging through gateway",
                ReportedAt = DateTime.UtcNow
            });
            var runningResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(running);
            Assert.Equal((int)ApiCode.Success, runningResult.Code);

            var missingCrc = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                TargetType = pulledTask.Address.TargetType,
                TargetKey = pulledTask.Address.TargetKey,
                RuntimeType = "iotembedded",
                InstanceId = "stm32-gateway-child-01",
                Status = EdgeTaskStatus.Succeeded,
                Progress = 100,
                Message = "script switched without crc",
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["packageId"] = uploadResult.Data.Id,
                    ["packageVersion"] = uploadResult.Data.Version,
                    ["sha256"] = uploadResult.Data.Sha256,
                    ["targetDeviceId"] = childId
                }
            });
            var missingCrcResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(missingCrc);
            Assert.Equal((int)ApiCode.InValidData, missingCrcResult.Code);
            Assert.Contains("scriptCrc32", missingCrcResult.Msg);

            var succeeded = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = edgeTask.TaskId,
                TargetType = pulledTask.Address.TargetType,
                TargetKey = pulledTask.Address.TargetKey,
                RuntimeType = "iotembedded",
                InstanceId = "stm32-gateway-child-01",
                Status = EdgeTaskStatus.Succeeded,
                Progress = 100,
                Message = "script switched with crc",
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["packageId"] = uploadResult.Data.Id,
                    ["packageVersion"] = uploadResult.Data.Version,
                    ["sha256"] = uploadResult.Data.Sha256,
                    ["targetDeviceId"] = childId,
                    ["scriptCrc32"] = expectedCrc32
                }
            });
            var succeededResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(succeeded);
            Assert.Equal((int)ApiCode.Success, succeededResult.Code);

            var completedPlan = await GetApiResultAsync<ReleasePlanDto>(client, $"/api/ReleaseCenter/Plans/{createResult.Data.Plan.Id}");
            Assert.Equal((int)ApiCode.Success, completedPlan.Code);
            Assert.Equal(ReleasePlanStatus.Succeeded, completedPlan.Data!.Status);
            var completedTask = Assert.Single(completedPlan.Data.Tasks);
            Assert.Equal(ReleaseTaskStatus.Succeeded, completedTask.Status);
            Assert.Equal(100, completedTask.Progress);

            using var verifyScope = _fixture.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedEdgeTask = await verifyDbContext.EdgeTasks.AsNoTracking().SingleAsync(task => task.Id == edgeTask.TaskId);
            Assert.Equal(gatewayId, storedEdgeTask.GatewayId);
            Assert.Equal(EdgeTaskStatus.Succeeded, storedEdgeTask.Status);
            Assert.Equal(3, await verifyDbContext.ReleaseReceipts.AsNoTracking().CountAsync(receipt => receipt.PlanId == completedPlan.Data.Id));
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

        [Fact]
        public async Task CollectionTemplatePublishConfig_CreatesVersionAssignmentAndEdgeTask()
        {
            using var client = _fixture.CreateClient();
            var created = await _fixture.CreateDeviceAsync(client, $"sqlite-edge-publish-{Guid.NewGuid():N}", DeviceType.Gateway);
            var deviceId = created.Data!.Id;
            var token = await _fixture.GetDeviceAccessTokenAsync(client, deviceId);

            await _fixture.AuthorizeClientAsync(client);
            var productName = $"sqlite-product-{Guid.NewGuid():N}";
            var saveProduct = await client.PostAsJsonAsync("/api/Products/Save", new ProductAddDto
            {
                Name = productName,
                Description = "collection publish test",
                ProductToken = $"ppt-{Guid.NewGuid():N}",
                DefaultDeviceType = DeviceType.Gateway,
                DefaultIdentityType = IdentityType.ProductToken,
                DefaultTimeout = 30,
                GatewayConfiguration = string.Empty
            });
            var savedProduct = await ReadApiResultAsync<bool>(saveProduct);
            Assert.Equal((int)ApiCode.Success, savedProduct.Code);

            var listedProducts = await GetApiResultAsync<PagedData<ProductDto>>(client,
                $"/api/Products/List?offset=0&limit=10&name={Uri.EscapeDataString(productName)}");
            var product = Assert.Single(listedProducts.Data!.rows, item => item.Name == productName);

            var createTemplate = await client.PostAsJsonAsync("/api/CollectionTemplates", new CollectionTemplateUpsertDto
            {
                ProductId = product.Id,
                TemplateKey = "sqlite-publish-modbus",
                Name = "SQLite publish Modbus",
                Status = CollectionTemplateStatus.Active,
                Enabled = true,
                Protocol = new ProtocolTemplateDto
                {
                    Protocol = CollectionProtocolType.Modbus,
                    ProtocolKind = "modbusTcp"
                },
                Connections =
                [
                    new ConnectionTemplateDto
                    {
                        ConnectionKey = "main-plc",
                        ConnectionName = "Main PLC",
                        Transport = "tcp",
                        Host = "127.0.0.1",
                        Port = 1502
                    }
                ],
                Points =
                [
                    new PointTemplateDto
                    {
                        ConnectionKey = "main-plc",
                        PointKey = "temperature",
                        SemanticId = "semantic.temperature",
                        BindingId = "binding.temperature",
                        Name = "Temperature",
                        DisplayName = "Temperature",
                        SourceType = "holding-register",
                        Address = "40001",
                        RawValueType = "Int16",
                        ValueType = CollectionValueType.Double,
                        Length = 1,
                        SamplingPolicy = new SamplingPolicyTemplateDto { ReadPeriodMs = 1000 },
                        Mapping = new MappingPolicyTemplateDto
                        {
                            TargetType = CollectionTargetType.Telemetry,
                            TargetName = "temperature",
                            ValueType = CollectionValueType.Double
                        }
                    }
                ]
            });
            var templateResult = await ReadApiResultAsync<CollectionTemplateDto>(createTemplate);
            Assert.Equal((int)ApiCode.Success, templateResult.Code);
            var templateId = templateResult.Data!.Id;

            var publish = await client.PostAsJsonAsync($"/api/CollectionTemplates/{templateId}/PublishConfig", new CollectionTemplateConfigurationPublishRequestDto
            {
                EdgeNodeId = deviceId,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = "sqlite-test"
                }
            });
            var publishResult = await ReadApiResultAsync<CollectionTemplateConfigurationPublishResultDto>(publish);
            Assert.Equal((int)ApiCode.Success, publishResult.Code);
            Assert.NotNull(publishResult.Data);
            Assert.Equal(EdgeTaskType.ConfigPullRequest, publishResult.Data!.Task.TaskType);
            Assert.Equal(deviceId, publishResult.Data.ConfigurationVersion.GatewayId);
            Assert.Equal(templateId.ToString(), publishResult.Data.ConfigurationVersion.SourceId);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Id, publishResult.Data.Assignment.CollectionConfigurationVersionId);
            Assert.Equal(publishResult.Data.ConfigurationVersion.ConfigurationHash, publishResult.Data.Assignment.ConfigurationHash);
            Assert.Equal(EdgeTaskStatus.Pending, publishResult.Data.Assignment.LastExecutionStatus);
            Assert.Equal(publishResult.Data.Task.TaskId, publishResult.Data.Assignment.LastExecutionTaskId);
            Assert.Equal(publishResult.Data.ConfigurationVersion.ConfigurationHash, publishResult.Data.Task.Parameters["configurationHash"].ToString());

            var pendingVersionStatus = await GetApiResultAsync<EdgeCollectionVersionStatusDto>(client, $"/api/Edge/{deviceId}/CollectionVersionStatus");
            Assert.Equal((int)ApiCode.Success, pendingVersionStatus.Code);
            Assert.NotNull(pendingVersionStatus.Data);
            Assert.Equal(publishResult.Data.Assignment.Id, pendingVersionStatus.Data!.AssignmentId);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Id, pendingVersionStatus.Data.TargetConfigurationVersionId);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Version, pendingVersionStatus.Data.TargetConfigurationVersion);
            Assert.Null(pendingVersionStatus.Data.CurrentConfigurationVersion);
            Assert.True(pendingVersionStatus.Data.HasDifference);
            Assert.False(pendingVersionStatus.Data.IsTargetApplied);
            Assert.Null(pendingVersionStatus.Data.VersionDelta);
            Assert.Equal(EdgeTaskStatus.Pending, pendingVersionStatus.Data.LastPublishStatus);
            Assert.Contains("尚未确认", pendingVersionStatus.Data.DifferenceSummary);

            var pulledTasks = await GetApiResultAsync<List<EdgeTaskRequestDto>>(client, $"/api/EdgeTask/Dispatch/{token}");
            Assert.Equal((int)ApiCode.Success, pulledTasks.Code);
            Assert.Contains(pulledTasks.Data!, task => task.TaskId == publishResult.Data.Task.TaskId && task.TaskType == EdgeTaskType.ConfigPullRequest);

            var sentVersionStatus = await GetApiResultAsync<EdgeCollectionVersionStatusDto>(client, $"/api/Edge/{deviceId}/CollectionVersionStatus");
            Assert.Equal((int)ApiCode.Success, sentVersionStatus.Code);
            Assert.NotNull(sentVersionStatus.Data);
            Assert.Equal(EdgeTaskStatus.Sent, sentVersionStatus.Data!.LastPublishStatus);
            Assert.Contains("拉取", sentVersionStatus.Data.LastPublishMessage);
            Assert.Null(sentVersionStatus.Data.CurrentConfigurationVersion);
            Assert.True(sentVersionStatus.Data.HasDifference);

            var pulledConfig = await GetApiResultAsync<EdgeCollectionConfigurationDto>(client, $"/api/Edge/{deviceId}/CollectionConfig");
            Assert.Equal((int)ApiCode.Success, pulledConfig.Code);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Version, pulledConfig.Data!.Version);
            Assert.Equal("sqlite-publish-modbus:main-plc", Assert.Single(pulledConfig.Data.Tasks).TaskKey);

            var targetConfig = await GetApiResultAsync<EdgeCollectionConfigurationPullResultDto>(client, $"/api/Edge/{token}/CollectionConfig/Pull");
            Assert.Equal((int)ApiCode.Success, targetConfig.Code);
            Assert.NotNull(targetConfig.Data);
            Assert.Equal(deviceId, targetConfig.Data!.GatewayId);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Id, targetConfig.Data.ConfigurationVersionId);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Version, targetConfig.Data.ConfigurationVersion);
            Assert.Equal(publishResult.Data.ConfigurationVersion.ConfigurationHash, targetConfig.Data.ConfigurationHash);
            Assert.NotNull(targetConfig.Data.Assignment);
            Assert.Equal(publishResult.Data.Assignment.Id, targetConfig.Data.Assignment!.Id);
            Assert.Equal(publishResult.Data.ConfigurationVersion.ConfigurationHash, targetConfig.Data.Assignment.ConfigurationHash);
            Assert.Equal("sqlite-publish-modbus:main-plc", Assert.Single(targetConfig.Data.Configuration.Tasks).TaskKey);

            var accepted = await client.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{token}/Accept", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                ReportedAt = DateTime.UtcNow
            });
            var acceptedResult = await ReadApiResultAsync<object>(accepted);
            Assert.Equal((int)ApiCode.Success, acceptedResult.Code);

            var wrongRunning = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                TargetType = publishResult.Data.Task.Address.TargetType,
                TargetKey = publishResult.Data.Task.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                Status = EdgeTaskStatus.Running,
                Progress = 10,
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["configurationVersion"] = publishResult.Data.ConfigurationVersion.Version,
                    ["configurationHash"] = "WRONG-HASH"
                }
            });
            var wrongRunningResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(wrongRunning);
            Assert.Equal((int)ApiCode.InValidData, wrongRunningResult.Code);

            var running = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                TargetType = publishResult.Data.Task.Address.TargetType,
                TargetKey = publishResult.Data.Task.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                Status = EdgeTaskStatus.Running,
                Progress = 50,
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["configurationVersion"] = publishResult.Data.ConfigurationVersion.Version,
                    ["configurationHash"] = publishResult.Data.ConfigurationVersion.ConfigurationHash
                }
            });
            var runningResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(running);
            Assert.Equal((int)ApiCode.Success, runningResult.Code);

            var metadataOnlySucceeded = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                TargetType = publishResult.Data.Task.Address.TargetType,
                TargetKey = publishResult.Data.Task.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                Status = EdgeTaskStatus.Succeeded,
                Progress = 100,
                ReportedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    ["configurationVersion"] = publishResult.Data.ConfigurationVersion.Version.ToString(),
                    ["configurationHash"] = publishResult.Data.ConfigurationVersion.ConfigurationHash
                }
            });
            var metadataOnlySucceededResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(metadataOnlySucceeded);
            Assert.Equal((int)ApiCode.InValidData, metadataOnlySucceededResult.Code);

            var failed = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = publishResult.Data.Task.TaskId,
                TargetType = publishResult.Data.Task.Address.TargetType,
                TargetKey = publishResult.Data.Task.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                Status = EdgeTaskStatus.Failed,
                Progress = 100,
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["configurationVersionId"] = publishResult.Data.ConfigurationVersion.Id,
                    ["configurationVersion"] = publishResult.Data.ConfigurationVersion.Version,
                    ["configurationHash"] = publishResult.Data.ConfigurationVersion.ConfigurationHash
                }
            });
            var failedResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(failed);
            Assert.Equal((int)ApiCode.Success, failedResult.Code);

            var failedVersionStatus = await GetApiResultAsync<EdgeCollectionVersionStatusDto>(client, $"/api/Edge/{deviceId}/CollectionVersionStatus");
            Assert.Equal((int)ApiCode.Success, failedVersionStatus.Code);
            Assert.NotNull(failedVersionStatus.Data);
            Assert.Equal(EdgeTaskStatus.Failed, failedVersionStatus.Data!.LastPublishStatus);
            Assert.True(failedVersionStatus.Data.HasDifference);

            var originalAudit = await GetApiResultAsync<List<EdgeTaskAuditLogDto>>(client, $"/api/EdgeTask/{publishResult.Data.Task.TaskId}/Audit");
            Assert.Equal((int)ApiCode.Success, originalAudit.Code);
            Assert.Contains(originalAudit.Data!, log => log.ActionName == "EdgeTaskTerminalReceipt" && log.ActionResult == EdgeTaskStatus.Failed.ToString());

            var retry = await client.PostAsJsonAsync($"/api/EdgeTask/{publishResult.Data.Task.TaskId}/Retry", new EdgeTaskRetryRequestDto
            {
                Reason = "sqlite retry after failed receipt",
                Metadata = new Dictionary<string, string>
                {
                    ["source"] = "sqlite-test"
                }
            });
            var retryResult = await ReadApiResultAsync<EdgeTaskRetryResultDto>(retry);
            Assert.Equal((int)ApiCode.Success, retryResult.Code);
            Assert.NotNull(retryResult.Data);
            var retryTask = retryResult.Data!.RetryTask;
            Assert.NotEqual(publishResult.Data.Task.TaskId, retryTask.TaskId);
            Assert.Equal(publishResult.Data.Task.TaskId.ToString("D"), retryTask.Metadata["retryOfTaskId"]);
            Assert.Equal(publishResult.Data.ConfigurationVersion.ConfigurationHash, retryTask.Parameters["configurationHash"].ToString());

            var retryAudit = await GetApiResultAsync<List<EdgeTaskAuditLogDto>>(client, $"/api/EdgeTask/{retryTask.TaskId}/Audit");
            Assert.Equal((int)ApiCode.Success, retryAudit.Code);
            Assert.Contains(retryAudit.Data!, log => log.ActionName == "EdgeTaskRetry" && log.ActionResult == EdgeTaskStatus.Pending.ToString());

            var retryPulledTasks = await GetApiResultAsync<List<EdgeTaskRequestDto>>(client, $"/api/EdgeTask/Dispatch/{token}");
            Assert.Equal((int)ApiCode.Success, retryPulledTasks.Code);
            Assert.Contains(retryPulledTasks.Data!, task => task.TaskId == retryTask.TaskId && task.TaskType == EdgeTaskType.ConfigPullRequest);

            var retryAccepted = await client.PostAsJsonAsync($"/api/EdgeTask/Dispatch/{token}/Accept", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = retryTask.TaskId,
                ReportedAt = DateTime.UtcNow
            });
            var retryAcceptedResult = await ReadApiResultAsync<object>(retryAccepted);
            Assert.Equal((int)ApiCode.Success, retryAcceptedResult.Code);

            var retryRunning = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = retryTask.TaskId,
                TargetType = retryTask.Address.TargetType,
                TargetKey = retryTask.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                Status = EdgeTaskStatus.Running,
                Progress = 75,
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["configurationVersion"] = publishResult.Data.ConfigurationVersion.Version,
                    ["configurationHash"] = publishResult.Data.ConfigurationVersion.ConfigurationHash
                }
            });
            var retryRunningResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(retryRunning);
            Assert.Equal((int)ApiCode.Success, retryRunningResult.Code);

            var retrySucceeded = await client.PostAsJsonAsync("/api/EdgeTask/Receipt", new EdgeTaskReceiptDto
            {
                ContractVersion = EdgeNodeContractVersions.EdgeTaskV1,
                TaskId = retryTask.TaskId,
                TargetType = retryTask.Address.TargetType,
                TargetKey = retryTask.Address.TargetKey,
                RuntimeType = EdgeRuntimeTypes.Gateway,
                Status = EdgeTaskStatus.Succeeded,
                Progress = 100,
                ReportedAt = DateTime.UtcNow,
                Result = new Dictionary<string, object>
                {
                    ["configurationVersionId"] = publishResult.Data.ConfigurationVersion.Id,
                    ["configurationVersion"] = publishResult.Data.ConfigurationVersion.Version,
                    ["configurationHash"] = publishResult.Data.ConfigurationVersion.ConfigurationHash
                }
            });
            var retrySucceededResult = await ReadApiResultAsync<EdgeTaskReceiptDto>(retrySucceeded);
            Assert.Equal((int)ApiCode.Success, retrySucceededResult.Code);

            using var scope = _fixture.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var storedTask = await dbContext.EdgeTasks.AsNoTracking().SingleAsync(task => task.Id == publishResult.Data.Task.TaskId);
            Assert.Equal(EdgeTaskStatus.Failed, storedTask.Status);
            Assert.Equal(EdgeTaskType.ConfigPullRequest, storedTask.TaskType);
            var storedRetryTask = await dbContext.EdgeTasks.AsNoTracking().SingleAsync(task => task.Id == retryTask.TaskId);
            Assert.Equal(EdgeTaskStatus.Succeeded, storedRetryTask.Status);
            Assert.Equal(EdgeTaskType.ConfigPullRequest, storedRetryTask.TaskType);
            using var parameters = JsonDocument.Parse(storedRetryTask.Parameters);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Version, parameters.RootElement.GetProperty("configurationVersion").GetInt32());
            Assert.Equal(publishResult.Data.ConfigurationVersion.ConfigurationHash, parameters.RootElement.GetProperty("configurationHash").GetString());

            var storedAssignment = await dbContext.EdgeCollectionAssignments.AsNoTracking().SingleAsync(assignment => assignment.Id == publishResult.Data.Assignment.Id);
            Assert.NotNull(storedAssignment.LastPulledAt);
            Assert.Equal(retryTask.TaskId, storedAssignment.LastExecutionTaskId);
            Assert.Equal(EdgeTaskStatus.Succeeded, storedAssignment.LastExecutionStatus);
            Assert.Equal(100, storedAssignment.LastExecutionProgress);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Version, storedAssignment.AppliedConfigurationVersion);
            Assert.Equal(publishResult.Data.ConfigurationVersion.ConfigurationHash, storedAssignment.AppliedConfigurationHash);
            Assert.NotNull(storedAssignment.AppliedAt);

            var versionStatus = await GetApiResultAsync<EdgeCollectionVersionStatusDto>(client, $"/api/Edge/{deviceId}/CollectionVersionStatus");
            Assert.Equal((int)ApiCode.Success, versionStatus.Code);
            Assert.NotNull(versionStatus.Data);
            Assert.Equal(publishResult.Data.Assignment.Id, versionStatus.Data!.AssignmentId);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Id, versionStatus.Data.TargetConfigurationVersionId);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Version, versionStatus.Data.TargetConfigurationVersion);
            Assert.Equal(publishResult.Data.ConfigurationVersion.Version, versionStatus.Data.CurrentConfigurationVersion);
            Assert.Equal(publishResult.Data.ConfigurationVersion.ConfigurationHash, versionStatus.Data.TargetConfigurationHash);
            Assert.Equal(publishResult.Data.ConfigurationVersion.ConfigurationHash, versionStatus.Data.CurrentConfigurationHash);
            Assert.False(versionStatus.Data.HasDifference);
            Assert.True(versionStatus.Data.IsTargetApplied);
            Assert.Equal(0, versionStatus.Data.VersionDelta);
            Assert.Equal(EdgeTaskStatus.Succeeded, versionStatus.Data.LastPublishStatus);
            Assert.Contains("一致", versionStatus.Data.DifferenceSummary);

            var edgeDetail = await GetApiResultAsync<EdgeNodeDto>(client, $"/api/Edge/{deviceId}");
            Assert.Equal((int)ApiCode.Success, edgeDetail.Code);
            Assert.NotNull(edgeDetail.Data);
            Assert.Equal(versionStatus.Data.TargetConfigurationVersion, edgeDetail.Data!.CollectionVersionStatus.TargetConfigurationVersion);
            Assert.Equal(versionStatus.Data.CurrentConfigurationVersion, edgeDetail.Data.CollectionVersionStatus.CurrentConfigurationVersion);

            var edgeList = await GetApiResultAsync<PagedData<EdgeNodeDto>>(client, "/api/Edge?offset=0&limit=100");
            Assert.Equal((int)ApiCode.Success, edgeList.Code);
            Assert.Contains(edgeList.Data!.rows, row =>
                row.GatewayId == deviceId &&
                row.CollectionVersionStatus.TargetConfigurationVersion == publishResult.Data.ConfigurationVersion.Version &&
                row.CollectionVersionStatus.IsTargetApplied);
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

        private static string GetParameterString(IReadOnlyDictionary<string, object> parameters, string key)
        {
            Assert.True(parameters.TryGetValue(key, out var value), $"Missing task parameter: {key}");
            return value switch
            {
                JsonElement { ValueKind: JsonValueKind.String } element => element.GetString()!,
                JsonElement element => element.GetRawText(),
                Guid id => id.ToString("D"),
                _ => value?.ToString() ?? string.Empty
            };
        }
    }
}
