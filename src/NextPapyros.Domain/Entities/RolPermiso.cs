namespace NextPapyros.Domain.Entities;

public class RolPermiso
{
    public int Id { get; set; }
    public int RolId { get; set; }
    public Rol Rol { get; set; } = default!;
    public int PermisoId { get; set; }
    public Permiso Permiso { get; set; } = default!;
}