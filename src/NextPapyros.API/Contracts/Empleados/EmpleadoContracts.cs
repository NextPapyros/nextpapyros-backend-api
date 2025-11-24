namespace NextPapyros.API.Contracts.Empleados;

public record CrearEmpleadoRequest(
    string Nombre,
    string Email,
    string Password,
    string Rol = "Empleado");

public record ActualizarEmpleadoRequest(
    string Nombre,
    string Email,
    string? Rol);

public record EmpleadoResponse(
    int Id,
    string Nombre,
    string Email,
    bool Activo,
    IEnumerable<string> Roles);
