using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UskokDB.Query.PropertyMapping.Date;

public class SecondPropertyTranslator : ISqlPropertyTranslator
{
    public static readonly MemberInfo Member =
        typeof(DateTime).GetMember(nameof(DateTime.Second))![0];


    public string Translate(IQueryContext queryContext, MemberExpression methodCall, string namePrefix,
        List<DbParam> dbParams,
        ref int propertyIndex, out Type? outType)
    {
        outType = typeof(int);

        var compiled =
            queryContext.AppendExpression(methodCall.Expression, namePrefix, dbParams, ref propertyIndex, out _);

        return UskokDb.SqlDialect switch
        {
            SqlDialect.MySql => $"SECOND({compiled})",
            SqlDialect.PostgreSql => $"EXTRACT(SECOND FROM {compiled})",
            SqlDialect.SqLite => $"CAST(strftime('%s', {compiled}) AS INTEGER)",
            _ => throw new UskokDbSqlDialectNotSet()
        };
    }
}