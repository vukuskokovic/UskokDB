using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UskokDB.Query.QueryFunctions;

namespace UskokDB.Query.FunctionMapping.Sql;

public class ValueInTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method = typeof(QueryFunctions.Sql).GetMethod(nameof(QueryFunctions.Sql.In))!;
    
    
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = typeof(bool);

        var paramName =
            queryContext.AppendExpression(methodCall.Arguments[0]!, namePrefix, dbParams, ref propertyIndex, out _);

        
        
        var enumeratorArg = methodCall.Arguments[1]!;
        Console.WriteLine(enumeratorArg.GetType().FullName);
        
        return "Test";
    }
}