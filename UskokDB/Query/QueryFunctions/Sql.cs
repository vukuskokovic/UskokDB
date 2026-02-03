using System.Collections;
using System.Collections.Generic;

namespace UskokDB.Query.QueryFunctions;

public static class Sql
{
    public static bool In<T>(T value, IEnumerable<T> values)
    {
        throw new UskokDbException("This functions is not for use outside of linq queries");
    }

    public static bool Exists(IQueryContext context)
    {
        throw new UskokDbException("This functions is not for use outside of linq queries");
    }
}