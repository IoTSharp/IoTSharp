using IoTSharp.Contracts;
using IoTSharp.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.EventBus
{
    /// <summary>
    /// 消息发布扩展类
    /// </summary>
    public static class EventBusPublisher
    {
        /// <summary>
        /// 将指定字典发布到指定设备中
        /// </summary>
        /// <param name="_queue"></param>
        /// <param name="device"></param>
        /// <param name="keyValues"></param>
        public static void PublishAttributeData(this IPublisher _queue, Device device, Dictionary<string, object> keyValues)
        {
            _queue.PublishAttributeData(new PlayloadData() { DeviceId = device.Id, MsgBody = keyValues, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.AttributeData });
        }
        /// <summary>
        /// 发布属性数据
        /// </summary>
        /// <param name="_queue"></param>
        /// <param name="device"></param>
        /// <param name="_data"></param>
        public static void PublishAttributeData(this IPublisher _queue, Device device, Action<PlayloadData> _data)
        {
            var dat = new PlayloadData() { DeviceId = device.Id, MsgBody = new Dictionary<string, object>(), DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.AttributeData };
            _data?.Invoke(dat);
            _queue.PublishAttributeData(dat);
        }
        /// <summary>
        /// 发布字典到遥测数据
        /// </summary>
        /// <param name="_queue"></param>
        /// <param name="device"></param>
        /// <param name="keyValues"></param>
        public static void PublishTelemetryData(this IPublisher _queue, Device device, Dictionary<string, object> keyValues)
        {
            _queue.PublishTelemetryData(new PlayloadData() { DeviceId = device.Id, MsgBody = keyValues, DataSide = DataSide.ClientSide, DataCatalog = DataCatalog.TelemetryData });
        }

    }
}
