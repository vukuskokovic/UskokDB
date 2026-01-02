using System;
using System.Collections.Generic;

namespace UskokDB.Query;

public class Many<T>  where T : class
{
    public static implicit operator List<T>(Many<T> _)
        => throw new InvalidOperationException(
            "Many<T> is a query placeholder and cannot be materialized directly.");
    
    public static implicit operator T[](Many<T> _)
        => throw new InvalidOperationException(
            "Many<T> is a query placeholder and cannot be materialized directly.");
}