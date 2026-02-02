using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UskokDB.Query.PropertyMapping.Date;

public class MinutePropertyTranslator : ISqlPropertyTranslator
{
    public static readonly MemberInfo Member =
        typeof(DateTime).GetMember(nameof(DateTime.Minute))![0];


    public string Translate(IQueryContext queryContext, MemberExpression methodCall, string namePrefix,
        List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = typeof(int);

        var compiled =
            queryContext.AppendExpression(methodCall.Expression, namePrefix, dbParams, ref propertyIndex, out _);

        return UskokDb.SqlDialect switch
        {
            SqlDialect.MySql => $"MINUTE({compiled})",
            SqlDialect.PostgreSql => $"EXTRACT(MINUTE FROM {compiled})",
            SqlDialect.SqLite => $"CAST(strftime('%M', {compiled}) AS INTEGER)",
            _ => throw new UskokDbSqlDialectNotSet()
        };
    }
}