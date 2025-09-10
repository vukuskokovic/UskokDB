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
    public ForeignKeyAction OnDelete { get; set; } = ForeignKeyAction.DbDefault;
    public ForeignKeyAction OnUpdate { get; set; } = ForeignKeyAction.DbDefault;
    public string ColumnName { get; } = columnName;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class UniqueAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class TableNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

public enum ForeignKeyAction : byte
{
    Cascade = 0,
    SetNull = 1,
    SetDefault = 2,
    Restrict = 3,
    NoAction = 4, 
    DbDefault = 5
}