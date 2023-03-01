using System;
using System.Collections.Generic;
using IoTSharp.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.VirtualRoutes.Abstractions;

namespace IoTSharp.Data.Shardings.Routes
{
    public class TelemetryDataHourRoute:AbstractShardingTimeKeyDateTimeVirtualTableRoute<TelemetryData>
    {
        private readonly AppSettings _setting;

        public TelemetryDataHourRoute(IShardingProvider provider)
        {
            var options = provider.ApplicationServiceProvider.GetService<IOptions<AppSettings>>();
            _setting = options.Value;
        }
        public override void Configure(EntityMetadataTableBuilder<TelemetryData> builder)
        {
            builder.ShardingProperty(o => o.DateTime);
        }

        public override Func<string, bool> GetRouteToFilter(DateTime shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = TimeFormatToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.GreaterThan:
                case ShardingOperatorEnum.GreaterThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) >= 0;
                case ShardingOperatorEnum.LessThan:
                {
                    //处于临界值 o=>o.time < [2021-01-01 01:00:00] 尾巴2021010101不应该被返回
                    if (shardingKey.Minute==0&&shardingKey.Second==0)
                        return tail => String.Compare(tail, t, StringComparison.Ordinal) < 0;
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                }
                case ShardingOperatorEnum.LessThanOrEqual:
                    return tail => String.Compare(tail, t, StringComparison.Ordinal) <= 0;
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
#if DEBUG
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");
#endif
                    return tail => true;
                }
            }
        }

        protected override List<string> CalcTailsOnStart()
        {
            var beginTime = GetBeginTime().Date;

            var tails = new List<string>();
            //提前创建表
            var nowTimeStamp = DateTime.UtcNow.Date;
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("begin time error");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var tail = ShardingKeyToTail(currentTimeStamp);
                tails.Add(tail);
                currentTimeStamp = currentTimeStamp.AddDays(1);
            }

            return tails;
        }

        public DateTime GetBeginTime()
        {
            return _setting.ShardingBeginTime;
        }

        public override bool AutoCreateTableByTime()
        {
            return true;
        }

        public override string[] GetCronExpressions()
        {
            return new[]
            {
                "0 */1 * * * ?",
            };
        }

        protected override string TimeFormatToTail(DateTime time)
        {
            return $"{time:yyyyMMdd}";
        }
    }
}