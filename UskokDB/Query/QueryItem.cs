using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UskokDB.Query;

public interface IQueryItem
{
    public string GetName();
    public Type GetUnderlyingType();
    public string? PreQuery(List<DbParam> paramList);

    public Type GetTypeMetadata();

    public TypeMetadataProperty GetMetadataPropertyFromName(string name);
}

public abstract class QueryItem<T>(DbContext dbContext) : IQueryItem, IJoinable<T>, IOrderable<T>, IGroupable<T>, ISelectable, ILimitable<T> where T : class, new()
{
    private static Type? _metadataType;
    private static Dictionary<string, TypeMetadataProperty>? _nameToPropertyMap;
    public abstract string GetName();
    public abstract Type GetUnderlyingType();
    public abstract string? PreQuery(List<DbParam> paramList);
    
    public Type GetTypeMetadata()
    {
        _metadataType ??= typeof(TypeMetadata<>).MakeGenericType(typeof(T));
        return _metadataType;
    }

    private Dictionary<string, TypeMetadataProperty> PropertyToNameMap()
    {
        _nameToPropertyMap ??= (Dictionary<string, TypeMetadataProperty>)GetTypeMetadata().GetProperty("NameToPropertyMap")!.GetValue(null);
        return _nameToPropertyMap;
    }

    public TypeMetadataProperty GetMetadataPropertyFromName(string name)
    {
        return PropertyToNameMap()[name];
    }
    
    public QueryContext<T> GroupBy<T0>(Expression<Func<T0, object>> expression) =>
        new QueryContext<T>(this, dbContext).GroupBy(expression);
    public QueryContext<T> GroupBy(Expression<Func<T, object>> expression) =>
        new QueryContext<T>(this, dbContext).GroupBy(expression);
    
    public QueryContext<T> OrderBy<T0>(Expression<Func<T0, object>> expression) =>
        new QueryContext<T>(this, dbContext).OrderBy(expression);

    public QueryContext<T> OrderByDescending<T0>(Expression<Func<T0, object>> expression) =>
        new QueryContext<T>(this, dbContext).OrderByDescending(expression);

    public QueryContext<T> Where(Expression<Func<T, bool>> selector) =>
        new QueryContext<T>(this, dbContext).Where(selector);

    public QueryContext<T> Join<T0>(QueryItem<T0> queryItem, Expression<Func<T, T0, bool>> selector) where T0 : class, new() =>
        new QueryContext<T>(this, dbContext).Join(queryItem, selector);

    public QueryContext<T> Join<T0, T1>(QueryItem<T0> queryItem, Expression<Func<T0, T1, bool>> selector) where T0 : class, new() =>
        new QueryContext<T>(this, dbContext).Join(queryItem, selector);

    public QueryContext<T> LeftJoin<T0>(QueryItem<T0> queryItem, Expression<Func<T, T0, bool>> selector) where T0 : class, new() =>
        new QueryContext<T>(this, dbContext).LeftJoin(queryItem, selector);

    public QueryContext<T> LeftJoin<T0, T1>(QueryItem<T0> queryItem, Expression<Func<T0, T1, bool>> selector) where T0 : class, new() =>
        new QueryContext<T>(this, dbContext).LeftJoin(queryItem, selector);

    public Task<TRead?> SelectOneAsync<TRead, T0>(Expression<Func<T0, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).SelectOneAsync(selector, printToConsole);

    public Task<TRead?> SelectOneAsync<TRead, T0, T1>(Expression<Func<T0, T1, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).SelectOneAsync(selector, printToConsole);

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2>(Expression<Func<T0, T1, T2, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).SelectOneAsync(selector, printToConsole);

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).SelectOneAsync(selector, printToConsole);

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4>(Expression<Func<T0, T1, T2, T3, T4, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).SelectOneAsync(selector, printToConsole);

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5>(Expression<Func<T0, T1, T2, T3, T4, T5, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).SelectOneAsync(selector, printToConsole);

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5, T6>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).SelectOneAsync(selector, printToConsole);

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).SelectOneAsync(selector, printToConsole);

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).SelectOneAsync(selector, printToConsole);

    public Task<List<TRead>> Select<TRead, T0>(Expression<Func<T0, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).Select(selector, printToConsole);

    public Task<List<TRead>> Select<TRead, T0, T1>(Expression<Func<T0, T1, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).Select(selector, printToConsole);

    public Task<List<TRead>> Select<TRead, T0, T1, T2>(Expression<Func<T0, T1, T2, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).Select(selector, printToConsole);

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).Select(selector, printToConsole);

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4>(Expression<Func<T0, T1, T2, T3, T4, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).Select(selector, printToConsole);

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4, T5>(Expression<Func<T0, T1, T2, T3, T4, T5, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).Select(selector, printToConsole);

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).Select(selector, printToConsole);

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>> selector, bool printToConsole = false) where TRead : class, new() =>
        new QueryContext<T>(this, dbContext).Select(selector, printToConsole);

    public QueryContext<T> Limit(int limit) =>
        new QueryContext<T>(this, dbContext).Limit(limit);
}