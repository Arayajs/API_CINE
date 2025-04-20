using API_CINE.Models.Domain;

namespace API_CINE.Repositories.Interfaces
{
    public interface ICinemaRepository : IRepository<Cinema>
    {
        Task<IEnumerable<Cinema>> GetActiveCinemasAsync();
        Task<Cinema> GetCinemaWithHallsAsync(int cinemaId);
        Task<Cinema> GetCinemaWithMoviesAsync(int cinemaId);
    }
}
