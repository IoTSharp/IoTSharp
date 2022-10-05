using IoTSharp.Contracts;

namespace IoTSharp.EventBus
{
    public class DeviceActivityStatus
    {
        public Guid DeviceId { get; }
        public ActivityStatus Activity { get; }

        public DeviceActivityStatus(Guid deviceId, ActivityStatus activity)
        {
            DeviceId = deviceId;
            Activity = activity;
        }

        public override bool Equals(object? obj)
        {
            return obj is DeviceActivityStatus other &&
                   DeviceId.Equals(other.DeviceId) &&
                   Activity == other.Activity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DeviceId, Activity);
        }
    }
}
