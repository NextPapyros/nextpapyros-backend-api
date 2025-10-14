using NextPapyros.Domain.Entities.Enums;

namespace NextPapyros.Domain.Entities;

public class Devolucion
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Motivo { get; set; } = default!;
    public DevolucionEstado Estado { get; private set; } = DevolucionEstado.Aprobada;

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = default!;

    public ICollection<DevolucionLinea> Lineas { get; set; } = new List<DevolucionLinea>();

    public void Aprobar()
    {
        if (Estado == DevolucionEstado.Rechazada)
            throw new InvalidOperationException("No se puede aprobar una devolución rechazada.");
        Estado = DevolucionEstado.Aprobada;
    }

    public void Rechazar(string razon)
    {
        if (Estado == DevolucionEstado.Aprobada)
            throw new InvalidOperationException("No se puede rechazar una devolución ya aprobada.");
        Motivo = $"{Motivo} | Rechazo: {razon}";
        Estado = DevolucionEstado.Rechazada;
    }

    public int CantidadTotalDevuelta() => Lineas.Sum(l => l.CantidadDevuelta);
}