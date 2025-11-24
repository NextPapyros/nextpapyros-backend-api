using NextPapyros.Domain.Entities;

namespace NextPapyros.Application.Reports;

/// <summary>
/// Servicio para la generación de comprobantes de venta en formato PDF.
/// </summary>
public interface IComprobanteService
{
    /// <summary>
    /// Genera un comprobante en formato PDF para una venta específica.
    /// </summary>
    /// <param name="venta">La venta para la cual se generará el comprobante.</param>
    /// <returns>Array de bytes que representa el documento PDF generado.</returns>
    /// <remarks>
    /// El comprobante incluye:
    /// - Información de la empresa (nombre, NIT, teléfono)
    /// - Detalles de la venta (número, fecha, estado, método de pago)
    /// - Tabla con líneas de venta (cantidad, producto, código, precio unitario, subtotal)
    /// - Totales generales
    /// </remarks>
    byte[] GenerarComprobantePdf(Venta venta);
}
