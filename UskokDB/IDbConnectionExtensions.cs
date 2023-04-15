using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Threading.Tasks;
namespace UskokDB
{
    public static class IDbConnectionExtensions
    {
        private static void OpenIfNotOpen(this IDbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        #region Dynamic read
        internal static dynamic ReadDynamic(this IDataReader reader, int fieldCount, string[] names)
        {
            IDictionary<string, object?> newValue = new ExpandoObject();
            for (var i = 0; i < fieldCount; i++)
            {
                var name = names[i];
                var value = reader.GetValue(i);
                if (value is DBNull or null)
                {
                    newValue.Add(name, null);
                    continue;
                }
                newValue.Add(name, value);
            }

            return newValue;
        }

        internal static string[] GetColumnNames(this IDataReader reader, int fieldCount)
        {
            string[] names = new string[fieldCount];
            for (var i = 0; i < fieldCount; i++)
                names[i] = reader.GetName(i);

            return names;
        }

        public static List<dynamic> Query(this IDbConnection connection, ReadOnlySpan<char> commandSpan, object? properties = null)
        {
            connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandSpan, properties);
            using var reader = command.ExecuteReader();
            List<dynamic> list = new();

            int columnCount = reader.FieldCount;
            string[] names = reader.GetColumnNames(columnCount);

            while (reader.Read())
            {
                list.Add(reader.ReadDynamic(columnCount, names));
            }

            return list;
        }

        public static dynamic? QuerySingle(this IDbConnection connection, ReadOnlySpan<char> commandSpan, object? properties = null)
        {
            connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandSpan, properties);
            using var reader = command.ExecuteReader();
            List<dynamic> list = new();
            int columnCount = reader.FieldCount;
            string[] names = reader.GetColumnNames(columnCount);

            if (!reader.Read()) return null;

            return reader.ReadDynamic(columnCount, names);
        }
        #endregion

        internal static T Read<T>(this IDataReader reader) where T : class, new()
        {
            T newValue = new();

            foreach (var property in TypeMetadata<T>.Properties)
            {
                int ordinal = reader.GetOrdinal(property.PropertyName);
                var value = reader.GetValue(ordinal);
                property.PropertyInfo.SetValue(newValue, ParameterHandler.ReadValue(value, property.Type));
            }

            return newValue;
        }


        public static List<T> Query<T>(this IDbConnection connection, ReadOnlySpan<char> commandSpan, object? properties = null) where T : class, new()
        {
            connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandSpan, properties);
            using var reader = command.ExecuteReader();
            List<T> list = new();
            while (reader.Read())
            {
                list.Add(reader.Read<T>());
            }

            return list;
        }

        public static T? QuerySingle<T>(this IDbConnection connection, ReadOnlySpan<char> commandSpan, object? properties = null) where T : class, new()
        {
            connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandSpan, properties);
            using var reader = command.ExecuteReader();

            if (!reader.Read()) return null;

            return reader.Read<T>();
        }

        public static int Execute(this IDbConnection connection, ReadOnlySpan<char> commandSpan, object? properties = null)
        {
            connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandSpan, properties);
            return command.ExecuteNonQuery();
        }

        public static T? ExecuteScalar<T>(this IDbConnection connection, string commandStr, object? properties = null)
        {
           connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandStr.AsSpan(), properties);
            var value = command.ExecuteScalar();
            if (value is null or DBNull) return default;
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
