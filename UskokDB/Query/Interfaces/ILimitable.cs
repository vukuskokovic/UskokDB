namespace UskokDB.Query;

public interface ILimitable<T> where T : class
{
    public QueryContext<T> Limit(int limit);
    public QueryContext<T> Offset(int offset);
}