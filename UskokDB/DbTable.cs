using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UskokDB.Attributes;
using UskokDB.Query;

namespace UskokDB;
public class DbTable<T>(DbContext context) : QueryItem<T>(context) where T : class, new()
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
    public DbCommand BuildInsertCommand(string initCommand, T item)
    {
        StringBuilder builder = new(initCommand);
        var command = DbContext.DbConnection.CreateCommand();
        int i = 0;
        AddInsertItem(command, item, builder, ref i);
        command.CommandText = builder.ToString();
        return command;
    }
    public DbCommand BuildInsertCommand(string initCommand, IEnumerable<T> items)
    {
        StringBuilder builder = new(initCommand);
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
    
    #endregion

    public Task InsertAsync(T item, CancellationToken cancellationToken = default) => DbContext.ExecuteAsync(BuildInsertCommand(InsertInitString, item), cancellationToken: cancellationToken);
    public Task InsertAsync(params T[] items) => DbContext.ExecuteAsync(BuildInsertCommand(InsertInitString, items));
    public Task InsertAsync(CancellationToken cancellationToken, params T[] items) => DbContext.ExecuteAsync(BuildInsertCommand(InsertInitString, items), cancellationToken: cancellationToken);
    public Task InsertAsync(IEnumerable<T> items, CancellationToken cancellationToken = default) => DbContext.ExecuteAsync(BuildInsertCommand(InsertInitString, items), cancellationToken: cancellationToken);


    private QueryContext<T> CreateQueryContext() => new(this, DbContext);
    public Task<List<T>> QueryAllAsync(CancellationToken cancellationToken = default) =>
        DbContext.QueryAsync<T>($"SELECT * FROM {TableName}", null, cancellationToken);
    public Task<int> UpdateAsync(Expression<Func<T>> update, Expression<Func<T, bool>> where, bool printToConsole = false, CancellationToken cancellationToken = default) =>
        CreateQueryContext().Where(where).UpdateAsync(update, printToConsole, cancellationToken);

    public Task<int> UpdateAsync(Expression<Func<T, T>> update, Expression<Func<T, bool>> where, bool printToConsole = false,
        CancellationToken cancellationToken = default) =>
        CreateQueryContext().Where(where).UpdateAsync(update, printToConsole, cancellationToken);

    public Task<int> DeleteAsync(Expression<Func<T, bool>> where, bool printToConsole = false, CancellationToken cancellationToken = default) =>
        CreateQueryContext().Where(where).DeleteAsync(printToConsole, cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> where, bool printToConsole = false,
        CancellationToken cancellationToken = default)
        => CreateQueryContext().Where(where).Exists(printToConsole);


    public void AppendInsert(T item) => DbContext.AppendQueueCmd(BuildInsertCommand(InsertInitString, item));
    public void AppendInsert(IEnumerable<T> items) => DbContext.AppendQueueCmd(BuildInsertCommand(InsertInitString, items));
    public void AppendInsert(params T[] items) => DbContext.AppendQueueCmd(BuildInsertCommand(InsertInitString, items));
    public void AppendUpdate(Expression<Func<T>> update, Expression<Func<T, bool>> where, bool printToConsole = false) =>
        DbContext.AppendExecute(CreateQueryContext().Where(where).Update(update), printToConsole);
    public void AppendUpdate(Expression<Func<T, T>> update, Expression<Func<T, bool>> where, bool printToConsole = false) =>
        DbContext.AppendExecute(CreateQueryContext().Where(where).Update(update), printToConsole);
    
    public void AppendDelete(Expression<Func<T, bool>> where, bool printToConsole = false) => 
        DbContext.AppendExecute(CreateQueryContext().Where(where).Delete(), printToConsole);

    public override string GetName() => TableName;
    public override Type GetUnderlyingType() => typeof(T);
    public override string? PreQuery(List<DbParam> _) => null;
}
