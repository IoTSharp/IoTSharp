
using IoTSharp.Contracts;

namespace IoTSharp.EventBus
{
    public class DeviceConnectStatus
    {
        public Guid DeviceId { get; set; }
        public ConnectStatus ConnectStatus { get; set;  }
         public DeviceConnectStatus()
        {
            DeviceId = Guid.Empty;
        }
        public DeviceConnectStatus(Guid deviceId, ConnectStatus connectStatus)
        {
            DeviceId = deviceId;
            ConnectStatus = connectStatus;
        }

        public override bool Equals(object? obj)
        {
            return obj is DeviceConnectStatus other &&
                   DeviceId.Equals(other.DeviceId) &&
                   ConnectStatus == other.ConnectStatus;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DeviceId, ConnectStatus);
        }
    }
}
