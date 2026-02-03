using System;
using System.Collections.Generic;
using System.Reflection;
using UskokDB.Query.FunctionMapping;
using UskokDB.Query.FunctionMapping.Sql;
using UskokDB.Query.FunctionMapping.StringFunctions;
using UskokDB.Query.PropertyMapping;
using UskokDB.Query.PropertyMapping.Date;

namespace UskokDB;

public static class UskokDb
{
    public static Dictionary<MethodInfo, ISqlMethodTranslator> MethodTranslators { get; } = new();
    public static Dictionary<MemberInfo, ISqlPropertyTranslator> MemberTranslators { get; } = new();
    private static bool _registryCreated = false;
    public static void InitLinqMethodRegistry()
    {
        if (_registryCreated) return;
        MethodTranslators[StartsWithMethodTranslator.Method] = new StartsWithMethodTranslator();
        var subStringTranslator = new SubStringMethodTranslator();
        MethodTranslators[SubStringMethodTranslator.Method1] = subStringTranslator;
        MethodTranslators[SubStringMethodTranslator.Method2] = subStringTranslator;
        MethodTranslators[ValueInTranslator.Method] = new ValueInTranslator();

        MemberTranslators[HourPropertyTranslator.Member] = new HourPropertyTranslator();
        MemberTranslators[MinutePropertyTranslator.Member] = new MinutePropertyTranslator();
        MemberTranslators[SecondPropertyTranslator.Member] = new SecondPropertyTranslator();
        MemberTranslators[TimeOfDayPropertyTranslator.Member] = new TimeOfDayPropertyTranslator();
        

        _registryCreated = true;
    }
    
    internal static bool SqlDialectSet = false;
    public static SqlDialect SqlDialect { get; set; }

    public static void SetSqlDialect(SqlDialect dialect)
    {
        SqlDialect = dialect;
        SqlDialectSet = true;
    }
}

public enum SqlDialect
{
    MySql,
    PostgreSql,
    SqLite
}