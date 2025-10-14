namespace NextPapyros.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public bool Activo { get; set; } = true;
    public DateTime? UltimoAcceso { get; private set; }

    public ICollection<UsuarioRol> Roles { get; set; } = new List<UsuarioRol>();
    public ICollection<LogOperacion> Logs { get; set; } = new List<LogOperacion>();

    public void IniciarSesion() => UltimoAcceso = DateTime.UtcNow;

    public void CerrarSesion()
    {
        UltimoAcceso = DateTime.UtcNow;
    }
}