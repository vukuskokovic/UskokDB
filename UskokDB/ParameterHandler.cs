using System;
using System.Collections.Generic;
using System.Text;

namespace UskokDB
{
    public static class ParameterHandler
    {
        /// <summary>
        /// When writing/reading a class or struct type 
        /// </summary>
        public static bool UseJsonForUnknownClassesAndStructs = false;

        public static Func<object?, string>? JsonWriter = null;
        public static Func<string?, Type, object?>? JsonReader = null;


        private static HashSet<Type> PrimitiveTypes = new() { 
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
        };

        public static Dictionary<Type, IParameterConverter> ParameterConverters = new()
        {
            [typeof(Guid)] = new DefaultParameterConverter<Guid, string>((guid) => guid.ToString(), Guid.Parse, Guid.Empty.ToString().Length)
        };

        public static bool ShouldJsonBeUsedForType(Type type)
        {
            return UseJsonForUnknownClassesAndStructs && (type.IsClass || type.IsArray || type.IsValueType);
        }

        private const string NullValue = "NULL";
        private const string EmptyString = "''";
        /// <summary>
        /// Writes a value as a part of an sql request
        /// </summary>
        /// <param name="value">The value of a parameter</param>
        /// <returns>A sql like string of a object also clears all prohibited characters in case of a string</returns>
        public static string WriteValue(object? value)
        {
            if(value is null) return NullValue;

            //all generic types
            if (value is byte or short or ushort or int or uint or long or ulong or bool or float or double or char or decimal)
            {
                return value.ToString();
            }

            if (value is DateTime dateTime)
            {
                return $"'{dateTime:yyyy-MM-dd HH:mm:ss}'";
            }

            if (value is string str)
            {
                return str.Length == 0 ? 
                    EmptyString : 
                    $"'{str.Replace('\'', '\0')}'";
            }
            
            var type = value.GetType();
            if (type.IsEnum)
            {
                return Convert.ChangeType(value, type.GetEnumUnderlyingType())?.ToString() ?? NullValue;
            }
            
            if(ParameterConverters.TryGetValue(type, out var converter))
            {
                return WriteValue(converter.Write(value));
            }

            if (ShouldJsonBeUsedForType(type))
            {
                if (JsonWriter == null) throw new NullReferenceException("Configured to use json for unknown types and structs but json writer was null");
                
                return WriteValue(JsonWriter(value));
            }
            
            return value?.ToString() ?? NullValue;
        }

        public static object? ReadValue(object? value, Type type)
        {
            if (value is null or DBNull) return null;
            
            if(ParameterConverters.TryGetValue(type, out var parameterConverter))
            {
                return parameterConverter.Read(value);
            }
            
            if (ShouldJsonBeUsedForType(type))
            {
                if (JsonReader == null) throw new NullReferenceException("Configured to use json for unknown types and structs but json reader was null");
                if(value is not string jsonStr) 
                    throw new Exception($"Error reading type {type.FullName} did not get json string from the database");
                
                return JsonReader(jsonStr, type);
            }
            
            if (PrimitiveTypes.Contains(type)) return value;

            return value;
        }

        public static Dictionary<string, string> GetParamsHashmap(object? obj)
        {
            Dictionary<string, string> values = new();
            if (obj == null) return values;
            var type = obj.GetType();
            var properties = type.GetProperties();
            
            foreach(var property in properties) {
                var value = property.GetValue(obj);
                values.Add(property.Name, WriteValue(value));
            }

            return values;
        }

        public static string PopulateParams(ReadOnlySpan<char> querySpan, object? paramsObj)
        {
            if (paramsObj == null) return querySpan.ToString();

            var paramsHashMap = GetParamsHashmap(paramsObj);
            StringBuilder builder = new();
            var startCursor = 0;
            var readingParam = false;
            for(var cursor = startCursor; cursor <= querySpan.Length; cursor++)
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
                if(readingParam && !char.IsLetter(querySpan[cursor]) && !char.IsDigit(querySpan[cursor]))
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
    }
}
