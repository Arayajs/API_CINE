using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface ISeatService
    {
        Task<SeatDto> GetSeatByIdAsync(int seatId);
        Task<IEnumerable<SeatDto>> GetSeatsByCinemaHallAsync(int hallId);
        Task<IEnumerable<SeatDto>> GetAvailableSeatsForScreeningAsync(int screeningId);
        Task<SeatDto> CreateSeatAsync(SeatRequest request);
        Task<SeatDto> UpdateSeatAsync(int seatId, SeatRequest request);
        Task<bool> DeleteSeatAsync(int seatId);
        Task<bool> IsSeatAvailableForScreeningAsync(int seatId, int screeningId);
    }
}
