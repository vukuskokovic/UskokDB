using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UskokDB.Query.FunctionMapping.Sql;

public class CastTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method = typeof(QueryFunctions.Sql).GetMethod(nameof(QueryFunctions.Sql.Cast))!;
    
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        var castToType = methodCall.Method.GetGenericArguments()[0];
        outType = castToType;
        StringBuilder builder = new("CAST(");
        builder.Append(queryContext.AppendExpression(methodCall.Arguments[0], namePrefix, dbParams, ref propertyIndex, out _));
        builder.Append(" AS ");
        builder.Append(DbInitialization.TypeInDatabase[castToType][UskokDb.SqlDialect]);
        builder.Append(')');
        
        
        
        return builder.ToString();
    }
}