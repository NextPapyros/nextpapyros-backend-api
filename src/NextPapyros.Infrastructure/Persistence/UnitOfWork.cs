using Microsoft.EntityFrameworkCore.Storage;
using NextPapyros.Domain.Repositories;

namespace NextPapyros.Infrastructure.Persistence;

public class UnitOfWork(NextPapyrosDbContext db) : IUnitOfWork
{
    private readonly NextPapyrosDbContext _db = db;
    private IDbContextTransaction? _tx;

    public async Task BeginAsync(CancellationToken ct = default)
        => _tx = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
        if (_tx is not null) await _tx.CommitAsync(ct);
    }

    public async Task RollbackAsync(CancellationToken ct = default)
        => await (_tx?.RollbackAsync(ct) ?? Task.CompletedTask);
}