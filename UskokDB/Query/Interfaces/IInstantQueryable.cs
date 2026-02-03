using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UskokDB.Query;

public interface IInstantQueryable<T>
{
    public Task<T?> QuerySingleAsync(CancellationToken cancellationToken = default, bool printToConsole = false);
    public Task<T?> QuerySingleWhereAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default, bool printToConsole = false);
    
    public Task<List<T>> QueryAsync(CancellationToken cancellationToken = default, bool printToConsole = false);
    public Task<List<T>> QueryWhereAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default, bool printToConsole = false);
}