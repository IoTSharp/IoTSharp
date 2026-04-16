#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace IoTSharp.Data.JsonDB.Internal
{
    internal sealed class SqlExecutionContext
    {
        public SqlExecutionContext(JsonNode root, IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>> methods)
        {
            Root = root;
            Methods = methods;
        }

        public JsonNode Root { get; }
        public IReadOnlyDictionary<string, Func<IReadOnlyList<object?>, object?>> Methods { get; }
    }

    internal readonly record struct SqlRowContext(JsonNode Node, int ArrayIndex);

    internal abstract record SqlStatement(string Table);

    internal sealed record SqlSelectStatement(
        string Table,
        IReadOnlyList<SqlSelectItem> Items,
        SqlExpression? Where,
        IReadOnlyList<SqlOrderByItem> OrderBy,
        SqlLimit? Limit) : SqlStatement(Table);

    internal sealed record SqlUpdateStatement(
        string Table,
        IReadOnlyList<SqlAssignment> Assignments,
        SqlExpression? Where,
        IReadOnlyList<SqlOrderByItem> OrderBy,
        SqlLimit? Limit) : SqlStatement(Table);

    internal sealed record SqlDeleteStatement(
        string Table,
        SqlExpression? Where,
        IReadOnlyList<SqlOrderByItem> OrderBy,
        SqlLimit? Limit) : SqlStatement(Table);

    internal sealed record SqlInsertStatement(string Table, IReadOnlyList<SqlAssignment> Assignments) : SqlStatement(Table);

    internal sealed record SqlSelectItem(SqlExpression? Expression, string Alias, bool IsWildcard);

    internal sealed record SqlOrderByItem(SqlExpression Expression, bool Descending, bool Numeric);

    internal sealed record SqlAssignment(string Path, SqlExpression Expression);

    internal sealed record SqlLimit(int Offset, int? Count);

    internal abstract record SqlExpression;

    internal sealed record SqlLiteralExpression(object? Value) : SqlExpression;

    internal sealed record SqlIdentifierExpression(string Path) : SqlExpression;

    internal sealed record SqlFunctionCallExpression(string Name, IReadOnlyList<SqlExpression> Arguments) : SqlExpression;

    internal sealed record SqlUnaryExpression(SqlUnaryOperator Operator, SqlExpression Operand) : SqlExpression;

    internal sealed record SqlBinaryExpression(SqlExpression Left, SqlBinaryOperator Operator, SqlExpression Right) : SqlExpression;

    internal enum SqlUnaryOperator
    {
        Plus,
        Minus,
        Not,
    }

    internal enum SqlBinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        And,
        Or,
    }

    internal enum SqlTokenKind
    {
        Identifier,
        String,
        Number,
        Boolean,
        Null,
        LeftParen,
        RightParen,
        Comma,
        Plus,
        Minus,
        Asterisk,
        Slash,
        Percent,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        And,
        Or,
        Not,
        End,
    }

    internal readonly record struct SqlToken(SqlTokenKind Kind, string Value);
}
