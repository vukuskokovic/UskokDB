﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Text;

namespace UskokDB;

public class DbIOOptions
{
    public Func<object?, string?> JsonWriter { get; init; } = (value) => value == null? null : System.Text.Json.JsonSerializer.Serialize(value);
    public Func<string?, Type, object?> JsonReader { get; init; } = (jsonStr, type) => jsonStr == null? null : System.Text.Json.JsonSerializer.Deserialize(jsonStr, type);
    public bool UseJsonForUnknownClassesAndStructs { get; init; } = false;
    public Dictionary<Type, IParameterConverter> ParameterConverters { get; } = new()
    {
        [typeof(Guid)] = new DefaultParameterConverter<Guid, string>((guid) => guid.ToString(), Guid.Parse, Guid.Empty.ToString().Length),
        [typeof(Guid?)] = new DefaultParameterConverter<Guid?, string?>((guid) => guid?.ToString(), (str) => str == null? null : Guid.Parse(str), Guid.Empty.ToString().Length)
    };
}

public class DbIO(DbContext dbContext)
{
    private static readonly HashSet<Type> PrimitiveTypes = [
        typeof(char),
        typeof(byte),
        typeof(int),
        typeof(uint),
        typeof(short),
        typeof(ushort),
        typeof(long),
        typeof(ulong),
        typeof(double),
        typeof(float),
        typeof(bool),
        typeof(char),
        typeof(string),
        typeof(decimal),
        typeof(DateTime)
    ];

    public bool ShouldJsonBeUsedForType(Type type)
    {
        return dbContext.DbIOOptions.UseJsonForUnknownClassesAndStructs && (type.IsClass || type.IsArray || type.IsValueType || type.IsEnum);
    }

    private const string NullValue = "NULL";
    private const string EmptyString = "''";
    /// <summary>
    /// Writes a value as a part of an sql request
    /// </summary>
    /// <param name="value">The value of a parameter</param>
    /// <returns>A sql like string of a object also clears all prohibited characters in case of a string</returns>

    public string WriteValue(object? value)
    {
        if (value is null) return NullValue;

        //all generic types
        if (value is byte or short or ushort or int or uint or long or ulong or bool or float or double or char or decimal)
        {
            return value.ToString()!;
        }

        if (value is DateTime dateTime)
        {
            return $"'{dateTime:yyyy-MM-dd HH:mm:ss}'";
        }

        if (value is string str)
        {
            return str.Length == 0 ?
                EmptyString :
                $"'{str.Replace("'", "\\'")}'";
        }

        var type = value.GetType();
        if (type.IsEnum)
        {
            return Convert.ChangeType(value, type.GetEnumUnderlyingType())?.ToString() ?? NullValue;
        }

        if (dbContext.DbIOOptions.ParameterConverters.TryGetValue(type, out var converter))
        {
            return WriteValue(converter.Write(value));
        }

        if (ShouldJsonBeUsedForType(type))
        {
            if (dbContext.DbIOOptions.JsonWriter == null) throw new NullReferenceException("Configured to use json for unknown types and structs but json writer was null");

            return WriteValue(dbContext.DbIOOptions.JsonWriter(value));
        }

        return value?.ToString() ?? NullValue;
    }

    public object? ReadValue(object? value, Type type)
    {
        if (value is null or DBNull) return null;

        if (dbContext.DbIOOptions.ParameterConverters.TryGetValue(type, out var parameterConverter))
        {
            return parameterConverter.Read(value);
        }

        if (PrimitiveTypes.Contains(type)) return value;

        if (type.IsEnum) return value;

        if (!ShouldJsonBeUsedForType(type)) return value;

        if (dbContext.DbIOOptions.JsonReader == null) throw new NullReferenceException("Configured to use json for unknown types and structs but json reader was null");
        if (value is not string jsonStr)
            throw new Exception($"Error reading type {type.FullName} did not get json string from the database");

        return dbContext.DbIOOptions.JsonReader(jsonStr, type);

    }

    public Dictionary<string, string> GetParamsHashmap(object? obj)
    {
        Dictionary<string, string> values = [];
        if (obj == null) return values;
        var type = obj.GetType();
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            values.Add(property.Name, WriteValue(value));
        }

        return values;
    }

    public string PopulateParams(string query, object? paramsObj)
    {
        if (paramsObj == null) return query;

        var querySpan = query.AsSpan();

        var paramsHashMap = GetParamsHashmap(paramsObj);
        StringBuilder builder = new();
        var startCursor = 0;
        var readingParam = false;
        for (var cursor = startCursor; cursor <= querySpan.Length; cursor++)
        {
            var isEnd = cursor == querySpan.Length;
            if (isEnd)
            {
                if (!readingParam)
                {
                    goto WriteCurrent;
                }
                goto WriteParam;
            }
            if (querySpan[cursor] == '@')
            {
                readingParam = true;
                goto WriteCurrent;
            }
            if (readingParam && !char.IsLetter(querySpan[cursor]) && !char.IsDigit(querySpan[cursor]) && querySpan[cursor] != '_')
            {
                readingParam = false;
                goto WriteParam;
            }

            continue;
        //Write the current text
        WriteCurrent:
            //Ignore if on current character
            if (startCursor == cursor) continue;
            builder.Append(querySpan.Slice(startCursor, cursor - startCursor).ToArray());
            startCursor = cursor;
            continue;

        //Write the parameter
        WriteParam:
            //Ignore the first character since it is going to be an '@'
            var paramName = querySpan.Slice(startCursor + 1, cursor - startCursor - 1).ToString();
            if (!paramsHashMap.TryGetValue(paramName, out var paramValue)) paramValue = NullValue;

            startCursor = cursor;
            builder.Append(paramValue);
            continue;
        }


        return builder.ToString();
    }

    internal T Read<T>(IDataReader reader) where T : class, new()
    {
        T newValue = new();

        foreach (var property in TypeMetadata<T>.Properties)
        {
            int ordinal = reader.GetOrdinal(property.PropertyName);
            var value = reader.GetValue(ordinal);
            property.PropertyInfo.SetValue(newValue, ReadValue(value, property.Type));
        }

        return newValue;
    }
}
