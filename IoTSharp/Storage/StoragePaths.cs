using System;
using System.IO;
using System.Runtime.InteropServices;

namespace IoTSharp.Storage
{
    public class StoragePaths
    {
        public StoragePaths()
        {
            BinPath = AppDomain.CurrentDomain.BaseDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                DataPath = Path.Combine(Environment.ExpandEnvironmentVariables("%appData%"), "Wirehome");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                DataPath = Path.Combine("/etc/wirehome");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {

                DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wirehome");
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public string BinPath { get; }

        public string DataPath { get; }
    }
}
