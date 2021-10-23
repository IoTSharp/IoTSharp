using RulesEngine.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.FlowRuleEngine
{
    public interface IFlowExcutor<T>
    {
        Task<List<RuleResultTree>> Excute(T Input);
    }

    public interface IFlowEntity
    {
    }

    public class FlowExcuteEntity : IFlowEntity
    {
        public dynamic Params { get; set; }
        public BaseRuleTask Task { get; set; }
        //public Action Action { get; set; }
        //public int WaitTime { get; set; }
    }

    public class SimpleFlowExcutor : IFlowExcutor<FlowExcuteEntity>
    {
        public async Task<List<RuleResultTree>> Excute(FlowExcuteEntity Input)
        {
            var mainRules = new Workflow
            {
                WorkflowName = Input.Task.id,
            };

            foreach (var item in Input.Task.outgoing)
            {
                item.Rule.Operator = item.id;
            }
            mainRules.Rules = Input.Task.outgoing.Select(c => c.Rule).ToList();
            var bre = new RulesEngine.RulesEngine(new[] { mainRules }, null);
            return await bre.ExecuteAllRulesAsync(Input.Task.id, Input.Params);
        }
    }
}