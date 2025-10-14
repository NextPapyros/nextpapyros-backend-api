using NextPapyros.Domain.Entities;

namespace NextPapyros.Domain.Repositories;

public interface IRoleRepository
{
    Task<Rol?> GetByNameAsync(string nombre, CancellationToken ct = default);
    Task<List<Rol>> GetAllAsync(CancellationToken ct = default);
}