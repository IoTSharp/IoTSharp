namespace IoTSharp.EventBus
{
    /// <summary>
    /// 消息队列统计信息
    /// </summary>
    public class EventBusMetrics
    {
        /// <summary>
        /// 消息队列服务器总数
        /// </summary>
        public int Servers { get; set; }
        /// <summary>
        /// 订阅器总数
        /// </summary>
        public int Subscribers { get; set; }
        /// <summary>
        /// 发布成功数量
        /// </summary>
        public int PublishedSucceeded { get; set; }
        /// <summary>
        /// 接收成功数量
        /// </summary>
        public int ReceivedSucceeded { get; set; }

        /// <summary>
        /// 发布失败数量
        /// </summary>
        public int PublishedFailed { get; set; }
        /// <summary>
        /// 接受数量
        /// </summary>
        public int ReceivedFailed { get; set; }

        /// <summary>
        ///时间 列表 
        /// </summary>
        public List<string> DayHour { get; set; }
        /// <summary>
        /// 对应DayHour的每小时发布成功数组
        /// </summary>
        public List<int> PublishSuccessed { get; set; }
        /// <summary>
        /// 对应DayHour的每小时发布失败数组
        /// </summary>
        public List<int> PublishFailed { get; set; }
        /// <summary>
        /// 对应DayHour的每小时订阅成功数组
        /// </summary>
        public List<int> SubscribeSuccessed { get; set; }
        /// <summary>
        /// 对应DayHour的每小时订阅失败数组
        /// </summary>
        public List<int> SubscribeFailed { get; set; }
        public EventBusMetrics()
        {
            DayHour = new List<string>();
            PublishSuccessed = new List<int> ();
            PublishFailed = new List<int>();
            SubscribeSuccessed = new List<int>();
            SubscribeFailed = new List<int>();
        }
        public EventBusMetrics(List<string> dayHour, List<int> publishSuccessed, List<int> publishFailed, List<int> subscribeSuccessed, List<int> subscribeFailed)
        {
            DayHour = dayHour;
            PublishSuccessed = publishSuccessed;
            PublishFailed = publishFailed;
            SubscribeSuccessed = subscribeSuccessed;
            SubscribeFailed = subscribeFailed;
        }
    
    }
}
