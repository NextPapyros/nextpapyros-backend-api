using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextPapyros.API.Contracts.Productos;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Entities.Enums;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("products")]
public class ProductosController(
    IProductoRepository productos,
    NextPapyrosDbContext db
) : ControllerBase
{
    /// <summary>
    /// Crea un nuevo producto en el inventario.
    /// </summary>
    /// <param name="req">Datos del producto (código, nombre, categoría, precios, etc.).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El producto creado con todos sus detalles.</returns>
    /// <response code="201">Producto creado exitosamente.</response>
    /// <response code="409">Ya existe un producto con ese código.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     POST /products
    ///     {
    ///        "codigo": "PROD001",
    ///        "nombre": "Lapicero Azul",
    ///        "categoria": "Papelería",
    ///        "costo": 0.50,
    ///        "precio": 1.00,
    ///        "stockMinimo": 10
    ///     }
    ///     
    /// El stock inicial siempre es 0. Se incrementa mediante recepciones de órdenes de compra.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductoResponse>> Crear([FromBody] CrearProductoRequest req, CancellationToken ct)
    {
        var existente = await productos.GetByCodigoAsync(req.Codigo, ct);
        if (existente is not null) return Conflict("Ya existe un producto con ese código.");

        var p = new Producto
        {
            Codigo = req.Codigo,
            Nombre = req.Nombre,
            Categoria = req.Categoria,
            Costo = req.Costo,
            Precio = req.Precio,
            Stock = 0,
            StockMinimo = req.StockMinimo,
            Activo = true,
            FechaIngreso = DateTime.UtcNow
        };

        await productos.AddAsync(p, ct);
        await productos.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Obtener), new { codigo = p.Codigo },
            new ProductoResponse(p.Codigo, p.Nombre, p.Categoria, p.Costo, p.Precio, p.Stock, p.StockMinimo, p.Activo));
    }

    /// <summary>
    /// Obtiene un producto por su código único.
    /// </summary>
    /// <param name="codigo">Código único del producto (ej: PROD001).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Los detalles completos del producto.</returns>
    /// <response code="200">Producto encontrado.</response>
    /// <response code="404">No existe un producto con ese código.</response>
    /// <response code="401">No autenticado.</response>
    /// <remarks>
    /// Ejemplo de request:
    /// 
    ///     GET /products/PROD001
    /// </remarks>
    [HttpGet("{codigo}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductoResponse>> Obtener([FromRoute] string codigo, CancellationToken ct)
    {
        var p = await productos.GetByCodigoAsync(codigo, ct);
        if (p is null) return NotFound();

        return new ProductoResponse(p.Codigo, p.Nombre, p.Categoria, p.Costo, p.Precio, p.Stock, p.StockMinimo, p.Activo);
    }

    /// <summary>
    /// Lista todos los productos activos con filtros opcionales.
    /// </summary>
    /// <param name="q">Búsqueda por código o nombre (opcional).</param>
    /// <param name="lowStock">Filtrar solo productos con stock bajo (opcional, default: false).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de productos que cumplen los criterios.</returns>
    /// <response code="200">Lista de productos (puede estar vacía).</response>
    /// <response code="401">No autenticado.</response>
    /// <remarks>
    /// Ejemplos de request:
    /// 
    ///     GET /products
    ///     GET /products?q=lapicero
    ///     GET /products?lowStock=true
    ///     GET /products?q=papel&amp;lowStock=true
    ///     
    /// **lowStock=true** retorna productos donde stock ≤ stockMinimo.
    /// </remarks>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ProductoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ProductoResponse>>> Listar([FromQuery] string? q, [FromQuery] bool lowStock = false, CancellationToken ct = default)
    {
        var query = db.Productos.AsNoTracking().Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Codigo.Contains(q) || p.Nombre.Contains(q) || p.Categoria.Contains(q));

        if (lowStock)
            query = query.Where(p => p.Stock <= p.StockMinimo);

        var data = await query
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoResponse(p.Codigo, p.Nombre, p.Categoria, p.Costo, p.Precio, p.Stock, p.StockMinimo, p.Activo))
            .ToListAsync(ct);

        return Ok(data);
    }

    [HttpPost("{codigo}/ajustestock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AjusteStock([FromRoute] string codigo, [FromBody] AjusteStockRequest req, CancellationToken ct)
    {
        var p = await productos.GetByCodigoAsync(codigo, ct);
        if (p is null) return NotFound("Producto no encontrado.");

        if (req.Cantidad == 0) return BadRequest("La cantidad de ajuste no puede ser 0.");

        var tipo = req.Cantidad > 0 ? TipoMov.AJUSTE : TipoMov.AJUSTE;

        var nuevoStock = p.Stock + req.Cantidad;
        if (nuevoStock < 0) return BadRequest("El ajuste deja el stock en negativo.");

        p.Stock = nuevoStock;

        db.MovimientosInventario.Add(new MovimientoInventario
        {
            Fecha = DateTime.UtcNow,
            Tipo = req.Cantidad > 0 ? TipoMov.ENTRADA : TipoMov.SALIDA,
            Cantidad = Math.Abs(req.Cantidad),
            Motivo = $"AJUSTE: {req.Motivo}",
            ProductoCodigo = p.Codigo
        });

        await productos.UpdateAsync(p, ct);
        await productos.SaveChangesAsync(ct);

        return NoContent();
    }
}