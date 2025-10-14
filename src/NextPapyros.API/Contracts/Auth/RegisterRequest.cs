namespace NextPapyros.API.Contracts.Auth;

public record RegisterRequest(string Nombre, string Email, string Password, string Rol = "Empleado");