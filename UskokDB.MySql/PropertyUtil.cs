using System;
using System.Linq;
using UskokDB;

internal static class PropertyUtil
{
    internal static Type? GetPropertyForeignKey(TypeMetadataProperty property){
            var foreignKeyAttribute = property.PropertyInfo.GetCustomAttributes(true).FirstOrDefault(attribute =>
            {
                var attributeType = attribute.GetType();
                return attributeType.IsGenericType;
            })?.GetType();

            if (foreignKeyAttribute != null)
            {
                var tableType = foreignKeyAttribute.GenericTypeArguments[0];
                return tableType;
            }

            return null;
        }
}