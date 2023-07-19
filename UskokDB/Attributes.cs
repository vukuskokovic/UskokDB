using System;
// ReSharper disable once CheckNamespace
namespace UskokDB.Attributes;

public sealed class ColumnAttribute : Attribute
{
    public string Name { get; }

    public ColumnAttribute(string name)
    {
        Name = name;
    }
}

public sealed class NotMappedAttribute : Attribute { }