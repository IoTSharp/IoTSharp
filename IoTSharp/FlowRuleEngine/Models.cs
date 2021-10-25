using RulesEngine.Models;
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

        public Rule Rule =>
            new Rule()
            {
                RuleName = this.id,
                RuleExpressionType = RuleExpressionType.LambdaExpression,
                Expression = this.Expression,
                SuccessEvent = this.id,
            };
    }
}