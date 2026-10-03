#nullable enable

using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Data.MySQL;
using IoTSharp.Data.SonnetDB;
using IoTSharp.Dtos;
using IoTSharp.Models;
using IoTSharp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SonnetDB.EntityFrameworkCore.Extensions;

namespace IoTSharp.Test
{
    [Collection(IntegrationTestCollectionNames.SqliteApplication)]
    public sealed class ConfigurationOperationLeaseTests
    {
        private readonly SqliteAppFixture _fixture;

        /// <summary>
        /// 复用本地 SQLite 应用测试夹具。
        /// </summary>
        /// <param name="fixture">本地应用测试夹具。</param>
        public ConfigurationOperationLeaseTests(SqliteAppFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// 验证当前 Microting 包实际暴露的 provider 名称与配置互斥支持边界一致。
        /// </summary>
        [Fact(Timeout = 10000)]
        public async Task MySqlProviderName_MatchesSupportedRuntimeFactory()
        {
            await Task.Yield();
            using var context = new MySqlDesignTimeApplicationDbContextFactory().CreateDbContext([]);
            Assert.Equal("Pomelo.EntityFrameworkCore.MySql", context.Database.ProviderName);
            var service = new ConfigurationOperationLeaseService(context);
            Assert.Equal("Gateway configuration operation is already in progress", service.UnavailableReason);
            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        /// <summary>
        /// 验证 SonnetDB 在打开连接或执行事务前拒绝配置互斥，不把缓冲事务当作行锁。
        /// </summary>
        [Fact(Timeout = 10000)]
        public async Task SonnetDbLease_FailsClosedBeforeAnyConnectionOrCommand()
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var interceptor = new RejectConnectionOpeningInterceptor();
            var services = new ServiceCollection();
            services.AddSingleton<IDataBaseModelBuilderOptions>(new SonnetDbModelBuilderOptions());
            services.AddDbContext<ApplicationDbContext>(options => options
                .UseSonnetDB("Data Source=not-opened-config-lease-test")
                .AddInterceptors(interceptor));
            using var provider = services.BuildServiceProvider(validateScopes: true);
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var service = new ConfigurationOperationLeaseService(context);
            Assert.Null(await service.TryAcquireAsync(Guid.NewGuid(), "test", cancellation.Token));
            Assert.Contains("not supported", service.UnavailableReason, StringComparison.Ordinal);
            Assert.Equal(0, interceptor.ConnectionOpeningCount);
            Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
        }

        /// <summary>
        /// 验证独立请求并发保存配置后版本号不重复，且仅保留一个当前目标。
        /// </summary>
        [Fact]
        public async Task ConcurrentDirectSaves_KeepOneActiveAssignmentAndUniqueVersions()
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var firstClient = _fixture.CreateClient();
            using var secondClient = _fixture.CreateClient();
            var created = await _fixture.CreateDeviceAsync(firstClient, $"config-lease-first-save-{Guid.NewGuid():N}", DeviceType.Gateway);
            var gatewayId = created.Data!.Id;
            await _fixture.AuthorizeClientAsync(secondClient);

            var responses = await Task.WhenAll(
                Task.Run(() => firstClient.PutAsJsonAsync($"/api/Edge/{gatewayId}/CollectionConfig", new EdgeCollectionConfigurationUpdateDto(), cancellation.Token)),
                Task.Run(() => secondClient.PutAsJsonAsync($"/api/Edge/{gatewayId}/CollectionConfig", new EdgeCollectionConfigurationUpdateDto(), cancellation.Token)))
                .WaitAsync(TimeSpan.FromSeconds(20), cancellation.Token);
            foreach (var response in responses)
            {
                using (response)
                {
                    var result = await ReadAsync<EdgeCollectionConfigurationDto>(response, cancellation.Token);
                    Assert.Equal((int)ApiCode.Success, result.Code);
                }
            }

            using var scope = _fixture.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var versions = await context.CollectionConfigurationVersions.Where(item => item.GatewayId == gatewayId)
                .OrderBy(item => item.Version).ToListAsync(cancellation.Token);
            Assert.Equal(new[] { 1, 2 }, versions.Select(item => item.Version));
            var active = Assert.Single(await context.EdgeCollectionAssignments.Where(item => item.GatewayId == gatewayId
                && item.Status == EdgeCollectionAssignmentStatus.Active).ToListAsync(cancellation.Token));
            Assert.Equal(versions[^1].Id, active.CollectionConfigurationVersionId);
            var node = await context.EdgeNodes.SingleAsync(item => item.GatewayId == gatewayId, cancellation.Token);
            Assert.Null(node.ConfigurationOperationId);
        }

        /// <summary>
        /// 验证模板发布与 Release Center 竞争同一 Gateway 时，仅创建一个在途配置任务。
        /// </summary>
        [Fact]
        public async Task TemplateAndReleaseCompetition_RejectsOverlappingConfigurationTask()
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            using var releaseClient = _fixture.CreateClient();
            using var templateClient = _fixture.CreateClient();
            var gatewayId = await CreateGatewayWithConfigAsync(releaseClient, cancellation.Token);
            await _fixture.AuthorizeClientAsync(templateClient);
            var templateId = await CreateTemplateAsync(templateClient, cancellation.Token);
            Guid versionId;
            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                versionId = await context.CollectionConfigurationVersions.Where(item => item.GatewayId == gatewayId)
                    .Select(item => item.Id).SingleAsync(cancellation.Token);
            }

            var releaseRequest = CreatePlanRequest(gatewayId, versionId);
            var releaseCall = Task.Run(() => releaseClient.PostAsJsonAsync("/api/ReleaseCenter/Plans", releaseRequest, cancellation.Token));
            var templateCall = Task.Run(() => templateClient.PostAsJsonAsync($"/api/CollectionTemplates/{templateId}/PublishConfig",
                new CollectionTemplateConfigurationPublishRequestDto { EdgeNodeId = gatewayId }, cancellation.Token));
            await Task.WhenAll(releaseCall, templateCall).WaitAsync(TimeSpan.FromSeconds(25), cancellation.Token);
            using var releaseResponse = await releaseCall;
            using var templateResponse = await templateCall;
            var release = await ReadAsync<ReleasePlanOperationResultDto>(releaseResponse, cancellation.Token);
            var template = await ReadAsync<CollectionTemplateConfigurationPublishResultDto>(templateResponse, cancellation.Token);
            Assert.Equal(1, new[] { release.Code, template.Code }.Count(code => code == (int)ApiCode.Success));
            Assert.Equal(1, new[] { release.Code, template.Code }.Count(code => code == (int)ApiCode.InValidData));

            using var verifyScope = _fixture.Services.CreateScope();
            var verify = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var task = Assert.Single(await verify.EdgeTasks.Where(item => item.GatewayId == gatewayId
                && item.TaskType == EdgeTaskType.ConfigPullRequest).ToListAsync(cancellation.Token));
            var active = Assert.Single(await verify.EdgeCollectionAssignments.Where(item => item.GatewayId == gatewayId
                && item.Status == EdgeCollectionAssignmentStatus.Active).ToListAsync(cancellation.Token));
            Assert.Equal(task.Id, active.LastExecutionTaskId);

            using var saveResponse = await releaseClient.PutAsJsonAsync($"/api/Edge/{gatewayId}/CollectionConfig",
                new EdgeCollectionConfigurationUpdateDto(), cancellation.Token);
            Assert.Equal((int)ApiCode.InValidData, (await ReadAsync<EdgeCollectionConfigurationDto>(saveResponse, cancellation.Token)).Code);
            using var dispatchResponse = await releaseClient.PostAsJsonAsync("/api/EdgeTask/Dispatch", new EdgeTaskRequestDto
            {
                TaskId = Guid.NewGuid(),
                TaskType = EdgeTaskType.ConfigPullRequest,
                Address = new EdgeTaskAddressDto { DeviceId = gatewayId, TargetKey = gatewayId.ToString("D") }
            }, cancellation.Token);
            Assert.Equal((int)ApiCode.InValidData, (await ReadAsync<EdgeTaskRequestDto>(dispatchResponse, cancellation.Token)).Code);
        }

        /// <summary>
        /// 验证回滚和模板新发布竞争时只产生一个目标，旧前向任务不能再重试覆盖新目标。
        /// </summary>
        [Fact]
        public async Task RollbackAndTemplateCompetition_KeepOneTargetAndRejectSupersededRetry()
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            using var rollbackClient = _fixture.CreateClient();
            using var templateClient = _fixture.CreateClient();
            var gatewayId = await CreateGatewayWithConfigAsync(rollbackClient, cancellation.Token);
            await _fixture.AuthorizeClientAsync(templateClient);
            var templateId = await CreateTemplateAsync(templateClient, cancellation.Token);
            using var secondSave = await rollbackClient.PutAsJsonAsync($"/api/Edge/{gatewayId}/CollectionConfig",
                new EdgeCollectionConfigurationUpdateDto(), cancellation.Token);
            Assert.Equal((int)ApiCode.Success, (await ReadAsync<EdgeCollectionConfigurationDto>(secondSave, cancellation.Token)).Code);
            Guid baselineId;
            Guid forwardId;
            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var versions = await context.CollectionConfigurationVersions.Where(item => item.GatewayId == gatewayId)
                    .OrderBy(item => item.Version).ToListAsync(cancellation.Token);
                baselineId = versions[0].Id;
                forwardId = versions[1].Id;
            }

            using var createResponse = await rollbackClient.PostAsJsonAsync("/api/ReleaseCenter/Plans",
                CreatePlanRequest(gatewayId, forwardId), cancellation.Token);
            var created = await ReadAsync<ReleasePlanOperationResultDto>(createResponse, cancellation.Token);
            Assert.Equal((int)ApiCode.Success, created.Code);
            var planId = created.Data!.Plan.Id;
            var forwardTaskId = Assert.Single(created.Data.EdgeTasks).TaskId;
            // 只准备本地数据库终态边界，测试不声称执行端已经应用前向配置。
            using (var scope = _fixture.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.EdgeTasks.Where(item => item.Id == forwardTaskId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Status, EdgeTaskStatus.Succeeded), cancellation.Token);
                await context.ReleaseTasks.Where(item => item.PlanId == planId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Status, ReleaseTaskStatus.Succeeded), cancellation.Token);
                await context.ReleasePlans.Where(item => item.Id == planId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Status, ReleasePlanStatus.Succeeded), cancellation.Token);
            }

            var rollbackCall = Task.Run(() => rollbackClient.PostAsJsonAsync($"/api/ReleaseCenter/Plans/{planId}/Rollback",
                new ReleasePlanActionRequestDto { RollbackConfigurationVersionId = baselineId }, cancellation.Token));
            var templateCall = Task.Run(() => templateClient.PostAsJsonAsync($"/api/CollectionTemplates/{templateId}/PublishConfig",
                new CollectionTemplateConfigurationPublishRequestDto { EdgeNodeId = gatewayId }, cancellation.Token));
            await Task.WhenAll(rollbackCall, templateCall).WaitAsync(TimeSpan.FromSeconds(25), cancellation.Token);
            using var rollbackResponse = await rollbackCall;
            using var templateResponse = await templateCall;
            var rollback = await ReadAsync<ReleasePlanOperationResultDto>(rollbackResponse, cancellation.Token);
            var template = await ReadAsync<CollectionTemplateConfigurationPublishResultDto>(templateResponse, cancellation.Token);
            Assert.Equal(1, new[] { rollback.Code, template.Code }.Count(code => code == (int)ApiCode.Success));
            Assert.Equal(1, new[] { rollback.Code, template.Code }.Count(code => code == (int)ApiCode.InValidData));
            var expectedVersionId = rollback.Code == (int)ApiCode.Success ? baselineId : template.Data!.ConfigurationVersion.Id;
            using (var verifyScope = _fixture.Services.CreateScope())
            {
                var context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var active = Assert.Single(await context.EdgeCollectionAssignments.Where(item => item.GatewayId == gatewayId
                    && item.Status == EdgeCollectionAssignmentStatus.Active).ToListAsync(cancellation.Token));
                Assert.Equal(expectedVersionId, active.CollectionConfigurationVersionId);
                Assert.Equal(2, await context.EdgeTasks.CountAsync(item => item.GatewayId == gatewayId, cancellation.Token));
                Assert.Equal(1, await context.EdgeTasks.CountAsync(item => item.GatewayId == gatewayId
                    && item.Status == EdgeTaskStatus.Pending, cancellation.Token));
                await context.EdgeTasks.Where(item => item.GatewayId == gatewayId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Status, EdgeTaskStatus.Succeeded), cancellation.Token);
                await context.EdgeTasks.Where(item => item.Id == forwardTaskId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Status, EdgeTaskStatus.Failed), cancellation.Token);
            }

            using var retryResponse = await rollbackClient.PostAsJsonAsync($"/api/EdgeTask/{forwardTaskId}/Retry",
                new EdgeTaskRetryRequestDto(), cancellation.Token);
            var retry = await ReadAsync<EdgeTaskRetryResultDto>(retryResponse, cancellation.Token);
            Assert.Equal((int)ApiCode.InValidData, retry.Code);
            Assert.Contains("superseded", retry.Msg, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 验证已持久化的未过期租约拒绝写入，过期租约可接管，未提交事务恢复原状态。
        /// </summary>
        [Fact]
        public async Task PersistedLease_RejectsBusyAllowsExpiryAndRollsBackAbandonedOperation()
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var client = _fixture.CreateClient();
            var gatewayId = await CreateGatewayWithConfigAsync(client, cancellation.Token);
            var staleLeaseId = Guid.NewGuid();
            using (var seedScope = _fixture.Services.CreateScope())
            {
                var context = seedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.EdgeNodes.Where(item => item.GatewayId == gatewayId).ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.ConfigurationOperationId, staleLeaseId)
                    .SetProperty(item => item.ConfigurationOperationExpiresAt, DateTime.UtcNow.AddMinutes(1))
                    .SetProperty(item => item.ConfigurationOperationOwner, "previous-instance"), cancellation.Token);
            }

            using var busyResponse = await client.PutAsJsonAsync($"/api/Edge/{gatewayId}/CollectionConfig",
                new EdgeCollectionConfigurationUpdateDto(), cancellation.Token);
            Assert.Equal((int)ApiCode.InValidData, (await ReadAsync<EdgeCollectionConfigurationDto>(busyResponse, cancellation.Token)).Code);
            using (var expireScope = _fixture.Services.CreateScope())
            {
                var context = expireScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.EdgeNodes.Where(item => item.GatewayId == gatewayId).ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.ConfigurationOperationExpiresAt, DateTime.UtcNow.AddMinutes(-1)), cancellation.Token);
            }

            using (var abandonedScope = _fixture.Services.CreateScope())
            {
                var context = abandonedScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var service = abandonedScope.ServiceProvider.GetRequiredService<ConfigurationOperationLeaseService>();
                await using var lease = await service.TryAcquireAsync(gatewayId, "abandoned-operation", cancellation.Token);
                Assert.NotNull(lease);
                var assignment = await context.EdgeCollectionAssignments.SingleAsync(item => item.GatewayId == gatewayId
                    && item.Status == EdgeCollectionAssignmentStatus.Active, cancellation.Token);
                assignment.Status = EdgeCollectionAssignmentStatus.Superseded;
                await context.SaveChangesAsync(cancellation.Token);
            }

            using (var verifyScope = _fixture.Services.CreateScope())
            {
                var context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var node = await context.EdgeNodes.SingleAsync(item => item.GatewayId == gatewayId, cancellation.Token);
                Assert.Equal(staleLeaseId, node.ConfigurationOperationId);
                Assert.Equal(1, await context.EdgeCollectionAssignments.CountAsync(item => item.GatewayId == gatewayId
                    && item.Status == EdgeCollectionAssignmentStatus.Active, cancellation.Token));
            }

            using var acquiredResponse = await client.PutAsJsonAsync($"/api/Edge/{gatewayId}/CollectionConfig",
                new EdgeCollectionConfigurationUpdateDto(), cancellation.Token);
            Assert.Equal((int)ApiCode.Success, (await ReadAsync<EdgeCollectionConfigurationDto>(acquiredResponse, cancellation.Token)).Code);
        }

        private async Task<Guid> CreateGatewayWithConfigAsync(HttpClient client, CancellationToken cancellationToken)
        {
            var created = await _fixture.CreateDeviceAsync(client, $"config-lease-{Guid.NewGuid():N}", DeviceType.Gateway);
            var gatewayId = created.Data!.Id;
            using var response = await client.PutAsJsonAsync($"/api/Edge/{gatewayId}/CollectionConfig",
                new EdgeCollectionConfigurationUpdateDto(), cancellationToken);
            Assert.Equal((int)ApiCode.Success, (await ReadAsync<EdgeCollectionConfigurationDto>(response, cancellationToken)).Code);
            return gatewayId;
        }

        private static ReleasePlanCreateRequestDto CreatePlanRequest(Guid gatewayId, Guid versionId)
            => new()
            {
                Name = $"config-lease-{Guid.NewGuid():N}",
                PlanType = ReleasePlanType.ConfigurationRollout,
                ConfigurationVersionId = versionId,
                AutoStart = true,
                ConfirmationPolicy = ReleaseConfirmationPolicy.None,
                Targets = [new ReleaseTargetDto { TargetType = ReleaseTargetType.Gateway, TargetId = gatewayId }]
            };

        private async Task<Guid> CreateTemplateAsync(HttpClient client, CancellationToken cancellationToken)
        {
            var name = $"config-lease-product-{Guid.NewGuid():N}";
            using var saved = await client.PostAsJsonAsync("/api/Products/Save", new ProductAddDto
            {
                Name = name,
                ProductToken = Guid.NewGuid().ToString("N"),
                DefaultDeviceType = DeviceType.Gateway,
                DefaultIdentityType = IdentityType.ProductToken
            }, cancellationToken);
            Assert.Equal((int)ApiCode.Success, (await ReadAsync<bool>(saved, cancellationToken)).Code);
            using var listed = await client.GetAsync($"/api/Products/List?offset=0&limit=10&name={name}", cancellationToken);
            var product = Assert.Single((await ReadAsync<PagedData<ProductDto>>(listed, cancellationToken)).Data!.rows);
            using var response = await client.PostAsJsonAsync("/api/CollectionTemplates", new CollectionTemplateUpsertDto
            {
                ProductId = product.Id,
                TemplateKey = name,
                Name = name,
                Status = CollectionTemplateStatus.Active,
                Enabled = true,
                Protocol = new ProtocolTemplateDto { Protocol = CollectionProtocolType.Modbus, ProtocolKind = "modbusTcp" },
                Connections = [new ConnectionTemplateDto { ConnectionKey = "plc", ConnectionName = "PLC", Transport = "tcp", Host = "127.0.0.1", Port = 1502 }],
                Points = [new PointTemplateDto
                {
                    ConnectionKey = "plc", PointKey = "temperature", SemanticId = "semantic.temperature", BindingId = "binding.temperature",
                    Name = "Temperature", SourceType = "holding-register", Address = "40001", RawValueType = "Int16",
                    ValueType = CollectionValueType.Double, Length = 1,
                    SamplingPolicy = new SamplingPolicyTemplateDto { ReadPeriodMs = 1000 },
                    Mapping = new MappingPolicyTemplateDto { TargetType = CollectionTargetType.Telemetry, TargetName = "temperature", ValueType = CollectionValueType.Double }
                }]
            }, cancellationToken);
            var result = await ReadAsync<CollectionTemplateDto>(response, cancellationToken);
            Assert.Equal((int)ApiCode.Success, result.Code);
            return result.Data!.Id;
        }

        private static async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ApiResult<T>>(cancellationToken))!;
        }

        private sealed class RejectConnectionOpeningInterceptor : DbConnectionInterceptor
        {
            internal int ConnectionOpeningCount { get; private set; }

            /// <inheritdoc />
            public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData,
                InterceptionResult result)
            {
                ConnectionOpeningCount++;
                throw new InvalidOperationException("Fail-closed provider must not open a database connection");
            }

            /// <inheritdoc />
            public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection,
                ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
            {
                ConnectionOpeningCount++;
                throw new InvalidOperationException("Fail-closed provider must not open a database connection");
            }
        }
    }
}
