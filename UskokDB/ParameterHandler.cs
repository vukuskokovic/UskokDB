using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UskokDB
{
    public static class ParameterHandler
    {
        private static HashSet<Type> primitiveTypes = new() { 
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
            typeof(decimal)
        };

        public static Dictionary<Type, IParamterConverter> ParamterConverters = new()
        {
            [typeof(Guid)] = new DefaultParamterConverter<Guid, string>((guid) => guid.ToString(), Guid.Parse, Guid.Empty.ToString().Length)
        };

        /// <summary>
        /// Default prohibited characters inside of a string
        /// </summary>
        public static HashSet<char> StringProhibitedCharacters = new() {
            '\'',
            '<',
            '>'
        };

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
                return value?.ToString() ?? NullValue;
            }

            if (value is string str)
            {
                if (str.Length == 0) return EmptyString;
                var asSpan = str.AsSpan();
                var sb = new StringBuilder();
                sb.Append('\'');
                int startCursor = 0;
                for(int cursor = 0; cursor <= asSpan.Length; cursor++)
                {
                    bool isEnd = cursor == asSpan.Length;
                    //Writes the current string and ignores a prohibited character
                    if (isEnd || StringProhibitedCharacters.Contains(asSpan[cursor]))
                    {
                        sb.Append(asSpan[startCursor..cursor]);
                        startCursor = cursor;
                    }
                }


                sb.Append('\'');
                return sb.ToString();
            }


            var type = value.GetType();
            if (type.IsEnum)
            {
                return Convert.ChangeType(value, type.GetEnumUnderlyingType())?.ToString() ?? NullValue;
            }

            if(ParamterConverters.TryGetValue(type, out var converter))
            {

                return WriteValue(converter.Write(value));
            }


            return value?.ToString() ?? NullValue;
        }

        public static object? ReadValue(object? value, Type type)
        {
            if (value is null or DBNull) return null;

            if (primitiveTypes.Contains(type)) return value;


            if(ParamterConverters.TryGetValue(type, out var paramterConverter))
            {
                return paramterConverter.Read(value);
            }

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
            int startCursor = 0;
            bool readingParam = false;
            for(int cursor = startCursor; cursor <= querySpan.Length; cursor++)
            {
                bool isEnd = cursor == querySpan.Length;
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
                else if(readingParam && !char.IsLetter(querySpan[cursor]) && !char.IsDigit(querySpan[cursor]))
                {
                    readingParam = false;
                    goto WriteParam;
                }

                continue;
            //Write the current text
            WriteCurrent:
                //Ignore if on current character
                if (startCursor == cursor) continue;
                builder.Append(querySpan[startCursor..cursor]);
                startCursor = cursor;
                continue;

            //Write the parameter
            WriteParam:
                //Ignore the first character since it is going to be an '@'
                string paramName = querySpan.Slice(startCursor + 1, cursor - startCursor - 1).ToString();
                if (!paramsHashMap.TryGetValue(paramName, out var paramValue)) paramValue = NullValue;

                startCursor = cursor;
                builder.Append(paramValue);
                continue;
            }


            return builder.ToString();
        }
    }
}
