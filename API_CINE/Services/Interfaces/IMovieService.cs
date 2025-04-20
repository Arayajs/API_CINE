using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface IMovieService
    {
        Task<MovieDto> GetMovieByIdAsync(int movieId);
        Task<IEnumerable<MovieDto>> GetAllMoviesAsync();
        Task<IEnumerable<MovieDto>> GetActiveMoviesAsync();
        Task<MovieDto> CreateMovieAsync(MovieRequest request);
        Task<MovieDto> UpdateMovieAsync(int movieId, MovieRequest request);
        Task<bool> DeleteMovieAsync(int movieId);
        Task<IEnumerable<MovieScreeningDto>> GetScreeningsByMovieAsync(int movieId);
    }
}
