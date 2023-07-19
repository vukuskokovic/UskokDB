using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace UskokDB.MySql.Attributes;

public sealed class ColumnNotNullAttribute : Attribute { }

public sealed class AutoIncrementAttribute : Attribute { }

public sealed class MaxLengthAttribute : Attribute
{
    public int Length { get; }

    public MaxLengthAttribute(int length)
    {
        Length = length;
    }
}

public sealed class KeyAttribute : Attribute
{
}

public sealed class ForeignKeyAttribute : Attribute
{
    public string TableString { get; }

    public ForeignKeyAttribute(string tableName, string propertyName)
    {
        TableString = $"{tableName}({propertyName})";
    }
}