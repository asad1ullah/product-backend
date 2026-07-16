using ProductApi.Models;
using ProductApi.Models.Dtos;

namespace ProductApi.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<User?> ValidateCredentialsAsync(LoginRequest request);
    Task<List<User>> GetAllUsersAsync();
    Task<int> GetUserCountAsync();
}
