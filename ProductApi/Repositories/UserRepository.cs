using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByUsernameAsync(string username) =>
        _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public Task<bool> UsernameExistsAsync(string username) =>
        _context.Users.AnyAsync(u => u.Username == username);

    public Task<List<User>> GetAllAsync() =>
        _context.Users.AsNoTracking().ToListAsync();

    public Task<int> CountAsync() => _context.Users.CountAsync();

    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
