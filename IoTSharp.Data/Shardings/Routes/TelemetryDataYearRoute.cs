using System;
using IoTSharp.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.VirtualRoutes.Years;

namespace IoTSharp.Data.Shardings.Routes
{
    public class TelemetryDataYearRoute:AbstractSimpleShardingYearKeyDateTimeVirtualTableRoute<TelemetryData>
    {
        private readonly AppSettings _setting;

        public TelemetryDataYearRoute(IShardingProvider provider)
        {
            var options = provider.ApplicationServiceProvider.GetService<IOptions<AppSettings>>();
            _setting = options.Value;
        }
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
            return _setting.ShardingBeginTime;
        }
    }
}