#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test;

[Collection(IntegrationTestCollectionNames.SqliteApplication)]
public sealed class OperationalEndpointTests
{
    private readonly SqliteAppFixture _fixture;

    public OperationalEndpointTests(SqliteAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HealthEndpoints_ReportLivenessAndDependencyReadiness()
    {
        using var client = _fixture.CreateClient();

        using var livenessResponse = await client.GetAsync("/healthz");
        using var readinessResponse = await client.GetAsync("/readyz");

        Assert.Equal(HttpStatusCode.OK, livenessResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readinessResponse.StatusCode);

        using var liveness = JsonDocument.Parse(await livenessResponse.Content.ReadAsStringAsync());
        using var readiness = JsonDocument.Parse(await readinessResponse.Content.ReadAsStringAsync());
        var liveEntries = liveness.RootElement.GetProperty("entries").EnumerateObject().Select(item => item.Name).ToArray();
        var readyEntries = readiness.RootElement.GetProperty("entries").EnumerateObject().Select(item => item.Name).ToArray();

        Assert.Equal(new[] { "self" }, liveEntries);
        Assert.Contains("ApplicationDatabase", readyEntries);
        Assert.Contains("TelemetryStorage", readyEntries);
    }

    [Theory]
    [InlineData("/api/Products/List?offset=0&limit=10")]
    [InlineData("/api/Asset/List?offset=0&limit=10")]
    [InlineData("/api/BlobStorage/List")]
    public async Task ManagementEndpoints_WhenAnonymous_ReturnUnauthorized(string path)
    {
        using var client = _fixture.CreateClient();

        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProductTokenIngestion_WhenAnonymous_RemainsAvailable()
    {
        using var client = _fixture.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/Products/missing-token/Telemetry",
            new Dictionary<string, object> { ["temperature"] = 20.5 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Cors_WhenOriginIsNotConfigured_DoesNotAllowCrossOriginRequest()
    {
        using var client = _fixture.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/healthz");
        request.Headers.Add("Origin", "https://untrusted.example");

        using var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
