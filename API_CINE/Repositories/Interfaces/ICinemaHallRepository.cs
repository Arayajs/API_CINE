using API_CINE.Models.Domain;

namespace API_CINE.Repositories.Interfaces
{
    public interface ICinemaHallRepository : IRepository<CinemaHall>
    {
        Task<IEnumerable<CinemaHall>> GetHallsByCinemaAsync(int cinemaId);
        Task<CinemaHall> GetHallWithSeatsAsync(int hallId);
        Task<CinemaHall> GetHallWithScreeningsAsync(int hallId);
    }
}
