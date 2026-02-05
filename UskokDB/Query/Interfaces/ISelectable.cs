using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UskokDB.Query;

public interface ISelectable
{
    public Task<TRead?> SelectOneAsync<TRead, T0>(Expression<Func<T0, TRead>> selector, bool printToConsole = false)
        where TRead : class, new();

    public Task<TRead?> SelectOneAsync<TRead, T0, T1>(Expression<Func<T0, T1, TRead>> selector,
        bool printToConsole = false) where TRead : class, new();

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2>(Expression<Func<T0, T1, T2, TRead>> selector,
        bool printToConsole = false) where TRead : class, new();

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3, TRead>> selector,
        bool printToConsole = false) where TRead : class, new();

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4>(Expression<Func<T0, T1, T2, T3, T4, TRead>> selector,
        bool printToConsole = false) where TRead : class, new();

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5>(
        Expression<Func<T0, T1, T2, T3, T4, T5, TRead>> selector, bool printToConsole = false)
        where TRead : class, new();

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5, T6>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, TRead>> selector, bool printToConsole = false)
        where TRead : class, new();

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5, T6, T7>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, TRead>> selector, bool printToConsole = false)
        where TRead : class, new();

    public Task<TRead?> SelectOneAsync<TRead, T0, T1, T2, T3, T4, T5, T6, T7, T8>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>> selector, bool printToConsole = false)
        where TRead : class, new();
    //=====================================================================
    public Task<List<TRead>> Select<TRead, T0>(Expression<Func<T0, TRead>> selector, bool printToConsole = false)
        where TRead : class, new();

    public Task<List<TRead>> Select<TRead, T0, T1>(Expression<Func<T0, T1, TRead>> selector,
        bool printToConsole = false) where TRead : class, new();

    public Task<List<TRead>> Select<TRead, T0, T1, T2>(Expression<Func<T0, T1, T2, TRead>> selector,
        bool printToConsole = false) where TRead : class, new();

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3, TRead>> selector,
        bool printToConsole = false) where TRead : class, new();

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4>(Expression<Func<T0, T1, T2, T3, T4, TRead>> selector,
        bool printToConsole = false) where TRead : class, new();

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4, T5>(
        Expression<Func<T0, T1, T2, T3, T4, T5, TRead>> selector, bool printToConsole = false)
        where TRead : class, new();

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4, T5, T6, T7>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, TRead>> selector, bool printToConsole = false)
        where TRead : class, new();

    public Task<List<TRead>> Select<TRead, T0, T1, T2, T3, T4, T5, T6, T7, T8>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>> selector, bool printToConsole = false)
        where TRead : class, new();

    public Task<bool> Exists(bool printToConsole = false);
}