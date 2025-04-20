using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface ITicketService
    {
        Task<TicketDto> GetTicketByIdAsync(int ticketId);
        Task<TicketDto> GetTicketByCodeAsync(string ticketCode);
        Task<IEnumerable<TicketDto>> GetTicketsByOrderAsync(int orderId);
        Task<IEnumerable<TicketDto>> GetTicketsByUserAsync(int userId);
        Task<bool> ValidateTicketAsync(string ticketCode);
        Task<bool> MarkTicketAsUsedAsync(string ticketCode);
    }
}
