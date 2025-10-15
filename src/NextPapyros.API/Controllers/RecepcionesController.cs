using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextPapyros.API.Contracts.Recepciones;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Entities.Enums;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Persistence;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("recepciones")]
public class RecepcionesController(
    IRecepcionRepository recepciones,
    IOrdenCompraRepository ordenes,
    NextPapyrosDbContext db
) : ControllerBase
{
    /// <summary>
    /// Registra la recepción de mercancía de una orden de compra.
    /// </summary>
    /// <param name="req">Datos de la recepción (orden de compra, factura, líneas recibidas).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>La recepción registrada con todos sus detalles.</returns>
    /// <response code="200">Recepción registrada exitosamente.</response>
    /// <response code="400">Datos inválidos (orden no existe, sin líneas, cantidades ≤ 0, productos inactivos).</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     POST /recepciones
    ///     {
    ///        "ordenCompraId": 5,
    ///        "nroFacturaGuia": "FACT-001-2025",
    ///        "lineas": [
    ///          {
    ///            "productoCodigo": "PROD001",
    ///            "cantidadRecibida": 100,
    ///            "costoUnitario": 0.45
    ///          },
    ///          {
    ///            "productoCodigo": "PROD002",
    ///            "cantidadRecibida": 50,
    ///            "costoUnitario": 2.00
    ///          }
    ///        ]
    ///     }
    ///     
    /// **Importante:**
    /// - Incrementa automáticamente el stock de los productos.
    /// - Crea un movimiento de inventario por cada línea.
    /// - Actualiza el estado de la orden de compra si corresponde.
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RecepcionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RecepcionResponse>> Registrar([FromBody] RegistrarRecepcionRequest req, CancellationToken ct)
    {
        if (req.Lineas is null || !req.Lineas.Any())
            return BadRequest("La recepción debe tener al menos una línea.");

        // Validar OC
        var oc = await ordenes.GetByIdAsync(req.OrdenCompraId, ct);
        if (oc is null) return BadRequest("Orden de compra no existe.");

        // Cargar productos
        var codigos = req.Lineas.Select(l => l.ProductoCodigo).Distinct().ToList();
        var productos = await db.Productos
            .Where(p => codigos.Contains(p.Codigo) && p.Activo)
            .ToDictionaryAsync(p => p.Codigo, ct);

        foreach (var l in req.Lineas)
        {
            if (!productos.ContainsKey(l.ProductoCodigo))
                return BadRequest($"Producto {l.ProductoCodigo} no existe o está inactivo.");
            if (l.CantidadRecibida <= 0) return BadRequest("Cantidad debe ser > 0.");
        }

        var r = new Recepcion
        {
            Fecha = DateTime.UtcNow,
            NroFacturaGuia = req.NroFacturaGuia,
            OrdenCompraId = oc.Id,
            OrdenCompra = oc,
            Lineas = new List<LineaRecepcion>()
        };

        foreach (var l in req.Lineas)
        {
            var p = productos[l.ProductoCodigo];

            r.Lineas.Add(new LineaRecepcion
            {
                ProductoCodigo = p.Codigo,
                Producto = p,
                CantidadRecibida = l.CantidadRecibida
            });

            p.Stock += l.CantidadRecibida;
            db.MovimientosInventario.Add(new MovimientoInventario
            {
                Fecha = DateTime.UtcNow,
                Tipo = TipoMov.ENTRADA,
                Cantidad = l.CantidadRecibida,
                Motivo = $"RECEPCION OC #{oc.Id} - {req.NroFacturaGuia}",
                ProductoCodigo = p.Codigo
            });

            db.Productos.Update(p);
        }

        await recepciones.AddAsync(r, ct);
        await recepciones.SaveChangesAsync(ct);

        var res = new RecepcionResponse(
            r.Id, r.Fecha, r.OrdenCompraId, r.NroFacturaGuia,
            r.Lineas.Select(li => new RecepcionLineaItem(
                li.Id, li.ProductoCodigo, productos[li.ProductoCodigo].Nombre, li.CantidadRecibida
            ))
        );

        return CreatedAtAction(nameof(Obtener), new { id = r.Id }, res);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<RecepcionResponse>> Obtener([FromRoute] int id, CancellationToken ct)
    {
        var r = await recepciones.GetByIdAsync(id, ct);
        if (r is null) return NotFound();

        var codigos = r.Lineas.Select(l => l.ProductoCodigo).Distinct().ToList();
        var map = await db.Productos.Where(p => codigos.Contains(p.Codigo))
            .ToDictionaryAsync(p => p.Codigo, ct);

        var res = new RecepcionResponse(
            r.Id, r.Fecha, r.OrdenCompraId, r.NroFacturaGuia,
            r.Lineas.Select(li => new RecepcionLineaItem(
                li.Id, li.ProductoCodigo, map[li.ProductoCodigo].Nombre, li.CantidadRecibida
            ))
        );
        return Ok(res);
    }
}