namespace NextPapyros.Domain.Entities;

public class Permiso
{
    public int Id { get; set; }
    public string Codigo { get; set; } = default!;
    public string Descripcion { get; set; } = default!;
    public ICollection<RolPermiso> Roles { get; set; } = [];
}