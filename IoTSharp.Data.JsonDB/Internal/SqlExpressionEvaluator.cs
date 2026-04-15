#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IoTSharp.Data.JsonDB.Internal
{
    internal static class SqlExpressionEvaluator
    {
        public static object? Evaluate(SqlExpression expression, SqlRowContext row, SqlExecutionContext context)
        {
            return expression switch
            {
                SqlLiteralExpression literalExpression => literalExpression.Value,
                SqlIdentifierExpression identifierExpression => JsonSqlQueryExecutor.ResolvePathValue(row.Node, identifierExpression.Path),
                SqlFunctionCallExpression functionCallExpression => InvokeFunction(functionCallExpression, row, context),
                SqlUnaryExpression unaryExpression => EvaluateUnary(unaryExpression, row, context),
                SqlBinaryExpression binaryExpression => EvaluateBinary(binaryExpression, row, context),
                _ => throw new InvalidOperationException("Unsupported SQL expression.")
            };
        }

        private static object? InvokeFunction(SqlFunctionCallExpression expression, SqlRowContext row, SqlExecutionContext context)
        {
            if (!context.Methods.TryGetValue(expression.Name, out var method))
            {
                throw new InvalidOperationException($"SQL method '{expression.Name}' is not registered.");
            }

            var arguments = expression.Arguments.Select(argument => Evaluate(argument, row, context)).ToArray();
            return method(arguments);
        }

        private static object? EvaluateUnary(SqlUnaryExpression expression, SqlRowContext row, SqlExecutionContext context)
        {
            var operand = Evaluate(expression.Operand, row, context);
            return expression.Operator switch
            {
                SqlUnaryOperator.Plus => operand,
                SqlUnaryOperator.Minus when JsonSqlQueryExecutor.TryConvertToDecimal(operand, out var number) => -number,
                SqlUnaryOperator.Not => !JsonSqlQueryExecutor.ToBoolean(operand),
                _ => throw new InvalidOperationException("Unsupported unary SQL expression.")
            };
        }

        private static object? EvaluateBinary(SqlBinaryExpression expression, SqlRowContext row, SqlExecutionContext context)
        {
            var left = Evaluate(expression.Left, row, context);
            if (expression.Operator == SqlBinaryOperator.And)
            {
                return JsonSqlQueryExecutor.ToBoolean(left) && JsonSqlQueryExecutor.ToBoolean(Evaluate(expression.Right, row, context));
            }

            if (expression.Operator == SqlBinaryOperator.Or)
            {
                return JsonSqlQueryExecutor.ToBoolean(left) || JsonSqlQueryExecutor.ToBoolean(Evaluate(expression.Right, row, context));
            }

            var right = Evaluate(expression.Right, row, context);
            return expression.Operator switch
            {
                SqlBinaryOperator.Add => Add(left, right),
                SqlBinaryOperator.Subtract => Calculate(left, right, static (l, r) => l - r),
                SqlBinaryOperator.Multiply => Calculate(left, right, static (l, r) => l * r),
                SqlBinaryOperator.Divide => Calculate(left, right, static (l, r) => l / r),
                SqlBinaryOperator.Modulo => Calculate(left, right, static (l, r) => l % r),
                SqlBinaryOperator.Equal => JsonSqlQueryExecutor.ValuesEqual(left, right),
                SqlBinaryOperator.NotEqual => !JsonSqlQueryExecutor.ValuesEqual(left, right),
                SqlBinaryOperator.GreaterThan => JsonSqlQueryExecutor.CompareValues(left, right, numericOnly: false) > 0,
                SqlBinaryOperator.GreaterThanOrEqual => JsonSqlQueryExecutor.CompareValues(left, right, numericOnly: false) >= 0,
                SqlBinaryOperator.LessThan => JsonSqlQueryExecutor.CompareValues(left, right, numericOnly: false) < 0,
                SqlBinaryOperator.LessThanOrEqual => JsonSqlQueryExecutor.CompareValues(left, right, numericOnly: false) <= 0,
                _ => throw new InvalidOperationException("Unsupported binary SQL expression.")
            };
        }

        private static object Add(object? left, object? right)
        {
            if (JsonSqlQueryExecutor.TryConvertToDecimal(left, out var leftNumber) && JsonSqlQueryExecutor.TryConvertToDecimal(right, out var rightNumber))
            {
                return leftNumber + rightNumber;
            }

            return $"{Convert.ToString(left, CultureInfo.InvariantCulture)}{Convert.ToString(right, CultureInfo.InvariantCulture)}";
        }

        private static decimal Calculate(object? left, object? right, Func<decimal, decimal, decimal> operation)
        {
            if (!JsonSqlQueryExecutor.TryConvertToDecimal(left, out var leftNumber) || !JsonSqlQueryExecutor.TryConvertToDecimal(right, out var rightNumber))
            {
                throw new InvalidOperationException("Arithmetic SQL expressions require numeric values.");
            }

            return operation(leftNumber, rightNumber);
        }
    }
}
