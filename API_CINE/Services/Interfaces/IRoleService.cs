using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface IRoleService
    {
        Task<RoleDto> GetRoleByIdAsync(int roleId);
        Task<RoleDto> GetRoleByNameAsync(string roleName);
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> CreateRoleAsync(RoleRequest request);
        Task<RoleDto> UpdateRoleAsync(int roleId, RoleRequest request);
        Task<bool> DeleteRoleAsync(int roleId);
    }
}
