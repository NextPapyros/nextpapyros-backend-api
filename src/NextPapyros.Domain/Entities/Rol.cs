namespace NextPapyros.Domain.Entities;

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<UsuarioRol> Usuarios { get; set; } = [];
    public ICollection<RolPermiso> Permisos { get; set; } = [];
}