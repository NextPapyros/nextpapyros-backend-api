using Microsoft.EntityFrameworkCore;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.Infrastructure.Repositories;

public class ProveedorRepository(NextPapyrosDbContext db) : IProveedorRepository
{
    private readonly NextPapyrosDbContext _db = db;

    public Task<Proveedor?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Proveedores.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Proveedor?> GetByNitAsync(string nit, CancellationToken ct = default) =>
        _db.Proveedores.FirstOrDefaultAsync(p => p.Nit == nit, ct);

    public Task<Proveedor?> GetByNombreAsync(string nombre, CancellationToken ct = default) =>
        _db.Proveedores.FirstOrDefaultAsync(p => p.Nombre == nombre, ct);

    public async Task<IEnumerable<Proveedor>> GetAllActivosAsync(CancellationToken ct = default) =>
        await _db.Proveedores.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync(ct);

    public async Task AddAsync(Proveedor proveedor, CancellationToken ct = default) =>
        await _db.Proveedores.AddAsync(proveedor, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
