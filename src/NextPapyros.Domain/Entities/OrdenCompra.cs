using NextPapyros.Domain.Entities.Enums;

namespace NextPapyros.Domain.Entities;

public class OrdenCompra
{
    public int Id { get; set; }
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public EstadoOrdenCompra Estado { get; private set; } = EstadoOrdenCompra.Emitida;
    public DateTime? FechaEsperada { get; set; }
    public double Total { get; private set; }

    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = default!;

    public ICollection<LineaOrdenCompra> Lineas { get; set; } = new List<LineaOrdenCompra>();

    public void Emitir()
    {
        if (Estado != EstadoOrdenCompra.Emitida)
            throw new InvalidOperationException("La OC ya fue emitida o no está en estado válido.");
        RecalcularTotal();
    }

    public void CerrarSiCompleta()
    {
        if (Lineas.Count == 0)
            throw new InvalidOperationException("No se puede cerrar una OC sin líneas.");

        // Regla simple: si todas las líneas tienen CantidadSolicitada > 0, se permite cierre.
        if (Lineas.All(l => l.CantidadSolicitada > 0))
            Estado = EstadoOrdenCompra.Cerrada;
        else
            throw new InvalidOperationException("La OC no está completa.");
    }

    public void Anular(string motivo)
    {
        if (Estado == EstadoOrdenCompra.Cerrada)
            throw new InvalidOperationException("No se puede anular una OC cerrada.");
        Estado = EstadoOrdenCompra.Anulada;
    }

    public void RecalcularTotal()
    {
        Total = Lineas.Sum(l => l.Subtotal);
    }
}