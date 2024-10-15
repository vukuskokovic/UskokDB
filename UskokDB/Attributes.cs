using System;
// ReSharper disable once CheckNamespace
namespace UskokDB.Attributes;
[AttributeUsage(AttributeTargets.Property)]
public sealed class ColumnAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotMappedAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public sealed class ColumnNotNullAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property)]
public sealed class AutoIncrementAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property)]
public sealed class MaxLengthAttribute(int length) : Attribute
{
    public int Length { get; } = length;
}
[AttributeUsage(AttributeTargets.Property)]
public sealed class KeyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ForeignKeyAttribute<T>(string columnName) : Attribute where T : class, new()
{
    public string ColumnName { get; } = columnName;
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class TableNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}