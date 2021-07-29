using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Models
{
    public class IPageParam
    {
        public int offset { get; set; } = 0;
        public int limit { get; set; } = 2;
        public string order { get; set; } = "desc";
   
    }

    public class RulePageParam: IPageParam
    {

        public string Name { get; set; }

        public string Creator{ get; set; }
        public DateTime[] CreatTime { get; set; }
    }


}
