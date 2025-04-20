using API_CINE.Models.DTOs;
using API_CINE.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text;

namespace API_CINE.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailService(
            IConfiguration configuration,
            ILoggingService loggingService,
            IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _loggingService = loggingService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendOrderConfirmationAsync(OrderDto order, string userEmail)
        {
            try
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order));

                if (string.IsNullOrEmpty(userEmail))
                    throw new ArgumentException("El correo electrónico del usuario es obligatorio");

                var subject = $"Confirmación de compra - Orden #{order.Id}";
                var body = GenerateOrderConfirmationEmail(order);

                await SendEmailAsync(userEmail, subject, body);
                _loggingService.LogInformation($"Correo de confirmación de orden enviado para la orden {order.Id} a {userEmail}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al enviar correo de confirmación para orden {order?.Id}: {ex.Message}", ex);
                // No lanzar excepción para no interrumpir el flujo principal
            }
        }

        public async Task SendTicketEmailAsync(TicketDto ticket, string userEmail)
        {
            try
            {
                if (ticket == null)
                    throw new ArgumentNullException(nameof(ticket));

                if (string.IsNullOrEmpty(userEmail))
                    throw new ArgumentException("El correo electrónico del usuario es obligatorio");

                var subject = $"Tu entrada para {ticket.MovieTitle}";
                var body = GenerateTicketEmail(ticket);

                await SendEmailAsync(userEmail, subject, body);
                _loggingService.LogInformation($"Correo con ticket enviado para el ticket {ticket.Id} a {userEmail}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al enviar correo con ticket {ticket?.Id}: {ex.Message}", ex);
                // No lanzar excepción para no interrumpir el flujo principal
            }
        }

        public async Task SendWelcomeEmailAsync(string userName, string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                    throw new ArgumentException("El correo electrónico del usuario es obligatorio");

                var subject = "Bienvenido a nuestro servicio de cine";
                var body = GenerateWelcomeEmail(userName);

                await SendEmailAsync(userEmail, subject, body);
                _loggingService.LogInformation($"Correo de bienvenida enviado a {userEmail}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al enviar correo de bienvenida a {userEmail}: {ex.Message}", ex);
                // No lanzar excepción para no interrumpir el flujo principal
            }
        }

        public async Task SendPasswordResetEmailAsync(string userEmail, string resetToken)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                    throw new ArgumentException("El correo electrónico del usuario es obligatorio");

                if (string.IsNullOrEmpty(resetToken))
                    throw new ArgumentException("El token de restablecimiento es obligatorio");

                var subject = "Restablecimiento de contraseña";
                var body = GeneratePasswordResetEmail(resetToken);

                await SendEmailAsync(userEmail, subject, body);
                _loggingService.LogInformation($"Correo de restablecimiento de contraseña enviado a {userEmail}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al enviar correo de restablecimiento de contraseña a {userEmail}: {ex.Message}", ex);
                // No lanzar excepción para no interrumpir el flujo principal
            }
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                // Verificar si estamos en modo de desarrollo
                bool isDevelopment = _webHostEnvironment.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);

                // En modo de desarrollo, solo registrar el correo, no enviarlo realmente
                if (isDevelopment)
                {
                    _loggingService.LogInformation($"[DEV] Simulando envío de correo a {to}");
                    _loggingService.LogInformation($"[DEV] Asunto: {subject}");
                    _loggingService.LogInformation($"[DEV] Cuerpo (truncado): {htmlBody.Substring(0, Math.Min(100, htmlBody.Length))}...");
                    return;
                }

                // Configuración del correo
                var emailSettings = new
                {
                    DisplayName = _configuration["EmailSettings:DisplayName"],
                    From = _configuration["EmailSettings:From"],
                    Host = _configuration["EmailSettings:Host"],
                    Port = int.Parse(_configuration["EmailSettings:Port"]),
                    UserName = _configuration["EmailSettings:UserName"],
                    Password = _configuration["EmailSettings:Password"],
                    EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]),
                    UseDefaultCredentials = bool.Parse(_configuration["EmailSettings:UseDefaultCredentials"])
                };

                // Crear el mensaje
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailSettings.DisplayName, emailSettings.From));
                message.To.Add(new MailboxAddress(to, to));
                message.Subject = subject;

                // Configurar el cuerpo del mensaje
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };

                message.Body = bodyBuilder.ToMessageBody();

                // Enviar el correo
                using (var client = new SmtpClient())
                {
                    // Configurar el cliente SMTP
                    await client.ConnectAsync(emailSettings.Host, emailSettings.Port, 
                        emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                    // Autenticación
                    if (!emailSettings.UseDefaultCredentials)
                    {
                        await client.AuthenticateAsync(emailSettings.UserName, emailSettings.Password);
                    }

                    // Enviar el mensaje
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error al enviar correo a {to}: {ex.Message}", ex);
                throw; // Relanzar la excepción para que el método que llama pueda manejarla
            }
        }

        // Métodos para generar el contenido de los correos
        private string GenerateOrderConfirmationEmail(OrderDto order)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\">");
            sb.AppendLine("<title>Confirmación de compra</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; background-color: #f8f9fa; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("h1 { color: #007bff; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }");
            sb.AppendLine("th { background-color: #f2f2f2; }");
            sb.AppendLine(".total { font-weight: bold; font-size: 18px; margin-top: 20px; text-align: right; }");
            sb.AppendLine(".footer { margin-top: 30px; font-size: 12px; color: #666; text-align: center; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"container\">");
            sb.AppendLine($"<h1>¡Gracias por tu compra!</h1>");
            sb.AppendLine($"<p>Estimado/a {order.UserName},</p>");
            sb.AppendLine($"<p>Hemos recibido tu orden #{order.Id} con fecha {order.OrderDate:dd/MM/yyyy HH:mm}.</p>");
            sb.AppendLine("<h2>Detalles de la compra:</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Película</th><th>Sala</th><th>Fecha y hora</th><th>Asiento</th><th>Precio</th></tr>");

            foreach (var ticket in order.Tickets)
            {
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{ticket.MovieTitle}</td>");
                sb.AppendLine($"<td>{ticket.CinemaName} - {ticket.HallName}</td>");
                sb.AppendLine($"<td>{ticket.ScreeningTime:dd/MM/yyyy HH:mm}</td>");
                sb.AppendLine($"<td>Fila {ticket.Row}, Asiento {ticket.SeatNumber}</td>");
                sb.AppendLine($"<td>{ticket.Price:C}</td>");
                sb.AppendLine($"</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine($"<div class=\"total\">Total: {order.TotalAmount:C}</div>");
            sb.AppendLine("<p>Los tickets están adjuntos a este correo. También puedes verlos en tu cuenta.</p>");
            sb.AppendLine("<p>¡Disfruta de la película!</p>");
            sb.AppendLine("<div class=\"footer\">");
            sb.AppendLine("<p>Este es un correo automático, por favor no responder.</p>");
            sb.AppendLine("<p>&copy; " + DateTime.Now.Year + " CinemaApp. Todos los derechos reservados.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string GenerateTicketEmail(TicketDto ticket)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\">");
            sb.AppendLine("<title>Tu entrada de cine</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }");
            sb.AppendLine(".ticket { max-width: 400px; margin: 0 auto; background-color: #fff; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1); border: 2px dashed #007bff; }");
            sb.AppendLine(".ticket-header { text-align: center; border-bottom: 1px solid #ddd; padding-bottom: 10px; margin-bottom: 20px; }");
            sb.AppendLine(".movie-title { font-size: 22px; font-weight: bold; color: #007bff; margin: 10px 0; }");
            sb.AppendLine(".ticket-info { margin-bottom: 5px; display: flex; }");
            sb.AppendLine(".label { font-weight: bold; width: 120px; }");
            sb.AppendLine(".value { flex: 1; }");
            sb.AppendLine(".ticket-code { font-size: 20px; font-family: monospace; text-align: center; margin: 20px 0; letter-spacing: 2px; font-weight: bold; }");
            sb.AppendLine(".footer { margin-top: 30px; font-size: 12px; color: #666; text-align: center; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"ticket\">");
            sb.AppendLine("<div class=\"ticket-header\">");
            sb.AppendLine("<h1>ENTRADA DE CINE</h1>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<div class=\"movie-title\">{ticket.MovieTitle}</div>");
            sb.AppendLine("<div class=\"ticket-info\"><div class=\"label\">Cine:</div><div class=\"value\">{ticket.CinemaName}</div></div>");
            sb.AppendLine($"<div class=\"ticket-info\"><div class=\"label\">Sala:</div><div class=\"value\">{ticket.HallName}</div></div>");
            sb.AppendLine($"<div class=\"ticket-info\"><div class=\"label\">Fecha y hora:</div><div class=\"value\">{ticket.ScreeningTime:dd/MM/yyyy HH:mm}</div></div>");
            sb.AppendLine($"<div class=\"ticket-info\"><div class=\"label\">Asiento:</div><div class=\"value\">Fila {ticket.Row}, Asiento {ticket.SeatNumber}</div></div>");
            sb.AppendLine($"<div class=\"ticket-info\"><div class=\"label\">Precio:</div><div class=\"value\">{ticket.Price:C}</div></div>");
            sb.AppendLine($"<div class=\"ticket-code\">{ticket.TicketCode}</div>");
            sb.AppendLine("<p>Presenta este código en la entrada del cine.</p>");
            sb.AppendLine("<div class=\"footer\">");
            sb.AppendLine("<p>Este es un correo automático, por favor no responder.</p>");
            sb.AppendLine("<p>&copy; " + DateTime.Now.Year + " CinemaApp. Todos los derechos reservados.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string GenerateWelcomeEmail(string userName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\">");
            sb.AppendLine("<title>Bienvenido a CinemaApp</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; background-color: #f8f9fa; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("h1 { color: #007bff; }");
            sb.AppendLine(".footer { margin-top: 30px; font-size: 12px; color: #666; text-align: center; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"container\">");
            sb.AppendLine($"<h1>¡Bienvenido a CinemaApp, {userName}!</h1>");
            sb.AppendLine("<p>Gracias por unirte a nuestra plataforma de reserva de entradas de cine.</p>");
            sb.AppendLine("<p>Ahora podrás:</p>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Ver la cartelera de películas</li>");
            sb.AppendLine("<li>Reservar entradas para tus películas favoritas</li>");
            sb.AppendLine("<li>Seleccionar los mejores asientos</li>");
            sb.AppendLine("<li>Recibir tus entradas directamente en tu correo</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("<p>No dudes en contactarnos si necesitas ayuda.</p>");
            sb.AppendLine("<p>¡Esperamos que disfrutes de nuestro servicio!</p>");
            sb.AppendLine("<div class=\"footer\">");
            sb.AppendLine("<p>Este es un correo automático, por favor no responder.</p>");
            sb.AppendLine("<p>&copy; " + DateTime.Now.Year + " CinemaApp. Todos los derechos reservados.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string GeneratePasswordResetEmail(string resetToken)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\">");
            sb.AppendLine("<title>Restablecimiento de contraseña</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; background-color: #f8f9fa; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("h1 { color: #007bff; }");
            sb.AppendLine(".token { font-size: 18px; font-family: monospace; text-align: center; margin: 20px 0; letter-spacing: 2px; font-weight: bold; padding: 10px; background-color: #f2f2f2; border-radius: 5px; }");
            sb.AppendLine(".footer { margin-top: 30px; font-size: 12px; color: #666; text-align: center; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"container\">");
            sb.AppendLine($"<h1>Restablecimiento de contraseña</h1>");
            sb.AppendLine("<p>Has solicitado restablecer tu contraseña. Utiliza el siguiente token para completar el proceso:</p>");
            sb.AppendLine($"<div class=\"token\">{resetToken}</div>");
            sb.AppendLine("<p>Si no has solicitado este cambio, puedes ignorar este correo.</p>");
            sb.AppendLine("<p>El token expirará en 1 hora por motivos de seguridad.</p>");
            sb.AppendLine("<div class=\"footer\">");
            sb.AppendLine("<p>Este es un correo automático, por favor no responder.</p>");
            sb.AppendLine("<p>&copy; " + DateTime.Now.Year + " CinemaApp. Todos los derechos reservados.</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}