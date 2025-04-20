using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface ICinemaHallService
    {
        Task<CinemaHallDto> GetCinemaHallByIdAsync(int hallId);
        Task<IEnumerable<CinemaHallDto>> GetHallsByCinemaAsync(int cinemaId);
        Task<CinemaHallDto> CreateCinemaHallAsync(CinemaHallRequest request);
        Task<CinemaHallDto> UpdateCinemaHallAsync(int hallId, CinemaHallRequest request);
        Task<bool> DeleteCinemaHallAsync(int hallId);
        Task<IEnumerable<SeatDto>> GetSeatsByCinemaHallAsync(int hallId);
    }
}
