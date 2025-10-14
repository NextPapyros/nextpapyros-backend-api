namespace NextPapyros.Domain.Entities;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string Nit { get; set; } = default!;
    public string Telefono { get; set; } = default!;
    public string Correo { get; set; } = default!;
    public string Observaciones { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<ProductoProveedor> Productos { get; set; } = [];
    public ICollection<OrdenCompra> OrdenesEmitidas { get; set; } = [];
}