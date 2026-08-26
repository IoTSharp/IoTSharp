using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace IoTSharp.Test;

/// <summary>
/// 集成测试共享宿主的集合名称。
/// </summary>
public static class IntegrationTestCollectionNames
{
    public const string SqliteApplication = "SqliteApplication";

    public const string SonnetDbApplication = "SonnetDbApplication";
}

/// <summary>
/// 在所有 SQLite 集成测试之间复用一套应用宿主，避免重复创建 EF Core 内部服务提供器。
/// </summary>
[CollectionDefinition(IntegrationTestCollectionNames.SqliteApplication, DisableParallelization = true)]
public sealed class SqliteApplicationCollection : ICollectionFixture<SqliteAppFixture>
{
}

/// <summary>
/// 在所有 SonnetDB 集成测试之间复用一套应用宿主和数据库容器。
/// </summary>
[CollectionDefinition(IntegrationTestCollectionNames.SonnetDbApplication, DisableParallelization = true)]
public sealed class SonnetDbApplicationCollection : ICollectionFixture<SonnetDbAppFixture>
{
}
