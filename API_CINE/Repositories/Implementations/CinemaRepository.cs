using API_CINE.Data;
using API_CINE.Models.Domain;
using API_CINE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_CINE.Repositories.Implementations
{
    public class CinemaRepository : Repository<Cinema>, ICinemaRepository
    {
        public CinemaRepository(CinemaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Cinema>> GetActiveCinemasAsync()
        {
            return await _dbSet.Where(c => c.IsActive).ToListAsync();
        }

        public async Task<Cinema> GetCinemaWithHallsAsync(int cinemaId)
        {
            return await _dbSet
                .Include(c => c.CinemaHalls)
                .FirstOrDefaultAsync(c => c.Id == cinemaId);
        }

        public async Task<Cinema> GetCinemaWithMoviesAsync(int cinemaId)
        {
            return await _dbSet
                .Include(c => c.CinemaHalls)
                .ThenInclude(h => h.MovieScreenings)
                .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(c => c.Id == cinemaId);
        }
    }
}
