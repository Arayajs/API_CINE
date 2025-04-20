using API_CINE.Data;
using API_CINE.Models.Domain;
using API_CINE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_CINE.Repositories.Implementations
{
    public class CinemaHallRepository : Repository<CinemaHall>, ICinemaHallRepository
    {
        public CinemaHallRepository(CinemaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CinemaHall>> GetHallsByCinemaAsync(int cinemaId)
        {
            return await _dbSet
                .Where(h => h.CinemaId == cinemaId)
                .ToListAsync();
        }

        public async Task<CinemaHall> GetHallWithSeatsAsync(int hallId)
        {
            return await _dbSet
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(h => h.Id == hallId);
        }

        public async Task<CinemaHall> GetHallWithScreeningsAsync(int hallId)
        {
            return await _dbSet
                .Include(h => h.MovieScreenings)
                .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(h => h.Id == hallId);
        }
    }
}
