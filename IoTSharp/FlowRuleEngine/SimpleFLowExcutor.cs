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
        /// <summary>
        /// 调用规则引擎处理线上的逻辑，判断是否为True，用来判断是否继续进行下一步节点
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
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
            //规则集合，可以参考RulesEngine.RulesEngine程序说明，其中需要的很多参数已经写死，只有Expression前端填写，如Age>18
            /**
             * //定义规则
    var var rulesStr = @"[{
        ""WorkflowName"":""UserInputworkflow"",
        ""Rules"":[
.         {
            "RuleName"":"CheckAge"
            "ErrorMessage"":""年龄必须大于18岁。"",
            ""ErrorType"":""Error"",
            ""RuleExpressionType"":"LambdaExpression"",
            ""Expression"":""Age > 18""
          },
         { 
            ""RuleName"" :"CheckIDNoIsEmpty"",
            ""ErrorMessage”":""身份证号不可以为空."",
            ""ErrorType"":"Error"",
            ""RuleExpressionType"":""LambdaExpression"",
            ""Expression"":""IdNo != null""
          }
        }]";
          **/
            mainRules.Rules = Input.Task.outgoing.Select(c => c.Rule).ToList();
            //调用规则引擎执行规则判断验证成功就会返回IsSuccess=true
            var bre = new RulesEngine.RulesEngine(new[] { mainRules }, null);
            return await bre.ExecuteAllRulesAsync(Input.Task.id, Input.Params);
        }
    }
}