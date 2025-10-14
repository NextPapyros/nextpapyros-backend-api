// src/NextPapyros.API/Controllers/AuthController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextPapyros.API.Contracts.Auth;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Auth;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    IUsuarioRepository users,
    IRoleRepository roles,
    IPasswordHasher hasher,
    ITokenService tokens
) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(req.Email, ct);
        if (user is null || !user.Activo) return Unauthorized("Credenciales inválidas.");

        if (!hasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Credenciales inválidas.");

        var roleNames = user.Roles.Select(ur => ur.Rol.Nombre).ToArray();
        var jwt = tokens.CreateToken(user, roleNames);

        user.IniciarSesion();
        await users.SaveChangesAsync(ct);

        return Ok(new LoginResponse(jwt, DateTime.UtcNow.AddHours(8)));
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<ActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (!User.IsInRole("Admin"))
            return Forbid();

        if (await users.EmailExistsAsync(req.Email, ct))
            return Conflict("El email ya está registrado.");

        var rol = await roles.GetByNameAsync(req.Rol, ct);
        if (rol is null) return BadRequest($"Rol '{req.Rol}' no existe.");

        var u = new Usuario
        {
            Nombre = req.Nombre,
            Email = req.Email,
            PasswordHash = hasher.Hash(req.Password),
            Activo = true
        };
        u.Roles.Add(new UsuarioRol { RolId = rol.Id, Rol = rol, Usuario = u });

        await users.AddAsync(u, ct);
        await users.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Me), new { }, null);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        if (!int.TryParse(sub, out var id)) return Unauthorized();

        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var user = await users.GetByEmailAsync(email, ct);
        if (user is null) return Unauthorized();

        var roleNames = user.Roles.Select(r => r.Rol.Nombre).ToArray();
        return Ok(new MeResponse(user.Id, user.Nombre, user.Email, roleNames));
    }
}