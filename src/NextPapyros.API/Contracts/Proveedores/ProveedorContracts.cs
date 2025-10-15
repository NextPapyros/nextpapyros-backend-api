namespace NextPapyros.API.Contracts.Proveedores;

public record CrearProveedorRequest(
    string Nombre,
    string Nit,
    string PersonaContacto,
    string Telefono,
    string Correo);

public record ProveedorResponse(
    int Id,
    string Nombre,
    string Nit,
    string PersonaContacto,
    string Telefono,
    string Correo,
    bool Activo);
