namespace NextPapyros.API.Contracts.Auth;

public record LoginResponse(string Token, DateTime ExpiresAtUtc);