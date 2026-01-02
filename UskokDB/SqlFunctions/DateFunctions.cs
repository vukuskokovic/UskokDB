using System;

namespace UskokDB.SqlFunctions;

public static partial class Sql
{
    public static string Date(DateTime date) => throw new UskokDbSqlFunctionUsedOutsideOfLinq();
    public static string Hour(DateTime date) => throw new UskokDbSqlFunctionUsedOutsideOfLinq();
    public static string Minute(DateTime date) => throw new UskokDbSqlFunctionUsedOutsideOfLinq();
    public static string Day(DateTime date) => throw new UskokDbSqlFunctionUsedOutsideOfLinq();
}