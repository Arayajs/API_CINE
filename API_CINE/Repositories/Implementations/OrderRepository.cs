using API_CINE.Data;
using API_CINE.Models.Domain;
using API_CINE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_CINE.Repositories.Implementations
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(CinemaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId)
        {
            return await _dbSet
                .Include(o => o.User)
                .Include(o => o.Tickets)
                .ThenInclude(t => t.MovieScreening)
                .ThenInclude(s => s.Movie)
                .Include(o => o.Tickets)
                .ThenInclude(t => t.MovieScreening)
                .ThenInclude(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Include(o => o.Tickets)
                .ThenInclude(t => t.Seat)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderWithDetailsAsync(int orderId)
        {
            return await _dbSet
                .Include(o => o.User)
                .Include(o => o.Tickets)
                .ThenInclude(t => t.MovieScreening)
                .ThenInclude(s => s.Movie)
                .Include(o => o.Tickets)
                .ThenInclude(t => t.MovieScreening)
                .ThenInclude(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Include(o => o.Tickets)
                .ThenInclude(t => t.Seat)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count)
        {
            return await _dbSet
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }
    }
}
