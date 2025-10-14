using Microsoft.EntityFrameworkCore;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.Infrastructure.Repositories;

public class UsuarioRepository(NextPapyrosDbContext db) : IUsuarioRepository
{
    private readonly NextPapyrosDbContext _db = db;

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Usuarios
           .Include(u => u.Roles).ThenInclude(ur => ur.Rol)
           .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        _db.Usuarios.AnyAsync(u => u.Email == email, ct);

    public async Task AddAsync(Usuario u, CancellationToken ct = default)
        => await _db.Usuarios.AddAsync(u, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}