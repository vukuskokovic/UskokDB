using System;
using System.Linq.Expressions;
using System.Reflection;
using UskokDB.Attributes;

namespace UskokDB;
public class DbTable<T>(DbContext context) where T : class, new()
{
    public DbContext DbContext { get; } = context;
    public static string TableName
    {
        get
        {
            var type = typeof(T);
            if (type.GetCustomAttribute<TableNameAttribute>() is { } attr)
            {
                return attr.Name;
            }
            return type.Name;
        }
    }

    public QueryBuilder<T> Where(Expression<Func<T, bool>> expression) => new QueryBuilder<T>(this).Where(expression);
    public QueryBuilder<T> Where(string query, object? paramsObject = null) => new QueryBuilder<T>(this).Where(query, paramsObject);
    public QueryBuilder<T> OrderBy(params string[] columns) => new QueryBuilder<T>(this).OrderBy(columns);
    public QueryBuilder<T> OrderByDesc(params string[] columns) => new QueryBuilder<T>(this).OrderByDesc(columns);
    public QueryBuilder<T> GroupBy(params string[] columns) => new QueryBuilder<T>(this).GroupBy(columns);
}
