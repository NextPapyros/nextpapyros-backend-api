using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextPapyros.API.Contracts.Ventas;
using NextPapyros.Application.Reports;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Entities.Enums;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("ventas")]
public class VentasController(
    IVentaRepository ventas,
    NextPapyrosDbContext db,
    IUnitOfWork unitOfWork,
    IComprobanteService comprobanteService
) : ControllerBase
{
    /// <summary>
    /// Registra una nueva venta y reduce el stock de los productos.
    /// </summary>
    /// <param name="req">Datos de la venta (líneas, método de pago).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La venta registrada con su ID y total calculado.</returns>
    /// <response code="200">Venta registrada exitosamente.</response>
    /// <response code="400">Datos inválidos (sin líneas, stock insuficiente, cantidades ≤ 0, productos inactivos).</response>
    /// <response code="401">No autenticado.</response>
    /// <remarks>
    /// Ejemplo de request:
    /// 
    ///     POST /ventas
    ///     {
    ///        "metodoPago": "Efectivo",
    ///        "lineas": [
    ///          {
    ///            "productoCodigo": "PROD001",
    ///            "cantidad": 5,
    ///            "precioUnitario": 1.00
    ///          },
    ///          {
    ///            "productoCodigo": "PROD002",
    ///            "cantidad": 3,
    ///            "precioUnitario": 2.50
    ///          }
    ///        ]
    ///     }
    ///     
    /// **Métodos de pago válidos:** Efectivo, Tarjeta, Transferencia, etc.
    /// 
    /// **Importante:**
    /// - Valida que haya stock suficiente antes de procesar.
    /// - Reduce automáticamente el stock de los productos.
    /// - Crea un movimiento de inventario por cada línea.
    /// - Calcula el total automáticamente (suma de subtotales).
    /// </remarks>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(VentaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<VentaResponse>> Registrar([FromBody] RegistrarVentaRequest req, CancellationToken ct)
    {
        if (req.Lineas is null || !req.Lineas.Any())
            return BadRequest("La venta debe tener al menos una línea.");

        var codigos = req.Lineas.Select(l => l.ProductoCodigo).Distinct().ToList();
        var productosMap = await db.Productos
            .Where(p => codigos.Contains(p.Codigo) && p.Activo)
            .ToDictionaryAsync(p => p.Codigo, ct);

        foreach (var l in req.Lineas)
        {
            if (!productosMap.TryGetValue(l.ProductoCodigo, out var p))
                return BadRequest($"Producto {l.ProductoCodigo} no existe o está inactivo.");

            if (l.Cantidad <= 0) return BadRequest("Cantidad debe ser > 0.");

            if (p.Stock < l.Cantidad)
                return BadRequest($"Stock insuficiente para {p.Codigo} ({p.Nombre}). Disponible: {p.Stock}.");
        }

        try
        {
            await unitOfWork.BeginAsync(ct);

            var venta = new Venta
            {
                Fecha = DateTime.UtcNow,
                MetodoPago = req.MetodoPago,
                Estado = "CONFIRMADA",
                Total = 0,
                Lineas = new List<LineaVenta>()
            };

            foreach (var l in req.Lineas)
            {
                var p = productosMap[l.ProductoCodigo];

                var linea = new LineaVenta
                {
                    ProductoCodigo = p.Codigo,
                    Producto = p,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario,
                    Subtotal = l.Cantidad * l.PrecioUnitario
                };
                venta.Lineas.Add(linea);

                p.Stock -= l.Cantidad;
                db.MovimientosInventario.Add(new MovimientoInventario
                {
                    Fecha = DateTime.UtcNow,
                    Tipo = TipoMov.SALIDA,
                    Cantidad = l.Cantidad,
                    Motivo = $"VENTA #{venta.Id}",
                    ProductoCodigo = p.Codigo
                });

                db.Productos.Update(p);
            }

            venta.Total = venta.Lineas.Sum(x => x.Subtotal);

            await ventas.AddAsync(venta, ct);
            await unitOfWork.CommitAsync(ct);

            var res = new VentaResponse(
                venta.Id, venta.Fecha, venta.Total, venta.Estado, venta.MetodoPago,
                venta.Lineas.Select(li => new VentaLineaItem(
                    li.Id, li.ProductoCodigo, productosMap[li.ProductoCodigo].Nombre, li.Cantidad, li.PrecioUnitario, li.Subtotal
                ))
            );

            return CreatedAtAction(nameof(Obtener), new { id = venta.Id }, res);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<VentaResponse>> Obtener([FromRoute] int id, CancellationToken ct)
    {
        var v = await ventas.GetByIdAsync(id, ct);
        if (v is null) return NotFound();

        var codigos = v.Lineas.Select(l => l.ProductoCodigo).Distinct().ToList();
        var map = await db.Productos.Where(p => codigos.Contains(p.Codigo))
            .ToDictionaryAsync(p => p.Codigo, ct);

        var res = new VentaResponse(
            v.Id, v.Fecha, v.Total, v.Estado, v.MetodoPago,
            v.Lineas.Select(li => new VentaLineaItem(
                li.Id, li.ProductoCodigo, map[li.ProductoCodigo].Nombre, li.Cantidad, li.PrecioUnitario, li.Subtotal
            ))
        );
        return Ok(res);
    }

    /// <summary>
    /// Busca productos activos para agregar a una venta en el punto de venta.
    /// </summary>
    /// <param name="q">Término de búsqueda (código o nombre del producto).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de productos activos con stock, precio y disponibilidad.</returns>
    /// <response code="200">Lista de productos encontrados (puede estar vacía).</response>
    /// <response code="401">No autenticado.</response>
    /// <remarks>
    /// 
    /// Ejemplos de request:
    /// 
    ///     GET /ventas/pos/buscar?q=lapicero
    ///     GET /ventas/pos/buscar?q=PROD001
    ///     
    /// **Reglas de negocio aplicadas:**
    /// - Solo productos activos son retornados
    /// - Incluye stock disponible para validación en el frontend
    /// - Precio cargado automáticamente desde el catálogo
    /// - Búsqueda insensible a mayúsculas/minúsculas
    /// - Búsqueda por código exacto o nombre parcial
    /// </remarks>
    [HttpGet("pos/buscar")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ProductoPosResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ProductoPosResponse>>> BuscarProductosPos(
        [FromQuery] string q,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(Array.Empty<ProductoPosResponse>());

        var productos = await db.Productos
            .AsNoTracking()
            .Where(p => p.Activo && (p.Codigo.Contains(q) || p.Nombre.Contains(q)))
            .OrderBy(p => p.Nombre)
            .Take(20) // Limitar resultados para performance
            .Select(p => new ProductoPosResponse(
                p.Codigo,
                p.Nombre,
                p.Categoria,
                p.Precio,
                p.Stock
            ))
            .ToListAsync(ct);

        return Ok(productos);
    }

    /// <summary>
    /// Valida que un producto puede ser agregado a la venta.
    /// Verifica stock, estado activo y retorna precio del catálogo.
    /// </summary>
    /// <param name="req">Código del producto y cantidad solicitada.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Información del producto validado con subtotal calculado.</returns>
    /// <response code="200">Producto validado correctamente, puede agregarse a la venta.</response>
    /// <response code="400">Producto inactivo, sin stock suficiente, o no existe.</response>
    /// <response code="401">No autenticado.</response>
    /// <remarks>
    /// - Valida stock suficiente antes de agregar
    /// - Muestra alerta si stock insuficiente
    /// - No permite agregar productos inactivos
    /// 
    /// Ejemplo de request:
    /// 
    ///     POST /ventas/pos/validar-producto
    ///     {
    ///        "productoCodigo": "PROD001",
    ///        "cantidad": 5
    ///     }
    ///     
    /// **Reglas de negocio:**
    /// - Solo productos activos pueden agregarse
    /// - Cantidad debe ser &gt; 0 y &lt;= stock disponible
    /// - Precio cargado automáticamente (no modificable por empleado)
    /// - Stock no se modifica hasta confirmar la venta
    /// 
    /// **Respuestas de error:**
    /// - "Producto no encontrado o inactivo"
    /// - "Stock insuficiente. Disponible: X"
    /// - "Cantidad debe ser mayor a 0"
    /// </remarks>
    [HttpPost("pos/validar-producto")]
    [Authorize]
    [ProducesResponseType(typeof(ProductoAgregadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductoAgregadoResponse>> ValidarProductoParaVenta(
        [FromBody] AgregarProductoRequest req,
        CancellationToken ct)
    {
        if (req.Cantidad <= 0)
            return BadRequest("Cantidad debe ser mayor a 0.");

        var producto = await db.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Codigo == req.ProductoCodigo && p.Activo, ct);

        if (producto is null)
            return BadRequest("Producto no encontrado o inactivo.");

        if (producto.Stock < req.Cantidad)
            return BadRequest($"Stock insuficiente. Disponible: {producto.Stock} unidades.");

        var subtotal = req.Cantidad * producto.Precio;
        var stockRestante = producto.Stock - req.Cantidad;

        var response = new ProductoAgregadoResponse(
            producto.Codigo,
            producto.Nombre,
            req.Cantidad,
            producto.Precio,
            subtotal,
            stockRestante
        );

        return Ok(response);
    }

    /// <summary>
    /// Genera el comprobante PDF de una venta.
    /// </summary>
    /// <param name="id">ID de la venta.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El archivo PDF del comprobante.</returns>
    /// <response code="200">Comprobante generado exitosamente.</response>
    /// <response code="404">Venta no encontrada.</response>
    /// <response code="401">No autenticado.</response>
    /// <remarks>
    /// Ejemplo de uso:
    /// 
    ///     GET /ventas/5/comprobante
    ///     
    /// Genera un comprobante en formato PDF con la siguiente información:
    /// - Encabezado con datos de la empresa (NextPapyros)
    /// - Número de comprobante, fecha y estado de la venta
    /// - Método de pago utilizado
    /// - Detalle de productos: cantidad, nombre, código, precio unitario y subtotal
    /// - Totales: subtotal general y total de la venta
    /// 
    /// El PDF se descarga automáticamente con el nombre: `comprobante-{id}.pdf`
    /// 
    /// **Importante:**
    /// - Solo se pueden generar comprobantes de ventas existentes.
    /// - El comprobante incluye todos los productos de la venta con sus respectivos detalles.
    /// 
    /// **Respuestas de error:**
    /// - "Venta con ID X no encontrada"
    /// </remarks>
    [HttpGet("{id}/comprobante")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerarComprobante(int id, CancellationToken ct)
    {
        var venta = await db.Ventas
            .Include(v => v.Lineas)
            .ThenInclude(l => l.Producto)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (venta is null)
            return NotFound($"Venta con ID {id} no encontrada.");

        var pdfBytes = comprobanteService.GenerarComprobantePdf(venta);

        return File(pdfBytes, "application/pdf", $"comprobante-{id:D8}.pdf");
    }
}