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
