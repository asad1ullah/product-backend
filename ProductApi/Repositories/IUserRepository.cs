using ProductApi.Models;

namespace ProductApi.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
    Task<List<User>> GetAllAsync();
    Task<int> CountAsync();
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
