using API_CINE.Models.Domain;

namespace API_CINE.Repositories.Interfaces
{
    public interface ISeatRepository : IRepository<Seat>
    {
        Task<IEnumerable<Seat>> GetSeatsByCinemaHallAsync(int hallId);
        Task<IEnumerable<Seat>> GetAvailableSeatsForScreeningAsync(int screeningId);
        Task<bool> IsSeatAvailableForScreeningAsync(int seatId, int screeningId);
    }
}
