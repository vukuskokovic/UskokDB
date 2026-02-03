using System;
using System.Linq.Expressions;

namespace UskokDB.Query;

public interface IOrderable<T> where T : class, new()
{
    public QueryContext<T> OrderBy<T0>(Expression<Func<T0, object>> expression);
    public QueryContext<T> OrderByDescending<T0>(Expression<Func<T0, object>> expression);
    
    public QueryContext<T> OrderBy(Expression<Func<T, object>> expression);
    public QueryContext<T> OrderByDescending(Expression<Func<T, object>> expression);
}