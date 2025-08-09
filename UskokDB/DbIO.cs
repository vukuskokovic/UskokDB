﻿﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Text;

namespace UskokDB;

public static class DbIOOptions
{
    #if !NETSTANDARD2_0
    public static Func<object?, string?> JsonWriter { get; set; } = (value) => value == null? null : System.Text.Json.JsonSerializer.Serialize(value);
    public static Func<string?, Type, object?> JsonReader { get; set; } = (jsonStr, type) => jsonStr == null? null : System.Text.Json.JsonSerializer.Deserialize(jsonStr, type);
    #else
    public static Func<object?, string?>? JsonWriter { get; set; } = null;
    public static Func<string?, Type, object?>? JsonReader { get; set; } = null;
    #endif
    
    public static bool UseJsonForUnknownClassesAndStructs { get; set; } = false;
    public static Dictionary<Type, IColumnValueConverter> ParameterConverters { get; } = [];
}

public static class DbIO
{
    private static readonly HashSet<Type> PrimitiveTypes = [
        typeof(char),
        typeof(char?),
        typeof(byte),
        typeof(byte?),
        typeof(int),
        typeof(int?),
        typeof(uint),
        typeof(uint?),
        typeof(short),
        typeof(short?),
        typeof(ushort),
        typeof(ushort?),
        typeof(long),
        typeof(long?),
        typeof(ulong),
        typeof(ulong?),
        typeof(double),
        typeof(double?),
        typeof(float),
        typeof(float?),
        typeof(bool),
        typeof(bool?),
        typeof(string),
        typeof(decimal),
        typeof(decimal?),
        typeof(DateTime),
        typeof(DateTime?),
        typeof(Guid),
        typeof(Guid?)
    ];

    public static bool ShouldJsonBeUsedForType(Type type)
    {
        return DbIOOptions.UseJsonForUnknownClassesAndStructs && (type.IsClass || type.IsArray || type.IsValueType || type.IsEnum);
    }
    
    /// <summary>
    /// Writes a value as a part of a sql request
    /// </summary>
    /// <param name="value">The value of a parameter</param>
    /// <returns>A sql like string of an object also clears all prohibited characters in case of a string</returns>

    public static object? WriteValue(object? value)
    {
        if (value is null) return null;
        
        
        var type = value.GetType();
        if (PrimitiveTypes.Contains(type))
        {
            return value;
        }
        
        if (type.IsEnum)
        {
            return Convert.ChangeType(value, type.GetEnumUnderlyingType());
        }

        if (DbIOOptions.ParameterConverters.TryGetValue(type, out var converter))
        {
            return WriteValue(converter.Write(value));
        }

        if (ShouldJsonBeUsedForType(type))
        {
            if (DbIOOptions.JsonWriter == null) 
                throw new UskokDbIoException("Configured to use json for unknown types and structs but json writer was null");

            return WriteValue(DbIOOptions.JsonWriter(value));
        }

        return value.ToString();
    }

    public static DbPopulateParamsResult PopulateParams(string query, object? paramObj)
    {
        var result = new DbPopulateParamsResult()
        {
            CompiledText = query,
            Params = GetParamsList(paramObj)
        };
        return result;
    }
    private static List<DbParam> GetParamsList(object? obj)
    {
        List<DbParam> paramList = [];
        if (obj == null) return paramList;
        var type = obj.GetType();
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            var propertyName = property.Name;
            //this can never happen but why not
            if (propertyName.Length == 0)
                throw new UskokDbException("Wtf is this shit propertyName length 0");

            if (propertyName[0] != '@')
            {
                propertyName = $"@{propertyName}";
            }
            
            paramList.Add(new DbParam()
            {
                Name = propertyName,
                Value = value
            });
        }

        return paramList;
    }
    
    internal static T Read<T>(DbDataReader reader) where T : class, new()
    {
        T valueToBePopulated = new();

        if (valueToBePopulated is IDbManualReader fastReader)
        {
            fastReader.ReadValue(reader);
        }
        else
        {
            var len = TypeMetadata<T>.Properties.Count;
            var values = new object[len];
            var objectsRead = reader.GetValues(values);
            if (len != objectsRead) throw new UskokDbIoException($"Values read is not same as objectsRead(read:{objectsRead}!=expected:{len})");
            for (var ordinal = 0; ordinal < len; ordinal++)
            {
                var property = TypeMetadata<T>.Properties[ordinal];
                ReadValue(values[ordinal], property, valueToBePopulated);
            }
        }
        return valueToBePopulated;
    }

    private static void ReadValue<T>(object? value, TypeMetadataProperty property, T objectHolder) where T : class
    {
        if (value is null or DBNull)
        {
            property.SetterMethod(objectHolder, null);
            return;
        }

        if (property.IsGuidOrNullableGuid)
        {
            property.SetterMethod(objectHolder, Guid.Parse((string)value));
            return;
        }

        if (property.IsCharOrNullableChar)
        {
            property.SetterMethod(objectHolder, char.Parse((string)value));
            return;
        }

        var type = property.Type;
        if (PrimitiveTypes.Contains(type))
        {
            property.SetterMethod(objectHolder, value);
            return;
        }

        if (DbIOOptions.ParameterConverters.TryGetValue(type, out var parameterConverter))
        {
            property.SetterMethod(objectHolder, parameterConverter.Read(value));
            return;
        }

        if (type.IsEnum || !ShouldJsonBeUsedForType(type))
        {
            property.SetterMethod(objectHolder, value);
            return;
        }

        if (DbIOOptions.JsonReader == null) throw new UskokDbIoException("Configured to use json for unknown types and structs but json reader was null");
        if (value is not string jsonStr)
            throw new UskokDbIoException($"Error reading type {type.FullName} did not get json string from the database");

        property.SetterMethod(objectHolder, DbIOOptions.JsonReader(jsonStr, type));
    }
}