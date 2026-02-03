using System;
using System.Linq.Expressions;

namespace UskokDB.Query;

public interface IJoinable<T> where T : class, new()
{
    public QueryContext<T> Join<T0>(QueryItem<T0> queryItem, Expression<Func<T, T0, bool>> selector) where T0 : class, new();
    public QueryContext<T> Join<T0, T1>(QueryItem<T0> queryItem, Expression<Func<T0, T1, bool>> selector) where T0 : class, new();
    public QueryContext<T> LeftJoin<T0>(QueryItem<T0> queryItem, Expression<Func<T, T0, bool>> selector) where T0 : class, new();
    public QueryContext<T> LeftJoin<T0, T1>(QueryItem<T0> queryItem, Expression<Func<T0, T1, bool>> selector) where T0 : class, new();
}