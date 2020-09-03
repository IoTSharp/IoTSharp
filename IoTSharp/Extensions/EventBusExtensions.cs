using DotNetCore.CAP;
using Dynamitey;
using IoTSharp.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Extensions
{
    public static class EventBusExtensions
    {

        public static void PublishAttributeData(this ICapPublisher cap, RawMsg msg)
        {
            cap.Publish("iotsharp.services.datastream.attributedata", msg);
        }
        public static void PublishTelemetryData(this ICapPublisher cap, RawMsg msg)
        {
            cap.Publish("iotsharp.services.datastream.telemetrydata", msg);
        }

    }
}
