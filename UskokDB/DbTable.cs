using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UskokDB.Attributes;

namespace UskokDB;
public class DbTable<T>(DbContext context) where T : class, new()
{
    private static string InsertInitString { get; } = $"INSERT INTO `{TableName}` VALUES (";
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

    public Task InsertAsync(params T[] items)
    {
        string cmd = GetInsertString(items);
        return DbContext.ExecuteAsync(cmd);
    }
    public Task InsertAsync(IEnumerable<T> items)
    {
        string cmd = GetInsertString(items);
        return DbContext.ExecuteAsync(cmd);
    }

    public string GetInsertString(IEnumerable<T> items)
    {
        StringBuilder builder = new();
        foreach(var item in items)
        {
            AddItemToInsertStringBuilder(builder, item);
        }
        return builder.ToString();
    }

    private void AddItemToInsertStringBuilder(StringBuilder builder, T item)
    {
        builder.Append(InsertInitString);
        int i = 0;
        int l = TypeMetadata<T>.Properties.Count;
        foreach (var property in TypeMetadata<T>.Properties)
        {
            builder.Append(DbContext.DbIO.WriteValue(property.PropertyInfo.GetValue(item)));

            if (++i != l) builder.Append(',');
        }
        builder.Append(");\n");
    }

    public QueryBuilder<T> Where(Expression<Func<T, bool>> expression) => new QueryBuilder<T>(this).Where(expression);
    public QueryBuilder<T> Where(string query, object? paramsObject = null) => new QueryBuilder<T>(this).Where(query, paramsObject);
    public QueryBuilder<T> OrderBy(params string[] columns) => new QueryBuilder<T>(this).OrderBy(columns);
    public QueryBuilder<T> GroupBy(params string[] columns) => new QueryBuilder<T>(this).GroupBy(columns);
}
