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
    NextPapyrosDbContext db,
    IUnitOfWork unitOfWork
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductoResponse>> Crear([FromBody] CrearProductoRequest req, CancellationToken ct)
    {
        // Validaciones de reglas de negocio
        if (req.Costo < 0)
            return BadRequest("El costo debe ser un valor positivo.");
        
        if (req.Precio < 0)
            return BadRequest("El precio debe ser un valor positivo.");
        
        if (req.Precio < req.Costo)
            return BadRequest("El precio de venta debe ser mayor o igual al costo.");

        var existente = await productos.GetByCodigoAsync(req.Codigo, ct);
        if (existente is not null) 
            return Conflict("Ya existe un producto con ese código.");

        try
        {
            await unitOfWork.BeginAsync(ct);

            var p = new Producto
            {
                Codigo = req.Codigo.Trim(),
                Nombre = req.Nombre.Trim(),
                Categoria = req.Categoria.Trim(),
                Costo = req.Costo,
                Precio = req.Precio,
                Stock = 0,
                StockMinimo = req.StockMinimo,
                Activo = true,
                FechaIngreso = DateTime.UtcNow
            };

            await productos.AddAsync(p, ct);
            await unitOfWork.CommitAsync(ct);

            return CreatedAtAction(nameof(Obtener), new { codigo = p.Codigo },
                new ProductoResponse(p.Codigo, p.Nombre, p.Categoria, p.Costo, p.Precio, p.Stock, p.StockMinimo, p.Activo));
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Actualiza los datos de un producto existente.
    /// </summary>
    /// <param name="codigo">Código único del producto.</param>
    /// <param name="req">Nuevos datos del producto.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="204">Producto actualizado exitosamente.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="404">Producto no encontrado.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// No se puede actualizar el código ni el estado (activo/inactivo).
    /// </remarks>
    [HttpPut("{codigo}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Actualizar([FromRoute] string codigo, [FromBody] ActualizarProductoRequest req, CancellationToken ct)
    {
        if (req.Costo < 0)
            return BadRequest("El costo debe ser un valor positivo.");
        
        if (req.Precio < 0)
            return BadRequest("El precio debe ser un valor positivo.");
        
        if (req.Precio < req.Costo)
            return BadRequest("El precio de venta debe ser mayor o igual al costo.");

        var p = await productos.GetByCodigoAsync(codigo, ct);
        if (p is null) return NotFound();

        p.Nombre = req.Nombre.Trim();
        p.Categoria = req.Categoria.Trim();
        p.Costo = req.Costo;
        p.Precio = req.Precio;
        p.StockMinimo = req.StockMinimo;

        await productos.UpdateAsync(p, ct);
        await productos.SaveChangesAsync(ct);

        return NoContent();
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
    /// <param name="includeInactive">Incluir productos desactivados (solo Admin, default: false).</param>
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
    public async Task<ActionResult<IEnumerable<ProductoResponse>>> Listar([FromQuery] string? q, [FromQuery] bool lowStock = false, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
    {
        var query = db.Productos.AsNoTracking().AsQueryable();

        if (!includeInactive || !User.IsInRole("Admin"))
        {
            query = query.Where(p => p.Activo);
        }

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

    /// <summary>
    /// Desactiva un producto (Soft Delete).
    /// </summary>
    /// <param name="codigo">Código del producto a desactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="204">Producto desactivado exitosamente.</response>
    /// <response code="404">Producto no encontrado.</response>
    [HttpDelete("{codigo}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Desactivar([FromRoute] string codigo, CancellationToken ct)
    {
        var p = await productos.GetByCodigoAsync(codigo, ct);
        if (p is null) return NotFound();

        p.Activo = false;
        await productos.UpdateAsync(p, ct);
        await productos.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>
    /// Reactiva un producto previamente desactivado.
    /// </summary>
    /// <param name="codigo">Código del producto a reactivar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <response code="204">Producto reactivado exitosamente.</response>
    /// <response code="404">Producto no encontrado.</response>
    [HttpPost("{codigo}/reactivate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Reactivar([FromRoute] string codigo, CancellationToken ct)
    {
        var p = await productos.GetByCodigoAsync(codigo, ct);
        if (p is null) return NotFound();

        p.Activo = true;
        await productos.UpdateAsync(p, ct);
        await productos.SaveChangesAsync(ct);

        return NoContent();
    }
}