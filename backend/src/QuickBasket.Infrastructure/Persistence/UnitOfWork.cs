using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Common;

namespace QuickBasket.Infrastructure.Persistence;

public sealed class UnitOfWork(QuickBasketDbContext db) : IUnitOfWork
{
    private const int SqlUniqueIndexViolation = 2601;
    private const int SqlUniqueConstraintViolation = 2627;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Order events are append-only. If EF wrongly thinks a brand-new one already exists, insert it.
        foreach (var e in db.ChangeTracker.Entries<QuickBasket.Domain.Entities.OrderEvent>()
                     .Where(x => x.State == EntityState.Modified))
            e.State = EntityState.Added;

        try
        {
            return await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var rows = string.Join("; ", ex.Entries.Select(x => $"{x.Metadata.ClrType.Name} is {x.State}"));
            throw new ConcurrencyConflictException($"{ex.Message} Rows involved: {rows}");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: SqlUniqueIndexViolation or SqlUniqueConstraintViolation })
        {
            throw new DuplicateRecordException("A record with the same unique key already exists.");
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var result = await action(ct);
            await SaveChangesAsync(ct);
            await tx.CommitAsync(ct); // disposing without commit rolls back on any exception
            return result;
        });
    }

    public void ClearTracking() => db.ChangeTracker.Clear();
}
