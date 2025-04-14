using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
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

    internal async Task<DbCommand> CreateCommand(string commandString, object? properties, CancellationToken? cancellationToken = null)
    {
        var finalCancToken = cancellationToken ?? CancellationToken.None;
        if (DbConnection.State != ConnectionState.Open)
            await DbConnection.OpenAsync(finalCancToken);

        var command = DbConnection.CreateCommand();
        command.CommandText = DbIO.PopulateParams(commandString, properties);
        return command;
    }

    public async IAsyncEnumerable<T> QueryAsyncEnumrable<T>(string commandString, object? properties, [EnumeratorCancellation]CancellationToken cancellationToken) where T : class, new()
    {
        await using var command = await CreateCommand(commandString, properties, cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while(await reader.ReadAsync(cancellationToken))
        {
            yield return DbIO.Read<T>(reader);
        }
    }
    public async Task<List<T>> QueryAsync<T>(string commandString, object? properties = null, CancellationToken? cancellationToken = null) where T : class, new()
    {
        var finalCancToken = cancellationToken ?? CancellationToken.None;
        await using var command = await CreateCommand(commandString, properties, finalCancToken);
        await using var reader = await command.ExecuteReaderAsync(finalCancToken);
        List<T> list = [];
        while (await reader.ReadAsync(finalCancToken))
        {
            list.Add(DbIO.Read<T>(reader));
        }

        return list;
    }
    public async Task<T?> QuerySingleAsync<T>(string commandString, object? properties = null, CancellationToken? cancellationToken = null) where T : class, new()
    {
        var finalCancToken = cancellationToken ?? CancellationToken.None;
        await using var command = await CreateCommand(commandString, properties, finalCancToken);
        await using var reader = await command.ExecuteReaderAsync(finalCancToken);
        if (!reader.HasRows || !await reader.ReadAsync(finalCancToken)) return null;

        return DbIO.Read<T>(reader);
    }
    public async Task<T?> ExecuteScalar<T>(string commandString, object? properties = null, CancellationToken? cancellationToken = null)
    {
        var finalCancToken = cancellationToken ?? CancellationToken.None;
        await using var command = await CreateCommand(commandString, properties, finalCancToken);
        var result = await command.ExecuteScalarAsync(finalCancToken);
        if (result is null or DBNull) return default;
        var readValue = DbIO.ReadValue(result, typeof(T))!;
        return (T)Convert.ChangeType(readValue, typeof(T));
    }
    public async Task<int> ExecuteAsync(string commandString, object? properties = null, CancellationToken? cancellationToken = null)
    {
        var finalCancToken = cancellationToken ?? CancellationToken.None;
        await using var command = await CreateCommand(commandString, properties, finalCancToken);
        return await command.ExecuteNonQueryAsync(finalCancToken);
    }

    public ValueTask DisposeAsync() => DbConnection.DisposeAsync();
    public void Dispose() => DbConnection.Dispose();
}
