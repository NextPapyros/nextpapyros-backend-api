using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextPapyros.Application.Reports;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("reportes")]
[Authorize(Roles = "Admin")]
public class ReportesController(IReporteRepository repo, IEnumerable<IReportExporter> exporters) : ControllerBase
{
    private readonly IReporteRepository _repo = repo;
    private readonly IEnumerable<IReportExporter> _exporters = exporters;

    /// <summary>
    /// Obtiene los productos más vendidos en un rango de fechas.
    /// </summary>
    /// <param name="desde">Fecha inicial del rango (opcional).</param>
    /// <param name="hasta">Fecha final del rango (opcional).</param>
    /// <param name="top">Cantidad de productos a retornar (default: 5).</param>
    /// <param name="format">
    /// Formato opcional para exportar el reporte: <c>csv</c> o <c>pdf</c>.
    /// Si se omite, la respuesta es JSON.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de los productos más vendidos ordenados por cantidad o un archivo exportado.</returns>
    /// <response code="200">Lista de productos más vendidos o archivo exportado.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    ///
    /// Ejemplos:
    /// - `GET /reportes/top-productos`
    /// - `GET /reportes/top-productos?top=10`
    /// - `GET /reportes/top-productos?desde=2025-01-01&amp;hasta=2025-01-31`
    /// - `GET /reportes/top-productos?format=csv`
    /// </remarks>
    [HttpGet("top-productos")]
    [Produces("application/json", "text/csv", "application/pdf")]
    [ProducesResponseType(typeof(IEnumerable<TopProductoRow>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TopProductos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int top = 5,
        [FromQuery] string? format = null,
        CancellationToken ct = default)
    {
        var data = await _repo.GetTopProductosAsync(desde, hasta, top, ct);

        if (string.IsNullOrWhiteSpace(format))
            return Ok(data);

        var exp = _exporters.FirstOrDefault(e => e.Format.Equals(format, StringComparison.OrdinalIgnoreCase));
        if (exp is null) return BadRequest($"Formato no soportado: {format}");

        var (bytes, contentType, fileName) = exp.Export(data, $"top-productos_{DateTime.UtcNow:yyyyMMddHHmmss}");
        return File(bytes, contentType, fileName);
    }

    /// <summary>
    /// Lista productos con stock bajo (stock ≤ stock mínimo).
    /// </summary>
    /// <param name="format">
    /// Formato opcional para exportar el reporte: <c>csv</c> o <c>pdf</c>.
    /// Si se omite, la respuesta es JSON.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de productos que requieren reabastecimiento o un archivo exportado.</returns>
    /// <response code="200">Lista de productos con stock bajo o archivo exportado.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    ///
    /// Ejemplo:
    /// - `GET /reportes/stock-bajo`
    /// - `GET /reportes/stock-bajo?format=csv`
    /// </remarks>
    [HttpGet("stock-bajo")]
    [Produces("application/json", "text/csv", "application/pdf")]
    [ProducesResponseType(typeof(IEnumerable<StockBajoRow>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StockBajo([FromQuery] string? format = null, CancellationToken ct = default)
    {
        var data = await _repo.GetStockBajoAsync(ct);

        if (string.IsNullOrWhiteSpace(format))
            return Ok(data);

        var exp = _exporters.FirstOrDefault(e => e.Format.Equals(format, StringComparison.OrdinalIgnoreCase));
        if (exp is null) return BadRequest($"Formato no soportado: {format}");

        var (bytes, contentType, fileName) = exp.Export(data, $"stock-bajo_{DateTime.UtcNow:yyyyMMddHHmmss}");
        return File(bytes, contentType, fileName);
    }

    /// <summary>
    /// Obtiene los ingresos totales agrupados por mes para un año específico.
    /// </summary>
    /// <param name="anio">Año a consultar (default: año actual).</param>
    /// <param name="format">
    /// Formato opcional para exportar el reporte: <c>csv</c> o <c>pdf</c>.
    /// Si se omite, la respuesta es JSON.
    /// </param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de ingresos mensuales o un archivo exportado.</returns>
    /// <response code="200">Lista de ingresos por mes o archivo exportado.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    ///
    /// Ejemplos:
    /// - `GET /reportes/ingresos-mensuales?anio=2025`
    /// - `GET /reportes/ingresos-mensuales?anio=2025&amp;format=csv`
    /// </remarks>
    [HttpGet("ingresos-mensuales")]
    [Produces("application/json", "text/csv", "application/pdf")]
    [ProducesResponseType(typeof(IEnumerable<IngresoMensualRow>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> IngresosMensuales([FromQuery] int anio, [FromQuery] string? format = null, CancellationToken ct = default)
    {
        if (anio <= 0) anio = DateTime.UtcNow.Year;

        var data = await _repo.GetIngresosMensualesAsync(anio, ct);

        if (string.IsNullOrWhiteSpace(format))
            return Ok(data);

        var exp = _exporters.FirstOrDefault(e => e.Format.Equals(format, StringComparison.OrdinalIgnoreCase));
        if (exp is null) return BadRequest($"Formato no soportado: {format}");

        var (bytes, contentType, fileName) = exp.Export(data, $"ingresos-{anio}_{DateTime.UtcNow:yyyyMMddHHmmss}");
        return File(bytes, contentType, fileName);
    }
}