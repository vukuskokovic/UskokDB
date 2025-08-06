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

            if (meta.IsKey)
            {
                Keys.Add(meta);
            }
        }
    }
    public static Type Type { get; }
    public static List<TypeMetadataProperty> Properties { get; } = [];
    public static List<TypeMetadataProperty> Keys { get; } = [];
    public static Dictionary<string, TypeMetadataProperty> NameToPropertyMap { get; } = [];
}

public class TypeMetadataProperty
{
    public PropertyInfo PropertyInfo { get; }
    public Type Type { get; }
    public string PropertyName { get; }
    public bool IsKey { get; } = false;

    public TypeMetadataProperty(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
        Type = propertyInfo.PropertyType;
        if (propertyInfo.GetCustomAttribute<ColumnAttribute>() is {} columnAttribute)
        {
            PropertyName = columnAttribute.Name;
        }
        else
        {
            PropertyName = propertyInfo.Name.FirstLetterLowerCase();
        }

        if (propertyInfo.GetCustomAttribute<KeyAttribute>() is not null)
        {
            IsKey = true;
        }
    }
}