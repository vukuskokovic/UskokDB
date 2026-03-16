using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UskokDB.Query.FunctionMapping.Sql;

public class RawSqlTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method = typeof(QueryFunctions.Sql).GetMethod(nameof(QueryFunctions.Sql.RawSql))!;
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = methodCall.Method.GetGenericArguments()[0];

        if (methodCall.Arguments[0] is not ConstantExpression constantExpression)
            throw new UskokDbException("Raw sql string is not a constant");

        if (methodCall.Arguments[1] is not NewExpression newExpression)
            throw new UskokDbException("ObjectParams is not new expression");

        if (newExpression.Members is null)
            throw new Exception("Members null");
        
        var str = (string)constantExpression.Value;
        var second = methodCall.Arguments[1].GetType().FullName;
        Console.WriteLine(newExpression.Arguments[0]);
        Console.Write(newExpression.Members.Count);

        return "TEST";
    }
}