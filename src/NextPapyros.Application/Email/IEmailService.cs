namespace NextPapyros.Application.Email;

/// <summary>
/// Servicio para el envío de correos electrónicos.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un correo electrónico de recuperación de contraseña.
    /// </summary>
    /// <param name="destinatario">Dirección de correo del destinatario.</param>
    /// <param name="nombreUsuario">Nombre del usuario que solicita la recuperación.</param>
    /// <param name="token">Token de recuperación de contraseña.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Task que representa la operación asíncrona.</returns>
    Task EnviarCorreoRecuperacionAsync(
        string destinatario,
        string nombreUsuario,
        string token,
        CancellationToken ct = default);
}
