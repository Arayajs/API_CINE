using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> UpdateUserAsync(int userId, UserUpdateRequest updateRequest);
        Task<bool> DeleteUserAsync(int userId);
        Task<UserDto> AssignRoleToUserAsync(UserRoleRequest request);
        Task<UserDto> RemoveRoleFromUserAsync(UserRoleRequest request);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName);
    }
}
