using Microsoft.EntityFrameworkCore;
using NextPapyros.Application.Reports;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.Infrastructure.Reports;

public class ReporteRepository(NextPapyrosDbContext db) : IReporteRepository
{
    private readonly NextPapyrosDbContext _db = db;

    public async Task<IEnumerable<TopProductoRow>> GetTopProductosAsync(DateTime? desde, DateTime? hasta, int top, CancellationToken ct = default)
    {
        var q = _db.LineasVenta
            .Include(l => l.Producto)
            .Include(l => l.Venta)
            .AsNoTracking()
            .Where(l => l.Venta.Estado == "CONFIRMADA");

        if (desde.HasValue) q = q.Where(l => l.Venta.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(l => l.Venta.Fecha <= hasta.Value);

        var data = await q
            .GroupBy(l => new { l.ProductoCodigo, l.Producto.Nombre })
            .Select(g => new TopProductoRow(
                g.Key.ProductoCodigo,
                g.Key.Nombre,
                g.Sum(x => x.Cantidad),
                g.Sum(x => x.Subtotal)
            ))
            .OrderByDescending(r => r.CantidadVendida)
            .Take(top)
            .ToListAsync(ct);

        return data;
    }

    public async Task<IEnumerable<StockBajoRow>> GetStockBajoAsync(CancellationToken ct = default)
    {
        return await _db.Productos
            .AsNoTracking()
            .Where(p => p.Activo && p.Stock <= p.StockMinimo)
            .OrderBy(p => p.Codigo)
            .Select(p => new StockBajoRow(p.Codigo, p.Nombre, p.Stock, p.StockMinimo))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<IngresoMensualRow>> GetIngresosMensualesAsync(int anio, CancellationToken ct = default)
    {
        var data = await _db.Ventas
            .AsNoTracking()
            .Where(v => v.Estado == "CONFIRMADA" && v.Fecha.Year == anio)
            .GroupBy(v => v.Fecha.Month)
            .Select(g => new IngresoMensualRow(g.Key, g.Sum(v => v.Total)))
            .OrderBy(r => r.Mes)
            .ToListAsync(ct);

        return data;
    }
}