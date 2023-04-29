using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UskokDB.MySql
{
    public interface IMySqlTable
    {
        public Task CreateIfNotExistAsync(DbConnection connection);
        public void CreateIfNotExist(IDbConnection connection);
        public string GetCreationString();
    }

    public class MySqlTable<T> : IMySqlTable where T : class, new()
    {
        public string MySqlTableInitString { get; }
        public string TableName { get; }

        private TypeMetadataProperty? PrimaryKey = null;
        private const string Space = " ";
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
            [typeof(DateTime)] = "DATETIME"
        };

        private string GetTypeTableType(Type type, int? maxLength, string? customTypeName = null)
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

            throw new InvalidOperationException($"Could not get type in table ({TableName}) of type {type}");
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


        public string GetCreationString() => MySqlTableInitString;

#if NETSTANDARD2_0
        public void CreateIfNotExist(IDbConnection connection) => connection.Execute(MySqlTableInitString.AsSpan());
#else
        public void CreateIfNotExist(IDbConnection connection) => connection.Execute(MySqlTableInitString);
#endif


        public string GetInstertInstance(T instance)
        {
            StringBuilder builder = new();
            builder.Append('(');
            var list = TypeMetadata<T>.Properties;
            var last = list[list.Count-1];
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

        private StringBuilder InstertInit(string command)
        {
            StringBuilder builder = new();
            builder.Append(command);
            builder.Append(" INTO `");
            builder.Append(TableName);
            builder.Append("` VALUES ");
            return builder;
        }



        public string GetInsertString(T value, string command)
        {
            var builder = InstertInit(command);
            builder.Append(GetInstertInstance(value));
            return builder.ToString();
        }

        public string GetInsertString(IEnumerable<T> values, string command)
        {
            var builder = InstertInit(command);
            IEnumerable<string> instertArray = values.Select(GetInstertInstance);
            builder.Append(string.Join(",", instertArray));
            return builder.ToString();
        }
        private const string INSERT = "INSERT";
        private const string REPLACE = "REPLACE";
        //Inseriting
        public Task<int> InsertAsync(DbConnection connection, T value) => connection.ExecuteAsync(GetInsertString(value, INSERT));
        public Task<int> InsertAsync(DbConnection connection, IEnumerable<T> values) => connection.ExecuteAsync(GetInsertString(values, INSERT));
        public Task<int> InsertAsync(DbConnection connection, params T[] values) => connection.ExecuteAsync(GetInsertString(values, INSERT));
        public Task<int> ReplaceAsync(DbConnection connection, T value) => connection.ExecuteAsync(GetInsertString(value, REPLACE));
        public Task<int> ReplaceAsync(DbConnection connection, IEnumerable<T> values) => connection.ExecuteAsync(GetInsertString(values, REPLACE));
        public Task<int> ReplaceAsync(DbConnection connection, params T[] values) => connection.ExecuteAsync(GetInsertString(values, REPLACE));


#if NETSTANDARD2_0
        public int Insert(IDbConnection connection, T value) => connection.Execute(GetInsertString(value, INSERT).AsSpan());
        public int Insert(IDbConnection connection, IEnumerable<T> values) => connection.Execute(GetInsertString(values, INSERT).AsSpan());
        public int Insert(IDbConnection connection, params T[] values) => connection.Execute(GetInsertString(values, INSERT).AsSpan());

        public int Replace(IDbConnection connection, T value) => connection.Execute(GetInsertString(value, REPLACE).AsSpan());
        public int Replace(IDbConnection connection, IEnumerable<T> values) => connection.Execute(GetInsertString(values, REPLACE).AsSpan());
        public int Replace(IDbConnection connection, params T[] values) => connection.Execute(GetInsertString(values, REPLACE).AsSpan());
#else
        public int Insert(IDbConnection connection, T value) => connection.Execute(GetInsertString(value, INSERT));
        public int Insert(IDbConnection connection, IEnumerable<T> values) => connection.Execute(GetInsertString(values, INSERT));
        public int Insert(IDbConnection connection, params T[] values) => connection.Execute(GetInsertString(values, INSERT));

        public int Replace(IDbConnection connection, T value) => connection.Execute(GetInsertString(value, REPLACE));
        public int Replace(IDbConnection connection, IEnumerable<T> values) => connection.Execute(GetInsertString(values, REPLACE));
        public int Replace(IDbConnection connection, params T[] values) => connection.Execute(GetInsertString(values, REPLACE));
#endif

        public string GetColumnInString<TValue>(string columnName, IEnumerable<TValue> values)
        {
            return $"SELECT * FROM `{TableName}` WHERE {columnName} IN ({string.Join(",", values.Select(x => ParameterHandler.WriteValue(x)))})";
        }

        public List<T> GetByColumn<TValue>(IDbConnection connection, string columnName, IEnumerable<TValue> values) => connection.Query<T>(GetColumnInString(columnName, values).AsSpan());
        public Task<List<T>> GetByColumnAsync<TValue>(DbConnection connection, string columnName, IEnumerable<TValue> values) => connection.QueryAsync<T>(GetColumnInString(columnName, values));
        
        private string GetByKeySqlString(object value)
        {
            if (PrimaryKey == null) throw new InvalidOperationException($"Table {TableName} has no primary key");
            return $"SELECT * FROM `{TableName}` WHERE {PrimaryKey.PropertyName}={ParameterHandler.WriteValue(value)} LIMIT 1";
        }

        public T? GetByKey(IDbConnection connection, object keyValue) =>
            connection.QuerySingle<T>(GetByKeySqlString(keyValue).AsSpan());
        public Task<T?> GetByKeyAsync(DbConnection connection, object keyValue) =>
            connection.QuerySingleAsync<T>(GetByKeySqlString(keyValue));


        private string GetByKeysSqlString<TValue>(IEnumerable<TValue> values)
        {
            if (PrimaryKey == null) throw new InvalidOperationException($"Table {TableName} has no primary key");

            return GetColumnInString(PrimaryKey.PropertyName, values);
        }
        
        public List<T?> GetByKeys<TValue>(IDbConnection connection, IEnumerable<TValue> values) =>
            connection.Query<T>(GetByKeysSqlString(values).AsSpan());
        public Task<List<T?>> GetByKeysAsync<TValue>(DbConnection connection, IEnumerable<TValue> values) =>
            connection.QueryAsync<T>(GetByKeysSqlString(values));
    }
}
