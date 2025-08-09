namespace UskokDB;

public static class UskokDb
{
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