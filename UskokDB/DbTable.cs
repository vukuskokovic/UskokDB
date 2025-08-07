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
    private static string InsertInitString { get; } = $"INSERT INTO {TableName} VALUES (";
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

    #region Query builders
    public string BuildInsertQueryString(T item)
    {
        StringBuilder builder = new();
        AddItemToInsertStringBuilder(builder, item);
        var cmd = builder.ToString();
        return cmd;
    }
    public string BuildInsertQueryString(IEnumerable<T> items) => GetInsertString(items);
    private string GetInsertString(IEnumerable<T> items)
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
            builder.Append(DbContext.DbIo.WriteValue(property.PropertyInfo.GetValue(item)));

            if (++i != l) builder.Append(',');
        }
        builder.Append(");\n");
    }
    public string UpdateStatementString(Expression<Func<T>> updateStatement, Expression<Func<T, bool>>? where = null)
    {
        if (updateStatement == null) throw new ArgumentException("Cannot be null, the only reason it could be in code is to remind to use where statment because if where is null all records are updated this is used to remind", nameof(updateStatement));
        StringBuilder queryStringBuilder = new($"UPDATE {DbTable<T>.TableName} SET ");
        if (updateStatement.Body is not MemberInitExpression initExpression) throw new ArgumentException("Must be a MemberInitExpression (i.e. () => new Record() { Name = \"Some name\" })", nameof(updateStatement));

        int bindingCount = initExpression.Bindings.Count;
        int counter = 0;
        foreach (var binding in initExpression.Bindings)
        {
            if (binding is not MemberAssignment assigment)
                throw new UskokDbInvalidLinqBinding();

            if (!TypeMetadata<T>.NameToPropertyMap.TryGetValue(binding.Member.Name, out var propertyMetaData))
                throw new UskokDbPropertyNotMapped(binding.Member.Name);

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
        queryStringBuilder.Append(";\n");
        return queryStringBuilder.ToString();
        
    }
    
    public string UpdateWithTypedStatementString(Expression<Func<T, T>> updateStatement, Expression<Func<T, bool>>? where = null)
    {
        if (updateStatement == null) throw new ArgumentException("Cannot be null, the only reason it could be in code is to remind to use where statment because if where is null all records are updated this is used to remind", nameof(updateStatement));
        StringBuilder queryStringBuilder = new($"UPDATE {DbTable<T>.TableName} SET ");
        if (updateStatement.Body is not MemberInitExpression initExpression) throw new ArgumentException("Must be a MemberInitExpression (i.e. () => new Record() { Name = \"Some name\" })", nameof(updateStatement));

        int bindingCount = initExpression.Bindings.Count;
        int counter = 0;
        foreach (var binding in initExpression.Bindings)
        {
            if (binding is not MemberAssignment assigment)
                throw new ArgumentException("Bindings must be assigment (i.e. x => x.Name = \"New Name\")");

            if (!TypeMetadata<T>.NameToPropertyMap.TryGetValue(binding.Member.Name, out var propertyMetaData))
                throw new UskokDbPropertyNotMapped(binding.Member.Name);

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
        queryStringBuilder.Append(";\n");
        return queryStringBuilder.ToString();
        
    }

    public string BuildDeleteQueryString(Expression<Func<T, bool>>? where)
    {
        var queryStringBuilder = new StringBuilder("DELETE FROM ");
        queryStringBuilder.Append(TableName);
        if(where != null)
        {
            queryStringBuilder.Append(" WHERE ");
            queryStringBuilder.Append(LinqToSql.Convert<T>(DbContext, where));
        }

        queryStringBuilder.Append(';');

        return queryStringBuilder.ToString();
    }

    private void AppendDeleteByKeyLine(StringBuilder builder, T item)
    {
        var tableKeys = TypeMetadata<T>.Keys;
        var tableKeysCount = tableKeys.Count;
        if (tableKeysCount == 0)
            throw new UskokDbTableNotPrimaryKey(TableName);
        builder.Append("DELETE FROM ");
        builder.Append(TableName);
        builder.Append(" WHERE ");
        int i = 0;
        foreach (var keyPropertyMetadata in tableKeys)
        {
            var keyValue = keyPropertyMetadata.PropertyInfo.GetValue(item);
            var keyPropertyName = keyPropertyMetadata.PropertyName;
            builder.Append(keyPropertyName);
            builder.Append("=");
            builder.Append(DbContext.DbIo.WriteValue(keyValue));
            if (i + 1 == tableKeysCount) break;
            builder.Append(" AND ");
            i++;
        }
        

        builder.Append(';');
    }

    public string BuildDeleteByKey(T item)
    {
        StringBuilder builder = new();
        AppendDeleteByKeyLine(builder, item);
        return builder.ToString();
    }
    
    public string BuildDeleteByKey(IEnumerable<T> items)
    {
        StringBuilder builder = new();
        foreach (var item in items)
        {
            AppendDeleteByKeyLine(builder, item);
            builder.Append('\n');
        }
        return builder.ToString();
    }
    
    #endregion

    public Task InsertAsync(T item, CancellationToken cancellationToken = default) => DbContext.ExecuteAsync(BuildInsertQueryString(item), cancellationToken: cancellationToken);
    public Task InsertAsync(params T[] items) => DbContext.ExecuteAsync(BuildInsertQueryString(items));
    public Task InsertAsync(CancellationToken cancellationToken, params T[] items) => DbContext.ExecuteAsync(BuildInsertQueryString(items), cancellationToken: cancellationToken);
    public Task InsertAsync(IEnumerable<T> items, CancellationToken cancellationToken = default) => DbContext.ExecuteAsync(BuildInsertQueryString(items), cancellationToken: cancellationToken);

    public QueryBuilder<T> Where(Expression<Func<T, bool>> expression) => new QueryBuilder<T>(this).Where(expression);
    public QueryBuilder<T> Where(string query, object? paramsObject = null) => new QueryBuilder<T>(this).Where(query, paramsObject);
    public QueryBuilder<T> OrderBy(params string[] columns) => new QueryBuilder<T>(this).OrderBy(columns);

    public QueryBuilder<T> GroupBy(params string[] columns) => new QueryBuilder<T>(this).GroupBy(columns);

    public Task<int> UpdateAsync(Expression<Func<T>> update, Expression<Func<T, bool>>? where = null, CancellationToken cancellationToken = default) =>
        DbContext.ExecuteAsync(UpdateStatementString(update, where), cancellationToken: cancellationToken);
    
    public Task<int> UpdateAsync(Expression<Func<T, T>> update, Expression<Func<T, bool>>? where = null, CancellationToken cancellationToken = default) =>
        DbContext.ExecuteAsync(UpdateWithTypedStatementString(update, where), cancellationToken: cancellationToken);
    
    public Task<int> DeleteAsync(Expression<Func<T, bool>>? where, CancellationToken cancellationToken = default) =>
        DbContext.ExecuteAsync(BuildDeleteQueryString(where), cancellationToken: cancellationToken);
    public Task<int> DeleteByKeyAsync(T item) =>
        DbContext.ExecuteAsync(BuildDeleteByKey(item));
    public Task<int> DeleteByKeyAsync(IEnumerable<T> items) =>
        DbContext.ExecuteAsync(BuildDeleteByKey(items));
    public Task<int> DeleteByKeyAsync(params T[] items) =>
        DbContext.ExecuteAsync(BuildDeleteByKey(items));
    
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>>? where, CancellationToken cancellationToken = default){
        var queryStringBuilder = new StringBuilder("SELECT 1 FROM ");
        queryStringBuilder.Append(TableName);
        if(where != null){
            queryStringBuilder.Append(" WHERE ");
            queryStringBuilder.Append(LinqToSql.Convert<T>(DbContext, where));
        }

        var finalQuery = queryStringBuilder.ToString();

        #if !NETSTANDARD2_0
        await using var command = await DbContext.CreateCommand(finalQuery, null, cancellationToken: cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken: cancellationToken);
        #else
        using var command = await DbContext.CreateCommand(finalQuery, null, cancellationToken: cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken: cancellationToken);
        #endif
        return reader.HasRows;
    }


    public void AppendInsert(T item) => DbContext.AppendQueryCmd(BuildInsertQueryString(item), appendSemiColor: false);
    public void AppendInsert(IEnumerable<T> items) => DbContext.AppendQueryCmd(BuildInsertQueryString(items), appendSemiColor: false);
    public void AppendInsert(params T[] items) => DbContext.AppendQueryCmd(BuildInsertQueryString(items), appendSemiColor: false);
    public void AppendUpdate(Expression<Func<T>> update, Expression<Func<T, bool>>? where = null) =>
        DbContext.AppendQueryCmd(UpdateStatementString(update, where), appendSemiColor: false);
    public void AppendUpdate(Expression<Func<T, T>> update, Expression<Func<T, bool>>? where = null) =>
        DbContext.AppendQueryCmd(UpdateWithTypedStatementString(update, where), appendSemiColor: false);
    
    public void AppendDelete(Expression<Func<T, bool>>? where) => DbContext.AppendQueryCmd(BuildDeleteQueryString(where), appendSemiColor: false);
    public void AppendDeleteByKey(T item) => DbContext.AppendQueryCmd(BuildDeleteByKey(item), appendSemiColor: false);
    public void AppendDeleteByKey(IEnumerable<T> items) => DbContext.AppendQueryCmd(BuildDeleteByKey(items), appendSemiColor: false);
    public void AppendDeleteByKey(params T[] items) => DbContext.AppendQueryCmd(BuildDeleteByKey(items), appendSemiColor: false);
}
