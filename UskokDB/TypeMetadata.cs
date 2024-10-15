using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UskokDB.Attributes;

namespace UskokDB;

public static class TypeMetadata<T> where T : class, new()
{
    static TypeMetadata()
    {
        Type = typeof(T);
        var properties = Type.GetProperties();
        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<NotMappedAttribute>() is not null) continue;

            var meta = new TypeMetadataProperty(property);
            NameToPropertyMap[property.Name] = meta;
            Properties.Add(meta);
        }
    }

    public static Type Type { get; }
    public static List<TypeMetadataProperty> Properties { get; } = new();
    public static Dictionary<string, TypeMetadataProperty> NameToPropertyMap = [];
}

public class TypeMetadataProperty
{
    public PropertyInfo PropertyInfo { get; }
    public Type Type { get; }
    public string PropertyName { get; }

    public TypeMetadataProperty(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
        Type = propertyInfo.PropertyType;
        if (propertyInfo.GetCustomAttribute<ColumnAttribute>() is ColumnAttribute columnAttribute)
        {
            PropertyName = columnAttribute.Name;
        }
        else
        {
            PropertyName = propertyInfo.Name.FirstLetterLowerCase();
        }
    }
}