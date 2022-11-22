using System;
using System.Collections.Generic;

namespace IoTSharp.Models.Rule
{
    public class ModelWorkFlow
    {
        public string Xml { get; set; }

        public string Biz { get; set; }
    }



    public class ModelDiagram
    {
        public List<NodeObject> nodes { get; set; }

        public List<LineObject> lines { get; set; }

        public Guid RuleId { get; set; }
    }



    public class LineObject
    {
        public string sourceId { get; set; }
        public string targetId { get; set; }
        public string linename { get; set; }
        public string condition { get; set; }
        public string lineId { get; set; }
        public string linenamespace { get; set; }
    }



    public class NodeObject
    {
        public string nodeId { get; set; }
        /// <summary>
        /// 节点分组类型 （脚本，执行器）
        /// </summary>

        public string nodetype { get; set; }
        /// <summary>
        /// 节点内容（脚本时为脚本内容，执行器时为配置）
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mata { get; set; }

        public string name { get; set; }
        public string nodenamespace { get; set; }
        public string top { get; set; }
        public string left { get; set; }

        public string icon { get; set; }
        public string nodeclass { get; set; }
    }
}