using API_CINE.Models.Domain;

namespace API_CINE.Repositories.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role> GetByNameAsync(string name);
        Task<Role> GetRoleWithUsersAsync(int roleId);
    }
}
