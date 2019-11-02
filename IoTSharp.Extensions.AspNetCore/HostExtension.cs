using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace IoTSharp.Extensions.AspNetCore
{
    public static class HostExtension
    {

        public static IHostBuilder ConfigureWindowsServices(this IHostBuilder hostBuilder)
        {
            bool IsWindowsService = false;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var process = GetParent(Process.GetCurrentProcess()))
                {
                    IsWindowsService = process != null && process.ProcessName == "services";
                }
            }
            if (Environment.CommandLine.Contains("--usebasedirectory") || (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && IsWindowsService))
            {
                hostBuilder.UseContentRoot(AppContext.BaseDirectory);
                System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                hostBuilder.UseWindowsService();
            }

            return hostBuilder;
        }

        public static IHostBuilder UseJsonToSettings(this IHostBuilder hostBuilder, string filename)
        {
            return hostBuilder.ConfigureAppConfiguration(builder =>
            {
                try
                {
                    if (System.IO.File.Exists(filename))
                    {
                        builder.AddJsonFile(filename, true);
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            });
        }

        /// Hashes an email with MD5.  Suitable for use with Gravatar profile
        /// image urls
        public static string Gravatar(this IdentityUser user)
        {
            string email = user.Email;
            // Create a new instance of the MD5CryptoServiceProvider object.  
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.  
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(email));

            // Create a new Stringbuilder to collect the bytes  
            // and create a string.  
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string.  
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return string.Format("http://www.gravatar.com/avatar/{0}", sBuilder.ToString()); ;  // Return the hexadecimal string. 
        }
        public static BadRequestObjectResult ExceptionRequest(this ControllerBase @base, Exception exception)
        {
            MethodBase mb = new StackTrace(exception).GetFrame(0).GetMethod();
            MethodBase cu = new StackTrace(true).GetFrame(0).GetMethod();
            return @base.BadRequest(new
            {
                exception.Message,
                exception.StackTrace,
                ExceptionMethod = mb.DeclaringType.FullName + "." + mb.Name,
                MethodName = cu.Name
            });
        }
        public static BadRequestObjectResult ExceptionRequest<T>(this ControllerBase @base, T code,string msg, Exception exception)
        {
            MethodBase mb = new StackTrace(exception).GetFrame(0).GetMethod();
            MethodBase cu = new StackTrace(true).GetFrame(0).GetMethod();
            return @base.BadRequest(new
            {
                code,
                msg,
                data = new
                {
                    ExceptionMethod = mb.DeclaringType.FullName + "." + mb.Name,
                    MethodName = cu.Name
                }
            });
        }
        public static BadRequestObjectResult ExceptionRequest(this ControllerBase @base, int code, string msg, Exception exception)
            => ExceptionRequest<int>(@base, code, msg, exception);


        public static IWebHostBuilder UseContentRootAsEnv(this IWebHostBuilder hostBuilder)
        {
            bool IsWindowsService = false;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var process = GetParent(Process.GetCurrentProcess()))
                {
                    IsWindowsService = process != null && process.ProcessName == "services";
                }
            }
            if (Environment.CommandLine.Contains("--usebasedirectory") || (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && IsWindowsService))
            {
                hostBuilder.UseContentRoot(AppContext.BaseDirectory);
            }
            else
            {
                if (!Debugger.IsAttached)
                {
                    hostBuilder.UseContentRoot(System.IO.Directory.GetCurrentDirectory());
                }
            }
            return hostBuilder;
        }
 

        private static Process GetParent(Process child)
        {
            var parentId = 0;

            var handle = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

            if (handle == IntPtr.Zero)
            {
                return null;
            }

            var processInfo = new PROCESSENTRY32
            {
                dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32))
            };

            if (!Process32First(handle, ref processInfo))
            {
                return null;
            }

            do
            {
                if (child.Id == processInfo.th32ProcessID)
                {
                    parentId = (int)processInfo.th32ParentProcessID;
                }
            } while (parentId == 0 && Process32Next(handle, ref processInfo));

            if (parentId > 0)
            {
                return Process.GetProcessById(parentId);
            }
            return null;
        }

        private static uint TH32CS_SNAPPROCESS = 2;

        [DllImport("kernel32.dll")]
        public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }
    }
}
