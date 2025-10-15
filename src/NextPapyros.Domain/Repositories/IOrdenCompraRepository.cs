using NextPapyros.Domain.Entities;

namespace NextPapyros.Domain.Repositories;

public interface IOrdenCompraRepository
{
    Task<OrdenCompra?> GetByIdAsync(int id, CancellationToken ct = default);
}