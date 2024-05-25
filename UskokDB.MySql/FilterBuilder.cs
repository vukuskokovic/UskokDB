using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UskokDB.MySql;

public class FilterBuilder
{
    private string TableName { get; }
    private string Type { get; }
    private List<string> Filters { get; } = [];
    private OrderGroupInfo? GroupInfo { get; set; }
    private OrderGroupInfo? OrderInfo { get; set; }
    private int? Limit { get; set; }
    private int? Offset { get; set; }
    public FilterBuilder(string tableName, string type = "AND")
    {
        if (type != "OR" && type != "AND") throw new ArgumentException("type must be either 'AND' or 'OR'");
        TableName = tableName;
        Type = type;
    }

    public static FilterBuilder Create<T>(string type = "AND") where T : MySqlTable<T>, new()
    {
        return new FilterBuilder(MySqlTable<T>.TableName, type);
    }

    private static string? CreateClause(string type, string fieldName, IEnumerable<FilterOperand> values)
    {
        if (!values.Any()) return null;
        return $"({string.Join($" {type} ", values.Select(x => x.Construct(fieldName)))})";
    }

    public static string? CreateOr(string fieldName, params FilterOperand[] values) => CreateClause("OR", fieldName, values);
    public static string? CreateAnd(string fieldName, params FilterOperand[] values) => CreateClause("AND", fieldName, values);

    public static string? CreateOr(string fieldName, IEnumerable<FilterOperand> values) => CreateClause("OR", fieldName, values);
    public static string? CreateAnd(string fieldName, IEnumerable<FilterOperand> values) => CreateClause("AND", fieldName, values);

    public FilterBuilder AddOr(string fieldName, params FilterOperand[] operands)
    {
        string? str = CreateOr(fieldName, operands);
        if (str == null) return this;
        Filters.Add(str);
        return this;
    }
    public FilterBuilder AddOr(string fieldName, IEnumerable<FilterOperand> operands)
    {
        string? str = CreateOr(fieldName, operands);
        if (str == null) return this;
        Filters.Add(str);
        return this;
    }
    public FilterBuilder AddAnd(string fieldName, params FilterOperand[] operands)
    {
        string? str = CreateAnd(fieldName, operands);
        if (str == null) return this;
        Filters.Add(str);
        return this;
    }
    public FilterBuilder AddAnd(string fieldName, IEnumerable<FilterOperand> operands)
    {
        string? str = CreateAnd(fieldName, operands);
        if (str == null) return this;
        Filters.Add(str);
        return this;
    }



    public FilterBuilder OrderBy(bool isDescending, params string[] fields)
    {
        OrderInfo = new OrderGroupInfo("ORDER", fields, isDescending);
        return this;
    }

    public FilterBuilder GroupBy(params string[] fields)
    {
        GroupInfo = new OrderGroupInfo("GROUP", fields);
        return this;
    }

    public FilterBuilder SetLimit(int limit)
    {
        Limit = limit;
        return this;
    }

    public FilterBuilder SetOffset(int offset)
    {
        Offset = offset;
        return this;
    }

    private void ApplyAfter(StringBuilder builder)
    {
        if (GroupInfo != null) builder.Append(GroupInfo.ToString());
        if (OrderInfo != null) builder.Append(OrderInfo.ToString());
        if (Limit != null && Offset != null) builder.Append($"LIMIT {Offset.Value},{Limit.Value}");
        else if (Limit != null) builder.Append($"LIMIT {Limit.Value}");
        else if (Offset != null) builder.Append($"OFFSET {Offset.Value}");
    }

    public override string ToString()
    {
        StringBuilder builder = new("SELECT * FROM ");
        builder.Append(TableName);
        builder.Append(" ");
        if (Filters.Count == 0)
        {
            ApplyAfter(builder);
            return builder.ToString();
        }

        builder.Append("WHERE ");
        builder.Append(string.Join(Type, Filters));
        builder.Append(" ");
        ApplyAfter(builder);
        return builder.ToString();
    }

    private class OrderGroupInfo
    {
        public bool IsDesc { get; }
        public string[] Fields { get; }
        public string Type { get; }

        public OrderGroupInfo(string type, string[] fields, bool isDesc = false) 
        {
            IsDesc = isDesc;
            Fields = fields;
            Type = type;
        }

        public override string ToString()
        {
            StringBuilder builder = new(Type);
            builder.Append(" BY ");
            builder.Append(string.Join(",", Fields));
            if(Type == "ORDER" && IsDesc) builder.Append(" DESC ");
            else builder.Append(" ");

            return builder.ToString();
        }
    }
}

public class FilterOperand
{
    private object Value { get; }
    private string Operand { get; }
    public FilterOperand(object value, string operand = "=")
    {
        Value = value;
        Operand = operand;
    }

    public static FilterOperand EqualsOperand(object value) => new(value, "=");
    public static FilterOperand HigherOperand(object value) => new(value, ">");
    public static FilterOperand LowerOperand(object value) => new(value, "<");
    public static FilterOperand HigherEqualsOperand(object value) => new(value, ">=");
    public static FilterOperand LowerEqualsOperand(object value) => new(value, "<=");

    internal string Construct(string fieldName) => $"`{fieldName}` {Operand} {ParameterHandler.WriteValue(Value)}";
}