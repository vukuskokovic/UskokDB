using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UskokDB.Attributes;

namespace UskokDB
{
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    public static class TypeMetadata<T> where T : class, new()
    {
        static TypeMetadata()
        {
            Type = typeof(T);
            var properties = Type.GetProperties();
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<NotMappedAttribute>() is not null) continue;

                Properties.Add(new TypeMetadataProperty(property));
            }
        }

        public static Type Type { get; }
        public static List<TypeMetadataProperty> Properties { get; } = new();
    }

    public class TypeMetadataProperty
    {
        public PropertyInfo PropertyInfo { get; }
        public Type Type { get; }
        public string PropertyName { get; }

        public TypeMetadataProperty(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;;
            Type = propertyInfo.PropertyType;
            if (propertyInfo.GetCustomAttribute<ColumnAttribute>() is { } columnAttribute)
            {
                PropertyName = columnAttribute.Name;
            }
            else
            {
                PropertyName = propertyInfo.Name.FirstLetterLowerCase();
            }
        }
    }
}
