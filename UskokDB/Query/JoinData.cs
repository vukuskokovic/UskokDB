using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UskokDB.Query;

namespace UskokDB;

internal class JoinData
{
    internal IQueryable JoinOn { get; set; } = null!;
    internal JoinType JoinType { get; set; }
    internal Expression Expression { get; set; } = null!;
}

internal enum JoinType
{
    Left,
    Inner
}