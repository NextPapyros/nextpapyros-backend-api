namespace NextPapyros.Domain.Entities;

public class LineaRecepcion
{
    public int Id { get; set; }
    public int CantidadRecibida { get; set; }

    public int RecepcionId { get; set; }
    public Recepcion Recepcion { get; set; } = default!;

    public string ProductoCodigo { get; set; } = default!;
    public Producto Producto { get; set; } = default!;
}