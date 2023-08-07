using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace UskokDB
{
    public static class DbConnectionExtensions
    {
        private static Task OpenIfNotOpen(this DbConnection dbConnection)
        {
            if(dbConnection.State != ConnectionState.Open)
            {
                return dbConnection.OpenAsync();
            }
            return Task.CompletedTask;
        }

        public static async Task<List<dynamic>> QueryAsync(this DbConnection connection, string commandStr, object? properties = null)
        {
            await connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandStr, properties);
            using var reader = await command.ExecuteReaderAsync();
            List<dynamic> list = new();

            int columnCount = reader.FieldCount;
            string[] names = reader.GetColumnNames(columnCount);

            while (await reader.ReadAsync())
            {
                list.Add(reader.ReadDynamic(columnCount, names));
            }

            return list;
        }

        public static async Task<dynamic?> QuerySingleAsync(this DbConnection connection, string commandString, object? properties = null)
        {
            await connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandString, properties);
            using var reader = await command.ExecuteReaderAsync();
            List<dynamic> list = new();
            var columnCount = reader.FieldCount;
            var names = reader.GetColumnNames(columnCount);

            if (!await reader.ReadAsync()) return null;

            return reader.ReadDynamic(columnCount, names);
        }

        public static async Task<List<T>> QueryAsync<T>(this DbConnection connection, string commandString, object? properties = null) where T : class, new()
        {
            await connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandString, properties);
            using var reader = await command.ExecuteReaderAsync();
            List<T> list = new();
            while (await reader.ReadAsync())
            {
                list.Add(reader.Read<T>());
            }

            return list;
        }

        public static async Task<T?> QuerySingleAsync<T>(this DbConnection connection, string commandString, object? properties = null) where T : class, new()
        {
            await connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandString, properties);
            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;
            return reader.Read<T>();
        }

        public static async Task<int> ExecuteAsync(this DbConnection connection, string commandString, object? properties = null)
        {
            await connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandString, properties);
            return await command.ExecuteNonQueryAsync();
        }

        public static async Task<T?> ExecuteScalarAsync<T>(this DbConnection connection, string commandString, object? properties = null)
        {
            await connection.OpenIfNotOpen();
            using var command = connection.CreateCommand();
            command.CommandText = ParameterHandler.PopulateParams(commandString, properties);
            var value = await command.ExecuteScalarAsync();
            if (value is null or DBNull) return default;
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
