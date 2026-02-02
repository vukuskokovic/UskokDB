using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UskokDB.Query.PropertyMapping.Date;

public class TimeOfDayPropertyTranslator : ISqlPropertyTranslator
{
    public static readonly MemberInfo Member =
        typeof(DateTime).GetMember(nameof(DateTime.TimeOfDay))![0];


    public string Translate(IQueryContext queryContext, MemberExpression methodCall, string namePrefix,
        List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = typeof(TimeSpan);

        var compiled =
            queryContext.AppendExpression(methodCall.Expression, namePrefix, dbParams, ref propertyIndex, out _);

        return UskokDb.SqlDialect switch
        {
            SqlDialect.MySql => $"TIME({compiled})",
            SqlDialect.PostgreSql => $"CAST({compiled} as TIME)",
            SqlDialect.SqLite => $"time({compiled})",
            _ => throw new UskokDbSqlDialectNotSet()
        };
    }
}