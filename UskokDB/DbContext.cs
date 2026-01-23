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



public abstract class DbContext : IDisposable
#if !NETSTANDARD2_0
,IAsyncDisposable
#endif
{
    public string GetTableCreationString() => DbInitialization.CreateDbString(this);
    public DbConnection DbConnection { get; }
    public DbContext(Func<DbConnection> connectionFactory)
    {
        DbConnection = connectionFactory();
    }
    
    private readonly List<DbCommand> _queuedCommands = [];
    
    public DbCommand CreateCommandFromParams(string commandString, object? properties)
    {
        var populateResult = DbIO.PopulateParams(commandString, properties);
        return populateResult.CreateCommandWithConnection(DbConnection);
    }
    public DbCommand CreateCommand(string commandString)
    {
        var cmd =  DbConnection.CreateCommand();
        cmd.CommandText = commandString;
        return cmd;
    }
    internal Task OpenConnectionIfNotOpen(CancellationToken cancellationToken = default)
    {
        if (DbConnection.State != ConnectionState.Open)
            return DbConnection.OpenAsync(cancellationToken);

        return Task.CompletedTask;
    }

    #region Instant Queries

    #if !NETSTANDARD2_0
    public async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(string commandString, object? properties, [EnumeratorCancellation]CancellationToken cancellationToken) where T : class, new()
    {
        await OpenConnectionIfNotOpen(cancellationToken);
        await using var command = CreateCommandFromParams(commandString, properties);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while(await reader.ReadAsync(cancellationToken))
        {
            yield return DbIO.Read<T>(reader);
        }
    }
    internal async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(DbCommand command, [EnumeratorCancellation]CancellationToken cancellationToken) where T : class, new()
    {
        try
        {
            await OpenConnectionIfNotOpen(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                yield return DbIO.Read<T>(reader);
            }
        }
        finally
        {
            await command.DisposeAsync();
        }
    }
    
    
    #endif
    public Task<List<T>> QueryAsync<T>(string commandString, object? properties = null, CancellationToken cancellationToken = default) where T : class, new()
        => QueryAsync<T>(CreateCommandFromParams(commandString, properties), cancellationToken);
    

    public async Task<List<T>> QueryAsync<T>(DbCommand command, CancellationToken cancellationToken = default) where T : class, new()
    {
        try
        {
            await OpenConnectionIfNotOpen(cancellationToken);
#if !NETSTANDARD2_0
            await
#endif
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
            
            List<T> list = [];
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(DbIO.Read<T>(reader));
            }

            return list;
        }
        finally
        {
        #if NETSTANDARD2_0
            command.Dispose();
        #else
            await command.DisposeAsync();
        #endif
        }
    }
    
    
    
    public Task<T?> QuerySingleAsync<T>(string commandString, object? properties = null, CancellationToken cancellationToken = default) where T : class, new()
        => QuerySingleAsync<T>(CreateCommandFromParams(commandString, properties), cancellationToken);
    

    public async Task<T?> QuerySingleAsync<T>(DbCommand command,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        try
        {
            await OpenConnectionIfNotOpen(cancellationToken);
#if !NETSTANDARD2_0
            await
#endif
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!reader.HasRows || !await reader.ReadAsync(cancellationToken)) return null;

            return DbIO.Read<T>(reader);
        }
        finally
        {
#if NETSTANDARD2_0
            command.Dispose();
#else
            await command.DisposeAsync();
#endif
        }
    }
    
    
    public Task<T?> ExecuteScalar<T>(string commandString, object? properties = null, CancellationToken cancellationToken = default)
        => ExecuteScalar<T?>(CreateCommandFromParams(commandString, properties), cancellationToken);
    
    public async Task<T?> ExecuteScalar<T>(DbCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await OpenConnectionIfNotOpen(cancellationToken);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result is null or DBNull) return default;
            return (T)Convert.ChangeType(result, typeof(T));
        }
        finally
        {
#if NETSTANDARD2_0
            command.Dispose();
#else
            await command.DisposeAsync();
#endif
        }
    }
    
    
    
    
    public Task<int> ExecuteAsync(string commandString, object? properties = null, CancellationToken cancellationToken = default)
    {
        var command = CreateCommandFromParams(commandString, properties);
        return ExecuteAsync(command, cancellationToken);
    }

    public async Task<int> ExecuteAsync(DbCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await OpenConnectionIfNotOpen(cancellationToken);

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            #if NETSTANDARD2_0
            command.Dispose();
            #else
            await command.DisposeAsync();
            #endif
        }
    }
    
    
    

    public Task<bool> ExistsAsync(string commandString, object? properties = null, CancellationToken cancellationToken = default) 
        => ExistsAsync(CreateCommandFromParams(commandString, properties), cancellationToken);
    public async Task<bool> ExistsAsync(DbCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await OpenConnectionIfNotOpen(cancellationToken);
#if !NETSTANDARD2_0
            await
#endif
                using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return reader.HasRows;
        }
        finally
        {
#if NETSTANDARD2_0
            command.Dispose();
#else
            await command.DisposeAsync();
#endif
        }
    }
    

    #endregion

    #region Queued Queries

    /// <summary>
    /// Clears the current queue
    /// </summary>
#if NETSTANDARD2_0
    public void ClearQueue()
    {
        foreach (var cmd in _queuedCommands)
        {
            cmd.Dispose();
        }
        _queuedCommands.Clear();
    }
    #else
    public async Task ClearQueue()
    {
        if (_queuedCommands.Count == 0) return;
        foreach (var cmd in _queuedCommands)
        {
            await cmd.DisposeAsync();
        }
        _queuedCommands.Clear();
    }
    #endif
    
    internal void AppendQueueCmd(string cmd)
    {
        var command = DbConnection.CreateCommand();
        command.CommandText = cmd;
        _queuedCommands.Add(command);
    }
    
    internal void AppendQueueCmd(DbCommand cmd)
    {
        _queuedCommands.Add(cmd);
    }

    public void AppendExecute(string query, object? paramsObject = null)
    {
        var result = DbIO.PopulateParams(query, paramsObject);
        AppendQueueCmd(result.CreateCommandWithConnection(DbConnection));
    }
    
    /// <summary>
    /// Executes all queued commands and clears the queue
    /// </summary>
    /// <remarks>IF ONE QUERY FAILS ALL QUERIES FAIL</remarks>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ExecuteQueueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await OpenConnectionIfNotOpen(cancellationToken);
            
#if NETSTANDARD2_0
        using DbTransaction dbTransaction = DbConnection.BeginTransaction();
#else
            await using DbTransaction dbTransaction = await DbConnection.BeginTransactionAsync(cancellationToken);
#endif

            foreach (var command in _queuedCommands)
            {
                command.Transaction = dbTransaction;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

#if NETSTANDARD2_0
        dbTransaction.Commit();
#else
            await dbTransaction.CommitAsync(cancellationToken);
#endif
        }
        finally
        {
            #if NETSTANDARD2_0
            ClearQueue();
            #else
            await ClearQueue();
            #endif
        }
    }
    #endregion


    public async Task InitDb(CancellationToken token = default)
    {
        await ExecuteAsync(GetTableCreationString(), cancellationToken: token);

        if (DbInitialization.DbIndexList.Count > 0)
        {
            var indexes = await QueryAsync<IndexQuery>("""
                                                        SELECT index_name
                                                       FROM information_schema.statistics
                                                       WHERE table_schema = DATABASE()
                                                       GROUP BY table_name, index_name;
                                                       """, cancellationToken: token);


            StringBuilder builder = new StringBuilder();
            var indexHashSet = new HashSet<string>(indexes.Select(x => x.IndexName));
            bool anyAdded = false;
            foreach (var dbIndex in DbInitialization.DbIndexList)
            {
                if (!indexHashSet.Contains(dbIndex.Name))
                {
                    builder.Append("CREATE ");
                    if(dbIndex.Unique)
                        builder.Append("UNIQUE ");

                    builder.Append("INDEX ");
                    builder.Append(dbIndex.Name);
                    builder.Append(" ON ");
                    builder.Append(dbIndex.TableName);
                    builder.Append('(');
                    for (int i = 0; i < dbIndex.Columns.Length; i++)
                    {
                        builder.Append(dbIndex.Columns[i]);
                        if (i + 1 != dbIndex.Columns.Length)
                            builder.Append(',');
                    }

                    builder.AppendLine(");");

                    anyAdded = true;
                }
            }

            if (anyAdded)
            {
                await ExecuteAsync(builder.ToString(), cancellationToken: token);
            }
        }

    }

    #region Dispose

#if !NETSTANDARD2_0
    public async ValueTask DisposeAsync()
    {
        await ClearQueue();
        await DbConnection.DisposeAsync();
    }
#endif
    public void Dispose()
    {
        #if NETSTANDARD2_0
        ClearQueue();
        #else
        ClearQueue().GetAwaiter().GetResult();
        #endif
        DbConnection.Dispose();
    }

    #endregion
    
}

internal class IndexQuery
{
    public string IndexName { get; set; } = null!;
}
