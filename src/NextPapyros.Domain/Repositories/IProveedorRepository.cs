using NextPapyros.Domain.Entities;

namespace NextPapyros.Domain.Repositories;

public interface IProveedorRepository
{
    Task<Proveedor?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Proveedor?> GetByNitAsync(string nit, CancellationToken ct = default);
    Task<Proveedor?> GetByNombreAsync(string nombre, CancellationToken ct = default);
    Task<IEnumerable<Proveedor>> GetAllActivosAsync(CancellationToken ct = default);
    Task AddAsync(Proveedor proveedor, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
