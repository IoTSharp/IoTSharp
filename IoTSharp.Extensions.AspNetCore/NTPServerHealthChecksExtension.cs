using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IoTSharp.Extensions.AspNetCore
{

    public static class NTPHealthChecksExtension
    {
        // 小端存储与大端存储的转换
        private static uint swapEndian(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
            ((x & 0x0000ff00) << 8) +
            ((x & 0x00ff0000) >> 8) +
            ((x & 0xff000000) >> 24));
        }
        public static DateTime getWebTime(string ntpServer)
        {
            // NTP message size - 16 bytes of the digest (RFC 2030)
            byte[] ntpData = new byte[48];
            // Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            IPAddress ip = IPAddress.Parse(ntpServer);

            // The UDP port number assigned to NTP is 123
            IPEndPoint ipEndPoint = new IPEndPoint(ip, 123);//addresses[0]

            // NTP uses UDP
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipEndPoint);
            // Stops code hang if NTP is blocked
            socket.ReceiveTimeout = 3000;
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();

            // Offset to get to the "Transmit Timestamp" field (time at which the reply 
            // departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;
            // Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            // Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);
            // Convert From big-endian to little-endian
            intPart = swapEndian(intPart);
            fractPart = swapEndian(fractPart);
            ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000UL);

            // UTC time
            DateTime webTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(milliseconds);
            string localTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            // Local time
            DateTime dt = webTime.ToLocalTime();
            return dt;
        }
        public static IHealthChecksBuilder AddNTPHealthCheck(this IHealthChecksBuilder builder, string ntpserver, double toleratesec = 10, string name = "NTPServer")
        {
            return builder.AddCheck(name, () =>
            {
                HealthCheckResult result;
                try
                {
                    var webtime = getWebTime(ntpserver);
                    var dt = DateTime.Now;
                    double ts = Math.Abs(dt.Subtract(webtime).TotalSeconds);
                    var hs = ts <= toleratesec ? HealthStatus.Healthy : (ts > toleratesec && ts < toleratesec * 2) ? HealthStatus.Degraded : HealthStatus.Unhealthy;
                    result = new HealthCheckResult(hs, description: hs != HealthStatus.Healthy ? $"NTP Server Date Time: {webtime}, Local Date Time:{dt}" : string.Empty);
                }
                catch (Exception ex)
                {

                    result = new HealthCheckResult(HealthStatus.Unhealthy, ex.Message, ex);
                }
                return result;
            });
        }
    }
}
