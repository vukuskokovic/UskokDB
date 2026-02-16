using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UskokDB.Query.FunctionMapping.Sql;

public class LikeTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method = typeof(QueryFunctions.Sql).GetMethod(nameof(QueryFunctions.Sql.Like))!;
    
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = typeof(bool);
        StringBuilder builder = new();
        builder.Append(queryContext.AppendExpression(methodCall.Arguments[0], namePrefix, dbParams, ref propertyIndex, out _));
        builder.Append(" LIKE ");
        builder.Append(queryContext.AppendExpression(methodCall.Arguments[1], namePrefix, dbParams, ref propertyIndex, out _));
        
        
        
        return builder.ToString();
    }
}