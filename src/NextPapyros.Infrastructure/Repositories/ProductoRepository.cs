using Microsoft.EntityFrameworkCore;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.Infrastructure.Repositories;

public class ProductoRepository(NextPapyrosDbContext db) : IProductoRepository
{
    private readonly NextPapyrosDbContext _db = db;

    public Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken ct = default) =>
        _db.Productos.FirstOrDefaultAsync(p => p.Codigo == codigo, ct);

    public async Task AddAsync(Producto p, CancellationToken ct = default)
    {
        await _db.Productos.AddAsync(p, ct);
    }

    public Task UpdateAsync(Producto p, CancellationToken ct = default)
    {
        _db.Productos.Update(p);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}