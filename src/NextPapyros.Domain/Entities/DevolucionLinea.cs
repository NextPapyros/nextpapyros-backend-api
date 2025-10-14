namespace NextPapyros.Domain.Entities;

public class DevolucionLinea
{
    public int Id { get; set; }
    public int CantidadDevuelta { get; set; }

    public int DevolucionId { get; set; }
    public Devolucion Devolucion { get; set; } = default!;

    public int LineaVentaId { get; set; }
    public LineaVenta LineaVenta { get; set; } = default!;
}