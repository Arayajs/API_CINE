using API_CINE.Models.Domain;

namespace API_CINE.Repositories.Interfaces
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<IEnumerable<Ticket>> GetTicketsByOrderAsync(int orderId);
        Task<IEnumerable<Ticket>> GetTicketsByScreeningAsync(int screeningId);
        Task<Ticket> GetTicketByCodeAsync(string ticketCode);
        Task<IEnumerable<Ticket>> GetTicketsByUserAsync(int userId);
    }
}
