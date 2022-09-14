using DotNetCore.CAP;
using Dynamitey;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.Dtos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    public static class EventBusExtensions
    {

        public static void PublishAttributeData(this ICapPublisher cap, PlayloadData msg)
        {
            cap.Publish("iotsharp.services.datastream.attributedata", msg);
        }

        public static void PublishAttributeData(this ICapPublisher cap, Device device, Dictionary<string, object> keyValues)
        {
            cap.PublishAttributeData(new PlayloadData() { DeviceId = device.Id, MsgBody = keyValues, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.AttributeData });
        }

        public static void PublishTelemetryData(this ICapPublisher cap, PlayloadData msg)
        {
            cap.Publish("iotsharp.services.datastream.telemetrydata", msg);
        }
        public static void PublishTelemetryData(this ICapPublisher cap,  Device device, Dictionary<string, object> keyValues)
        {
            cap.PublishTelemetryData(new PlayloadData() { DeviceId = device.Id, MsgBody = keyValues, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
        }

        public static void PublishDeviceStatus(this ICapPublisher cap,  Guid  devid , DeviceStatus devicestatus)
        {
            cap.Publish("iotsharp.services.datastream.devicestatus",  new  PlayloadData {  DeviceId=devid,  DeviceStatus= devicestatus }  );
        }
        public static void PublishSubDeviceOnline(this ICapPublisher _queue, Guid _gatewaydevid,  Device subdev)
        {
            //如果是_dev的子设备， 则更新状态。
            if (!subdev.Online &&  subdev.DeviceType == DeviceType.Device && subdev.Id != _gatewaydevid)
            {
                _queue.PublishDeviceStatus(subdev.Id, DeviceStatus.Good);
            }
        }
        public static void PublishDeviceAlarm(this ICapPublisher cap, CreateAlarmDto alarmDto)
        {
            cap.Publish("iotsharp.services.datastream.alarm", alarmDto);
        }
    }
}
