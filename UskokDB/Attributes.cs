using System;
// ReSharper disable once CheckNamespace
namespace UskokDB.Attributes;
[AttributeUsage(AttributeTargets.Property)]
public sealed class ColumnAttribute : Attribute
{
    public string Name { get; }

    public ColumnAttribute(string name)
    {
        Name = name;
    }
}
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotMappedAttribute : Attribute { }