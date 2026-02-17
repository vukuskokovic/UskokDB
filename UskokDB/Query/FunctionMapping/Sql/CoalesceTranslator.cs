using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UskokDB.Query.FunctionMapping.Sql;

public class CoalesceTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method = typeof(QueryFunctions.Sql).GetMethod(nameof(QueryFunctions.Sql.Coalesce))!;
    
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = methodCall.Method.GetGenericArguments()[0];
        
        
        StringBuilder builder = new("COALESCE(");
        var arrayExp = (NewArrayExpression)methodCall.Arguments[0];
        for (var i = 0; i < arrayExp.Expressions.Count; i++)
        {
            var arg = arrayExp.Expressions[i];
            builder.Append(queryContext.AppendExpression(arg, namePrefix, dbParams, ref propertyIndex, out _));
            
            if (i + 1 == arrayExp.Expressions.Count)
                break;

            builder.Append(", ");
        }

        builder.Append(')');
        
        return builder.ToString();
    }
}