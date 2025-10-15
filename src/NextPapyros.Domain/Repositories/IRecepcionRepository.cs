using NextPapyros.Domain.Entities;

namespace NextPapyros.Domain.Repositories;

public interface IRecepcionRepository
{
    Task<Recepcion?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Recepcion r, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}