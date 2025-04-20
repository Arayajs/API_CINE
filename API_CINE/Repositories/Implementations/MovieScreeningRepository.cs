using API_CINE.Data;
using API_CINE.Models.Domain;
using API_CINE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_CINE.Repositories.Implementations
{
    public class MovieScreeningRepository : Repository<MovieScreening>, IMovieScreeningRepository
    {
        public MovieScreeningRepository(CinemaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MovieScreening>> GetScreeningsByMovieAsync(int movieId)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Where(s => s.MovieId == movieId && s.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovieScreening>> GetScreeningsByCinemaAsync(int cinemaId)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Where(s => s.CinemaHall.CinemaId == cinemaId && s.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovieScreening>> GetScreeningsByCinemaHallAsync(int hallId)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Where(s => s.CinemaHallId == hallId && s.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovieScreening>> GetScreeningsByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1).AddTicks(-1);

            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Where(s => s.StartTime >= startDate && s.StartTime <= endDate && s.IsActive)
                .ToListAsync();
        }

        public async Task<MovieScreening> GetScreeningWithDetailsAsync(int screeningId)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .Include(s => s.Tickets)
                .ThenInclude(t => t.Seat)
                .FirstOrDefaultAsync(s => s.Id == screeningId);
        }
    }
}
