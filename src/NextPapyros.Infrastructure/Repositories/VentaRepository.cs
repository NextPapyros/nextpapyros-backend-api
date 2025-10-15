using Microsoft.EntityFrameworkCore;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.Infrastructure.Repositories;

public class VentaRepository(NextPapyrosDbContext db) : IVentaRepository
{
    private readonly NextPapyrosDbContext _db = db;

    public Task<Venta?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Ventas
           .Include(v => v.Lineas)
           .ThenInclude(l => l.Producto)
           .FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task AddAsync(Venta v, CancellationToken ct = default) =>
        await _db.Ventas.AddAsync(v, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}