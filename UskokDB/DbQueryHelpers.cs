using System.Collections;
using System.Collections.Generic;

namespace UskokDB;

public static class DbQueryHelpers
{
    public static DbQueryHelperInResult In<T>(IEnumerable<T> items)
    {
        return new DbQueryHelperInResult(items);
    }

    public static DbQueryHelperInResult In<T>(params T[] items) => In((IEnumerable<T>)items);
}

public class DbQueryHelperInResult
{
    private readonly IEnumerable _values;

    public DbQueryHelperInResult(IEnumerable values)
    {
        _values = values;
    }

    public IEnumerable<object?> Values
    {
        get
        {
            foreach (var v in _values)
                yield return v;
        }
    }
}