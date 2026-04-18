using IoTSharp.Controllers.Models;
using System;
using System.Collections.Generic;

namespace IoTSharp.Dtos
{
    public class EdgeRegistrationDto
    {
        public string RuntimeType { get; set; }
        public string RuntimeName { get; set; }
        public string Version { get; set; }
        public string InstanceId { get; set; }
        public string Platform { get; set; }
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class EdgeHeartbeatDto
    {
        public DateTime? Timestamp { get; set; }
        public string Status { get; set; }
        public bool? Healthy { get; set; }
        public long? UptimeSeconds { get; set; }
        public string IpAddress { get; set; }
        public Dictionary<string, object> Metrics { get; set; }
    }

    public class EdgeCapabilityReportDto
    {
        public string[] Protocols { get; set; }
        public string[] Features { get; set; }
        public string[] Tasks { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class EdgeRegistrationResultDto
    {
        public Guid DeviceId { get; set; }
        public string Name { get; set; }
        public string AccessToken { get; set; }
        public string ContractVersion { get; set; }
        public int Timeout { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    public class EdgeNodeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AccessToken { get; set; }
        public int Timeout { get; set; }
        public bool Active { get; set; }
        public DateTime? LastActivityDateTime { get; set; }
        public string RuntimeType { get; set; }
        public string RuntimeName { get; set; }
        public string Version { get; set; }
        public string InstanceId { get; set; }
        public string Platform { get; set; }
        public string HostName { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; }
        public bool? Healthy { get; set; }
        public long? UptimeSeconds { get; set; }
        public DateTime? LastHeartbeatDateTime { get; set; }
        public DateTime? LastRegistrationDateTime { get; set; }
        public string Capabilities { get; set; }
        public string Metadata { get; set; }
        public string Metrics { get; set; }
        public string LastTaskStatus { get; set; }
        public DateTime? LastReceiptDateTime { get; set; }
    }

    public class EdgeNodeQueryDto : QueryDto
    {
        public string RuntimeType { get; set; }
        public string Status { get; set; }
        public bool? Healthy { get; set; }
        public bool? Active { get; set; }
        public string Version { get; set; }
        public string Platform { get; set; }
        public string Sorter { get; set; }
        public string Sort { get; set; }
    }
}
