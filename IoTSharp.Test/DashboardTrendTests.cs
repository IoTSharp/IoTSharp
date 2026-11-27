using IoTSharp.Contracts;
using IoTSharp.Controllers;
using IoTSharp.Data;
using IoTSharp.Data.InMemory;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test;

public sealed class DashboardTrendTests
{
    private static readonly DateTime NowUtc = new(2026, 11, 25, 12, 30, 0, DateTimeKind.Utc);

    [Theory(Timeout = 15000)]
    [InlineData("24h", 60, 24)]
    [InlineData("7d", 360, 28)]
    public async Task RangeAggregatesTenantCountsAndRealSnapshots(string range, int bucketMinutes, int bucketCount)
    {
        using var services = CreateServices();
        await using var db = services.GetRequiredService<ApplicationDbContext>();
        var tenant = new Tenant();
        var otherTenant = new Tenant();
        db.Tenant.AddRange(tenant, otherTenant);
        var current = NowUtc.Date.AddHours(12);
        var previous = current.AddMinutes(-bucketMinutes);
        var start = current.AddMinutes(-(bucketCount - 1) * bucketMinutes);
        db.Set<DashboardTrendSample>().AddRange(
            new() { TenantId = tenant.Id, CollectorId = Guid.NewGuid(), StartUtc = previous, Received = 4, Telemetry = 2 },
            new() { TenantId = tenant.Id, CollectorId = Guid.NewGuid(), StartUtc = previous, Received = 6, Processed = 1, Attributes = 1 },
            new() { TenantId = tenant.Id, CollectorId = Guid.NewGuid(), StartUtc = previous.AddMinutes(2), OnlineDevices = 5, SnapshotUtc = previous.AddMinutes(2).AddSeconds(10) },
            new() { TenantId = tenant.Id, CollectorId = Guid.NewGuid(), StartUtc = previous.AddMinutes(3), OnlineDevices = 0, SnapshotUtc = previous.AddMinutes(3).AddSeconds(10) },
            new() { TenantId = tenant.Id, CollectorId = Guid.NewGuid(), StartUtc = current, Processed = 7, Attributes = 0 },
            new() { TenantId = tenant.Id, CollectorId = Guid.NewGuid(), StartUtc = start, Received = 0 },
            new() { TenantId = tenant.Id, CollectorId = Guid.NewGuid(), StartUtc = start.AddMinutes(-1), Received = 300 },
            new() { TenantId = tenant.Id, CollectorId = Guid.NewGuid(), StartUtc = NowUtc.AddMinutes(1), Received = 300 },
            new() { TenantId = otherTenant.Id, CollectorId = Guid.NewGuid(), StartUtc = previous, Received = 500, OnlineDevices = 80, SnapshotUtc = previous.AddSeconds(10) });
        await db.SaveChangesAsync();

        var data = await GetTrendAsync(db, tenant.Id, range);
        Assert.Equal((range, bucketMinutes, "ready"), (data.Range, data.BucketMinutes, data.State));
        Assert.Equal(bucketCount, data.Points.Count);
        Assert.Equal(new DashboardTrendPointDto(start, 0, null, null, null, null, true), data.Points[0]);
        Assert.Equal(new DashboardTrendPointDto(previous, 10, 1, 2, 1, 0, true), data.Points[^2]);
        Assert.Equal(new DashboardTrendPointDto(current, null, 7, null, 0, null, true), data.Points[^1]);
        Assert.Equal(DateTimeKind.Utc, data.Points[0].StartUtc.Kind);
        Assert.Null(data.Points[1].Received);
        Assert.Null(data.Points[1].OnlineDevices);
    }

    [Fact(Timeout = 15000)]
    public async Task CompleteIdleHourIsReadyAndZero()
    {
        using var services = CreateServices();
        await using var db = services.GetRequiredService<ApplicationDbContext>();
        var tenant = new Tenant();
        var collector = Guid.NewGuid();
        var hour = NowUtc.Date.AddHours(11);
        db.Tenant.Add(tenant);
        db.Set<DashboardTrendSample>().AddRange(Enumerable.Range(0, 60).Select(minute => new DashboardTrendSample
        {
            TenantId = tenant.Id,
            CollectorId = collector,
            StartUtc = hour.AddMinutes(minute),
            SnapshotUtc = hour.AddMinutes(minute).AddSeconds(10),
            Received = 0,
            Processed = 0,
            Telemetry = 0,
            Attributes = 0,
            OnlineDevices = 0
        }));
        await db.SaveChangesAsync();

        var data = await GetTrendAsync(db, tenant.Id, "24h");
        Assert.Equal("ready", data.State);
        Assert.Equal(new DashboardTrendPointDto(hour, 0, 0, 0, 0, 0, false), data.Points[^2]);
    }

    [Fact(Timeout = 15000)]
    public async Task MissingHistoryDistinguishesEmptyTenantAndUncollectedData()
    {
        using var services = CreateServices();
        await using var db = services.GetRequiredService<ApplicationDbContext>();
        var empty = new Tenant();
        var collecting = new Tenant();
        db.Tenant.AddRange(empty, collecting);
        db.Device.Add(new Device { TenantId = collecting.Id, Name = "device", DeviceType = DeviceType.Device });
        await db.SaveChangesAsync();

        Assert.Equal("empty", (await GetTrendAsync(db, empty.Id, "7d")).State);
        var missing = await GetTrendAsync(db, collecting.Id, "24h");
        Assert.Equal("collecting", missing.State);
        Assert.All(missing.Points, point => Assert.Null(point.OnlineDevices));

        db.Set<DashboardTrendSample>().Add(new DashboardTrendSample
        {
            TenantId = collecting.Id,
            CollectorId = Guid.NewGuid(),
            StartUtc = NowUtc.AddDays(-3),
            Received = 3
        });
        await db.SaveChangesAsync();
        Assert.Equal("empty", (await GetTrendAsync(db, collecting.Id, "24h")).State);
        Assert.Equal("ready", (await GetTrendAsync(db, collecting.Id, "7d")).State);
    }

    [Fact(Timeout = 15000)]
    public async Task RejectsBadRangeOrTenantAndSurfacesStorageFailure()
    {
        using var services = CreateServices();
        await using var db = services.GetRequiredService<ApplicationDbContext>();
        var valid = new Tenant();
        var deleted = new Tenant { Deleted = true };
        db.Tenant.AddRange(valid, deleted);
        await db.SaveChangesAsync();

        Assert.IsType<BadRequestObjectResult>((await Controller(db, valid.Id.ToString()).Trend("30d", CancellationToken.None)).Result);
        Assert.IsType<ForbidResult>((await Controller(db, null).Trend("24h", CancellationToken.None)).Result);
        Assert.IsType<ForbidResult>((await Controller(db, "invalid").Trend("24h", CancellationToken.None)).Result);
        Assert.IsType<ForbidResult>((await Controller(db, Guid.Empty.ToString()).Trend("24h", CancellationToken.None)).Result);
        Assert.IsType<ForbidResult>((await Controller(db, deleted.Id.ToString()).Trend("24h", CancellationToken.None)).Result);
        Assert.IsType<ForbidResult>((await Controller(db, Guid.NewGuid().ToString()).Trend("24h", CancellationToken.None)).Result);

        var controller = Controller(db, valid.Id.ToString());
        await db.DisposeAsync();
        var failure = Assert.IsType<ObjectResult>((await controller.Trend("24h", CancellationToken.None)).Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, failure.StatusCode);
    }

    [Fact(Timeout = 15000)]
    public async Task HttpTrendRequiresAuthenticationAndReturnsUncachedTenantData()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(DashboardTrendTests).Assembly.GetName().Name
        });
        builder.WebHost.UseTestServer();
        RegisterDatabase(builder.Services);
        builder.Services.AddAuthentication("TestTenant")
            .AddScheme<AuthenticationSchemeOptions, TestTenantAuthentication>("TestTenant", _ => { });
        builder.Services.AddControllers().AddApplicationPart(typeof(DashBoardController).Assembly);
        await using var app = builder.Build();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        using var startupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        await app.StartAsync(startupTimeout.Token);

        var tenant = new Tenant();
        var otherTenant = new Tenant();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Tenant.AddRange(tenant, otherTenant);
            var now = DateTime.UtcNow;
            var minute = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerMinute, DateTimeKind.Utc);
            db.Set<DashboardTrendSample>().Add(new DashboardTrendSample
            {
                TenantId = otherTenant.Id,
                CollectorId = Guid.NewGuid(),
                StartUtc = minute,
                Received = 17
            });
            await db.SaveChangesAsync();
        }

        using var client = app.GetTestClient();
        client.Timeout = TimeSpan.FromSeconds(8);
        var endpoint = $"/api/DashBoard/Trend?range=24h&tenantId={otherTenant.Id}";
        using var anonymous = await client.GetAsync(endpoint);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        using var forbiddenRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        forbiddenRequest.Headers.Add("X-Test-Tenant", "invalid");
        using var forbidden = await client.SendAsync(forbiddenRequest);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        using var validRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        validRequest.Headers.Add("X-Test-Tenant", tenant.Id.ToString());
        using var response = await client.SendAsync(validRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("empty", data.GetProperty("state").GetString());
        Assert.Equal(60, data.GetProperty("bucketMinutes").GetInt32());
        var points = data.GetProperty("points");
        Assert.Equal(24, points.GetArrayLength());
        Assert.Equal(JsonValueKind.Null, points[23].GetProperty("received").ValueKind);
        Assert.Equal(JsonValueKind.Null, points[23].GetProperty("onlineDevices").ValueKind);
        Assert.EndsWith("Z", points[23].GetProperty("startUtc").GetString());
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        RegisterDatabase(services);
        return services.BuildServiceProvider();
    }

    private static void RegisterDatabase(IServiceCollection services)
    {
        services.AddLogging();
        services.AddEntityFrameworkInMemoryDatabase();
        services.AddSingleton<IDataBaseModelBuilderOptions, InMemoryModelBuilderOptions>();
        var database = Guid.NewGuid().ToString();
        services.AddDbContext<ApplicationDbContext>((provider, options) =>
            options.UseInMemoryDatabase(database).UseInternalServiceProvider(provider));
    }

    private static DashBoardController Controller(ApplicationDbContext db, string tenantClaim)
    {
        var identity = new ClaimsIdentity("test");
        if (tenantClaim != null)
            identity.AddClaim(new Claim(IoTSharpClaimTypes.Tenant, tenantClaim));

        return new DashBoardController(db, NullLogger<DashBoardController>.Instance, new FixedClock())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } }
        };
    }

    private static async Task<DashboardTrendDto> GetTrendAsync(ApplicationDbContext db, Guid tenantId, string range)
    {
        var result = await Controller(db, tenantId.ToString()).Trend(range, CancellationToken.None);
        var payload = Assert.IsType<ApiResult<DashboardTrendDto>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal((int)ApiCode.Success, payload.Code);
        return payload.Data;
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => NowUtc;
    }

    private sealed class TestTenantAuthentication : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestTenantAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-Tenant", out var tenantClaim))
                return Task.FromResult(AuthenticateResult.NoResult());

            var identity = new ClaimsIdentity([new Claim(IoTSharpClaimTypes.Tenant, tenantClaim.ToString())], Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
