using Microsoft.EntityFrameworkCore;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.Infrastructure.Repositories;

public class RecepcionRepository(NextPapyrosDbContext db) : IRecepcionRepository
{
    private readonly NextPapyrosDbContext _db = db;

    public Task<Recepcion?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Recepciones
           .Include(r => r.Lineas)
           .ThenInclude(l => l.Producto)
           .Include(r => r.OrdenCompra)
           .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(Recepcion r, CancellationToken ct = default) =>
        await _db.Recepciones.AddAsync(r, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}