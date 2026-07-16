using ProductApi.Models;
using ProductApi.Models.Dtos;
using ProductApi.Repositories;

namespace ProductApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenGenerator _tokenGenerator;

    public AuthService(IUserRepository userRepository, JwtTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            throw new InvalidOperationException("Username already taken.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var (token, expiresAt) = _tokenGenerator.GenerateToken(user);
        return new AuthResponse { Token = token, Username = user.Username, Role = user.Role, ExpiresAt = expiresAt };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await ValidateCredentialsAsync(request);
        if (user is null)
        {
            return null;
        }

        var (token, expiresAt) = _tokenGenerator.GenerateToken(user);
        return new AuthResponse { Token = token, Username = user.Username, Role = user.Role, ExpiresAt = expiresAt };
    }

    public async Task<User?> ValidateCredentialsAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }

    public Task<List<User>> GetAllUsersAsync() => _userRepository.GetAllAsync();

    public Task<int> GetUserCountAsync() => _userRepository.CountAsync();
}
