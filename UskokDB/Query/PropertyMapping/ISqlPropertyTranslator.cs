using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UskokDB.Query.PropertyMapping;

public interface ISqlPropertyTranslator
{
    public string Translate(IQueryContext queryContext, MemberExpression methodCall, string namePrefix,
        List<DbParam> dbParams, ref int propertyIndex, out Type? outType);
}