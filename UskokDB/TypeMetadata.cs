using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UskokDB.Attributes;

namespace UskokDB;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public static class TypeMetadata<T> where T : class, new()
{
    static TypeMetadata()
    {
        var classType = typeof(T);
        var properties = classType.GetProperties();
        foreach (var property in properties.Where(p => p.GetCustomAttribute<NotMappedAttribute>() is null))
        {
            var meta = new TypeMetadataProperty(property);
            var getterMethod = CreateGetter(property);
            

            meta.GetMethod = (obj) => getterMethod((T)obj);
            meta.SetterMethod = CreateSetter(property);
            NameToPropertyMap[property.Name] = meta;
            Properties.Add(meta);

            if (meta.IsKey)
            {
                Keys.Add(meta);
            }
        }
    }
    
    private static Func<T, object?> CreateGetter(PropertyInfo property)
    {
        var instance = Expression.Parameter(typeof(T), "instance");
        var propertyAccess = Expression.Property(instance, property);
        var convert = Expression.Convert(propertyAccess, typeof(object)); // box value types
        var lambda = Expression.Lambda<Func<T, object?>>(convert, instance);
        return lambda.Compile();
    }
    
    private static Action<object, object?> CreateSetter(PropertyInfo property)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var valueParam = Expression.Parameter(typeof(object), "value");

        // Cast instance to the right type
        var typedInstance = Expression.Convert(instanceParam, property.DeclaringType!);

        // Cast value to property type (unbox for value types)
        var typedValue = Expression.Convert(valueParam, property.PropertyType);

        var propertyAccess = Expression.Property(typedInstance, property);
        var assign = Expression.Assign(propertyAccess, typedValue);

        var lambda = Expression.Lambda<Action<object, object?>>(
            assign,
            instanceParam,
            valueParam
        );

        return lambda.Compile();
    }
    
    public static List<TypeMetadataProperty> Properties { get; } = [];
    public static List<TypeMetadataProperty> Keys { get; } = [];
    public static Dictionary<string, TypeMetadataProperty> NameToPropertyMap { get; } = [];
}

public class TypeMetadataProperty
{
    public PropertyInfo PropertyInfo { get; }
    public Type Type { get; }
    public string PropertyName { get; }
    public bool IsKey { get; }
    public Func<object, object?> GetMethod { get; set; } = null!;
    public Action<object, object?> SetterMethod { get; set; } = null!;
    internal bool IsGuidOrNullableGuid { get; set; }
    internal bool IsCharOrNullableChar { get; set; }

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
        
        IsGuidOrNullableGuid = Type == typeof(Guid) || Type == typeof(Guid?);
        IsCharOrNullableChar = Type == typeof(char) || Type == typeof(char?);
    }
}