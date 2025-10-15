using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextPapyros.API.Contracts.Ventas;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Entities.Enums;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("ventas")]
public class VentasController(
    IVentaRepository ventas,
    NextPapyrosDbContext db
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
        await ventas.SaveChangesAsync(ct);

        var res = new VentaResponse(
            venta.Id, venta.Fecha, venta.Total, venta.Estado, venta.MetodoPago,
            venta.Lineas.Select(li => new VentaLineaItem(
                li.Id, li.ProductoCodigo, productosMap[li.ProductoCodigo].Nombre, li.Cantidad, li.PrecioUnitario, li.Subtotal
            ))
        );

        return CreatedAtAction(nameof(Obtener), new { id = venta.Id }, res);
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
}