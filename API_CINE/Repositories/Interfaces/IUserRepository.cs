using API_CINE.Models.Domain;

namespace API_CINE.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
        Task<User> GetUserWithRolesAsync(int userId);
        Task DeleteAsync(UserRole userRole);
        Task AddAsync(UserRole userRole);
    }
}
