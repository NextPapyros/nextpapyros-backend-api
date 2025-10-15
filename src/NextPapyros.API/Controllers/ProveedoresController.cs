using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextPapyros.API.Contracts.Proveedores;
using NextPapyros.Domain.Entities;
using NextPapyros.Domain.Repositories;

namespace NextPapyros.API.Controllers;

[ApiController]
[Route("proveedores")]
public class ProveedoresController(
    IProveedorRepository proveedores,
    IUnitOfWork unitOfWork
) : ControllerBase
{
    /// <summary>
    /// Registra un nuevo proveedor en el sistema.
    /// </summary>
    /// <param name="req">Datos del proveedor (nombre, NIT, persona de contacto, teléfono, correo).</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El proveedor creado con todos sus detalles.</returns>
    /// <response code="201">Proveedor registrado exitosamente.</response>
    /// <response code="400">Datos inválidos (formato de correo incorrecto, campos vacíos).</response>
    /// <response code="409">Ya existe un proveedor con ese nombre o NIT.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <remarks>
    /// **Requiere rol:** Administrador
    /// 
    /// Ejemplo de request:
    /// 
    ///     POST /proveedores
    ///     {
    ///        "nombre": "Distribuidora Papelera S.A.",
    ///        "nit": "900123456-7",
    ///        "personaContacto": "Juan Pérez",
    ///        "telefono": "+57 300 1234567",
    ///        "correo": "contacto@distribuidora.com"
    ///     }
    ///     
    /// **Reglas de negocio aplicadas:**
    /// - El nombre y el NIT deben ser únicos en el sistema
    /// - Todos los campos son obligatorios
    /// - El correo electrónico debe tener formato válido
    /// - El proveedor se crea con estado Activo por defecto
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProveedorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProveedorResponse>> Crear(
        [FromBody] CrearProveedorRequest req, 
        CancellationToken ct)
    {
        // Validación de campos obligatorios
        if (string.IsNullOrWhiteSpace(req.Nombre))
            return BadRequest("El nombre del proveedor es obligatorio.");
        
        if (string.IsNullOrWhiteSpace(req.Nit))
            return BadRequest("El NIT es obligatorio.");
        
        if (string.IsNullOrWhiteSpace(req.PersonaContacto))
            return BadRequest("La persona de contacto es obligatoria.");
        
        if (string.IsNullOrWhiteSpace(req.Telefono))
            return BadRequest("El teléfono es obligatorio.");
        
        if (string.IsNullOrWhiteSpace(req.Correo))
            return BadRequest("El correo electrónico es obligatorio.");

        // Validación de formato de correo electrónico
        if (!new EmailAddressAttribute().IsValid(req.Correo))
            return BadRequest("El formato del correo electrónico no es válido.");

        // Verificar duplicados por nombre
        var existenteNombre = await proveedores.GetByNombreAsync(req.Nombre, ct);
        if (existenteNombre is not null)
            return Conflict("Ya existe un proveedor con ese nombre.");

        // Verificar duplicados por NIT
        var existenteNit = await proveedores.GetByNitAsync(req.Nit, ct);
        if (existenteNit is not null)
            return Conflict("Ya existe un proveedor con ese NIT.");

        try
        {
            await unitOfWork.BeginAsync(ct);

            var proveedor = new Proveedor
            {
                Nombre = req.Nombre.Trim(),
                Nit = req.Nit.Trim(),
                PersonaContacto = req.PersonaContacto.Trim(),
                Telefono = req.Telefono.Trim(),
                Correo = req.Correo.Trim().ToLowerInvariant(),
                Activo = true
            };

            await proveedores.AddAsync(proveedor, ct);
            await unitOfWork.CommitAsync(ct);

            var response = new ProveedorResponse(
                proveedor.Id,
                proveedor.Nombre,
                proveedor.Nit,
                proveedor.PersonaContacto,
                proveedor.Telefono,
                proveedor.Correo,
                proveedor.Activo
            );

            return CreatedAtAction(nameof(Obtener), new { id = proveedor.Id }, response);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Obtiene un proveedor por su ID.
    /// </summary>
    /// <param name="id">ID único del proveedor.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Los detalles completos del proveedor.</returns>
    /// <response code="200">Proveedor encontrado.</response>
    /// <response code="404">No existe un proveedor con ese ID.</response>
    /// <response code="401">No autenticado.</response>
    /// <remarks>
    /// Ejemplo de request:
    /// 
    ///     GET /proveedores/5
    /// </remarks>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ProveedorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProveedorResponse>> Obtener(
        [FromRoute] int id, 
        CancellationToken ct)
    {
        var proveedor = await proveedores.GetByIdAsync(id, ct);
        if (proveedor is null)
            return NotFound();

        var response = new ProveedorResponse(
            proveedor.Id,
            proveedor.Nombre,
            proveedor.Nit,
            proveedor.PersonaContacto,
            proveedor.Telefono,
            proveedor.Correo,
            proveedor.Activo
        );

        return Ok(response);
    }

    /// <summary>
    /// Obtiene la lista de todos los proveedores activos.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de proveedores activos ordenados alfabéticamente por nombre.</returns>
    /// <response code="200">Lista de proveedores activos.</response>
    /// <response code="401">No autenticado.</response>
    /// <remarks>
    /// Solo retorna proveedores con estado Activo=true.
    /// 
    /// Ejemplo de request:
    /// 
    ///     GET /proveedores
    /// </remarks>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ProveedorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ProveedorResponse>>> ListarActivos(CancellationToken ct)
    {
        var proveedoresActivos = await proveedores.GetAllActivosAsync(ct);

        var response = proveedoresActivos.Select(p => new ProveedorResponse(
            p.Id,
            p.Nombre,
            p.Nit,
            p.PersonaContacto,
            p.Telefono,
            p.Correo,
            p.Activo
        ));

        return Ok(response);
    }
}
