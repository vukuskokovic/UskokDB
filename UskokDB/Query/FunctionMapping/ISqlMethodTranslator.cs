using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UskokDB.Query.FunctionMapping;

public interface ISqlMethodTranslator
{
    public string Translate(IQueryContext queryContext, MethodCallExpression methodCall, string namePrefix,
        List<DbParam> dbParams, ref int propertyIndex, out Type? outType);
}