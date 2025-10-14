namespace NextPapyros.Domain.Entities;

public class Producto
{
    public string Codigo { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string Categoria { get; set; } = default!;
    public double Costo { get; set; }
    public double Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public DateTime FechaIngreso { get; set; }
    public bool Activo { get; set; }

    public ICollection<LineaOrdenCompra> LineasOrdenCompra { get; set; } = [];
    public ICollection<LineaRecepcion> LineasRecepcion { get; set; } = [];
    public ICollection<LineaVenta> LineasVenta { get; set; } = [];
    public ICollection<MovimientoInventario> Movimientos { get; set; } = [];
    public ICollection<ProductoProveedor> Proveedores { get; set; } = [];
}