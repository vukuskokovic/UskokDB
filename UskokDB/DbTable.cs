﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UskokDB.Attributes;

namespace UskokDB;
public class DbTable<T>(DbContext context) where T : class, new()
{
    private static string InsertInitString { get; } = $"INSERT INTO {TableName} VALUES ";
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
    public DbCommand BuildInsertCommand(T item)
    {
        StringBuilder builder = new(InsertInitString);
        var command = DbContext.DbConnection.CreateCommand();
        int i = 0;
        AddInsertItem(command, item, builder, ref i);
        command.CommandText = builder.ToString();
        return command;
    }
    public DbCommand BuildInsertCommand(IEnumerable<T> items)
    {
        StringBuilder builder = new(InsertInitString);
        var command = DbContext.DbConnection.CreateCommand();
        int index = 0;
        foreach(var item in items)
        {
            AddInsertItem(command, item, builder, ref index);
            builder.Append(',');
        }
        builder.Length--;
        command.CommandText = builder.ToString();
        return command;
    }

    private void AddInsertItem(DbCommand command, T item, StringBuilder builder, ref int index)
    {
        builder.Append('(');
        int i = 0;
        int l = TypeMetadata<T>.Properties.Count;
        foreach (var property in TypeMetadata<T>.Properties)
        {
            var dbParamName = $"@iT{index}p{i}";
            var dbParameter = command.CreateParameter();
            dbParameter.ParameterName = dbParamName;
            dbParameter.Value = DbIO.WriteValue(property.GetMethod(item));
            
            command.Parameters.Add(dbParameter);

            builder.Append(dbParamName);
            if (++i != l) builder.Append(',');
        }

        builder.Append(')');
        index++;
    }
    
    public DbCommand BuildUpdateCommand(Expression<Func<T>> updateStatement, Expression<Func<T, bool>>? where = null)
    {
        if (updateStatement == null) throw new ArgumentException("Cannot be null, the only reason it could be in code is to remind to use where statement because if where is null all records are updated this is used to remind", nameof(updateStatement));
        StringBuilder queryStringBuilder = new($"UPDATE {TableName} SET ");
        if (updateStatement.Body is not MemberInitExpression initExpression) throw new ArgumentException("Must be a MemberInitExpression (i.e. () => new Record() { Name = \"Some name\" })", nameof(updateStatement));

        List<DbParam> paramList = [];
        int bindingCount = initExpression.Bindings.Count;
        int counter = 0;
        foreach (var binding in initExpression.Bindings)
        {
            if (binding is not MemberAssignment assigment)
                throw new UskokDbInvalidLinqBinding();

            if (!TypeMetadata<T>.NameToPropertyMap.TryGetValue(binding.Member.Name, out var propertyMetaData))
                throw new UskokDbPropertyNotMapped(binding.Member.Name);

            var valueWritten = LinqToSql.CompileExpression<T>(assigment.Expression, paramList);
            queryStringBuilder.Append($"{propertyMetaData.PropertyName}={valueWritten}");
            if (++counter == bindingCount) continue;

            queryStringBuilder.Append(',');
        }
        if (where != null)
        {
            var cmd = LinqToSql.Convert(where, paramList).CompiledText;
            queryStringBuilder.Append(" WHERE ");
            queryStringBuilder.Append(cmd);
        }

        return new DbPopulateParamsResult()
        {
            CompiledText = queryStringBuilder.ToString(),
            Params = paramList
        }.CreateCommandWithConnection(DbContext.DbConnection);

    }
    
    public DbCommand BuildUpdateCommandWithRowContext(Expression<Func<T, T>> updateStatement, Expression<Func<T, bool>>? where = null)
    {
        if (updateStatement == null) throw new ArgumentException("Cannot be null, the only reason it could be in code is to remind to use where statment because if where is null all records are updated this is used to remind", nameof(updateStatement));
        StringBuilder queryStringBuilder = new($"UPDATE {TableName} SET ");
        if (updateStatement.Body is not MemberInitExpression initExpression) throw new ArgumentException("Must be a MemberInitExpression (i.e. () => new Record() { Name = \"Some name\" })", nameof(updateStatement));

        List<DbParam> paramList = [];
        int bindingCount = initExpression.Bindings.Count;
        int counter = 0;
        foreach (var binding in initExpression.Bindings)
        {
            if (binding is not MemberAssignment assigment)
                throw new ArgumentException("Bindings must be assigment (i.e. x => x.Name = \"New Name\")");

            if (!TypeMetadata<T>.NameToPropertyMap.TryGetValue(binding.Member.Name, out var propertyMetaData))
                throw new UskokDbPropertyNotMapped(binding.Member.Name);

            var valueWritten = LinqToSql.CompileExpression<T>(assigment.Expression, paramList);
            queryStringBuilder.Append($"{propertyMetaData.PropertyName}={valueWritten}");
            if (++counter == bindingCount) continue;

            queryStringBuilder.Append(',');
        }
        if (where != null)
        {
            var cmd = LinqToSql.Convert(where, paramList).CompiledText;
            queryStringBuilder.Append(" WHERE ");
            queryStringBuilder.Append(cmd);
        }
        
        return new DbPopulateParamsResult()
        {
            CompiledText = queryStringBuilder.ToString(),
            Params = paramList
        }.CreateCommandWithConnection(DbContext.DbConnection);
        
    }

    public DbCommand BuildDeleteCommand(Expression<Func<T, bool>>? where)
    {
        var queryStringBuilder = new StringBuilder("DELETE FROM ");
        queryStringBuilder.Append(TableName);
        DbPopulateParamsResult? populateParamsResult = null;
        if(where != null)
        {
            queryStringBuilder.Append(" WHERE ");
            populateParamsResult = LinqToSql.Convert(where);
            queryStringBuilder.Append(populateParamsResult.CompiledText);
        }

        var finalCmd = queryStringBuilder.ToString();
        populateParamsResult ??= new DbPopulateParamsResult()
        {
            CompiledText = finalCmd,
            Params = []
        };

        populateParamsResult.CompiledText = finalCmd;
            
        return populateParamsResult.CreateCommandWithConnection(DbContext.DbConnection);
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
            var keyValue = keyPropertyMetadata.GetMethod(item);
            var keyPropertyName = keyPropertyMetadata.PropertyName;
            builder.Append(keyPropertyName);
            builder.Append('=');
            builder.Append(DbIO.WriteValue(keyValue));
            if (i + 1 == tableKeysCount) break;
            builder.Append(" AND ");
            i++;
        }
        

        builder.Append(';');
    }

    public DbCommand BuildDeleteByKey(T item)
    {
        StringBuilder builder = new();
        AppendDeleteByKeyLine(builder, item);
        return DbContext.CreateCommand(builder.ToString());
    }
    
    public DbCommand BuildDeleteByKey(IEnumerable<T> items)
    {
        StringBuilder builder = new();
        foreach (var item in items)
        {
            AppendDeleteByKeyLine(builder, item);
            builder.Append('\n');
        }
        
        return DbContext.CreateCommand(builder.ToString());
    }
    
    #endregion

    public Task InsertAsync(T item, CancellationToken cancellationToken = default) => DbContext.ExecuteAsync(BuildInsertCommand(item), cancellationToken: cancellationToken);
    public Task InsertAsync(params T[] items) => DbContext.ExecuteAsync(BuildInsertCommand(items));
    public Task InsertAsync(CancellationToken cancellationToken, params T[] items) => DbContext.ExecuteAsync(BuildInsertCommand(items), cancellationToken: cancellationToken);
    public Task InsertAsync(IEnumerable<T> items, CancellationToken cancellationToken = default) => DbContext.ExecuteAsync(BuildInsertCommand(items), cancellationToken: cancellationToken);

    public QueryBuilder<T> Where(Expression<Func<T, bool>> expression) => new QueryBuilder<T>(this).Where(expression);
    public QueryBuilder<T> Where(string query, object? paramsObject = null) => new QueryBuilder<T>(this).Where(query, paramsObject);
    public QueryBuilder<T> OrderBy(params string[] columns) => new QueryBuilder<T>(this).OrderBy(columns);

    public QueryBuilder<T> GroupBy(params string[] columns) => new QueryBuilder<T>(this).GroupBy(columns);

    public Task<int> UpdateAsync(Expression<Func<T>> update, Expression<Func<T, bool>>? where = null, CancellationToken cancellationToken = default) =>
        DbContext.ExecuteAsync(BuildUpdateCommand(update, where), cancellationToken: cancellationToken);
    
    public Task<int> UpdateAsync(Expression<Func<T, T>> update, Expression<Func<T, bool>>? where = null, CancellationToken cancellationToken = default) =>
        DbContext.ExecuteAsync(BuildUpdateCommandWithRowContext(update, where), cancellationToken: cancellationToken);
    
    public Task<int> DeleteAsync(Expression<Func<T, bool>>? where, CancellationToken cancellationToken = default) =>
        DbContext.ExecuteAsync(BuildDeleteCommand(where), cancellationToken: cancellationToken);
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
            queryStringBuilder.Append(LinqToSql.Convert<T>(where));
        }

        var finalQuery = queryStringBuilder.ToString();

        #if !NETSTANDARD2_0
        await using var command = DbContext.CreateCommand(finalQuery);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken: cancellationToken);
        #else
        using var command = DbContext.CreateCommand(finalQuery);
        using var reader = await command.ExecuteReaderAsync(cancellationToken: cancellationToken);
        #endif
        return reader.HasRows;
    }


    public void AppendInsert(T item) => DbContext.AppendQueueCmd(BuildInsertCommand(item));
    public void AppendInsert(IEnumerable<T> items) => DbContext.AppendQueueCmd(BuildInsertCommand(items));
    public void AppendInsert(params T[] items) => DbContext.AppendQueueCmd(BuildInsertCommand(items));
    public void AppendUpdate(Expression<Func<T>> update, Expression<Func<T, bool>>? where = null) =>
        DbContext.AppendQueueCmd(BuildUpdateCommand(update, where));
    public void AppendUpdate(Expression<Func<T, T>> update, Expression<Func<T, bool>>? where = null) =>
        DbContext.AppendQueueCmd(BuildUpdateCommandWithRowContext(update, where));
    
    public void AppendDelete(Expression<Func<T, bool>>? where) => DbContext.AppendQueueCmd(BuildDeleteCommand(where));
    public void AppendDeleteByKey(T item) => DbContext.AppendQueueCmd(BuildDeleteByKey(item));
    public void AppendDeleteByKey(IEnumerable<T> items) => DbContext.AppendQueueCmd(BuildDeleteByKey(items));
    public void AppendDeleteByKey(params T[] items) => DbContext.AppendQueueCmd(BuildDeleteByKey(items));
}
