using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace UskokDB.MySql.Attributes;
[AttributeUsage(AttributeTargets.Property)]
public sealed class ColumnNotNullAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property)]
public sealed class AutoIncrementAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Property)]
public sealed class MaxLengthAttribute : Attribute
{
    public int Length { get; }

    public MaxLengthAttribute(int length)
    {
        Length = length;
    }
}
[AttributeUsage(AttributeTargets.Property)]
public sealed class KeyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ForeignKeyAttribute<T> : Attribute where T : MySqlTable<T>, new()
{
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class TableNameAttribute : Attribute
{
    public string Name { get; }

    public TableNameAttribute(string name)
    {
        Name = name;
    }
}