using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UskokDB.MySql
{
    public class MySqlTable<T> where T : class, new()
    {
        public string MySqlTableInitString { get; }
        public string TableName { get; }

        private TypeMetadataProperty? PrimaryKey = null;
        private const char Space = ' ';
        private string GetPropertyTableInit(TypeMetadataProperty property)
        {
            var type = property.Type;
            bool isKey = property.PropertyInfo.GetCustomAttribute<KeyAttribute>() is not null;
            bool isNotNull = property.PropertyInfo.GetCustomAttribute<ColumnNotNull>() is not null;
            bool isAutoIncrement = property.PropertyInfo.GetCustomAttribute<AutoIncrement>() is not null;
            int? maxLength = property.PropertyInfo.GetCustomAttribute<MaxLengthAttribute>()?.Length;
            List<string> attributes = new();
            if(isNotNull)
            {
                attributes.Add("NOT NULL");
            }
            if (isAutoIncrement)
            {
                attributes.Add("AUTO_INCREMENT");
            }

            if (isKey)
            {
                if (PrimaryKey != null) throw new InvalidOperationException($"Table `{TableName}` has two primary keys");
                PrimaryKey = property;
                attributes.Add("PRIMARY KEY");
            }

            StringBuilder builder = new(property.PropertyName);
            builder.Append(Space);
            builder.Append(GetTypeTableType(type, maxLength));
            if(attributes.Count > 0)
            {
                builder.Append(Space);
                builder.Append(string.Join(Space, attributes));
            }
            attributes.Clear();
            

            return builder.ToString();
        }

        private static readonly Dictionary<Type, string> typeInDatabase = new()
        {
            [typeof(int)] = "INT",
            [typeof(uint)] = "INT UNSIGNED",
            [typeof(short)] = "SMALLINT",
            [typeof(ushort)] = "SMALLINT UNSIGNED",
            [typeof(long)] = "BIGINT",
            [typeof(ulong)] = "BIGINT UNSIGNED",

            [typeof(float)] = "FLOAT",
            [typeof(double)] = "DOUBLE",
            [typeof(decimal)] = "DECIMAL",

            [typeof(byte)] = "BYTE",
            [typeof(bool)] = "BOOL",
            [typeof(char)] = "CHAR",
        };

        private static string GetTypeTableType(Type type, int? maxLength, string? customTypeName = null)
        {
            if (type == typeof(string))
            {
                if(customTypeName != null)
                {
                    return customTypeName;
                }

                if (maxLength.HasValue)
                    return $"VARCHAR({maxLength.Value})";
                
                return "TEXT";
            }
            if (type.IsEnum)
            {
                return GetTypeTableType(type.GetEnumUnderlyingType(), maxLength);
            }

            if(typeInDatabase.TryGetValue(type, out var str))
            {
                return str;
            }

            if(ParameterHandler.ParamterConverters.TryGetValue(type, out var converter))
            {
                maxLength = converter.GetCustomMaxLength();
                customTypeName = converter.GetCustomTypeInTable();
                return GetTypeTableType(converter.GetTableType(), maxLength, customTypeName);
            }

            throw new InvalidOperationException($"Could not get type in table of type {type}");
        }

        public MySqlTable(string tableName){
            TableName = tableName;
            var properties = TypeMetadata<T>.Properties;
            StringBuilder sb = new("CREATE TABLE IF NOT EXISTS `");
            sb.Append(tableName);
            sb.Append("` ");
            sb.Append('(');
            for(var i = 0; i < properties.Count; i++)
            {
                var isLast = i == properties.Count - 1;

                sb.Append(GetPropertyTableInit(properties[i]));

                if (!isLast)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(')');
            MySqlTableInitString = sb.ToString();
            sb.Clear();
        }

        public Task CreateIfNotExistAsync(DbConnection connection) => connection.ExecuteAsync(MySqlTableInitString);
        public void CreateIfNotExist(IDbConnection connection) => connection.Execute(MySqlTableInitString);


        public string GetInstertInstance(T instance)
        {
            StringBuilder builder = new();
            builder.Append('(');
            var list = TypeMetadata<T>.Properties;
            var last = list[^1];
            foreach (var property in list)
            {
                var value = property.PropertyInfo.GetValue(instance);
                builder.Append(ParameterHandler.WriteValue(value));
                if(property != last)
                {
                    builder.Append(',');
                }
            }
            builder.Append(')');
            return builder.ToString();
        }

        private StringBuilder InstertInit()
        {
            StringBuilder builder = new();
            builder.Append("INSERT INTO `");
            builder.Append(TableName);
            builder.Append("` VALUES ");
            return builder;
        }

        public string GetInstertString(T value)
        {
            var builder = InstertInit();
            builder.Append(GetInstertInstance(value));
            return builder.ToString();
        }

        public string GetInstertString(IEnumerable<T> values)
        {
            var builder = InstertInit();
            IEnumerable<string> instertArray = values.Select(GetInstertInstance);
            builder.Append(string.Join(',', instertArray));
            return builder.ToString();
        }

        public Task<int> InsertAsync(DbConnection connection, T value) => connection.ExecuteAsync(GetInstertString(value));
        public int Insert(IDbConnection connection, T value) => connection.Execute(GetInstertString(value));

        public Task<int> InstertAsync(DbConnection connection, IEnumerable<T> values) => connection.ExecuteAsync(GetInstertString(values));
        public int Instert(IDbConnection connection, IEnumerable<T> values) => connection.Execute(GetInstertString(values));

        public Task<int> InstertAsync(DbConnection connection, params T[] values) => connection.ExecuteAsync(GetInstertString(values));
        public int Instert(IDbConnection connection, params T[] values) => connection.Execute(GetInstertString(values));
    }
}
