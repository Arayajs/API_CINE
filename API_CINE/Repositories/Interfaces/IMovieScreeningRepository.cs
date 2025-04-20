using API_CINE.Models.Domain;

namespace API_CINE.Repositories.Interfaces
{
    public interface IMovieScreeningRepository : IRepository<MovieScreening>
    {
        Task<IEnumerable<MovieScreening>> GetScreeningsByMovieAsync(int movieId);
        Task<IEnumerable<MovieScreening>> GetScreeningsByCinemaAsync(int cinemaId);
        Task<IEnumerable<MovieScreening>> GetScreeningsByCinemaHallAsync(int hallId);
        Task<IEnumerable<MovieScreening>> GetScreeningsByDateAsync(DateTime date);
        Task<MovieScreening> GetScreeningWithDetailsAsync(int screeningId);
    }
}
