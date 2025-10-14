namespace NextPapyros.Domain.Entities;

public class LineaOrdenCompra
{
    public int Id { get; set; }
    public int CantidadSolicitada { get; set; }
    public double CostoUnitario { get; set; }
    public double Subtotal { get; set; }

    public int OrdenCompraId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = default!;

    public string ProductoCodigo { get; set; } = default!;
    public Producto Producto { get; set; } = default!;
}