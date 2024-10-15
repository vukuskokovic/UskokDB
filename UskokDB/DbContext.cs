using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UskokDB;

public enum DbType
{
    MySQL,
    PostgreSQL,
    SQLite
}

public delegate DbConnection DbConnectionFactory();

public abstract class DbContext : IDisposable, IAsyncDisposable
{
    private static DbIOOptions? _defaultOptions;
    public DbContext(DbType type, DbConnectionFactory connectionFactory, DbIOOptions? ioOptions = null)
    {
        DbType = type;
        DbConnection = connectionFactory();
        DbIO = new DbIO(this);
        if (ioOptions == null)
        {
            _defaultOptions = new();
            DbIOOptions = _defaultOptions;
        }
        else DbIOOptions = ioOptions;
    }


    public string GetTableCreationString()
    {
        return DbInitilization.CreateDbString(this);
    }
    public DbIOOptions DbIOOptions { get; }
    public DbIO DbIO { get; }
    public DbConnection DbConnection { get; }
    public DbType DbType { get; }
    private List<Type> TablesToCreate { get; } = [];
    protected void AddTableForCreation<T>() where T : class => TablesToCreate.Add(typeof(T));

    internal async Task<DbCommand> CreateCommand(string commandString, object? properties, CancellationToken token)
    {
        if (DbConnection.State != ConnectionState.Open)
            await DbConnection.OpenAsync(token);

        var command = DbConnection.CreateCommand();
        command.CommandText = DbIO.PopulateParams(commandString, properties);
        return command;
    }

    public Task<List<T>> QueryAsync<T>(string commandString, object? properties = null) where T : class, new() => QueryAsync<T>(commandString, properties, CancellationToken.None);
    public async Task<List<T>> QueryAsync<T>(string commandString, object? properties, CancellationToken token) where T : class, new()
    {
        await using var command = await CreateCommand(commandString, properties, token);
        await using var reader = await command.ExecuteReaderAsync(token);
        List<T> list = [];
        while (await reader.ReadAsync(token))
        {
            list.Add(DbIO.Read<T>(reader));
        }

        return list;
    }
    public Task<T?> QuerySingleAsync<T>(string commandString, object? properties = null) where T : class, new() => QuerySingleAsync<T>(commandString, properties, CancellationToken.None);
    public async Task<T?> QuerySingleAsync<T>(string commandString, object? properties, CancellationToken token) where T : class, new()
    {
        await using var command = await CreateCommand(commandString, properties, token);
        await using var reader = await command.ExecuteReaderAsync(token);
        if (!reader.HasRows || !await reader.ReadAsync(token)) return null;

        return DbIO.Read<T>(reader);
    }

    public Task<T?> ExecuteScalar<T>(string commandString, object? properties = null) => ExecuteScalar<T>(commandString, properties, CancellationToken.None);
    public async Task<T?> ExecuteScalar<T>(string commandString, object? properties, CancellationToken token)
    {
        await using var command = await CreateCommand(commandString, properties, token);
        var result = await command.ExecuteScalarAsync(token);
        if (result is null or DBNull) return default;
        var readValue = DbIO.ReadValue(result, typeof(T))!;
        return (T)Convert.ChangeType(readValue, typeof(T));
    }

    public Task<int> ExecuteAsync(string commandString, object? properties = null) => ExecuteAsync(commandString, properties, CancellationToken.None);
    public async Task<int> ExecuteAsync(string commandString, object? properties, CancellationToken token)
    {
        await using var command = await CreateCommand(commandString, properties, token);
        return await command.ExecuteNonQueryAsync(token);
    }

    public ValueTask DisposeAsync() => DbConnection.DisposeAsync();
    public void Dispose() => DbConnection.Dispose();
}
