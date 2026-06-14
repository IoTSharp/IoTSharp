#nullable enable

using Xunit;

namespace IoTSharp.Test;

public sealed class AppWithPostgreSqlInfluxRabbitMongoTest
    : IoTSharpBusinessTestSuite<PostgreSqlInfluxRabbitMongoAppFixture>,
        IClassFixture<PostgreSqlInfluxRabbitMongoAppFixture>
{
    public AppWithPostgreSqlInfluxRabbitMongoTest(PostgreSqlInfluxRabbitMongoAppFixture fixture)
        : base(fixture)
    {
    }
}
