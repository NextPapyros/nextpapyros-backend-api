using Microsoft.Extensions.Configuration;
using NextPapyros.Application.Email;
using Resend;

namespace NextPapyros.Infrastructure.Email;

/// <summary>
/// Implementación del servicio de correo usando Resend.
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;

    public ResendEmailService(IResend resend, IConfiguration configuration)
    {
        _resend = resend;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task EnviarCorreoRecuperacionAsync(
        string destinatario,
        string nombreUsuario,
        string token,
        CancellationToken ct = default)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? throw new InvalidOperationException("FrontendUrl no configurado");
        
        // Configurar remitente desde appsettings o usar uno por defecto verificado en Resend
        var emailConfig = _configuration.GetSection("Email");
        var remitenteEmail = emailConfig["From"] ?? "onboarding@resend.dev";
        var remitenteNombre = "NextPapyros";

        var htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #a3ebb1; color: black; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                    .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 5px 5px; }}
                    .token-box {{ background-color: white; border: 2px solid #a3ebb1; padding: 15px; margin: 20px 0; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 2px; border-radius: 5px; }}
                    .footer {{ margin-top: 20px; text-align: center; font-size: 12px; color: #666; }}
                    .warning {{ color: #dc2626; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🔐 Recuperación de Contraseña</h1>
                    </div>
                    <div class='content'>
                        <p>Hola <strong>{nombreUsuario}</strong>,</p>
                        <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en NextPapyros.</p>
                        <p>Puedes acceder a la página de recuperación de contraseña en el siguiente enlace: <a href='{frontendUrl}/reset-password?token={token}&email={destinatario}' target='_blank' rel='noopener noreferrer'>Restablecer Contraseña</a></p>
                        <p>Este enlace es válido por <strong>30 minutos</strong>.</p>
                        <p class='warning'>⚠️ Si no solicitaste este cambio, ignora este correo y tu contraseña permanecerá sin cambios.</p>
                    </div>
                    <div class='footer'>
                        <p>© 2025 NextPapyros - Sistema de Gestión Empresarial</p>
                        <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        var message = new EmailMessage();
        message.From = $"{remitenteNombre} <{remitenteEmail}>";
        message.To.Add(destinatario);
        message.Subject = "Recuperación de Contraseña - NextPapyros";
        message.HtmlBody = htmlContent;

        await _resend.EmailSendAsync(message, ct);
    }
}
