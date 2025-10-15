namespace NextPapyros.Application.Reports;

public interface IReporteRepository
{
    Task<IEnumerable<TopProductoRow>> GetTopProductosAsync(DateTime? desde, DateTime? hasta, int top, CancellationToken ct = default);
    Task<IEnumerable<StockBajoRow>>   GetStockBajoAsync(CancellationToken ct = default);
    Task<IEnumerable<IngresoMensualRow>> GetIngresosMensualesAsync(int anio, CancellationToken ct = default);
}