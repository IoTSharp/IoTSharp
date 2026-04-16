#nullable enable

using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test
{
    public sealed class AppWithPostgreSqlTest : IClassFixture<PostgreSqlAppFixture>
    {
        private readonly PostgreSqlAppFixture _fixture;

        public AppWithPostgreSqlTest(PostgreSqlAppFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public Task AppIsInstalled() => _fixture.AssertAppIsInstalledAsync();

        [Fact]
        public Task AppAccountLogin() => _fixture.AssertAppAccountLoginAsync();

        [Fact]
        public Task AppDevicesCreate() => _fixture.AssertAppDevicesCreateAsync();
    }
}
