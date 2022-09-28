using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.EventBus
{
    public static class EventBusExtensions
    {

        public static void PublishAttributeData(this IPublisher cap, Device device, Dictionary<string, object> keyValues)
        {
            cap.PublishAttributeData(new PlayloadData() { DeviceId = device.Id, MsgBody = keyValues, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.AttributeData });
        }

        public static void PublishTelemetryData(this IPublisher cap, Device device, Dictionary<string, object> keyValues)
        {
            cap.PublishTelemetryData(new PlayloadData() { DeviceId = device.Id, MsgBody = keyValues, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
        }

        public static void PublishSubDeviceOnline(this IPublisher _queue, Guid _gatewaydevid, Device subdev)
        {
            //如果是_dev的子设备， 则更新状态。
            if (!subdev.Online && subdev.DeviceType == DeviceType.Device && subdev.Id != _gatewaydevid)
            {
                _queue.PublishDeviceStatus(subdev.Id, DeviceStatus.Good);
            }
        }

    }
}
