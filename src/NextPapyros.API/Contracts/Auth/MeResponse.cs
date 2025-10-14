namespace NextPapyros.API.Contracts.Auth;

public record MeResponse(int Id, string Nombre, string Email, string[] Roles);