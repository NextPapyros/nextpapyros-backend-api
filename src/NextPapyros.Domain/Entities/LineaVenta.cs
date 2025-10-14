namespace NextPapyros.Domain.Entities;

public class LineaVenta
{
    public int Id { get; set; }
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double Subtotal { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = default!;

    public string ProductoCodigo { get; set; } = default!;
    public Producto Producto { get; set; } = default!;
}