using IoTSharp.Contracts;

namespace IoTSharp.Storage
{
    /// <summary>
    /// 本地聚合方法， 针对那些关系数据库或者没法完美支持聚合的方式。 
    /// </summary>
    internal static class AggregateDataHelpers
    {
        /// <summary>
        /// 聚合数据
        /// </summary>
        /// <param name="data">被聚合的数据</param>
        /// <param name="begin">数据起始</param>
        /// <param name="end">数据终止时间</param>
        /// <param name="every">时间区间长度</param>
        /// <param name="aggregate">聚合方法</param>
        /// <returns></returns>
        internal static List<TelemetryDataDto> AggregateData(List<TelemetryDataDto> data, DateTime begin, DateTime end, TimeSpan every, Aggregate aggregate)
        {
            List<TelemetryDataDto> result = new List<TelemetryDataDto>();
            if (aggregate == Aggregate.None)
            {
                result = data;
            }
            else
            {
                if (every.TotalMilliseconds == 0) every = every.Add(TimeSpan.FromMinutes(1));
                var ts = end.Subtract(begin) / every;//算出来一共多少个区间
                for (int i = 0; i < ts; i++)
                {
                    var xb = begin + (i * every);//区间起始时间
                    var xe = begin + ((i + 1) * every);//区间终止时间
                    var dt = from x in data where (x.DataType == DataType.Double || x.DataType == DataType.Long) && x.DateTime >= xb && x.DateTime < xe select x;
                    if (dt.Any())//如果此区间有数据
                    {
                        dt.GroupBy<TelemetryDataDto, string>(d => d.KeyName).ToList().ForEach(d =>
                        {
                            //按key进行分组进行计算
                            if (d.Count() == 1) //如果只有一个， 就不计算
                            {
                                result.Add(d.First());
                            }
                            else
                            {
                                var dxx = d.FirstOrDefault();//用第一个值来取类型和keyname , 
                                if (dxx != null)
                                {
                                    var tdd = new TelemetryDataDto()
                                    {
                                        KeyName = dxx.KeyName,
                                        DataType = dxx.DataType,
                                        DateTime = xe
                                    };
                                    switch (aggregate)
                                    {
                                        case Aggregate.Mean:
                                            if (tdd.DataType == DataType.Long)
                                            {
                                                tdd.Value = (long)d.Average(f => (long)f.Value);
                                            }
                                            else if (tdd.DataType == DataType.Double)
                                            {
                                                tdd.Value = (double)d.Average(f => (double)f.Value);
                                            }
                                            break;
                                        case Aggregate.Median:
                                            if (tdd.DataType == DataType.Long)
                                            {
                                                var _vxx = d.OrderBy(f => (long)f.Value).ToList();
                                                var indx = _vxx.Count / 2;
                                                tdd.Value = _vxx[indx].Value;
                                            }
                                            else if (tdd.DataType == DataType.Double)
                                            {
                                                var _vxx = d.OrderBy(f => (double)f.Value).ToList();
                                                var indx = _vxx.Count / 2;
                                                tdd.Value = _vxx[indx].Value;
                                            }
                                            break;
                                        case Aggregate.Last:
                                            tdd.Value = d.Last().Value;
                                            break;
                                        case Aggregate.First:
                                            tdd.Value = d.First().Value;
                                            break;
                                        case Aggregate.Max:
                                            if (tdd.DataType == DataType.Long)
                                            {
                                                tdd.Value = (long)d.Max(f => (long)f.Value);
                                            }
                                            else if (tdd.DataType == DataType.Double)
                                            {
                                                tdd.Value = (double)d.Max(f => (double)f.Value);
                                            }
                                            break;
                                        case Aggregate.Min:
                                            if (tdd.DataType == DataType.Long)
                                            {
                                                tdd.Value = (long)d.Min(f => (long)f.Value);
                                            }
                                            else if (tdd.DataType == DataType.Double)
                                            {
                                                tdd.Value = (double)d.Min(f => (double)f.Value);
                                            }
                                            break;
                                        case Aggregate.Sum:
                                            if (tdd.DataType == DataType.Long)
                                            {
                                                tdd.Value = (long)d.Sum(f => (long)f.Value);
                                            }
                                            else if (tdd.DataType == DataType.Double)
                                            {
                                                tdd.Value = (double)d.Sum(f => (double)f.Value);
                                            }
                                            break;
                                        case Aggregate.None:
                                        default:
                                            break;
                                    }
                                    result.Add(tdd);
                                }
                            }
                        });
                    }
                }
            }
            return result;
        }
    }
}