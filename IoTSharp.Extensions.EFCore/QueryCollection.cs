using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace IoTSharp.EasyEFQuery
{
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum Operators
    {
        None = 0,
        Equal = 1,
        GreaterThan = 2,
        GreaterThanOrEqual = 3,
        LessThan = 4,
        LessThanOrEqual = 5,
        Contains = 6,
        StartWith = 7,
        EndWidth = 8,
        Range = 9
    }
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum Condition
    {
        OrElse = 1,
        AndAlso = 2
    }
    public class Query
    {
        
        public string Name { get; set; }
        public Operators Operator { get; set; }
        public object Value { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object ValueMin { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object ValueMax { get; set; }
    }

    public class QueryCollection : Collection<Query>
    {
        public static QueryCollection Create()
        {
            return new QueryCollection();
        }
        public static QueryCollection Create(string json)
        {
            return  Newtonsoft.Json.JsonConvert.DeserializeObject<QueryCollection>(json);
        }
        public  override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

    }
    public class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;

        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression Visit(Expression node)
        {
            if (node == _oldValue)
            {
                return _newValue;
            }
            return base.Visit(node);
        }
    }
    public static class QueryCollectionExtension
    {
       
        public static QueryCollection Parse(this  QueryCollection queries, string json)
        {
            var jo= Newtonsoft.Json.Linq.JObject.Parse(json);
            jo.Merge(queries);
            return jo.ToObject<QueryCollection>();
        }
        public static Expression<Func<T, bool>> AndWith<T>(this Expression<Func<T, bool>> first, QueryCollection queries ) where T : class
        {
            return first.AndWith(queries.AsExpression<T>(), Expression.AndAlso);
        }
        public static Expression<Func<T, bool>> OrWith<T>(this Expression<Func<T, bool>> first, QueryCollection queries) where T : class
        {
            return first.AndWith(queries.AsExpression<T>(), Expression.OrElse);
        }


        private static Expression<Func<T, bool>> AndWith<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2, Func<Expression, Expression, BinaryExpression> func)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
            Expression arg = new ReplaceExpressionVisitor(expr1.Parameters[0], parameterExpression).Visit(expr1.Body);
            Expression arg2 = new ReplaceExpressionVisitor(expr2.Parameters[0], parameterExpression).Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(func(arg, arg2), new ParameterExpression[1] { parameterExpression });
        }
        public static Expression<Func<T, bool>> AsExpression<T>(this QueryCollection queries, Condition? condition = Condition.OrElse) where T : class
        {
            Type targetType = typeof(T);
            TypeInfo typeInfo = targetType.GetTypeInfo();
            var parameter = Expression.Parameter(targetType, "m");
            Expression expression = null;
            Func<Expression, Expression, Expression> Append = (exp1, exp2) =>
            {
                if (exp1 == null)
                {
                    return exp2;
                }
                return (condition ?? Condition.OrElse) == Condition.OrElse ? Expression.OrElse(exp1, exp2) : Expression.AndAlso(exp1, exp2);
            };
            foreach (var item in queries)
            {
                var property = typeInfo.GetProperty(item.Name);
                if (property == null ||
                    !property.CanRead ||
                    (item.Operator != Operators.Range && item.Value == null) ||
                    (item.Operator == Operators.Range && item.ValueMin == null && item.ValueMax == null))
                {
                    continue;
                }
                Type realType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (item.Value != null)
                {
                    item.Value = Convert.ChangeType(item.Value, realType);
                }
                Expression<Func<object>> valueLamba = () => item.Value;
                switch (item.Operator)
                {
                    case Operators.Equal:
                        {
                            expression = Append(expression, Expression.Equal(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.GreaterThan:
                        {
                            expression = Append(expression, Expression.GreaterThan(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.GreaterThanOrEqual:
                        {
                            expression = Append(expression, Expression.GreaterThanOrEqual(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.LessThan:
                        {
                            expression = Append(expression, Expression.LessThan(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.LessThanOrEqual:
                        {
                            expression = Append(expression, Expression.LessThanOrEqual(Expression.Property(parameter, item.Name),
                                Expression.Convert(valueLamba.Body, property.PropertyType)));
                            break;
                        }
                    case Operators.Contains:
                        {
                            var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, Expression.Property(parameter, item.Name)));
                            var contains = Expression.Call(Expression.Property(parameter, item.Name), "Contains", null,
                                Expression.Convert(valueLamba.Body, property.PropertyType));
                            expression = Append(expression, Expression.AndAlso(nullCheck, contains));
                            break;
                        }
                    case Operators.StartWith:
                        {
                            var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, Expression.Property(parameter, item.Name)));
                            var startsWith = Expression.Call(Expression.Property(parameter, item.Name), "StartsWith", null,
                                Expression.Convert(valueLamba.Body, property.PropertyType));
                            expression = Append(expression, Expression.AndAlso(nullCheck, startsWith));
                            break;
                        }
                    case Operators.EndWidth:
                        {
                            var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, Expression.Property(parameter, item.Name)));
                            var endsWith = Expression.Call(Expression.Property(parameter, item.Name), "EndsWith", null,
                                Expression.Convert(valueLamba.Body, property.PropertyType));
                            expression = Append(expression, Expression.AndAlso(nullCheck, endsWith));
                            break;
                        }
                    case Operators.Range:
                        {
                            Expression minExp = null, maxExp = null;
                            if (item.ValueMin != null)
                            {
                                var minValue = Convert.ChangeType(item.ValueMin, realType);
                                Expression<Func<object>> minValueLamda = () => minValue;
                                minExp = Expression.GreaterThanOrEqual(Expression.Property(parameter, item.Name), Expression.Convert(minValueLamda.Body, property.PropertyType));
                            }
                            if (item.ValueMax != null)
                            {
                                var maxValue = Convert.ChangeType(item.ValueMax, realType);
                                Expression<Func<object>> maxValueLamda = () => maxValue;
                                maxExp = Expression.LessThanOrEqual(Expression.Property(parameter, item.Name), Expression.Convert(maxValueLamda.Body, property.PropertyType));
                            }

                            if (minExp != null && maxExp != null)
                            {
                                expression = Append(expression, Expression.AndAlso(minExp, maxExp));
                            }
                            else if (minExp != null)
                            {
                                expression = Append(expression, minExp);
                            }
                            else if (maxExp != null)
                            {
                                expression = Append(expression, maxExp);
                            }

                            break;
                        }
                }
            }
            if (expression == null)
            {
                return null;
            }
            return ((Expression<Func<T, bool>>)Expression.Lambda(expression, parameter));
        }
    }
}