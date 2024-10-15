﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UskokDB;

public class QueryBuilder<T>(DbTable<T> table) where T : class, new()
{
    private string? whereClause = null;
    private string? orderByClause = null;
    private string? groupByClause = null;

    public Task<List<T>> QueryAsync() => table.DbContext.QueryAsync<T>(CompileQuery());
    public Task<T?> QuerySingleAsync() => table.DbContext.QuerySingleAsync<T>(CompileQuery());

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
        orderByClause = $" ORDER BY {string.Join(',', columns)}";
        return this;
    }

    public QueryBuilder<T> OrderByDesc(params string[] columns)
    {
        orderByClause = $" ORDER BY {string.Join(',', columns)} DESC";
        return this;
    }

    public QueryBuilder<T> GroupBy(params string[] columns)
    {
        groupByClause = $" GROUP BY {string.Join(',', columns)}";
        return this;
    }

    public string CompileQuery()
    {
        return $"SELECT * FROM {DbTable<T>.TableName}{whereClause}{groupByClause}{orderByClause}";
    }
}
