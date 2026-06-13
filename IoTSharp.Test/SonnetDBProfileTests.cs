using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using IoTSharp.Contracts;
using Xunit;

namespace IoTSharp.Test;

public sealed class SonnetDBProfileTests
{
    [Fact]
    public void AppsettingsSonnetDB_EnablesAllSonnetDBBackends()
    {
        var root = LocateRepositoryRoot();
        var profilePath = Path.Combine(root, "appsettings.SonnetDB.json");

        Assert.True(File.Exists(profilePath), "appsettings.SonnetDB.json must exist so ASPNETCORE_ENVIRONMENT=SonnetDB can select it.");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(profilePath), options);

        Assert.NotNull(settings);
        Assert.Equal(DataBaseType.SonnetDB, settings!.DataBase);
        Assert.Equal(TelemetryStorage.SonnetDB, settings.TelemetryStorage);
        Assert.Equal(CachingUseIn.SonnetDB, settings.CachingUseIn);
        Assert.Equal(EventBusStore.InMemory, settings.EventBusStore);
        Assert.Equal(EventBusMQ.InMemory, settings.EventBusMQ);

        Assert.NotNull(settings.ConnectionStrings);
        Assert.StartsWith("Data Source=", settings.ConnectionStrings!["IoTSharp"], StringComparison.Ordinal);
        Assert.Contains("TelemetryData", settings.ConnectionStrings["TelemetryStorage"], StringComparison.Ordinal);
        Assert.StartsWith("sonnetdb://", settings.ConnectionStrings["BlobStorage"], StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("Data Source=", settings.CachingUseSonnetDBConnectionString, StringComparison.Ordinal);
        Assert.DoesNotContain("Password=", File.ReadAllText(profilePath), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DockerComposeSonnetDBProfile_IsParallelToDefaultCompose()
    {
        var root = LocateRepositoryRoot();
        var defaultCompose = File.ReadAllText(Path.Combine(root, "docker-compose.yml"));
        var sonnetComposePath = Path.Combine(root, "docker-compose.sonnetdb.yml");

        Assert.Contains("pgsql:", defaultCompose, StringComparison.Ordinal);
        Assert.Contains("appsettings.Development.json", defaultCompose, StringComparison.Ordinal);
        Assert.True(File.Exists(sonnetComposePath), "docker-compose.sonnetdb.yml must provide the rollback-friendly SonnetDB profile instead of changing the default stack.");

        var sonnetCompose = File.ReadAllText(sonnetComposePath);

        Assert.Contains("ASPNETCORE_ENVIRONMENT: SonnetDB", sonnetCompose, StringComparison.Ordinal);
        Assert.Contains("sonnetdb:", sonnetCompose, StringComparison.Ordinal);
        Assert.Contains("ConnectionStrings__IoTSharp: Data Source=sonnetdb+http://sonnetdb:5080/iotsharp", sonnetCompose, StringComparison.Ordinal);
        Assert.Contains("ConnectionStrings__TelemetryStorage: Data Source=sonnetdb+http://sonnetdb:5080/telemetry", sonnetCompose, StringComparison.Ordinal);
        Assert.Contains("CachingUseSonnetDBConnectionString: Data Source=sonnetdb+http://sonnetdb:5080/cache", sonnetCompose, StringComparison.Ordinal);
        Assert.Contains("ConnectionStrings__BlobStorage: sonnetdb://blob", sonnetCompose, StringComparison.Ordinal);
        Assert.Contains("appsettings.SonnetDB.json:/app/appsettings.SonnetDB.json:ro", sonnetCompose, StringComparison.Ordinal);
    }

    private static string LocateRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "IoTSharp.slnx"))
                && Directory.Exists(Path.Combine(directory.FullName, "IoTSharp")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the IoTSharp repository root.");
    }
}
