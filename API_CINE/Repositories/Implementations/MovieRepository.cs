using API_CINE.Data;
using API_CINE.Models.Domain;
using API_CINE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_CINE.Repositories.Implementations
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        public MovieRepository(CinemaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Movie>> GetActiveMoviesAsync()
        {
            return await _dbSet.Where(m => m.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Movie>> GetMoviesByCinemaAsync(int cinemaId)
        {
            return await _context.MovieScreenings
                .Include(s => s.Movie)
                .Where(s => s.CinemaHall.CinemaId == cinemaId && s.IsActive && s.Movie.IsActive)
                .Select(s => s.Movie)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Movie> GetMovieWithScreeningsAsync(int movieId)
        {
            return await _dbSet
                .Include(m => m.MovieScreenings)
                .ThenInclude(s => s.CinemaHall)
                .ThenInclude(h => h.Cinema)
                .FirstOrDefaultAsync(m => m.Id == movieId);
        }
    }
}
