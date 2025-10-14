using NextPapyros.Domain.Entities.Enums;

namespace NextPapyros.Domain.Entities;

public class MovimientoInventario
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public TipoMov Tipo { get; set; }
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = default!;

    public string ProductoCodigo { get; set; } = default!;
    public Producto Producto { get; set; } = default!;
}