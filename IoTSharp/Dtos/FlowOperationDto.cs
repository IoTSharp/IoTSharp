using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Dtos
{
    public class FlowOperationDto
    {
        public Guid OperationId { get; set; }
        public DateTime? AddDate { get; set; }
        /// <summary>
        /// 节点处理状态，0 创建完
        /// </summary>
        public int? NodeStatus { get; set; }
        public string OperationDesc { get; set; }
        public string Data { get; set; }
        public string BizId { get; set; }
        public string bpmnid { get; set; }
        public Guid ruleid { get; set; }


        public Guid flowid { get; set; }
        public Guid eventid { get; set; }
        public int Step { get; set; }
    }
}
