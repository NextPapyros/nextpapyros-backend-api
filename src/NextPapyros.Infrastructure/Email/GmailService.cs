using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NextPapyros.Application.Email;

namespace NextPapyros.Infrastructure.Email;

/// <summary>
/// Implementación del servicio de correo usando Gmail SMTP.
/// </summary>
public class GmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public GmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task EnviarCorreoRecuperacionAsync(
        string destinatario,
        string nombreUsuario,
        string token,
        CancellationToken ct = default)
    {
        var emailConfig = _configuration.GetSection("Email");
        var remitente = emailConfig["From"] ?? throw new InvalidOperationException("Email:From no configurado");
        var password = emailConfig["Password"] ?? throw new InvalidOperationException("Email:Password no configurado");
        var smtp = emailConfig["SmtpServer"] ?? "smtp.gmail.com";
        var port = int.Parse(emailConfig["SmtpPort"] ?? "587");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("NextPapyros", remitente));
        message.To.Add(new MailboxAddress(nombreUsuario, destinatario));
        message.Subject = "Recuperación de Contraseña - NextPapyros";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2563eb; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 5px 5px; }}
                        .token-box {{ background-color: white; border: 2px solid #2563eb; padding: 15px; margin: 20px 0; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 2px; border-radius: 5px; }}
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
                            <p>Tu código de recuperación es:</p>
                            <div class='token-box'>{token}</div>
                            <p>Este código es válido por <strong>30 minutos</strong>.</p>
                            <p class='warning'>⚠️ Si no solicitaste este cambio, ignora este correo y tu contraseña permanecerá sin cambios.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2025 NextPapyros - Sistema de Gestión Empresarial</p>
                            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
                        </div>
                    </div>
                </body>
                </html>
            ",
            TextBody = $@"
                Recuperación de Contraseña - NextPapyros
                
                Hola {nombreUsuario},
                
                Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en NextPapyros.
                
                Tu código de recuperación es: {token}
                
                Este código es válido por 30 minutos.
                
                Si no solicitaste este cambio, ignora este correo y tu contraseña permanecerá sin cambios.
                
                © 2025 NextPapyros - Sistema de Gestión Empresarial
            "
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(smtp, port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(remitente, password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al enviar correo: {ex.Message}", ex);
        }
    }
}
