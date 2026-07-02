#nullable enable

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Xunit;

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
        public async Task Produces_UpdatePersistsDefaultDeviceType()
        {
            using var client = _fixture.CreateClient();
            await _fixture.AuthorizeClientAsync(client);

            var produceName = $"product-type-{Guid.NewGuid():N}";
            var produceToken = $"pt-{Guid.NewGuid():N}";
            var saveProduce = await client.PostAsJsonAsync("/api/Produces/Save", new ProduceAddDto
            {
                Name = produceName,
                Description = "product default type update test",
                ProduceToken = produceToken,
                DefaultDeviceType = DeviceType.Gateway,
                DefaultIdentityType = IdentityType.ProduceToken,
                DefaultTimeout = 30,
                GatewayConfiguration = string.Empty
            });
            var saved = await ReadApiResultAsync<bool>(saveProduce);
            Assert.Equal((int)ApiCode.Success, saved.Code);

            var listed = await GetApiResultAsync<PagedData<ProduceDto>>(client,
                $"/api/Produces/List?offset=0&limit=10&name={Uri.EscapeDataString(produceName)}");
            var produce = Assert.Single(listed.Data!.rows, p => p.Name == produceName);

            var update = await client.PutAsJsonAsync("/api/Produces/Update", new ProduceAddDto
            {
                Id = produce.Id,
                Name = produceName,
                Description = "product default type update test",
                ProduceToken = produceToken,
                DefaultDeviceType = DeviceType.Device,
                DefaultIdentityType = IdentityType.ProduceToken,
                DefaultTimeout = 45,
                GatewayConfiguration = string.Empty
            });
            var updated = await ReadApiResultAsync<bool>(update);
            Assert.Equal((int)ApiCode.Success, updated.Code);
            Assert.True(updated.Data);

            var detail = await GetApiResultAsync<ProduceAddDto>(client, $"/api/Produces/Get?id={produce.Id}");
            Assert.Equal(DeviceType.Device, detail.Data!.DefaultDeviceType);
            Assert.Equal(45, detail.Data.DefaultTimeout);
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
