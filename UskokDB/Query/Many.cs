using System;
using System.Collections.Generic;

namespace UskokDB.Query;

/// <summary>
/// The type must contain a primary key in order to be used
/// <b>Never use many when expecting a large query result!!</b>
/// </summary>
/// <typeparam name="T">The type of the many(base type stored in db)</typeparam>
public class Many<T>  where T : class
{
    public static implicit operator List<T>(Many<T> _)
        => throw new InvalidOperationException(
            "Many<T> is a query placeholder and cannot be materialized directly.");
    
    public static implicit operator T[](Many<T> _)
        => throw new InvalidOperationException(
            "Many<T> is a query placeholder and cannot be materialized directly.");

    public T0[] MapToArray<T0>(Func<T, T0> func)
    {
        throw new InvalidOperationException(
            "Many<T> is a query placeholder and cannot be materialized directly.");
    }
    
    public List<T0> MapToList<T0>(Func<T, T0> func)
    {
        throw new InvalidOperationException(
            "Many<T> is a query placeholder and cannot be materialized directly.");
    }
}