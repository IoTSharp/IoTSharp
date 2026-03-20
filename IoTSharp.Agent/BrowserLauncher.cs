using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IoTSharp.Agent;

internal static class BrowserLauncher
{
    public static void Open(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", EscapeArgument(url));
            return;
        }

        Process.Start("xdg-open", EscapeArgument(url));
    }

    private static string EscapeArgument(string value)
    {
        return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }
}
