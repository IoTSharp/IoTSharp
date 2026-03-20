namespace IoTSharp.Agent;

internal static class AgentUrlResolver
{
    private const string DefaultProjectUrl = "http://localhost:5000/";

    public static string Resolve(IEnumerable<string> args)
    {
        var explicitUrl = args
            .Select(arg => arg?.Trim())
            .FirstOrDefault(arg => !string.IsNullOrWhiteSpace(arg) && arg.StartsWith("--url=", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(explicitUrl))
        {
            return Normalize(explicitUrl["--url=".Length..]);
        }

        var environmentUrl = Environment.GetEnvironmentVariable("IOTSHARP_AGENT_URL");
        return Normalize(environmentUrl);
    }

    private static string Normalize(string? candidate)
    {
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return uri.ToString();
        }

        return DefaultProjectUrl;
    }
}
