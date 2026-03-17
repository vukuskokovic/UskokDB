using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace UskokDB.Query;

public interface IUpdatable<T> where T : class, new()
{
    public QueryContext<T> Update(Expression<Func<T>> updateFunc);
    public QueryContext<T> Update(Expression<Func<T, T>> updateFunc);
    public QueryContext<T> Update<T1>(Expression<Func<T, T1>> updateFunc);
    public QueryContext<T> Update<T1, T2>(Expression<Func<T, T1, T2>> updateFunc);
    public QueryContext<T> Update<T1, T2, T3>(Expression<Func<T, T1, T2, T3>> updateFunc);
    public QueryContext<T> Update<T1, T2, T3, T4>(Expression<Func<T, T1, T2, T3, T4>> updateFunc);
    public QueryContext<T> Update<T1, T2, T3, T4, T5>(Expression<Func<T, T1, T2, T3, T4, T5>> updateFunc);
    public QueryContext<T> Update<T1, T2, T3, T4, T5, T6>(Expression<Func<T, T1, T2, T3, T4, T5, T6>> updateFunc);
    
    public Task<int> UpdateAsync(Expression<Func<T>> updateFunc, bool printToConsole = false, CancellationToken cancellationToken = default);
    public Task<int> UpdateAsync(Expression<Func<T, T>> updateFunc, bool printToConsole = false, CancellationToken cancellationToken = default);
    public Task<int> UpdateAsync<T1>(Expression<Func<T, T1>> updateFunc, bool printToConsole = false, CancellationToken cancellationToken = default);
    public Task<int> UpdateAsync<T1, T2>(Expression<Func<T, T1, T2>> updateFunc, bool printToConsole = false, CancellationToken cancellationToken = default);
    public Task<int> UpdateAsync<T1, T2, T3>(Expression<Func<T, T1, T2, T3>> updateFunc, bool printToConsole = false, CancellationToken cancellationToken = default);
    public Task<int> UpdateAsync<T1, T2, T3, T4>(Expression<Func<T, T1, T2, T3, T4>> updateFunc, bool printToConsole = false, CancellationToken cancellationToken = default);
    public Task<int> UpdateAsync<T1, T2, T3, T4, T5>(Expression<Func<T, T1, T2, T3, T4, T5>> updateFunc, bool printToConsole = false, CancellationToken cancellationToken = default);
    public Task<int> UpdateAsync<T1, T2, T3, T4, T5, T6>(Expression<Func<T, T1, T2, T3, T4, T5, T6>> updateFunc, bool printToConsole = false, CancellationToken cancellationToken = default);
}