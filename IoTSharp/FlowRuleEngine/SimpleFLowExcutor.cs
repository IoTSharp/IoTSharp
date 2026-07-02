using IoTSharp.Extensions;
using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
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
        /// 执行当前任务的所有外连线条件，并返回每条规则的命中结果。
        /// </summary>
        /// <param name="Input">包含当前任务、外连线和输入参数的流程执行上下文。</param>
        /// <returns>每条外连线条件的执行结果。</returns>
        public Task<List<RuleResultTree>> Excute(FlowExcuteEntity Input)
        {
            var results = new List<RuleResultTree>();
            using var engine = CreateExpressionEngine();
            BindInput(engine, Input.Params);

            foreach (var rule in Input.Task.outgoing.Select(c => c.Rule))
            {
                results.Add(EvaluateRule(engine, rule));
            }

            return Task.FromResult(results);
        }

        /// <summary>
        /// 创建受限的表达式执行引擎，避免规则表达式长期运行或占用过多内存。
        /// </summary>
        /// <returns>配置了资源限制的 Jint 引擎。</returns>
        private static Engine CreateExpressionEngine()
        {
            return new Engine(options =>
            {
                options.LimitMemory(4_000_000);
                options.TimeoutInterval(TimeSpan.FromSeconds(2));
            });
        }

        /// <summary>
        /// 执行单条条件表达式，并把异常转换为失败结果。
        /// </summary>
        /// <param name="engine">已绑定输入变量的表达式引擎。</param>
        /// <param name="rule">需要执行的规则。</param>
        /// <returns>规则执行结果。</returns>
        private static RuleResultTree EvaluateRule(Engine engine, FlowConditionRule rule)
        {
            try
            {
                var value = engine.Evaluate($"({rule.Expression})");
                return new RuleResultTree
                {
                    Rule = rule,
                    IsSuccess = ToBoolean(value)
                };
            }
            catch (Exception ex)
            {
                return new RuleResultTree
                {
                    Rule = rule,
                    IsSuccess = false,
                    ExceptionMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// 将输入参数绑定为 input 变量，并把一层属性展开为同名变量以兼容旧表达式。
        /// </summary>
        /// <param name="engine">需要绑定变量的表达式引擎。</param>
        /// <param name="input">流程输入参数。</param>
        private static void BindInput(Engine engine, object input)
        {
            var normalized = NormalizeValue(input);
            engine.SetValue("input", normalized);

            foreach (var property in ReadProperties(normalized))
            {
                engine.SetValue(property.Key, NormalizeValue(property.Value));
            }
        }

        /// <summary>
        /// 读取字典、ExpandoObject 或普通对象上的公开属性。
        /// </summary>
        /// <param name="value">需要读取的对象。</param>
        /// <returns>对象的一层键值集合。</returns>
        private static IEnumerable<KeyValuePair<string, object>> ReadProperties(object value)
        {
            if (value is IDictionary<string, object> dictionary)
            {
                return dictionary;
            }

            if (value is ExpandoObject expando)
            {
                return (IDictionary<string, object>)expando;
            }

            if (value == null)
            {
                return Enumerable.Empty<KeyValuePair<string, object>>();
            }

            return value.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.CanRead)
                .Select(property => new KeyValuePair<string, object>(property.Name, property.GetValue(value)));
        }

        /// <summary>
        /// 将 System.Text.Json 产生的 JsonElement 转为普通 CLR 值。
        /// </summary>
        /// <param name="value">需要归一化的输入值。</param>
        /// <returns>可直接绑定到表达式引擎的值。</returns>
        private static object NormalizeValue(object value)
        {
            return value is System.Text.Json.JsonElement element ? element.ToClrObject() : value;
        }

        /// <summary>
        /// 将表达式返回值转换为布尔结果，兼容布尔、字符串和数字返回值。
        /// </summary>
        /// <param name="value">表达式求值结果。</param>
        /// <returns>表示规则是否命中的布尔值。</returns>
        private static bool ToBoolean(JsValue value)
        {
            if (value.IsBoolean())
            {
                return value.AsBoolean();
            }

            var obj = value.ToObject();
            if (obj is bool boolean)
            {
                return boolean;
            }

            if (obj is string text && bool.TryParse(text, out var parsed))
            {
                return parsed;
            }

            return obj is IConvertible convertible && convertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture) != 0d;
        }
    }
}
