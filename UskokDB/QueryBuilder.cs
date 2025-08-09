using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UskokDB;

public class QueryBuilder<T>(DbTable<T> table) where T : class, new()
{
    private List<DbParam> _params = [];
    private string? whereClause;
    private string? orderByClause;
    private string? groupByClause;
    private string? limitClause;

    public Task<List<T>> QueryAsync(CancellationToken cancellationToken = default) => table.DbContext.QueryAsync<T>(CompileQuery(), cancellationToken);
    #if !NETSTANDARD2_0
    public IAsyncEnumerable<T> QueryAsyncEnumerable(CancellationToken cancellationToken = default) => table.DbContext.QueryAsyncEnumerable<T>(CompileQuery(), cancellationToken);
    #endif
    public Task<T?> QuerySingleAsync(CancellationToken cancellationToken = default) => table.DbContext.QuerySingleAsync<T>(limitClause == null? Limit(1).CompileQuery() : CompileQuery(), cancellationToken);

    public QueryBuilder<T> Where(Expression<Func<T, bool>> expression)
    {
        var res = LinqToSql.Convert(expression);
        _params.AddRange(res.Params);
        whereClause = $" WHERE {res.CompiledText}";
        return this;
    }

    public QueryBuilder<T> Where(string query, object? paramsObject = null)
    {
        var res = DbIO.PopulateParams(query, paramsObject);
        _params.AddRange(res.Params);
        whereClause = $" WHERE {res.CompiledText}";
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

    //For debug purposes
    public string GetCommandText()
    {
        return $"SELECT * FROM {DbTable<T>.TableName}{whereClause}{groupByClause}{orderByClause}{limitClause}";
    }

    public DbCommand CompileQuery()
    {
        DbPopulateParamsResult populateParamsResult = new()
        {
            CompiledText = GetCommandText(),
            Params = _params
        };
        return populateParamsResult.CreateCommandWithConnection(table.DbContext.DbConnection);
    }
}