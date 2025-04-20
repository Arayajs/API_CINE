using API_CINE.Models.Domain;

namespace API_CINE.Repositories.Interfaces
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task<IEnumerable<Movie>> GetActiveMoviesAsync();
        Task<IEnumerable<Movie>> GetMoviesByCinemaAsync(int cinemaId);
        Task<Movie> GetMovieWithScreeningsAsync(int movieId);
    }
}
