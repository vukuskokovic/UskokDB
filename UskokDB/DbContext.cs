using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UskokDB;

public enum DbType
{
    MySQL,
    PostgreSQL,
    SQLite
}

public abstract class DbContext : IDisposable
#if !NETSTANDARD2_0
,IAsyncDisposable
#endif
{
    public string GetTableCreationString() => DbInitialization.CreateDbString(this);
    public DbIOOptions DbIoOptions { get; }
    public DbIO DbIo { get; }
    public DbConnection DbConnection { get; }
    public DbType DbType { get; }
    public DbContext(DbType type, Func<DbConnection> connectionFactory, DbIOOptions? ioOptions = null)
    {
        DbType = type;
        DbConnection = connectionFactory();
        DbIo = new DbIO(this);
        if (ioOptions == null)
        {
            _defaultOptions = new DbIOOptions();
            DbIoOptions = _defaultOptions;
        }
        else DbIoOptions = ioOptions;
    }
    
    internal readonly StringBuilder queueQueriesBuilder = new();
    private static DbIOOptions? _defaultOptions;

    internal async Task<DbCommand> CreateCommand(string commandString, object? properties, CancellationToken cancellationToken = default)
    {
        if (DbConnection.State != ConnectionState.Open)
            await DbConnection.OpenAsync(cancellationToken);

        var command = DbConnection.CreateCommand();
        command.CommandText = DbIo.PopulateParams(commandString, properties);
        return command;
    }

    #region Instant Queries

    #if !NETSTANDARD2_0
    public async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(string commandString, object? properties, [EnumeratorCancellation]CancellationToken cancellationToken) where T : class, new()
    {
        await using var command = await CreateCommand(commandString, properties, cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while(await reader.ReadAsync(cancellationToken))
        {
            yield return DbIo.Read<T>(reader);
        }
    }
    #endif
    public async Task<List<T>> QueryAsync<T>(string commandString, object? properties = null, CancellationToken cancellationToken = default) where T : class, new()
    {
        #if !NETSTANDARD2_0
        await 
        #endif
        using var command = await CreateCommand(commandString, properties, cancellationToken);
        
        #if !NETSTANDARD2_0
        await 
        #endif
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        List<T> list = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(DbIo.Read<T>(reader));
        }

        return list;
    }
    public async Task<T?> QuerySingleAsync<T>(string commandString, object? properties = null, CancellationToken cancellationToken = default) where T : class, new()
    {
        #if !NETSTANDARD2_0
        await 
        #endif
        using var command = await CreateCommand(commandString, properties, cancellationToken);
        
        #if !NETSTANDARD2_0
        await 
        #endif
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!reader.HasRows || !await reader.ReadAsync(cancellationToken)) return null;

        return DbIo.Read<T>(reader);
    }
    public async Task<T?> ExecuteScalar<T>(string commandString, object? properties = null, CancellationToken cancellationToken = default)
    {
        #if !NETSTANDARD2_0
        await
        #endif
        using var command = await CreateCommand(commandString, properties, cancellationToken);
        
        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null or DBNull) return default;
        var readValue = DbIo.ReadValue(result, typeof(T))!;
        return (T)Convert.ChangeType(readValue, typeof(T));
    }
    public async Task<int> ExecuteAsync(string commandString, object? properties = null, CancellationToken cancellationToken = default)
    {
        #if NETSTANDARD2_0
        using var command = await CreateCommand(commandString, properties, cancellationToken);
        #else
        await using var command = await CreateCommand(commandString, properties, cancellationToken);
        #endif
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string commandString, object? properties = null,
        CancellationToken cancellationToken = default)
    {
#if !NETSTANDARD2_0
        await
#endif
        using var command = await CreateCommand(commandString, properties, cancellationToken);
        
#if !NETSTANDARD2_0
        await
#endif
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return reader.HasRows;
    }

    #endregion

    #region Queued Queries

    /// <summary>
    /// Clears the current queue
    /// </summary>
    public void ClearQueue() => queueQueriesBuilder.Clear();

    /// <summary>
    /// Builds the current command queue
    /// </summary>
    /// <returns>The compiled command from all the queues</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public string? BuildQueue()
    {
        return queueQueriesBuilder.Length == 0 ? null : queueQueriesBuilder.ToString();
    }

    internal void AppendQueryCmd(string cmd, bool appendSemiColor = true)
    {
        queueQueriesBuilder.Append(cmd);
        if (appendSemiColor) queueQueriesBuilder.Append(';');
    }

    public void Execute(string query, object? paramsObject = null)
    {
        var finalQuery = DbIo.PopulateParams(query, paramsObject);
        AppendQueryCmd(finalQuery, finalQuery[finalQuery.Length-1] != ';');
        
    }
    
    /// <summary>
    /// Executes all queued commands and clears the queue
    /// </summary>
    /// <remarks>IF ONE QUERY FAILS ALL QUERIES FAIL</remarks>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task ExecuteQueue(CancellationToken cancellationToken = default)
    {
        var command = BuildQueue();
        queueQueriesBuilder.Clear();
        return command == null ? Task.CompletedTask : ExecuteAsync(command, cancellationToken: cancellationToken);
    }
    #endregion


    public Task<int> ExecuteTableCreationCommand(CancellationToken token = default) =>
        ExecuteAsync(GetTableCreationString(), cancellationToken: token);

    #region Dispose

#if !NETSTANDARD2_0
    public ValueTask DisposeAsync() => DbConnection.DisposeAsync();
#endif
    public void Dispose() => DbConnection.Dispose();

    #endregion
    
}
