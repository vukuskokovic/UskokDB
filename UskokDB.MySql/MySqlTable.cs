using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UskokDB.MySql.Attributes;

namespace UskokDB.MySql
{
    public interface IMySqlTable
    {
    }

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    public class MySqlTable<T> : IMySqlTable where T : class, new()
    {
        public static TypeMetadataProperty? PrimaryKey { get; private set; } = null;
        public static string MySqlTableInitString { get; }
        public static string TableName { get; }
        private static readonly List<Tuple<string, string>> ForeignKeys = new();
        private const string Space = " ";
        static MySqlTable()
        {
            var tableType = typeof(T);
            var tableNameAttribute = tableType.GetCustomAttribute<TableNameAttribute>();

            TableName = tableNameAttribute?.Name ?? tableType.Name;
            
            var properties = TypeMetadata<T>.Properties;
            StringBuilder sb = new("CREATE TABLE IF NOT EXISTS `");
            sb.Append(TableName);
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

            if (ForeignKeys.Count > 0)
            {
                sb.Append(", ");
                sb.Append(string.Join(",", ForeignKeys.Select(x => $"FOREIGN KEY ({x.Item1}) REFERENCES {x.Item2}")));
            }

            
            sb.Append(')');
            MySqlTableInitString = sb.ToString();
            sb.Clear();
        }

        

        private static string GetPropertyTableInit(TypeMetadataProperty property)
        {
            var type = property.Type;
            var isKey = property.PropertyInfo.GetCustomAttribute<KeyAttribute>() is not null;
            
            var isNotNull = property.PropertyInfo.GetCustomAttribute<ColumnNotNullAttribute>() is not null;
            var isAutoIncrement = property.PropertyInfo.GetCustomAttribute<AutoIncrementAttribute>() is not null;
            var maxLength = property.PropertyInfo.GetCustomAttribute<MaxLengthAttribute>()?.Length;
            List<string> attributes = new();
            if(isNotNull)
            {
                attributes.Add("NOT NULL");
            }
            if (isAutoIncrement)
            {
                attributes.Add("AUTO_INCREMENT");
            }
            //IF property is foreignkey
            if(PropertyUtil.GetPropertyForeignKey(property) is Type tableType){
                const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
                var primaryKeyObj = tableType.GetProperty(nameof(PrimaryKey), bindingFlags)?.GetValue(null);
                if (primaryKeyObj is not TypeMetadataProperty primaryKeyMetaData)
                {
                    throw new Exception("Foreign key points to no primary key in foreign table, table: " + TableName);
                }
                var tableName = tableType.GetProperty(nameof(TableName), bindingFlags)?.GetValue(null);
                if(tableName is not string tableNameStr) 
                    throw new Exception("Foreign table has no name? Table: " + TableName);
                
                ForeignKeys.Add(new Tuple<string, string>(property.PropertyName, $"{tableNameStr}({primaryKeyMetaData.PropertyName})"));
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

        private static readonly Dictionary<Type, string> TypeInDatabase = new()
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

            if(TypeInDatabase.TryGetValue(type, out var str))
            {
                return str;
            }

            if(ParameterHandler.ParameterConverters.TryGetValue(type, out var converter))
            {
                maxLength = converter.GetCustomMaxLength();
                customTypeName = converter.GetCustomTypeInTable();
                return GetTypeTableType(converter.GetTableType(), maxLength, customTypeName);
            }

            if (ParameterHandler.ShouldJsonBeUsedForType(type))
            {
                return "JSON";
            }

            throw new InvalidOperationException($"Could not get type in table ({TableName}) of type {type}");
        }

        public static Task CreateIfNotExistAsync(DbConnection connection) => connection.ExecuteAsync(MySqlTableInitString);
        
        public static void CreateIfNotExist(IDbConnection connection) => connection.Execute(MySqlTableInitString);

        public static string GetInsertInstance(T instance)
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

        private static StringBuilder InsertInit(string command)
        {
            StringBuilder builder = new();
            builder.Append(command);
            builder.Append(" INTO `");
            builder.Append(TableName);
            builder.Append("` VALUES ");
            return builder;
        }



        public static string GetInsertString(T value, string command)
        {
            var builder = InsertInit(command);
            builder.Append(GetInsertInstance(value));
            return builder.ToString();
        }

        public static string GetInsertString(IEnumerable<T> values, string command)
        {
            var builder = InsertInit(command);
            var insertArray = values.Select(GetInsertInstance);
            builder.Append(string.Join(",", insertArray));
            return builder.ToString();
        }
        private const string INSERT = "INSERT";
        private const string REPLACE = "REPLACE";
        public static Task<int> InsertAsync(DbConnection connection, T value) => connection.ExecuteAsync(GetInsertString(value, INSERT));
        public static Task<int> InsertAsync(DbConnection connection, IEnumerable<T> values) => connection.ExecuteAsync(GetInsertString(values, INSERT));
        public static Task<int> InsertAsync(DbConnection connection, params T[] values) => connection.ExecuteAsync(GetInsertString(values, INSERT));
        public static Task<int> ReplaceAsync(DbConnection connection, T value) => connection.ExecuteAsync(GetInsertString(value, REPLACE));
        public static Task<int> ReplaceAsync(DbConnection connection, IEnumerable<T> values) => connection.ExecuteAsync(GetInsertString(values, REPLACE));
        public static Task<int> ReplaceAsync(DbConnection connection, params T[] values) => connection.ExecuteAsync(GetInsertString(values, REPLACE));

        
        public static int Insert(IDbConnection connection, T value) => connection.Execute(GetInsertString(value, INSERT));
        public static int Insert(IDbConnection connection, IEnumerable<T> values) => connection.Execute(GetInsertString(values, INSERT));
        public static int Insert(IDbConnection connection, params T[] values) => connection.Execute(GetInsertString(values, INSERT));

        public static int Replace(IDbConnection connection, T value) => connection.Execute(GetInsertString(value, REPLACE));
        public static int Replace(IDbConnection connection, IEnumerable<T> values) => connection.Execute(GetInsertString(values, REPLACE));
        public static int Replace(IDbConnection connection, params T[] values) => connection.Execute(GetInsertString(values, REPLACE));

        public static string GetColumnInString<TValue>(string columnName, IEnumerable<TValue> values)
        {
            return $"SELECT * FROM `{TableName}` WHERE {columnName} IN ({string.Join(",", values.Select(x => ParameterHandler.WriteValue(x)))})";
        }

        public static List<T> GetByColumn<TValue>(IDbConnection connection, string columnName, IEnumerable<TValue> values) => connection.Query<T>(GetColumnInString(columnName, values));
        public static Task<List<T>> GetByColumnAsync<TValue>(DbConnection connection, string columnName, IEnumerable<TValue> values) => connection.QueryAsync<T>(GetColumnInString(columnName, values));
        
        private static string GetByKeySqlString(object value)
        {
            if (PrimaryKey == null) throw new InvalidOperationException($"Table {TableName} has no primary key");
            return $"SELECT * FROM `{TableName}` WHERE {PrimaryKey.PropertyName}={ParameterHandler.WriteValue(value)} LIMIT 1";
        }

        public static T? GetByKey(IDbConnection connection, object keyValue) =>
            connection.QuerySingle<T>(GetByKeySqlString(keyValue));
        public static Task<T?> GetByKeyAsync(DbConnection connection, object keyValue) =>
            connection.QuerySingleAsync<T>(GetByKeySqlString(keyValue));


        private static string GetByKeysSqlString<TValue>(IEnumerable<TValue> values)
        {
            if (PrimaryKey == null) throw new InvalidOperationException($"Table {TableName} has no primary key");

            return GetColumnInString(PrimaryKey.PropertyName, values);
        }
        
        public static List<T> GetByKeys<TValue>(IDbConnection connection, IEnumerable<TValue> values) =>
            connection.Query<T>(GetByKeysSqlString(values));
        public static Task<List<T>> GetByKeysAsync<TValue>(DbConnection connection, IEnumerable<TValue> values) =>
            connection.QueryAsync<T>(GetByKeysSqlString(values));

        public static FilterBuilder CreateFilterBuilder(string type = "AND") => new(TableName, type);
    }
}
