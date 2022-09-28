using System;
using System.Collections.Generic;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.VirtualRoutes.Abstractions;

namespace IoTSharp.Data.Shardings.Routes
{
    public class TelemetryDataMinuteRoute:AbstractShardingTimeKeyDateTimeVirtualTableRoute<TelemetryData>
    {
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
                    //处于临界值 o=>o.time < [2021-01-01 01:01:00] 尾巴202101010101不应该被返回
                    if (shardingKey.Second==0)
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
            var nowTimeStamp = DateTime.Now.Date;
            if (beginTime > nowTimeStamp)
                throw new ArgumentException("begin time error");
            var currentTimeStamp = beginTime;
            while (currentTimeStamp <= nowTimeStamp)
            {
                var tail = ShardingKeyToTail(currentTimeStamp);
                tails.Add(tail);
                currentTimeStamp = currentTimeStamp.AddMinutes(1);
            }

            return tails;
        }

        public DateTime GetBeginTime()
        {
            //原则上不应该使用datetime.now但是我看你之前的程序是这么写的如果是now那么每次启动都算是开始时间之前的表可能会访问不到
            //因为访问只会访问begintime之后的时间
            return DateTime.Now;
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
            return $"{time:yyyyMMddHHmm}";
        }
    }
}