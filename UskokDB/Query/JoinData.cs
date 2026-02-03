using System.Linq.Expressions;

namespace UskokDB.Query;

internal class JoinData
{
    internal IQueryItem JoinOn { get; set; } = null!;
    internal JoinType JoinType { get; set; }
    internal Expression Expression { get; set; } = null!;
}

internal enum JoinType
{
    Left,
    Inner
}