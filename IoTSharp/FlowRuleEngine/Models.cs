using System.Collections.Generic;

namespace IoTSharp.FlowRuleEngine
{
    public class BaseRuleObject
    {
        public string Eventid { get; set; }
        public string id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class BaseRuleTask : BaseRuleObject
    {
        public List<BaseRuleFlow> incoming { get; set; }
        public List<BaseRuleFlow> outgoing { get; set; }
    }

    public class BaseRuleFlow : BaseRuleObject
    {
        public BaseRuleObject incoming { get; set; }
        public BaseRuleObject outgoing { get; set; }

        public string Expression { get; set; }

        /// <summary>
        /// 将连线上的条件表达式转换为运行时可执行的规则描述。
        /// </summary>
        public FlowConditionRule Rule =>
            new FlowConditionRule
            {
                RuleName = id,
                Expression = Expression,
                SuccessEvent = id,
            };
    }

    /// <summary>
    /// 描述流程连线上的条件规则，供轻量规则执行器判断是否命中。
    /// </summary>
    public class FlowConditionRule
    {
        /// <summary>
        /// 规则名称，通常使用连线或节点标识。
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// 需要执行的布尔表达式。
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// 规则命中后返回的事件标识。
        /// </summary>
        public string SuccessEvent { get; set; }
    }

    /// <summary>
    /// 表示单条条件规则的执行结果。
    /// </summary>
    public class RuleResultTree
    {
        /// <summary>
        /// 本次执行的规则。
        /// </summary>
        public FlowConditionRule Rule { get; set; }

        /// <summary>
        /// 表达式是否执行成功并返回 true。
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 表达式执行失败时的错误信息。
        /// </summary>
        public string ExceptionMessage { get; set; }
    }
}
