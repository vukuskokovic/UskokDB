using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

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

    public static T Cast<T>(object value)
    {
        throw new UskokDbException("This functions is not for use outside of linq queries");
    }

    public static T Function<T>(string functionName, params object[] args)
    {
        throw new UskokDbException("This functions is not for use outside of linq queries");
    }

    public static bool Like(string searchString, string format)
    {
        throw new UskokDbException("This functions is not for use outside of linq queries");
    }

    public static T Coalesce<T>(params T?[] values)
    {
        throw new UskokDbException("This functions is not for use outside of linq queries");
    }

    public static T[] JsonCreateArray<T, T0>(Expression<Func<T0, T>> expression)
    {
        throw new UskokDbException("This functions is not for use outside of linq queries");
    }
    
    public static T[] JsonCreateArray<T, T0, T1>(Expression<Func<T0, T1, T>> expression)
    {
        throw new UskokDbException("This functions is not for use outside of linq queries");
    }
}