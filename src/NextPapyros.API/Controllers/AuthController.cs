// src/NextPapyros.API/Controllers/AuthController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextPapyros.API.Contracts.Auth;
using NextPapyros.Application.Email;
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
    ITokenService tokens,
    IEmailService emailService
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

    /// <summary>
    /// Solicita un token de recuperación de contraseña.
    /// </summary>
    /// <param name="req">Email del usuario que solicita la recuperación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="200">Solicitud procesada. Si el email existe, se enviará un correo con el token.</response>
    /// <remarks>
    /// Ejemplo de request:
    /// 
    ///     POST /auth/forgot-password
    ///     {
    ///        "email": "usuario@example.com"
    ///     }
    ///     
    /// **Importante:**
    /// - El token es válido por 30 minutos
    /// - Si el email no existe, se retorna 200 por seguridad (no revelar usuarios)
    /// - El token se envía por correo electrónico
    /// - Solo usuarios activos pueden solicitar recuperación
    /// </remarks>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(req.Email, ct);
        
        // Por seguridad, siempre retornamos 200 aunque el email no exista
        if (user is null || !user.Activo)
            return Ok(new { message = "Si el email existe, recibirás un correo con instrucciones." });

        // Generar token aleatorio de 6 dígitos
        var token = new Random().Next(100000, 999999).ToString();
        
        user.ResetPasswordToken = token;
        user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(30);
        
        await users.SaveChangesAsync(ct);

        try
        {
            await emailService.EnviarCorreoRecuperacionAsync(
                user.Email,
                user.Nombre,
                token,
                ct);
        }
        catch (Exception)
        {
            // Log del error pero no revelamos el fallo al usuario
            return Ok(new { message = "Si el email existe, recibirás un correo con instrucciones." });
        }

        return Ok(new { message = "Si el email existe, recibirás un correo con instrucciones." });
    }

    /// <summary>
    /// Restablece la contraseña usando el token de recuperación.
    /// </summary>
    /// <param name="req">Email, token de recuperación y nueva contraseña.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="200">Contraseña restablecida exitosamente.</response>
    /// <response code="400">Token inválido, expirado o datos incorrectos.</response>
    /// <remarks>
    /// Ejemplo de request:
    /// 
    ///     POST /auth/reset-password
    ///     {
    ///        "email": "usuario@example.com",
    ///        "token": "123456",
    ///        "newPassword": "NuevaPassword123*"
    ///     }
    ///     
    /// **Validaciones:**
    /// - El token debe ser válido y no estar expirado (30 minutos)
    /// - La nueva contraseña debe cumplir los requisitos de seguridad
    /// - El usuario debe estar activo
    /// 
    /// **Nota:** Después de restablecer la contraseña, el token se invalida automáticamente.
    /// </remarks>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 8)
            return BadRequest("La contraseña debe tener al menos 8 caracteres.");

        var user = await users.GetByEmailAsync(req.Email, ct);
        
        if (user is null || !user.Activo)
            return BadRequest("Token inválido o expirado.");

        if (string.IsNullOrEmpty(user.ResetPasswordToken) || 
            user.ResetPasswordToken != req.Token)
            return BadRequest("Token inválido o expirado.");

        if (user.ResetPasswordTokenExpiry is null || 
            user.ResetPasswordTokenExpiry < DateTime.UtcNow)
            return BadRequest("Token inválido o expirado.");

        // Actualizar contraseña y limpiar token
        user.PasswordHash = hasher.Hash(req.NewPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiry = null;

        await users.SaveChangesAsync(ct);

        return Ok(new { message = "Contraseña restablecida exitosamente." });
    }
}