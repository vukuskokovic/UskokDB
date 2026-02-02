using System;
using System.Linq.Expressions;

namespace UskokDB.Query;

public interface IJoinable<T> where T : class
{
    public QueryContext<T> Join<T0>(Queryable<T0> queryable, Expression<Func<T, T0, bool>> selector);
    public QueryContext<T> Join<T0, T1>(Queryable<T0> queryable, Expression<Func<T0, T1, bool>> selector);
    public QueryContext<T> LeftJoin<T0>(Queryable<T0> queryable, Expression<Func<T, T0, bool>> selector);
    public QueryContext<T> LeftJoin<T0, T1>(Queryable<T0> queryable, Expression<Func<T0, T1, bool>> selector);
}