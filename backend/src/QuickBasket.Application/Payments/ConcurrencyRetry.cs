using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Common;

namespace QuickBasket.Application.Payments;

internal static class ConcurrencyRetry
{
    /// <summary>
    /// Runs a unit of work in a transaction. If another writer changed the same payment first, tracking is
    /// cleared and the action re-runs from fresh data. The action must read its entities inside the delegate.
    /// </summary>
    public static async Task<T> RunAsync<T>(IUnitOfWork uow, Func<CancellationToken, Task<T>> action, CancellationToken ct, int attempts = 3)
    {
        for (var attempt = 1; ; attempt++)
        {
            try { return await uow.ExecuteInTransactionAsync(action, ct); }
            catch (ConcurrencyConflictException) when (attempt < attempts) { uow.ClearTracking(); }
        }
    }
}
