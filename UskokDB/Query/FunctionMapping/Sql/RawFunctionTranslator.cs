using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UskokDB.Query.FunctionMapping.Sql;

public class RawFunctionTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method = typeof(QueryFunctions.Sql).GetMethod(nameof(QueryFunctions.Sql.Function))!;
    
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = methodCall.Method.GetGenericArguments()[0];
        var functionName = (string)(methodCall.Arguments[0] as ConstantExpression)!.Value;
        var builder = new StringBuilder();

        builder.Append(functionName);
        builder.Append('(');

        var arrayExpression = methodCall.Arguments[1] as NewArrayExpression;
        for (int i = 0; i < arrayExpression!.Expressions.Count; i++)
        {
            builder.Append(queryContext.AppendExpression(arrayExpression.Expressions[i], namePrefix, dbParams, ref propertyIndex,
                out _));
            if(i+1 == arrayExpression.Expressions.Count)continue;

            builder.Append(", ");
        }
        
        
        builder.Append(')');
        
        
        return builder.ToString();
    }
}