using API_CINE.Models.Domain;
using API_CINE.Models.DTOs;

namespace API_CINE.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterUserAsync(RegisterRequest register);
        Task<AuthResponse> LoginAsync(LoginRequest login);
        Task<string> GenerateJwtTokenAsync(User user);
        int? GetUserIdFromToken(string token);
    }
}
