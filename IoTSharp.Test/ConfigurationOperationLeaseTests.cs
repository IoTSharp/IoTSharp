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
using System.Net;
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
            var embeddedQualification = new ConfigurationOperationLeaseService(context, qualifySonnetDb: true);
            Assert.Null(await embeddedQualification.TryAcquireAsync(Guid.NewGuid(), "test", cancellation.Token));
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
        public Task TemplateAndReleaseCompetition_RejectsOverlappingConfigurationTask() => ValidateTemplateAndReleaseCompetitionAsync(_fixture);

        internal static async Task ValidateTemplateAndReleaseCompetitionAsync(AppInstance fixture)
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            using var releaseClient = fixture.CreateClient();
            using var templateClient = fixture.CreateClient();
            var gatewayId = await CreateGatewayWithConfigAsync(fixture, releaseClient, cancellation.Token);
            await fixture.AuthorizeClientAsync(templateClient);
            var templateId = await CreateTemplateAsync(templateClient, cancellation.Token);
            Guid versionId;
            using (var scope = fixture.Services.CreateScope())
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

            using var verifyScope = fixture.Services.CreateScope();
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
        public Task RollbackAndTemplateCompetition_KeepOneTargetAndRejectSupersededRetry() => ValidateRollbackAndTemplateCompetitionAsync(_fixture);

        internal static async Task ValidateRollbackAndTemplateCompetitionAsync(AppInstance fixture)
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            using var rollbackClient = fixture.CreateClient();
            using var templateClient = fixture.CreateClient();
            var gatewayId = await CreateGatewayWithConfigAsync(fixture, rollbackClient, cancellation.Token);
            await fixture.AuthorizeClientAsync(templateClient);
            var templateId = await CreateTemplateAsync(templateClient, cancellation.Token);
            using var secondSave = await rollbackClient.PutAsJsonAsync($"/api/Edge/{gatewayId}/CollectionConfig",
                new EdgeCollectionConfigurationUpdateDto(), cancellation.Token);
            Assert.Equal((int)ApiCode.Success, (await ReadAsync<EdgeCollectionConfigurationDto>(secondSave, cancellation.Token)).Code);
            Guid baselineId;
            Guid forwardId;
            using (var scope = fixture.Services.CreateScope())
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
            using (var scope = fixture.Services.CreateScope())
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
            using (var verifyScope = fixture.Services.CreateScope())
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
        public Task PersistedLease_RejectsBusyAllowsExpiryAndRollsBackAbandonedOperation() => ValidatePersistedLeaseAsync(_fixture);

        internal static async Task ValidatePersistedLeaseAsync(AppInstance fixture)
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var client = fixture.CreateClient();
            var gatewayId = await CreateGatewayWithConfigAsync(fixture, client, cancellation.Token);
            var staleLeaseId = Guid.NewGuid();
            using (var seedScope = fixture.Services.CreateScope())
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
            using (var expireScope = fixture.Services.CreateScope())
            {
                var context = expireScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.EdgeNodes.Where(item => item.GatewayId == gatewayId).ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.ConfigurationOperationExpiresAt, DateTime.UtcNow.AddMinutes(-1)), cancellation.Token);
            }

            using (var abandonedScope = fixture.Services.CreateScope())
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

            using (var verifyScope = fixture.Services.CreateScope())
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

        internal static async Task<Guid> CreateGatewayWithConfigAsync(AppInstance fixture, HttpClient client,
            CancellationToken cancellationToken)
        {
            var created = await fixture.CreateDeviceAsync(client, $"config-lease-{Guid.NewGuid():N}", DeviceType.Gateway);
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

        private static async Task<Guid> CreateTemplateAsync(HttpClient client, CancellationToken cancellationToken)
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


    /// <summary>
    /// 在远程 SonnetDB 上验证平台的配置操作和租约状态。
    /// </summary>
    public sealed class SonnetDbConfigurationOperationLeaseTests : IClassFixture<SonnetDbLeaseAppFixture>
    {
        private readonly SonnetDbLeaseAppFixture _fixture;

        /// <summary>使用租约测试专属的临时容器和 IoTSharp 宿主。</summary>
        public SonnetDbConfigurationOperationLeaseTests(SonnetDbLeaseAppFixture fixture) => _fixture = fixture;

        /// <summary>验证模板发布和 Release Center 共享同一 Gateway 的在途任务限制。</summary>
        [SonnetDbLeaseQualificationFact]
        public Task TemplateAndReleaseCompetition_RejectsOverlappingConfigurationTask()
            => ConfigurationOperationLeaseTests.ValidateTemplateAndReleaseCompetitionAsync(_fixture);

        /// <summary>验证回滚、模板发布与过时重试仍遵守同一配置目标。</summary>
        [SonnetDbLeaseQualificationFact]
        public Task RollbackAndTemplateCompetition_KeepOneTargetAndRejectSupersededRetry()
            => ConfigurationOperationLeaseTests.ValidateRollbackAndTemplateCompetitionAsync(_fixture);

        /// <summary>验证忙碌与过期租约，并回读放弃写入后的业务数据。</summary>
        [SonnetDbLeaseQualificationFact]
        public Task PersistedLease_RejectsBusyAllowsExpiryAndRollsBackAbandonedOperation()
            => ConfigurationOperationLeaseTests.ValidatePersistedLeaseAsync(_fixture);

        /// <summary>保持竞争事务打开时拒绝同一 Gateway，批次失败时撤销较早的目标。</summary>
        [SonnetDbLeaseQualificationFact]
        public async Task CompetingScope_RejectsBatchAndRollsBackEarlierTarget()
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            var token = cancellation.Token;
            using var client = _fixture.CreateClient();
            var first = await ConfigurationOperationLeaseTests.CreateGatewayWithConfigAsync(_fixture, client, token);
            var second = await ConfigurationOperationLeaseTests.CreateGatewayWithConfigAsync(_fixture, client, token);
            var ids = new[] { first, second }.OrderBy(id => id).ToArray();

            using (var holderScope = _fixture.Services.CreateScope())
            using (var contenderScope = _fixture.Services.CreateScope())
            {
                var holder = holderScope.ServiceProvider.GetRequiredService<ConfigurationOperationLeaseService>();
                var contender = contenderScope.ServiceProvider.GetRequiredService<ConfigurationOperationLeaseService>();
                await using var lease = await holder.TryAcquireAsync(ids[1], "first-instance", token);
                Assert.NotNull(lease);
                Assert.Null(await contender.TryAcquireManyAsync(ids, "second-instance", token));
            }

            using var verifier = _fixture.Services.CreateScope();
            var service = verifier.ServiceProvider.GetRequiredService<ConfigurationOperationLeaseService>();
            await using var batch = await service.TryAcquireManyAsync(ids, "next-instance", token);
            Assert.NotNull(batch);
            await batch.CompleteAsync(token);
            var context = verifier.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var nodes = await context.EdgeNodes.Where(node => node.GatewayId == first || node.GatewayId == second).ToListAsync(token);
            Assert.Equal(2, nodes.Count);
            Assert.All(nodes, node =>
            {
                Assert.Null(node.ConfigurationOperationId);
                Assert.Null(node.ConfigurationOperationExpiresAt);
                Assert.Null(node.ConfigurationOperationOwner);
            });
        }

        /// <summary>验证不同租约标识无法释放，即使已暂存其它业务更改也会回滚。</summary>
        [SonnetDbLeaseQualificationFact]
        public async Task WrongOwner_RollsBackBusinessWriteAndLeaseRelease()
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            var token = cancellation.Token;
            using var client = _fixture.CreateClient();
            var gateway = await ConfigurationOperationLeaseTests.CreateGatewayWithConfigAsync(_fixture, client, token);
            using (var owner = _fixture.Services.CreateScope())
            {
                var service = owner.ServiceProvider.GetRequiredService<ConfigurationOperationLeaseService>();
                var context = owner.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await using var lease = await service.TryAcquireAsync(gateway, "expected-owner", token);
                Assert.NotNull(lease);
                var assignment = await context.EdgeCollectionAssignments.SingleAsync(item => item.GatewayId == gateway, token);
                assignment.Status = EdgeCollectionAssignmentStatus.Superseded;
                await context.SaveChangesAsync(token);
                var foreignLeaseId = Guid.NewGuid();
                Assert.Equal(1, await context.EdgeNodes.Where(node => node.GatewayId == gateway)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(node => node.ConfigurationOperationId, foreignLeaseId)
                        .SetProperty(node => node.ConfigurationOperationOwner, "foreign-owner"), token));
                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => lease.CompleteAsync(token));
            }

            using var verifier = _fixture.Services.CreateScope();
            var stored = verifier.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(EdgeCollectionAssignmentStatus.Active, await stored.EdgeCollectionAssignments.Where(item => item.GatewayId == gateway)
                .Select(item => item.Status).SingleAsync(token));
            Assert.Null(await stored.EdgeNodes.Where(node => node.GatewayId == gateway)
                .Select(node => node.ConfigurationOperationId).SingleAsync(token));
        }
        /// <summary>在真实 HTTP 配置保存进入提交阶段时注入失败并从新上下文核对业务状态。</summary>
        [SonnetDbLeaseQualificationFact]
        public async Task CommitStageFailure_LeavesNoConfigurationVersionOrAssignment()
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            var token = deadline.Token;
            using var client = _fixture.CreateClient();
            var gateway = await ConfigurationOperationLeaseTests.CreateGatewayWithConfigAsync(_fixture, client, token);
            var sourceId = $"lease-fault-{Guid.NewGuid():N}";
            _fixture.CommitFailure.Arm(sourceId);

            using var response = await client.PutAsJsonAsync($"/api/Edge/{gateway}/CollectionConfig",
                new EdgeCollectionConfigurationUpdateDto { SourceId = sourceId }, token);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.True(_fixture.CommitFailure.Triggered);

            using var verifier = _fixture.Services.CreateScope();
            var context = verifier.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var version = Assert.Single(await context.CollectionConfigurationVersions
                .Where(item => item.GatewayId == gateway).ToListAsync(token));
            Assert.Equal(1, version.Version);
            var assignment = Assert.Single(await context.EdgeCollectionAssignments
                .Where(item => item.GatewayId == gateway).ToListAsync(token));
            Assert.Equal(version.Id, assignment.CollectionConfigurationVersionId);
            Assert.Equal(EdgeCollectionAssignmentStatus.Active, assignment.Status);
            Assert.Equal(1L, await context.Set<AttributeLatest>()
                .Where(item => item.DeviceId == gateway && item.KeyName == Constants._EdgeCollectionConfigVersion)
                .Select(item => item.Value_Long).SingleAsync(token));
            Assert.Null(await context.EdgeNodes.Where(item => item.GatewayId == gateway)
                .Select(item => item.ConfigurationOperationId).SingleAsync(token));
        }

        /// <summary>数据库确认提交后让 HTTP 写请求失败，并用调用方编号查询唯一的持久版本。</summary>
        [SonnetDbLeaseQualificationFact]
        public async Task CommittedBeforeHttpFailure_CanReconcileByCallerSourceId()
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(40));
            var token = deadline.Token;
            using var client = _fixture.CreateClient();
            var gateway = await ConfigurationOperationLeaseTests.CreateGatewayWithConfigAsync(_fixture, client, token);
            var sourceId = $"lease-committed-{Guid.NewGuid():N}";
            _fixture.CommitFailure.Arm(sourceId, afterCommit: true);

            using var response = await client.PutAsJsonAsync($"/api/Edge/{gateway}/CollectionConfig",
                new EdgeCollectionConfigurationUpdateDto { SourceId = sourceId }, token);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.True(_fixture.CommitFailure.Triggered);

            using var reconciler = _fixture.CreateClient();
            await _fixture.AuthorizeClientAsync(reconciler);
            var history = await reconciler.GetFromJsonAsync<ApiResult<PagedData<CollectionConfigurationVersionDto>>>(
                $"/api/Edge/{gateway}/CollectionConfigVersions?sourceId={Uri.EscapeDataString(sourceId)}", token);
            Assert.Equal((int)ApiCode.Success, history!.Code);
            var version = Assert.Single(history.Data!.rows);
            Assert.Equal(2, version.Version);
            Assert.Equal(sourceId, version.SourceId);

            using var verifier = _fixture.Services.CreateScope();
            var context = verifier.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var assignment = Assert.Single(await context.EdgeCollectionAssignments.Where(item => item.GatewayId == gateway
                && item.Status == EdgeCollectionAssignmentStatus.Active).ToListAsync(token));
            Assert.Equal(version.Id, assignment.CollectionConfigurationVersionId);
            Assert.Null(await context.EdgeNodes.Where(item => item.GatewayId == gateway)
                .Select(item => item.ConfigurationOperationId).SingleAsync(token));
        }
    }

    /// <summary>为租约用例创建单独的业务库，避免影响其它测试对设备列表的断言。</summary>
    public sealed class SonnetDbLeaseAppFixture : SonnetDbAppFixture
    {
        internal ConfigurationCommitFailureInterceptor CommitFailure { get; } = new();

        /// <inheritdoc />
        protected override void ConfigureLeaseQualificationServices(IServiceCollection services)
        {
            base.ConfigureLeaseQualificationServices(services);
            services.ConfigureDbContext<ApplicationDbContext>(options => options.AddInterceptors(CommitFailure));
        }

        internal sealed class ConfigurationCommitFailureInterceptor : DbTransactionInterceptor
        {
            private CommitFault? _fault;
            private bool _triggered;
            internal bool Triggered => Volatile.Read(ref _triggered);

            internal void Arm(string sourceId, bool afterCommit = false)
            {
                Volatile.Write(ref _triggered, false);
                Volatile.Write(ref _fault, new CommitFault(sourceId, afterCommit));
            }

            /// <inheritdoc />
            public override ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction,
                TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
            {
                FailIfArmed(eventData, afterCommit: false);
                return new ValueTask<InterceptionResult>(result);
            }

            /// <inheritdoc />
            public override Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData,
                CancellationToken cancellationToken = default)
            {
                FailIfArmed(eventData, afterCommit: true);
                return Task.CompletedTask;
            }

            private void FailIfArmed(TransactionEventData eventData, bool afterCommit)
            {
                var fault = Volatile.Read(ref _fault);
                if (fault is { } && fault.AfterCommit == afterCommit && eventData.Context is ApplicationDbContext context
                    && context.ChangeTracker.Entries<CollectionConfigurationVersion>()
                        .Any(entry => entry.Entity.SourceId == fault.SourceId)
                    && Interlocked.CompareExchange(ref _fault, null, fault) == fault)
                {
                    Volatile.Write(ref _triggered, true);
                    throw new InvalidOperationException(afterCommit
                        ? "Injected failure after committing the configuration transaction"
                        : "Injected failure before committing the configuration transaction");
                }
            }

            private sealed record CommitFault(string SourceId, bool AfterCommit);
        }
    }

    /// <summary>将远程租约候选验证限定在显式配置的集成测试宿主。</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SonnetDbLeaseQualificationFactAttribute : FactAttribute
    {
        /// <summary>仅在启用候选 provider 的测试运行中选择用例。</summary>
        public SonnetDbLeaseQualificationFactAttribute()
        {
            if (Environment.GetEnvironmentVariable(SonnetDbAppFixture.ConfigurationLeaseQualificationVariable) != "1")
            {
                Skip = $"Set {SonnetDbAppFixture.ConfigurationLeaseQualificationVariable}=1 to qualify the SonnetDB lease contract";
            }
        }
    }
}
