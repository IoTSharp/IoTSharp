#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IoTSharp.Contracts.Semantic;
using Xunit;

namespace IoTSharp.Test;

public sealed class SemanticCoreContractTests
{
    [Fact]
    public void SemanticCoreArtifacts_ExistAndAreParseable()
    {
        var contractsPath = Path.Combine(FindRepositoryRoot(), "IoTSharp.Contracts");
        var files = new[]
        {
            "semantic-core.v1.schema.json",
            Path.Combine("docs", "semantic-core-opcua-binding-v1.md"),
            Path.Combine("examples", "semantic-core.v1.sample.json"),
            Path.Combine("examples", "semantic-core.modbus-bindings.v1.sample.json"),
            Path.Combine("examples", "semantic-core.mqtt-uns-bindings.v1.sample.json"),
            Path.Combine("examples", "semantic-core.opcua-bindings.v1.sample.json"),
            Path.Combine("examples", "semantic-core.protocol-bindings.v1.sample.json")
        };

        foreach (var file in files)
        {
            var path = Path.Combine(contractsPath, file);
            Assert.True(File.Exists(path), $"Missing semantic-core artifact: {file}");

            if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
            }
        }

        var schema = File.ReadAllText(Path.Combine(contractsPath, "semantic-core.v1.schema.json"));
        Assert.Contains("\"$id\": \"https://iotsharp.net/schemas/semantic-core.v1.schema.json\"", schema);
        Assert.Contains("\"modbusTcp\"", schema);
        Assert.Contains("\"modbusRtu\"", schema);
        Assert.Contains("\"opcUa\"", schema);
        Assert.Contains("\"mqtt\"", schema);
    }

    [Theory]
    [MemberData(nameof(ValidSemanticCoreSamples))]
    public void SemanticCoreSamples_ValidateWithoutErrors(string fileName)
    {
        var path = Path.Combine(FindRepositoryRoot(), "IoTSharp.Contracts", "examples", fileName);
        var json = File.ReadAllText(path);
        var model = JsonSerializer.Deserialize<SemanticModel>(json, SemanticCoreJson.CreateOptions());

        Assert.NotNull(model);
        Assert.Equal(SemanticCoreJson.SchemaVersion, model!.SchemaVersion);

        var diagnostics = SemanticModelValidator.Validate(model);
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Severity == SemanticValidationSeverity.Error);
    }

    [Theory]
    [MemberData(nameof(InvalidSemanticCoreSamples))]
    public void SemanticCoreInvalidSamples_AreRejected(string fileName)
    {
        var path = Path.Combine(FindRepositoryRoot(), "IoTSharp.Contracts", "examples", "invalid", fileName);
        var json = File.ReadAllText(path);

        try
        {
            var model = JsonSerializer.Deserialize<SemanticModel>(json, SemanticCoreJson.CreateOptions());
            var diagnostics = SemanticModelValidator.Validate(model);
            Assert.Contains(diagnostics, diagnostic => diagnostic.Severity == SemanticValidationSeverity.Error);
        }
        catch (JsonException)
        {
            Assert.True(true);
        }
    }

    public static IEnumerable<object[]> ValidSemanticCoreSamples()
    {
        yield return ["semantic-core.v1.sample.json"];
        yield return ["semantic-core.modbus-bindings.v1.sample.json"];
        yield return ["semantic-core.mqtt-uns-bindings.v1.sample.json"];
        yield return ["semantic-core.opcua-bindings.v1.sample.json"];
        yield return ["semantic-core.protocol-bindings.v1.sample.json"];
    }

    public static IEnumerable<object[]> InvalidSemanticCoreSamples()
    {
        yield return ["semantic-core.asset-parent-cycle.invalid.json"];
        yield return ["semantic-core.external-reference-state.invalid.json"];
        yield return ["semantic-core.invalid-access.invalid.json"];
        yield return ["semantic-core.invalid-asset-path.invalid.json"];
        yield return ["semantic-core.missing-source.invalid.json"];
        yield return ["semantic-core.missing-unit.invalid.json"];
        yield return ["semantic-core.orphan-point.invalid.json"];
        yield return ["semantic-core.protocol-binding-missing-address.invalid.json"];
        yield return ["semantic-core.protocol-binding-secret.invalid.json"];
        yield return ["semantic-core.protocol-binding-unused.invalid.json"];
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var contractsProject = Path.Combine(directory.FullName, "IoTSharp.Contracts", "IoTSharp.Contracts.csproj");
            if (File.Exists(contractsProject))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Cannot locate IoTSharp repository root from test output path.");
    }
}
