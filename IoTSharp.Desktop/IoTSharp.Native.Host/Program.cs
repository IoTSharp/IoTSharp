using WebWindows.Blazor;
using System;

namespace IoTSharp.Native.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ComponentsDesktop.Run<Startup>("IoTSharp.Desktop", "wwwroot/index.html");
        }
    }
}
