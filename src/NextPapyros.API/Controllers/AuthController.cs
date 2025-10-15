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
    /// <summary>
    /// Autentica un usuario y genera un token JWT.
    /// </summary>
    /// <param name="req">Credenciales de inicio de sesión (email y contraseña).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Token JWT y fecha de expiración.</returns>
    /// <response code="200">Login exitoso. Retorna el token JWT.</response>
    /// <response code="401">Credenciales inválidas o usuario inactivo.</response>
    /// <remarks>
    /// Ejemplo de request:
    /// 
    ///     POST /auth/login
    ///     {
    ///        "email": "mail@mail.com",
    ///        "password": "Password123"
    ///     }
    ///     
    /// El token retornado debe ser usado en el header Authorization:
    /// `Authorization: Bearer {token}`
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Registra un nuevo usuario en el sistema (solo administradores).
    /// </summary>
    /// <param name="req">Datos del nuevo usuario (nombre, email, contraseña, rol).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="200">Usuario registrado exitosamente.</response>
    /// <response code="403">El usuario no tiene permisos de administrador.</response>
    /// <response code="409">El email ya está registrado en el sistema.</response>
    /// <response code="404">El rol especificado no existe.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     POST /auth/register
    ///     {
    ///        "nombre": "Juan Pérez",
    ///        "email": "juan.perez@example.com",
    ///        "password": "Password123*",
    ///        "rol": "Empleado"
    ///     }
    /// </remarks>
    [HttpPost("register")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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