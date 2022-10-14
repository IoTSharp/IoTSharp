using System;
using IoTSharp.Data;

namespace IoTSharp.Controllers.Models
{
    public class IPageParam
    {
        public int offset { get; set; } = 0;
        public int limit { get; set; } = 10;
        public string order { get; set; } = "desc";
        public string where { get; set; } = "";
    }


    public class ProduceParam : IPageParam
    {

        public string Name { get; set; }
    }


    public class AssetParam : IPageParam
    {
      
        public string Name { get; set; }
    }


    public class AssetEntityParam : IPageParam
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }


    public class AlarmParam : IPageParam
    {
        public Guid? OriginatorId { get; set; }
        public int Serverity { get; set; }
        public int AlarmStatus { get; set; }
        public DateTime[] ClearDateTime { get; set; }
        public DateTime[] StartDateTime { get; set; }
        public int OriginatorType { get; set; }
        

        public DateTime[] EndDateTime { get; set; }
        public DateTime[] AckDateTime { get; set; }
     
        public string AlarmType { get; set; }
    }
    public class DeviceParam : IPageParam
    {
    
        public Guid customerId { get; set; }

        public Guid ruleId { get; set; }
 
        public bool OnlyActive { get; set; }
        public string Name { get; set; }

    }

    public class RulePageParam : IPageParam
    {
        public string Name { get; set; }

        public string Creator { get; set; }
        public DateTime[] CreatTime { get; set; }
    }

    public class EventParam : IPageParam
    {
        public string Name { get; set; }
        public Guid? RuleId { get; set; }
        public string CreatorName { get; set; }

        public Guid? Creator { get; set; }
        public DateTime[] CreatTime { get; set; }
    }

    public class DictionaryParam : IPageParam
    {
        public string DictionaryName { get; set; }
        public int DictionaryGroupId { get; set; }
    }

    public class I18NParam : IPageParam
    {
        public string KeyName { get; set; }
    }

    public class CustomerParam : IPageParam
    {
        public Guid tenantId { get; set; }
    }


    public class ExecutorParam : IPageParam
    {
     
    }


    public class SubscriptionParam : IPageParam
    {
        public string Name { get; set; }
    }
    public class DeviceModelParam : IPageParam
    {
        public string Name { get; set; }






    }

}