using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UskokDB.Query;

public interface IQueryContext
{
    internal string AppendExpression(Expression expression, string namePrefix, List<DbParam> dbParams,
        ref int propertyIndex, out Type? outputType);

    internal string AddParam(List<DbParam> paramList, object value, string namePrefix, ref int propertyIndex);
}