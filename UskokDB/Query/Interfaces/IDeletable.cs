using System.Threading;
using System.Threading.Tasks;

namespace UskokDB.Query;

public interface IDeletable<T> where T : class, new()
{
    public QueryContext<T> Delete();
    public Task<int> DeleteAsync(bool printToConsole = false, CancellationToken cancellationToken = default);
}