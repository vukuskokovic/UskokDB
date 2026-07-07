using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UskokDB.Query;

public interface ISelectable
{
    public Task<TRead?> SelectOneAsync<T0, TRead>(Expression<Func<T0, TRead>> selector, bool printToConsole = false)
        ;

    public Task<TRead?> SelectOneAsync<T0, T1, TRead>(Expression<Func<T0, T1, TRead>> selector,
        bool printToConsole = false) ;

    public Task<TRead?> SelectOneAsync<T0, T1, T2, TRead>(Expression<Func<T0, T1, T2, TRead>> selector,
        bool printToConsole = false) ;

    public Task<TRead?> SelectOneAsync<T0, T1, T2, T3, TRead>(Expression<Func<T0, T1, T2, T3, TRead>> selector,
        bool printToConsole = false) ;

    public Task<TRead?> SelectOneAsync<T0, T1, T2, T3, T4, TRead>(Expression<Func<T0, T1, T2, T3, T4, TRead>> selector,
        bool printToConsole = false) ;

    public Task<TRead?> SelectOneAsync<T0, T1, T2, T3, T4, T5, TRead>(
        Expression<Func<T0, T1, T2, T3, T4, T5, TRead>> selector, bool printToConsole = false)
        ;

    public Task<TRead?> SelectOneAsync<T0, T1, T2, T3, T4, T5, T6, TRead>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, TRead>> selector, bool printToConsole = false)
        ;

    public Task<TRead?> SelectOneAsync<T0, T1, T2, T3, T4, T5, T6, T7, TRead>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, TRead>> selector, bool printToConsole = false)
        ;

    public Task<TRead?> SelectOneAsync<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>> selector, bool printToConsole = false)
        ;
    //=====================================================================

    public Task<List<TRead>> Select<T0, TRead>(Expression<Func<T0, TRead>> selector, bool printToConsole = false)
        ;

    public Task<List<TRead>> Select<T0, T1, TRead>(Expression<Func<T0, T1, TRead>> selector,
        bool printToConsole = false) ;

    public Task<List<TRead>> Select<T0, T1, T2, TRead>(Expression<Func<T0, T1, T2, TRead>> selector,
        bool printToConsole = false) ;

    public Task<List<TRead>> Select<T0, T1, T2, T3, TRead>(Expression<Func<T0, T1, T2, T3, TRead>> selector,
        bool printToConsole = false) ;

    public Task<List<TRead>> Select<T0, T1, T2, T3, T4, TRead>(Expression<Func<T0, T1, T2, T3, T4, TRead>> selector,
        bool printToConsole = false) ;

    public Task<List<TRead>> Select<T0, T1, T2, T3, T4, T5, TRead>(
        Expression<Func<T0, T1, T2, T3, T4, T5, TRead>> selector, bool printToConsole = false)
        ;

    public Task<List<TRead>> Select<T0, T1, T2, T3, T4, T5, T6, TRead>(
            Expression<Func<T0, T1, T2, T3, T4, T5, T6, TRead>> selector, bool printToConsole = false)
        ;
    
    public Task<List<TRead>> Select<T0, T1, T2, T3, T4, T5, T6, T7, TRead>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, TRead>> selector, bool printToConsole = false)
        ;

    public Task<List<TRead>> Select<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>(
        Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, TRead>> selector, bool printToConsole = false)
        ;

    public Task<bool> Exists(bool printToConsole = false);
    public Task<int> Count(bool printToConsole = false);
}