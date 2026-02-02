using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UskokDB.Query.FunctionMapping.StringFunctions;

public class SubStringMethodTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method1 =
        typeof(string).GetMethod(
            nameof(string.Substring),
            [typeof(int)]
        )!;
    
    public static readonly MethodInfo Method2 =
        typeof(string).GetMethod(
            nameof(string.Substring),
            [typeof(int), typeof(int)]
        )!;
    
    
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = typeof(string);
        var arg1 = methodCall.Arguments[0]!;
        var objectCompiled = queryContext.AppendExpression(methodCall.Object!, namePrefix, dbParams, ref propertyIndex, out _);
        var arg1Compiled = queryContext.AppendExpression(arg1, namePrefix, dbParams, ref propertyIndex, out _);
        
        //Substring no length
        if (methodCall.Arguments.Count == 1)
        {
            return UskokDb.SqlDialect switch
            {
                SqlDialect.MySql => $"SUBSTRING({objectCompiled}, {arg1Compiled} + 1)",
                SqlDialect.PostgreSql => $"SUBSTRING({objectCompiled} FROM {arg1Compiled} + 1)",
                SqlDialect.SqLite => $"SUBSTR({objectCompiled}, {arg1} + 1)",
                _ => throw new UskokDbException("No db dialect found?")
            };
        }
        else
        {
            var arg2Compiled = queryContext.AppendExpression(methodCall.Arguments[1]!, namePrefix, dbParams, ref propertyIndex, out _);
            return UskokDb.SqlDialect switch
            {
                SqlDialect.MySql => $"SUBSTRING({objectCompiled}, {arg1Compiled} + 1, {arg2Compiled})",
                SqlDialect.PostgreSql => $"SUBSTRING({objectCompiled} FROM {arg1Compiled} + 1 FOR {arg2Compiled})",
                SqlDialect.SqLite => $"SUBSTR({objectCompiled}, {arg1} + 1, {arg2Compiled})",
                _ => throw new UskokDbException("No db dialect found?")
            };
        }
    }
}