using System;
using System.Linq.Expressions;

namespace UskokDB.Query;

public interface IGroupable<T> where T : class, new()
{
    public QueryContext<T> GroupBy(Expression<Func<T, object>> expression);
    public QueryContext<T> GroupBy<T0>(Expression<Func<T0, object>> expression);
}