namespace UskokDB.Query;

public interface ILimitable<T> where T : class, new()
{
    public QueryContext<T> Limit(int limit);
}