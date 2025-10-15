using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("reportes")]
[Authorize(Roles = "Admin")]
public class ReportesController(NextPapyrosDbContext db) : ControllerBase
{
    /// <summary>
    /// Obtiene los productos más vendidos en un rango de fechas.
    /// </summary>
    /// <param name="desde">Fecha inicial del rango (opcional).</param>
    /// <param name="hasta">Fecha final del rango (opcional).</param>
    /// <param name="top">Cantidad de productos a retornar (default: 5).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de los productos más vendidos ordenados por cantidad.</returns>
    /// <response code="200">Lista de productos más vendidos.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplos de request:
    /// 
    ///     GET /reportes/top-productos
    ///     GET /reportes/top-productos?top=10
    ///     GET /reportes/top-productos?desde=2025-01-01&amp;hasta=2025-01-31
    ///     GET /reportes/top-productos?desde=2025-01-01&amp;hasta=2025-01-31&amp;top=20
    ///     
    /// Si no se especifican fechas, considera todas las ventas registradas.
    /// </remarks>
    [HttpGet("top-productos")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<object>>> TopProductos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int top = 5,
        CancellationToken ct = default)
    {
        var q = db.LineasVenta
            .Include(l => l.Venta)
            .AsNoTracking()
            .AsQueryable();

        if (desde.HasValue) q = q.Where(l => l.Venta.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(l => l.Venta.Fecha <= hasta.Value);

        var data = await q
            .GroupBy(l => l.ProductoCodigo)
            .Select(g => new { ProductoCodigo = g.Key, Cantidad = g.Sum(x => x.Cantidad), Ingresos = g.Sum(x => x.Subtotal) })
            .OrderByDescending(x => x.Cantidad)
            .Take(top)
            .ToListAsync(ct);

        var codigos = data.Select(d => d.ProductoCodigo).ToList();
        var map = await db.Productos.Where(p => codigos.Contains(p.Codigo))
            .ToDictionaryAsync(p => p.Codigo, ct);

        var res = data.Select(d => new {
            Codigo = d.ProductoCodigo,
            Nombre = map.TryGetValue(d.ProductoCodigo, out var p) ? p.Nombre : d.ProductoCodigo,
            d.Cantidad,
            d.Ingresos
        });

        return Ok(res);
    }

    /// <summary>
    /// Lista productos con stock bajo (stock ≤ stock mínimo).
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de productos que requieren reabastecimiento.</returns>
    /// <response code="200">Lista de productos con stock bajo.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     GET /reportes/stock-bajo
    ///     
    /// Retorna solo productos activos donde stock ≤ stockMinimo,
    /// ordenados por criticidad (stock - stockMinimo ascendente).
    /// </remarks>
    [HttpGet("stock-bajo")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<object>>> StockBajo(CancellationToken ct)
    {
        var data = await db.Productos.AsNoTracking()
            .Where(p => p.Activo && p.Stock <= p.StockMinimo)
            .Select(p => new { p.Codigo, p.Nombre, p.Stock, p.StockMinimo })
            .OrderBy(p => p.Stock - p.StockMinimo)
            .ToListAsync(ct);

        return Ok(data);
    }

    /// <summary>
    /// Obtiene los ingresos totales agrupados por mes para un año específico.
    /// </summary>
    /// <param name="anio">Año a consultar (default: año actual).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de ingresos mensuales del año especificado.</returns>
    /// <response code="200">Lista de ingresos por mes.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplos de request:
    /// 
    ///     GET /reportes/ingresos-mensuales
    ///     GET /reportes/ingresos-mensuales?anio=2024
    ///     GET /reportes/ingresos-mensuales?anio=2025
    ///     
    /// Retorna un objeto por mes con el total de ingresos.
    /// Solo considera ventas en estado CONFIRMADA.
    /// </remarks>
    [HttpGet("ingresos-mensuales")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<object>>> IngresosMensuales([FromQuery] int anio, CancellationToken ct)
    {
        if (anio <= 0) anio = DateTime.UtcNow.Year;

        var data = await db.Ventas.AsNoTracking()
            .Where(v => v.Fecha.Year == anio && v.Estado == "CONFIRMADA")
            .GroupBy(v => v.Fecha.Month)
            .Select(g => new { Mes = g.Key, Ingresos = g.Sum(x => x.Total) })
            .OrderBy(x => x.Mes)
            .ToListAsync(ct);

        return Ok(data);
    }
}