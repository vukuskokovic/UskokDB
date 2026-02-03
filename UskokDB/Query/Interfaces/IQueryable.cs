using System;
using System.Linq.Expressions;

namespace UskokDB.Query;

public interface IQueryable<T> where T : class, new()
{
    public QueryContext<T> Where(Expression<Func<T, bool>> selector);
    public QueryContext<T> Where<T0>(Expression<Func<T0, bool>> selector);
    public QueryContext<T> Where<T0, T1>(Expression<Func<T0, T1, bool>> selector);
    public QueryContext<T> Where<T0, T1, T2>(Expression<Func<T0, T1, T2, bool>> selector);
    public QueryContext<T> Where<T0, T1, T2, T3>(Expression<Func<T0, T1, T2, T3, bool>> selector);
    public QueryContext<T> Where<T0, T1, T2, T3, T4>(Expression<Func<T0, T1, T2, T3, T4, bool>> selector);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5>(Expression<Func<T0, T1, T2, T3, T4, T5, bool>> selector);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5, T6>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, bool>> selector);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, bool>> selector);
    public QueryContext<T> Where<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T0, T1, T2, T3, T4, T5, T6, T7, T8, bool>> selector);
}