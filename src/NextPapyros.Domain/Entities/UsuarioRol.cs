namespace NextPapyros.Domain.Entities;

public class UsuarioRol
{
    public int Id { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = default!;

    public int RolId { get; set; }
    public Rol Rol { get; set; } = default!;
}