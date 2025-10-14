using Microsoft.AspNetCore.Mvc;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Auth;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUsuarioRepository usuarios, IPasswordHasher hasher, ITokenService tokens) : ControllerBase
{
    private readonly IUsuarioRepository _usuarios = usuarios;
    private readonly IPasswordHasher _hasher = hasher;
    private readonly ITokenService _tokens = tokens;

    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string Token, string Nombre, string[] Roles);

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _usuarios.GetByEmailAsync(req.Email, ct);
        if (user is null || !user.Activo) return Unauthorized();

        if (!_hasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized();

        var roles = user.Roles.Select(r => r.Rol.Nombre).ToArray();
        var token = _tokens.CreateToken(user, roles);
        return Ok(new LoginResponse(token, user.Nombre, roles));
    }
}