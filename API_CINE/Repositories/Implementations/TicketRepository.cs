using API_CINE.Data;
using API_CINE.Models.Domain;
using API_CINE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_CINE.Repositories.Implementations
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(CinemaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByOrderAsync(int orderId)
        {
            return await _dbSet
                .Include(t => t.MovieScreening)
                .ThenInclude(s => s.Movie)
                .Include(t => t.MovieScreening)
                .ThenInclude(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Include(t => t.Seat)
                .Where(t => t.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByScreeningAsync(int screeningId)
        {
            return await _dbSet
                .Include(t => t.Seat)
                .Include(t => t.Order)
                .Where(t => t.MovieScreeningId == screeningId)
                .ToListAsync();
        }

        public async Task<Ticket> GetTicketByCodeAsync(string ticketCode)
        {
            return await _dbSet
                .Include(t => t.MovieScreening)
                .ThenInclude(s => s.Movie)
                .Include(t => t.MovieScreening)
                .ThenInclude(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Include(t => t.Seat)
                .Include(t => t.Order)
                .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByUserAsync(int userId)
        {
            return await _dbSet
                .Include(t => t.MovieScreening)
                .ThenInclude(s => s.Movie)
                .Include(t => t.MovieScreening)
                .ThenInclude(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Include(t => t.Seat)
                .Include(t => t.Order)
                .Where(t => t.Order.UserId == userId)
                .OrderByDescending(t => t.Order.OrderDate)
                .ToListAsync();
        }
    }
}
