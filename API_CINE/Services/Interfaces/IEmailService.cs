using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(OrderDto order, string userEmail);
        Task SendTicketEmailAsync(TicketDto ticket, string userEmail);
        Task SendWelcomeEmailAsync(string userName, string userEmail);
        Task SendPasswordResetEmailAsync(string userEmail, string resetToken);
    }
}
