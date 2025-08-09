using System;
using System.Linq.Expressions;

namespace UskokDB;

public class UskokDbException(string message) : Exception(message);

public sealed class UskokDbUnsupportedDbTypeException() : UskokDbException("This db type is not supported");
public sealed class UskokDbIoException(string message) : UskokDbException(message);

public sealed class UskokDbPropertyNotMapped(string propertyName) : UskokDbException($"Property '{propertyName}' is not mapped to db")
{
    public string PropertyName { get; set; } = propertyName;
}

public abstract class UskokDbLinqException(string message) : UskokDbException(message);

public sealed class UskokDbInvalidLinqBinding() : UskokDbLinqException("Bindings must be assigment (i.e. x => x.Name = \"New Name\")");

public sealed class UskokDbInvalidLinqExpression(Expression expression)
    : UskokDbLinqException($"{expression.GetType().FullName} expression is not supported")
{
    public Expression Expression { get; set; } = expression;
}

public sealed class UskokDbTableNotPrimaryKey(string tableName) : UskokDbException($"Table '{tableName}' has no primary keys")
{
    public string TableName { get; set; } = tableName;
}

public sealed class UskokDbSqlDialectNotSet() : UskokDbException("Sql dialect is not set");