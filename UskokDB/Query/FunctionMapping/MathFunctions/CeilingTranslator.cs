using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UskokDB.Query.FunctionMapping.MathFunctions;

public class CeilingTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo[] Method =
    [
        typeof(Math).GetMethod(nameof(Math.Ceiling), [typeof(double)])!,
        typeof(Math).GetMethod(nameof(Math.Ceiling), [typeof(decimal)])!
    ];
    
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        var castToType = methodCall.Method.ReturnType;
        outType = castToType;
        StringBuilder builder = new("CEILING(");
        builder.Append(queryContext.AppendExpression(methodCall.Arguments[0], namePrefix, dbParams, ref propertyIndex, out _));
        builder.Append(')');
        
        
        
        return builder.ToString();
    }
}