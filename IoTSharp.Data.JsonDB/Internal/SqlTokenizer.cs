#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace IoTSharp.Data.JsonDB.Internal
{
    internal static class SqlTokenizer
    {
        public static IReadOnlyList<SqlToken> Tokenize(string text)
        {
            var tokens = new List<SqlToken>();
            for (var index = 0; index < text.Length;)
            {
                var current = text[index];
                if (char.IsWhiteSpace(current))
                {
                    index++;
                    continue;
                }

                switch (current)
                {
                    case '(':
                        tokens.Add(new SqlToken(SqlTokenKind.LeftParen, "("));
                        index++;
                        continue;
                    case ')':
                        tokens.Add(new SqlToken(SqlTokenKind.RightParen, ")"));
                        index++;
                        continue;
                    case ',':
                        tokens.Add(new SqlToken(SqlTokenKind.Comma, ","));
                        index++;
                        continue;
                    case '+':
                        tokens.Add(new SqlToken(SqlTokenKind.Plus, "+"));
                        index++;
                        continue;
                    case '-':
                        if (index + 1 < text.Length && char.IsDigit(text[index + 1]))
                        {
                            tokens.Add(ReadNumber(text, ref index));
                            continue;
                        }

                        tokens.Add(new SqlToken(SqlTokenKind.Minus, "-"));
                        index++;
                        continue;
                    case '*':
                        tokens.Add(new SqlToken(SqlTokenKind.Asterisk, "*"));
                        index++;
                        continue;
                    case '/':
                        tokens.Add(new SqlToken(SqlTokenKind.Slash, "/"));
                        index++;
                        continue;
                    case '%':
                        tokens.Add(new SqlToken(SqlTokenKind.Percent, "%"));
                        index++;
                        continue;
                    case '!':
                        if (Match(text, index, "!="))
                        {
                            tokens.Add(new SqlToken(SqlTokenKind.NotEqual, "!="));
                            index += 2;
                            continue;
                        }

                        throw new ArgumentException($"Unsupported character '{current}' in SQL expression.", nameof(text));
                    case '>':
                        if (Match(text, index, ">="))
                        {
                            tokens.Add(new SqlToken(SqlTokenKind.GreaterThanOrEqual, ">="));
                            index += 2;
                            continue;
                        }

                        tokens.Add(new SqlToken(SqlTokenKind.GreaterThan, ">"));
                        index++;
                        continue;
                    case '<':
                        if (Match(text, index, "<="))
                        {
                            tokens.Add(new SqlToken(SqlTokenKind.LessThanOrEqual, "<="));
                            index += 2;
                            continue;
                        }

                        tokens.Add(new SqlToken(SqlTokenKind.LessThan, "<"));
                        index++;
                        continue;
                    case '=':
                        tokens.Add(new SqlToken(SqlTokenKind.Equal, "="));
                        index++;
                        continue;
                    case '\'' or '"':
                        tokens.Add(ReadQuotedString(text, ref index));
                        continue;
                    default:
                        if (char.IsDigit(current))
                        {
                            tokens.Add(ReadNumber(text, ref index));
                            continue;
                        }

                        if (IsIdentifierCharacter(current))
                        {
                            tokens.Add(ReadIdentifier(text, ref index));
                            continue;
                        }

                        throw new ArgumentException($"Unsupported character '{current}' in SQL expression.", nameof(text));
                }
            }

            tokens.Add(new SqlToken(SqlTokenKind.End, string.Empty));
            return tokens;
        }

        private static SqlToken ReadQuotedString(string text, ref int index)
        {
            var quote = text[index++];
            StringBuilder buffer = new();
            while (index < text.Length)
            {
                var current = text[index++];
                if (current == '\\' && index < text.Length)
                {
                    buffer.Append(text[index++]);
                    continue;
                }

                if (current == quote)
                {
                    return new SqlToken(SqlTokenKind.String, buffer.ToString());
                }

                buffer.Append(current);
            }

            throw new ArgumentException("Unterminated SQL string literal.", nameof(text));
        }

        private static SqlToken ReadNumber(string text, ref int index)
        {
            var start = index;
            index++;
            while (index < text.Length && (char.IsDigit(text[index]) || text[index] == '.'))
            {
                index++;
            }

            return new SqlToken(SqlTokenKind.Number, text[start..index]);
        }

        private static SqlToken ReadIdentifier(string text, ref int index)
        {
            var start = index;
            index++;
            while (index < text.Length && IsIdentifierCharacter(text[index]))
            {
                index++;
            }

            var value = text[start..index].Replace("`", string.Empty);
            return value.ToLowerInvariant() switch
            {
                "and" => new SqlToken(SqlTokenKind.And, value),
                "or" => new SqlToken(SqlTokenKind.Or, value),
                "not" => new SqlToken(SqlTokenKind.Not, value),
                "true" => new SqlToken(SqlTokenKind.Boolean, bool.TrueString),
                "false" => new SqlToken(SqlTokenKind.Boolean, bool.FalseString),
                "null" => new SqlToken(SqlTokenKind.Null, value),
                _ => new SqlToken(SqlTokenKind.Identifier, value)
            };
        }

        private static bool Match(string text, int index, string token)
        {
            return text.AsSpan(index).StartsWith(token, StringComparison.Ordinal);
        }

        private static bool IsIdentifierCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value is '_' or '.' or '`';
        }
    }
}
