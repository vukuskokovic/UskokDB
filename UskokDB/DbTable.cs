using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UskokDB.Attributes;

namespace UskokDB;
public class DbTable<T>(DbContext context) where T : class, new()
{
    
    private static string InsertInitString { get; } = $"INSERT INTO `{TableName}` VALUES (";
    public DbContext DbContext { get; } = context;

    private static string? _tableName = null;
    public static string TableName
    {
        get
        {
            if(_tableName is not null)return _tableName;
            var type = typeof(T);
            if (type.GetCustomAttribute<TableNameAttribute>() is { } attr)
            {
                _tableName = attr.Name;
                return _tableName;
            }
            _tableName = type.Name;
            return _tableName;
        }
    }

    public Task InsertAsync(T item, CancellationToken? cancellationToken = null){
        StringBuilder builder = new();
        AddItemToInsertStringBuilder(builder, item);
        var cmd = builder.ToString();
        return DbContext.ExecuteAsync(cmd, cancellationToken: cancellationToken);
    }
    public Task InsertAsync(params T[] items)
    {
        string cmd = GetInsertString(items);
        return DbContext.ExecuteAsync(cmd);
    }
    public Task InsertAsync(CancellationToken cancellationToken, params T[] items)
    {
        string cmd = GetInsertString(items);
        return DbContext.ExecuteAsync(cmd, cancellationToken: cancellationToken);
    }

    public Task InsertAsync(IEnumerable<T> items, CancellationToken? cancellationToken = null)
    {
        string cmd = GetInsertString(items);
        return DbContext.ExecuteAsync(cmd, cancellationToken: cancellationToken);
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

    public Task<int> UpdateAsync(Expression<Func<T>> update, Expression<Func<T, bool>>? where = null, CancellationToken? cancellationToken = null)
    {
        if (update == null) throw new ArgumentException("Cannot be null, the only reason it could be in code is to remind to use where statment because if where is null all records are updated this is used to remind", nameof(update));
        var updateString = UpdateStatementString(update, where);
        return DbContext.ExecuteAsync(updateString, cancellationToken: cancellationToken);
    }
    public string UpdateStatementString(Expression<Func<T>> updateStatement, Expression<Func<T, bool>>? where = null)
    {
        StringBuilder queryStringBuilder = new($"UPDATE {DbTable<T>.TableName} SET ");
        if (updateStatement.Body is not MemberInitExpression initExpression) throw new ArgumentException("Must be a MemberInitExpression (i.e. () => new Record() { Name = \"Some name\" })", nameof(updateStatement));

        int bindingCount = initExpression.Bindings.Count;
        int counter = 0;
        foreach (var binding in initExpression.Bindings)
        {
            if (binding is not MemberAssignment assigment)
                throw new ArgumentException("Bindings must be assigment (i.e. x => x.Name = \"New Name\")");

            if (!TypeMetadata<T>.NameToPropertyMap.TryGetValue(binding.Member.Name, out var propertyMetaData))
                throw new Exception($"\"{binding.Member.Name}\" property is not mapped to db");

            var valueWritten = LinqToSql.CompileExpression<T>(DbContext, assigment.Expression);
            queryStringBuilder.Append($"{propertyMetaData.PropertyName}={valueWritten}");
            if (++counter == bindingCount) continue;

            queryStringBuilder.Append(',');
        }
        if (where != null)
        {
            queryStringBuilder.Append(" WHERE ");
            queryStringBuilder.Append(LinqToSql.Convert<T>(DbContext, where));
        }
        queryStringBuilder.Append(';');
        return queryStringBuilder.ToString();
        
    }

    public Task<int> DeleteAsync(Expression<Func<T, bool>>? where, CancellationToken? cancellationToken = null)
    {
        var queryStringBuilder = new StringBuilder("DELETE FROM ");
        queryStringBuilder.Append(TableName);
        if(where != null)
        {
            queryStringBuilder.Append(" WHERE ");
            queryStringBuilder.Append(LinqToSql.Convert<T>(DbContext, where));
        }

        return DbContext.ExecuteAsync(queryStringBuilder.ToString(), cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>>? where, CancellationToken? cancellationToken = null){
        var finalCToken = cancellationToken ?? CancellationToken.None;
        var queryStringBuilder = new StringBuilder("SELECT 1 FROM ");
        queryStringBuilder.Append(TableName);
        if(where != null){
            queryStringBuilder.Append(" WHERE ");
            queryStringBuilder.Append(LinqToSql.Convert<T>(DbContext, where));
        }

        var finalQuery = queryStringBuilder.ToString();

        #if !NETSTANDARD2_0
        await using var command = await DbContext.CreateCommand(finalQuery, null, cancellationToken: finalCToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken: finalCToken);
        #else
        using var command = await DbContext.CreateCommand(finalQuery, null, cancellationToken: finalCToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken: finalCToken);
        #endif
        return reader.HasRows;
    }
}
