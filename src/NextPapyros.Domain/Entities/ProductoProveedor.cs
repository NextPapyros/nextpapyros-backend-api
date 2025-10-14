namespace NextPapyros.Domain.Entities;

public class ProductoProveedor
{
    public string ProductoCodigo { get; set; } = default!;
    public int ProveedorId { get; set; }

    public double CostoReferencial { get; set; }
    public bool Preferente { get; set; }

    public Producto Producto { get; set; } = default!;
    public Proveedor Proveedor { get; set; } = default!;
}