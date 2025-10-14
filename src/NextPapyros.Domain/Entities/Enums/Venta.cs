using NextPapyros.Domain.Entities.Enums;

namespace NextPapyros.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public double Total { get; set; }
    public string Estado { get; set; } = "CONFIRMADA";
    public MetodoPago MetodoPago { get; set; }
    public string? MotivoAnulacion { get; set; }

    public ICollection<LineaVenta> Lineas { get; set; } = [];
    public ICollection<Devolucion> Devoluciones { get; set; } = [];
}