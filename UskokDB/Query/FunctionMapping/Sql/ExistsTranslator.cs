using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UskokDB.Query.FunctionMapping.Sql;

public class ExistsTranslator : ISqlMethodTranslator
{
    public static readonly MethodInfo Method = typeof(QueryFunctions.Sql).GetMethod(nameof(QueryFunctions.Sql.Exists))!;
    
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = typeof(bool);
        
        
        return "EXISTS (SELECT 1 FROM Test)";
    }
}