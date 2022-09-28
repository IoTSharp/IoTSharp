using System;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.VirtualRoutes.Years;

namespace IoTSharp.Data.Shardings.Routes
{
    public class TelemetryDataYearRoute:AbstractSimpleShardingYearKeyDateTimeVirtualTableRoute<TelemetryData>
    {
        public override void Configure(EntityMetadataTableBuilder<TelemetryData> builder)
        {
            builder.ShardingProperty(o => o.DateTime);
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override DateTime GetBeginTime()
        {
            //原则上不应该使用datetime.now但是我看你之前的程序是这么写的如果是now那么每次启动都算是开始时间之前的表可能会访问不到
            //因为访问只会访问begintime之后的时间
            return DateTime.Now;
        }
    }
}