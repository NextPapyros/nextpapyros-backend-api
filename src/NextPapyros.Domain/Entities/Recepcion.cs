namespace NextPapyros.Domain.Entities;

public class Recepcion
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string NroFacturaGuia { get; set; } = default!;

    public int OrdenCompraId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = default!;

    public ICollection<LineaRecepcion> Lineas { get; set; } = new List<LineaRecepcion>();

    public void Registrar()
    {
        if (Lineas.Count == 0)
            throw new InvalidOperationException("No se puede registrar una recepción sin líneas.");
    }
}