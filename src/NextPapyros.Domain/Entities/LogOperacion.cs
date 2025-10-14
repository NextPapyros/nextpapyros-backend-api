namespace NextPapyros.Domain.Entities;

public class LogOperacion
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int UsuarioId { get; set; }
    public string Entidad { get; set; } = default!;
    public string IdEntidad { get; set; } = default!;
    public string Accion { get; set; } = default!;
    public string Detalle { get; set; } = string.Empty;

    public Usuario Usuario { get; set; } = default!;
}