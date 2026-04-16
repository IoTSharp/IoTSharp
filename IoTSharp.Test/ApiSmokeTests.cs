#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IoTSharp.Dtos;
using Xunit;

namespace IoTSharp.Test
{
    public sealed class ApiSmokeTests : IClassFixture<SqliteAppFixture>
    {
        private readonly SqliteAppFixture _fixture;

        public ApiSmokeTests(SqliteAppFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task All_Documented_Api_Endpoints_Do_Not_Return_Server_Error()
        {
            using var discoveryClient = _fixture.CreateClient();
            using var document = JsonDocument.Parse(await discoveryClient.GetStringAsync("/swagger/v1/swagger.json"));
            var failures = new List<string>();

            foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject())
            {
                foreach (var operation in path.Value.EnumerateObject())
                {
                    using var client = _fixture.CreateClient();
                    await _fixture.AuthorizeClientAsync(client);

                    using var request = await BuildRequestAsync(path.Name, operation.Name, operation.Value);
                    using var response = await client.SendAsync(request);

                    if ((int)response.StatusCode >= 500)
                    {
                        failures.Add($"{operation.Name.ToUpperInvariant()} {path.Name} => {(int)response.StatusCode} {response.StatusCode}");
                    }
                }
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }

        private async Task<HttpRequestMessage> BuildRequestAsync(string path, string method, JsonElement operation)
        {
            var request = new HttpRequestMessage(new HttpMethod(method.ToUpperInvariant()), BuildUrl(path, operation));

            switch (method.ToUpperInvariant())
            {
                case "POST":
                case "PUT":
                case "PATCH":
                case "DELETE":
                    var content = await BuildRequestBodyAsync(path, method);
                    if (content is not null)
                    {
                        request.Content = content;
                    }
                    break;
            }

            return request;
        }

        private async Task<HttpContent?> BuildRequestBodyAsync(string path, string method)
        {
            if (path.Equals("/api/Installer/Install", StringComparison.OrdinalIgnoreCase))
            {
                return JsonContent.Create(new InstallDto
                {
                    CustomerEMail = "customer@iotsharp.net",
                    CustomerName = "customer_test",
                    Email = "admin@iotsharp.net",
                    Password = "P@ssw0rd",
                    PhoneNumber = "18900000000",
                    TenantEMail = "tenant@iotsharp.net",
                    TenantName = "tenant_test"
                });
            }

            if (path.Equals("/api/Account/Login", StringComparison.OrdinalIgnoreCase))
            {
                using var client = _fixture.CreateClient();
                var captchaClientId = Guid.NewGuid().ToString("N");
                await client.GetAsync($"/api/Captcha/Index?clientid={captchaClientId}");

                return JsonContent.Create(new
                {
                    userName = "admin@iotsharp.net",
                    password = "P@ssw0rd",
                    captchaClientId,
                    captchaMove = 100
                });
            }

            if (path.Contains("/api/Devices", StringComparison.OrdinalIgnoreCase) && method.Equals("post", StringComparison.OrdinalIgnoreCase))
            {
                return JsonContent.Create(new
                {
                    deviceType = 0,
                    name = $"smoke-{Guid.NewGuid():N}",
                    timeout = 30
                });
            }

            if (path.Equals("/api/Captcha/Vertify", StringComparison.OrdinalIgnoreCase))
            {
                return JsonContent.Create(new { move = 100, action = Array.Empty<int>() });
            }

            return new StringContent("{}", Encoding.UTF8, "application/json");
        }

        private static string BuildUrl(string path, JsonElement operation)
        {
            var url = path;
            if (operation.TryGetProperty("parameters", out var parameters))
            {
                var query = new List<string>();
                foreach (var parameter in parameters.EnumerateArray())
                {
                    var name = parameter.GetProperty("name").GetString() ?? string.Empty;
                    var value = GetSampleValue(name);
                    var location = parameter.GetProperty("in").GetString();

                    if (string.Equals(location, "path", StringComparison.OrdinalIgnoreCase))
                    {
                        url = url.Replace($"{{{name}}}", Uri.EscapeDataString(value), StringComparison.OrdinalIgnoreCase);
                    }
                    else if (string.Equals(location, "query", StringComparison.OrdinalIgnoreCase))
                    {
                        query.Add($"{name}={Uri.EscapeDataString(value)}");
                    }
                }

                if (query.Count > 0)
                {
                    url = $"{url}?{string.Join("&", query)}";
                }
            }

            return url;
        }

        private static string GetSampleValue(string name)
        {
            if (name.Contains("access_token", StringComparison.OrdinalIgnoreCase))
            {
                return "smoke-token";
            }

            if (name.Contains("id", StringComparison.OrdinalIgnoreCase))
            {
                return Guid.Empty.ToString();
            }

            if (name.Contains("keys", StringComparison.OrdinalIgnoreCase))
            {
                return "temperature";
            }

            if (name.Contains("begin", StringComparison.OrdinalIgnoreCase))
            {
                return DateTime.UtcNow.AddHours(-1).ToString("O");
            }

            if (name.Contains("end", StringComparison.OrdinalIgnoreCase))
            {
                return DateTime.UtcNow.ToString("O");
            }

            if (name.Contains("format", StringComparison.OrdinalIgnoreCase) || name.Contains("fromat", StringComparison.OrdinalIgnoreCase))
            {
                return "json";
            }

            if (name.Contains("dataSide", StringComparison.OrdinalIgnoreCase))
            {
                return "server";
            }

            if (name.Contains("clientid", StringComparison.OrdinalIgnoreCase))
            {
                return Guid.NewGuid().ToString("N");
            }

            return "test";
        }
    }
}