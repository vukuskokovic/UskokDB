using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UskokDB.Query.FunctionMapping.StringFunctions;

public class StartsWithMethodTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method =
        typeof(string).GetMethod(
            nameof(string.StartsWith),
            [typeof(string)]
        )!;
    
    
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = typeof(bool);
        var arg1 = methodCall.Arguments[0]!;
        
        var argCompiled = queryContext.AppendExpression(arg1, namePrefix, dbParams, ref propertyIndex, out _);
        string compiledLike;
        if (UskokDb.SqlDialect == SqlDialect.MySql)
            compiledLike = $"CONCAT({argCompiled}, '%')";
        else
            compiledLike = $"({argCompiled} || '%')";
        
        
        
        return $"{queryContext.AppendExpression(methodCall.Object!, namePrefix, dbParams, ref propertyIndex, out _)} LIKE {compiledLike}";
    }
}