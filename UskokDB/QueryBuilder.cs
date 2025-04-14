using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UskokDB;

public class QueryBuilder<T>(DbTable<T> table) where T : class, new()
{
    private string? whereClause;
    private string? orderByClause;
    private string? groupByClause;
    private string? limitClause;

    public Task<List<T>> QueryAsync(CancellationToken? cancellationToken = null) => table.DbContext.QueryAsync<T>(CompileQuery(), null, cancellationToken ?? CancellationToken.None);
    #if !NETSTANDARD2_0
    public IAsyncEnumerable<T> QueryAsyncEnumerable(CancellationToken? cancellationToken = null) => table.DbContext.QueryAsyncEnumerable<T>(CompileQuery(), null, cancellationToken ?? CancellationToken.None);
    #endif
    public Task<T?> QuerySingleAsync(CancellationToken? cancellationToken = null) => table.DbContext.QuerySingleAsync<T>(limitClause == null? Limit(1).CompileQuery() : CompileQuery(), null, cancellationToken ?? CancellationToken.None);

    public QueryBuilder<T> Where(Expression<Func<T, bool>> expression)
    {
        whereClause = " WHERE " + LinqToSql.Convert(table.DbContext, expression);
        return this;
    }

    public QueryBuilder<T> Where(string query, object? paramsObject = null)
    {
        whereClause = " WHERE " + table.DbContext.DbIO.PopulateParams(query, paramsObject);
        return this;
    }

    public QueryBuilder<T> OrderBy(params string[] columns)
    {
        orderByClause = $" ORDER BY {string.Join(",", columns)}";
        return this;
    }

    public QueryBuilder<T> GroupBy(params string[] columns)
    {
        groupByClause = $" GROUP BY {string.Join(",", columns)}";
        return this;
    }

    public QueryBuilder<T> Limit(int limit)
    {
        limitClause = $" LIMIT {limit}";
        return this;
    }

    public string CompileQuery()
    {
        return $"SELECT * FROM {DbTable<T>.TableName}{whereClause}{groupByClause}{orderByClause}{limitClause}";
    }
}