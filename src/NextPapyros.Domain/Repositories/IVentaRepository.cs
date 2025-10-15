using NextPapyros.Domain.Entities;

namespace NextPapyros.Domain.Repositories;

public interface IVentaRepository
{
    Task<Venta?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Venta v, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}