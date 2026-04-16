#nullable enable

using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test
{
    public sealed class ApiContractTests : IClassFixture<SqliteAppFixture>
    {
        private readonly SqliteAppFixture _fixture;

        public ApiContractTests(SqliteAppFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task OpenApi_Documents_All_Controller_Api_Endpoints()
        {
            using var client = _fixture.CreateClient();
            using var document = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
            var openApiOperations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject())
            {
                foreach (var operation in path.Value.EnumerateObject())
                {
                    openApiOperations.Add($"{operation.Name.ToUpperInvariant()} {path.Name}");
                }
            }

            var routeOperations = _fixture.Services
                .GetRequiredService<EndpointDataSource>()
                .Endpoints
                .OfType<RouteEndpoint>()
                .Where(endpoint => endpoint.RoutePattern.RawText is not null)
                .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not null)
                .SelectMany(endpoint => endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Select(method =>
                    $"{method.ToUpperInvariant()} /{NormalizeRoute(endpoint.RoutePattern)}") ?? Enumerable.Empty<string>())
                .Where(route => route.Contains(" /api/", StringComparison.OrdinalIgnoreCase))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missing = routeOperations.Where(route => !openApiOperations.Contains(route)).OrderBy(route => route).ToArray();
            Assert.True(missing.Length == 0, string.Join(Environment.NewLine, missing));
        }

        private static string NormalizeRoute(RoutePattern pattern)
        {
            var rawText = pattern.RawText ?? string.Empty;
            return rawText.Replace("{*", "{");
        }
    }
}