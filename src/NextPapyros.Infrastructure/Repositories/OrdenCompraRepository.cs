using Microsoft.EntityFrameworkCore;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.Infrastructure.Repositories;

public class OrdenCompraRepository(NextPapyrosDbContext db) : IOrdenCompraRepository
{
    private readonly NextPapyrosDbContext _db = db;

    public Task<OrdenCompra?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.OrdenesCompra
           .Include(o => o.Proveedor)
           .Include(o => o.Lineas)
           .ThenInclude(l => l.Producto)
           .FirstOrDefaultAsync(o => o.Id == id, ct);
}