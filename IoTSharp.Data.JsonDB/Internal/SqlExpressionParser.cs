#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace IoTSharp.Data.JsonDB.Internal
{
    internal static class SqlExpressionParser
    {
        public static SqlExpression Parse(string text)
        {
            var parser = new Parser(SqlTokenizer.Tokenize(text));
            return parser.Parse();
        }

        private sealed class Parser
        {
            private readonly IReadOnlyList<SqlToken> _tokens;
            private int _index;

            public Parser(IReadOnlyList<SqlToken> tokens)
            {
                _tokens = tokens;
            }

            public SqlExpression Parse()
            {
                var expression = ParseOrExpression();
                Expect(SqlTokenKind.End);
                return expression;
            }

            private SqlExpression ParseOrExpression()
            {
                var expression = ParseAndExpression();
                while (Match(SqlTokenKind.Or))
                {
                    expression = new SqlBinaryExpression(expression, SqlBinaryOperator.Or, ParseAndExpression());
                }

                return expression;
            }

            private SqlExpression ParseAndExpression()
            {
                var expression = ParseComparisonExpression();
                while (Match(SqlTokenKind.And))
                {
                    expression = new SqlBinaryExpression(expression, SqlBinaryOperator.And, ParseComparisonExpression());
                }

                return expression;
            }

            private SqlExpression ParseComparisonExpression()
            {
                var expression = ParseAdditiveExpression();
                while (Current.Kind is SqlTokenKind.Equal or SqlTokenKind.NotEqual or SqlTokenKind.GreaterThan or SqlTokenKind.GreaterThanOrEqual or SqlTokenKind.LessThan or SqlTokenKind.LessThanOrEqual)
                {
                    var operatorKind = ReadBinaryOperator();
                    expression = new SqlBinaryExpression(expression, operatorKind, ParseAdditiveExpression());
                }

                return expression;
            }

            private SqlExpression ParseAdditiveExpression()
            {
                var expression = ParseMultiplicativeExpression();
                while (Current.Kind is SqlTokenKind.Plus or SqlTokenKind.Minus)
                {
                    var operatorKind = Current.Kind == SqlTokenKind.Plus ? SqlBinaryOperator.Add : SqlBinaryOperator.Subtract;
                    _index++;
                    expression = new SqlBinaryExpression(expression, operatorKind, ParseMultiplicativeExpression());
                }

                return expression;
            }

            private SqlExpression ParseMultiplicativeExpression()
            {
                var expression = ParseUnaryExpression();
                while (Current.Kind is SqlTokenKind.Asterisk or SqlTokenKind.Slash or SqlTokenKind.Percent)
                {
                    var operatorKind = Current.Kind switch
                    {
                        SqlTokenKind.Asterisk => SqlBinaryOperator.Multiply,
                        SqlTokenKind.Slash => SqlBinaryOperator.Divide,
                        _ => SqlBinaryOperator.Modulo
                    };

                    _index++;
                    expression = new SqlBinaryExpression(expression, operatorKind, ParseUnaryExpression());
                }

                return expression;
            }

            private SqlExpression ParseUnaryExpression()
            {
                if (Match(SqlTokenKind.Plus))
                {
                    return new SqlUnaryExpression(SqlUnaryOperator.Plus, ParseUnaryExpression());
                }

                if (Match(SqlTokenKind.Minus))
                {
                    return new SqlUnaryExpression(SqlUnaryOperator.Minus, ParseUnaryExpression());
                }

                if (Match(SqlTokenKind.Not))
                {
                    return new SqlUnaryExpression(SqlUnaryOperator.Not, ParseUnaryExpression());
                }

                return ParsePrimaryExpression();
            }

            private SqlExpression ParsePrimaryExpression()
            {
                if (Match(SqlTokenKind.LeftParen))
                {
                    var expression = ParseOrExpression();
                    Expect(SqlTokenKind.RightParen);
                    return expression;
                }

                var token = Current;
                _index++;
                return token.Kind switch
                {
                    SqlTokenKind.Identifier => ParseIdentifier(token.Value),
                    SqlTokenKind.String => new SqlLiteralExpression(token.Value),
                    SqlTokenKind.Number => new SqlLiteralExpression(decimal.Parse(token.Value, CultureInfo.InvariantCulture)),
                    SqlTokenKind.Boolean => new SqlLiteralExpression(bool.Parse(token.Value)),
                    SqlTokenKind.Null => new SqlLiteralExpression(null),
                    _ => throw new ArgumentException($"Unsupported token '{token.Value}' in SQL expression.", nameof(_tokens))
                };
            }

            private SqlExpression ParseIdentifier(string value)
            {
                if (!Match(SqlTokenKind.LeftParen))
                {
                    return new SqlIdentifierExpression(value);
                }

                var arguments = new List<SqlExpression>();
                if (!Match(SqlTokenKind.RightParen))
                {
                    do
                    {
                        arguments.Add(ParseOrExpression());
                    }
                    while (Match(SqlTokenKind.Comma));

                    Expect(SqlTokenKind.RightParen);
                }

                return new SqlFunctionCallExpression(value, arguments);
            }

            private SqlBinaryOperator ReadBinaryOperator()
            {
                var token = Current;
                _index++;
                return token.Kind switch
                {
                    SqlTokenKind.Equal => SqlBinaryOperator.Equal,
                    SqlTokenKind.NotEqual => SqlBinaryOperator.NotEqual,
                    SqlTokenKind.GreaterThan => SqlBinaryOperator.GreaterThan,
                    SqlTokenKind.GreaterThanOrEqual => SqlBinaryOperator.GreaterThanOrEqual,
                    SqlTokenKind.LessThan => SqlBinaryOperator.LessThan,
                    SqlTokenKind.LessThanOrEqual => SqlBinaryOperator.LessThanOrEqual,
                    _ => throw new ArgumentException($"Unsupported operator '{token.Value}'.", nameof(_tokens))
                };
            }

            private bool Match(SqlTokenKind kind)
            {
                if (Current.Kind != kind)
                {
                    return false;
                }

                _index++;
                return true;
            }

            private void Expect(SqlTokenKind kind)
            {
                if (!Match(kind))
                {
                    throw new ArgumentException($"Expected token {kind} but found '{Current.Value}'.", nameof(_tokens));
                }
            }

            private SqlToken Current => _index < _tokens.Count ? _tokens[_index] : _tokens[^1];
        }
    }
}
