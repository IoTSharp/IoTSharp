
using IoTSharp.Contracts;

namespace IoTSharp.EventBus
{
    public class DeviceActivityStatus
    {
        public Guid DeviceId { get; set; }
        public ActivityStatus Activity { get; set; }
        public DateTime EventTimeUtc { get; set; }
        public DeviceActivityStatus()
        {
            DeviceId = Guid.Empty;
            EventTimeUtc = DateTime.UtcNow;
        }
        public DeviceActivityStatus(Guid deviceId, ActivityStatus activity)
            : this(deviceId, activity, DateTime.UtcNow)
        {
        }

        public DeviceActivityStatus(Guid deviceId, ActivityStatus activity, DateTime eventTimeUtc)
        {
            DeviceId = deviceId;
            Activity = activity;
            EventTimeUtc = eventTimeUtc.Kind == DateTimeKind.Utc
                ? eventTimeUtc
                : eventTimeUtc.ToUniversalTime();
        }

        public override bool Equals(object? obj)
        {
            return obj is DeviceActivityStatus other &&
                   DeviceId.Equals(other.DeviceId) &&
                   Activity == other.Activity &&
                   EventTimeUtc.Equals(other.EventTimeUtc);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DeviceId, Activity, EventTimeUtc);
        }
    }
}
