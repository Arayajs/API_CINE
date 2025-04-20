using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface ICinemaService
    {
        Task<CinemaDto> GetCinemaByIdAsync(int cinemaId);
        Task<IEnumerable<CinemaDto>> GetAllCinemasAsync();
        Task<IEnumerable<CinemaDto>> GetActiveCinemasAsync();
        Task<CinemaDto> CreateCinemaAsync(CinemaRequest request);
        Task<CinemaDto> UpdateCinemaAsync(int cinemaId, CinemaRequest request);
        Task<bool> DeleteCinemaAsync(int cinemaId);
        Task<IEnumerable<MovieDto>> GetMoviesByCinemaAsync(int cinemaId);
    }
}
