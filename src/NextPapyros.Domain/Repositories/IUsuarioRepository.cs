using NextPapyros.Domain.Entities;

namespace NextPapyros.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task AddAsync(Usuario u, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}