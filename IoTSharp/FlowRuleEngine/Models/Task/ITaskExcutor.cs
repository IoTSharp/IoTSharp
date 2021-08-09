using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using RulesEngine.Models;

namespace IoTSharp.FlowRuleEngine.Models.Task
{
    public interface ITaskExcutor<ITaskEntity>
    {
        public Task<List<RuleResultTree>> Excute(ITaskEntity Input);
    }


    public class ExcuteEntity 
    {
        public dynamic Params { get; set; }
        public BaseRuleTask Task { get; set; }
        public Action Action { get; set; }
        public int WaitTime { get; set; }
    }

    public class AbstractTaskExcutor : ITaskExcutor<ExcuteEntity>
    {


        public async Task<List<RuleResultTree>> Excute(ExcuteEntity Input)
        {

            if (Input.Action != null)
            {
                Input.Action();
            }
            if (Input.WaitTime > 0)
            {
                await System.Threading.Tasks.Task.Delay(Input.WaitTime);
            }
            var mainRules = new WorkflowRules
            {
                WorkflowName = Input.Task.Name,
            };

            foreach (var item in Input.Task.outgoing)
            {
                item.Rule.Properties = new Dictionary<string, object> {{"Flow", item}};
            }
            mainRules.Rules = Input.Task.outgoing.Select(c => c.Rule).ToList();
            var bre = new RulesEngine.RulesEngine(new[] { mainRules }, null);
            return await bre.ExecuteAllRulesAsync(Input.Task.Name, Input.Params);
        }

    }


}
