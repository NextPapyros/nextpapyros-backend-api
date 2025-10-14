using Microsoft.EntityFrameworkCore;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly NextPapyrosDbContext _db;
    public RoleRepository(NextPapyrosDbContext db) => _db = db;

    public Task<Rol?> GetByNameAsync(string nombre, CancellationToken ct = default) =>
        _db.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre && r.Activo, ct);

    public Task<List<Rol>> GetAllAsync(CancellationToken ct = default) =>
        _db.Roles.Where(r => r.Activo).ToListAsync(ct);
}