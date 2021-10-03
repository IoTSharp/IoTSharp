using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using RulesEngine.Models;

namespace IoTSharp.FlowRuleEngine.Models.Task
{
    

    public interface IFlowExcutor<T>
    {
         Task<List<RuleResultTree>> Excute(T Input);
    }
    public interface IFlowEntity
    {

    }

    public class FlowExcuteEntity: IFlowEntity
    {
        public dynamic Params { get; set; }
        public BaseRuleTask Task { get; set; }
        //public Action Action { get; set; }
        //public int WaitTime { get; set; }
    }

    public class SimpleFLowExcutor : IFlowExcutor<FlowExcuteEntity>
    {
         

        public async Task<List<RuleResultTree>> Excute(FlowExcuteEntity Input)
        {

            var mainRules = new WorkflowRules
            {
                WorkflowName = Input.Task.id, 
            };

            foreach (var item in Input.Task.outgoing)
            {
                item.Rule.Operator=item.id;
            }
            mainRules.Rules = Input.Task.outgoing.Select(c => c.Rule).ToList();
            var bre = new RulesEngine.RulesEngine(new[] { mainRules }, null);
            return await bre.ExecuteAllRulesAsync(Input.Task.id, Input.Params);
        }

    }







}
