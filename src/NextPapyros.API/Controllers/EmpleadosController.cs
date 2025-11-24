using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextPapyros.API.Contracts.Empleados;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;
using NextPapyros.Infrastructure.Auth;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("empleados")]
public class EmpleadosController(
    IUsuarioRepository usuarios,
    IRoleRepository roles,
    IPasswordHasher hasher
) : ControllerBase
{
    /// <summary>
    /// Crea un nuevo empleado en el sistema (solo administradores).
    /// </summary>
    /// <param name="req">Datos del empleado (nombre, email, contraseña, rol).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El empleado creado con su ID y datos.</returns>
    /// <response code="201">Empleado creado exitosamente.</response>
    /// <response code="403">El usuario no tiene permisos de administrador.</response>
    /// <response code="409">El email ya está registrado en el sistema.</response>
    /// <response code="404">El rol especificado no existe.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     POST /empleados
    ///     {
    ///        "nombre": "Juan Pérez",
    ///        "email": "juan.perez@example.com",
    ///        "password": "Password123*",
    ///        "rol": "Empleado"
    ///     }
    ///     
    /// **Roles válidos:** Empleado, Admin
    /// 
    /// **Reglas de negocio:**
    /// - Solo administradores pueden crear empleados
    /// - El email debe ser único
    /// - La contraseña se hashea automáticamente
    /// - El empleado se crea activo por defecto
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EmpleadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpleadoResponse>> Crear([FromBody] CrearEmpleadoRequest req, CancellationToken ct)
    {
        if (await usuarios.EmailExistsAsync(req.Email, ct))
            return Conflict("El email ya está registrado.");

        var rol = await roles.GetByNameAsync(req.Rol, ct);
        if (rol is null) return NotFound($"Rol '{req.Rol}' no existe.");

        var empleado = new Usuario
        {
            Nombre = req.Nombre,
            Email = req.Email,
            PasswordHash = hasher.Hash(req.Password),
            Activo = true,
            Roles = new List<UsuarioRol>()
        };

        empleado.Roles.Add(new UsuarioRol
        {
            Usuario = empleado,
            Rol = rol,
            FechaAsignacion = DateTime.UtcNow
        });

        await usuarios.AddAsync(empleado, ct);
        await usuarios.SaveChangesAsync(ct);

        var response = new EmpleadoResponse(
            empleado.Id,
            empleado.Nombre,
            empleado.Email,
            empleado.Activo,
            empleado.Roles.Select(r => r.Rol.Nombre)
        );

        return CreatedAtAction(nameof(Obtener), new { id = empleado.Id }, response);
    }

    /// <summary>
    /// Obtiene los datos de un empleado por su ID (solo administradores).
    /// </summary>
    /// <param name="id">ID del empleado.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Los datos del empleado.</returns>
    /// <response code="200">Empleado encontrado.</response>
    /// <response code="403">El usuario no tiene permisos de administrador.</response>
    /// <response code="404">Empleado no encontrado.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     GET /empleados/1
    /// </remarks>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EmpleadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpleadoResponse>> Obtener([FromRoute] int id, CancellationToken ct)
    {
        var empleado = await usuarios.GetByIdAsync(id, ct);
        if (empleado is null) return NotFound();

        var response = new EmpleadoResponse(
            empleado.Id,
            empleado.Nombre,
            empleado.Email,
            empleado.Activo,
            empleado.Roles.Select(r => r.Rol.Nombre)
        );

        return Ok(response);
    }

    /// <summary>
    /// Actualiza los datos de un empleado (solo administradores).
    /// </summary>
    /// <param name="id">ID del empleado a actualizar.</param>
    /// <param name="req">Nuevos datos del empleado (nombre, email, rol).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El empleado actualizado.</returns>
    /// <response code="200">Empleado actualizado exitosamente.</response>
    /// <response code="403">El usuario no tiene permisos de administrador.</response>
    /// <response code="404">Empleado o rol no encontrado.</response>
    /// <response code="409">El nuevo email ya está registrado por otro usuario.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     PUT /empleados/1
    ///     {
    ///        "nombre": "Juan Pérez Actualizado",
    ///        "email": "juan.nuevo@example.com",
    ///        "rol": "Admin"
    ///     }
    ///     
    /// **Reglas de negocio:**
    /// - Solo administradores pueden editar empleados
    /// - Si se cambia el email, debe ser único
    /// - Se puede cambiar el rol del empleado
    /// - No se puede editar la contraseña (usar endpoint específico)
    /// </remarks>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EmpleadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmpleadoResponse>> Actualizar(
        [FromRoute] int id,
        [FromBody] ActualizarEmpleadoRequest req,
        CancellationToken ct)
    {
        var empleado = await usuarios.GetByIdAsync(id, ct);
        if (empleado is null) return NotFound();

        // Validar email único si cambió
        if (empleado.Email != req.Email && await usuarios.EmailExistsAsync(req.Email, ct))
            return Conflict("El email ya está registrado por otro usuario.");

        empleado.Nombre = req.Nombre;
        empleado.Email = req.Email;

        // Actualizar rol si se especificó
        if (!string.IsNullOrWhiteSpace(req.Rol))
        {
            var nuevoRol = await roles.GetByNameAsync(req.Rol, ct);
            if (nuevoRol is null) return NotFound($"Rol '{req.Rol}' no existe.");

            // Remover roles anteriores y asignar el nuevo
            empleado.Roles.Clear();
            empleado.Roles.Add(new UsuarioRol
            {
                Usuario = empleado,
                Rol = nuevoRol,
                FechaAsignacion = DateTime.UtcNow
            });
        }

        await usuarios.SaveChangesAsync(ct);

        var response = new EmpleadoResponse(
            empleado.Id,
            empleado.Nombre,
            empleado.Email,
            empleado.Activo,
            empleado.Roles.Select(r => r.Rol.Nombre)
        );

        return Ok(response);
    }

    /// <summary>
    /// Inhabilita un empleado en el sistema (solo administradores).
    /// El empleado no podrá iniciar sesión pero sus datos se conservan.
    /// </summary>
    /// <param name="id">ID del empleado a inhabilitar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resultado de la operación.</returns>
    /// <response code="200">Empleado inhabilitado exitosamente.</response>
    /// <response code="403">El usuario no tiene permisos de administrador.</response>
    /// <response code="404">Empleado no encontrado.</response>
    /// <response code="400">No se puede inhabilitar al propio administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     PATCH /empleados/1/inhabilitar
    ///     
    /// **Reglas de negocio:**
    /// - Solo administradores pueden inhabilitar empleados
    /// - No se puede inhabilitar a sí mismo
    /// - El empleado inhabilitado no puede iniciar sesión
    /// - Los datos se conservan para auditoría
    /// - Se puede reactivar cambiando el estado Activo
    /// </remarks>
    [HttpPatch("{id:int}/inhabilitar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Inhabilitar([FromRoute] int id, CancellationToken ct)
    {
        var empleado = await usuarios.GetByIdAsync(id, ct);
        if (empleado is null) return NotFound();

        // Obtener ID del usuario actual
        var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        if (empleado.Id == currentUserId)
            return BadRequest("No puedes inhabilitarte a ti mismo.");

        empleado.Activo = false;
        await usuarios.SaveChangesAsync(ct);

        return Ok(new { message = "Empleado inhabilitado exitosamente." });
    }

    /// <summary>
    /// Lista todos los empleados del sistema (solo administradores).
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de todos los empleados con sus datos.</returns>
    /// <response code="200">Lista de empleados (puede estar vacía).</response>
    /// <response code="403">El usuario no tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     GET /empleados
    ///     
    /// **Incluye:**
    /// - Empleados activos e inactivos
    /// - Roles asignados a cada empleado
    /// - Información básica de contacto
    /// </remarks>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<EmpleadoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<EmpleadoResponse>>> Listar(CancellationToken ct)
    {
        var empleados = await usuarios.GetAllAsync(ct);

        var response = empleados.Select(e => new EmpleadoResponse(
            e.Id,
            e.Nombre,
            e.Email,
            e.Activo,
            e.Roles.Select(r => r.Rol.Nombre)
        ));

        return Ok(response);
    }
}
