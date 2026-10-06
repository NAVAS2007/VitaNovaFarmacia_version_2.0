using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace VitaNovaFarmacia.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> EnviarFacturaPorEmailAsync(string emailDestino, string nombreCliente, byte[] pdfBytes, int numeroFactura)
    {
        try
        {
            if (string.IsNullOrEmpty(emailDestino))
            {
                _logger.LogWarning("Email destino vacío para factura {NumeroFactura}", numeroFactura);
                return false;
            }

            var emailSettings = _configuration.GetSection("EmailSettings");
            var emailRemitente = emailSettings["EmailRemitente"];
            var contrasena = emailSettings["Contrasena"];
            var smtpServer = emailSettings["SmtpServer"];
            var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");

            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Vita Nova Farmacia", emailRemitente));
            mensaje.To.Add(new MailboxAddress(nombreCliente, emailDestino));
            mensaje.Subject = $"Factura de Compra #{numeroFactura} - Vita Nova Farmacia";

            var builder = new BodyBuilder();
            builder.HtmlBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>¡Gracias por su compra!</h2>
                    <p>Estimado(a) {nombreCliente},</p>
                    <p>Adjunto encontrará la factura de su compra en Vita Nova Farmacia.</p>
                    <p><strong>Número de Factura:</strong> {numeroFactura}</p>
                    <p>Si tiene alguna pregunta, no dude en contactarnos.</p>
                    <br/>
                    <p>Saludos,<br/>
                    <strong>Vita Nova Farmacia</strong></p>
                </body>
                </html>";

            builder.Attachments.Add($"Factura_{numeroFactura}.pdf", pdfBytes, new ContentType("application", "pdf"));
            mensaje.Body = builder.ToMessageBody();

            using (var cliente = new SmtpClient())
            {
                await cliente.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                await cliente.AuthenticateAsync(emailRemitente, contrasena);
                await cliente.SendAsync(mensaje);
                await cliente.DisconnectAsync(true);
            }

            _logger.LogInformation("Factura {NumeroFactura} enviada exitosamente a {EmailDestino}", numeroFactura, emailDestino);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar factura {NumeroFactura} a {EmailDestino}", numeroFactura, emailDestino);
            return false;
        }
    }
}
