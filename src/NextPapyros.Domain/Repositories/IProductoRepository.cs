using NextPapyros.Domain.Entities;

namespace NextPapyros.Domain.Repositories;

public interface IProductoRepository
{
    Task<Producto?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task AddAsync(Producto p, CancellationToken ct = default);
    Task UpdateAsync(Producto p, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}