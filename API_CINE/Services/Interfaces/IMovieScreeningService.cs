using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface IMovieScreeningService
    {
        Task<MovieScreeningDto> GetScreeningByIdAsync(int screeningId);
        Task<IEnumerable<MovieScreeningDto>> GetAllScreeningsAsync();
        Task<IEnumerable<MovieScreeningDto>> GetScreeningsByMovieAsync(int movieId);
        Task<IEnumerable<MovieScreeningDto>> GetScreeningsByCinemaAsync(int cinemaId);
        Task<IEnumerable<MovieScreeningDto>> GetScreeningsByCinemaHallAsync(int hallId);
        Task<IEnumerable<MovieScreeningDto>> GetScreeningsByDateAsync(DateTime date);
        Task<MovieScreeningDto> CreateScreeningAsync(MovieScreeningRequest request);
        Task<MovieScreeningDto> UpdateScreeningAsync(int screeningId, MovieScreeningRequest request);
        Task<bool> DeleteScreeningAsync(int screeningId);
        Task<IEnumerable<SeatDto>> GetAvailableSeatsForScreeningAsync(int screeningId);
    }
}
