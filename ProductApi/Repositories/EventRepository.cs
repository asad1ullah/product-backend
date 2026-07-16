using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Event?> GetByIdAsync(int id) =>
        _context.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id);

    public Task<List<Event>> GetAllAsync() =>
        _context.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .AsNoTracking()
            .ToListAsync();

    public Task<bool> CategoryExistsAsync(int categoryId) =>
        _context.Categories.AnyAsync(c => c.Id == categoryId);

    public Task<bool> OrganizerExistsAsync(int organizerId) =>
        _context.Users.AnyAsync(u => u.Id == organizerId);

    public Task<List<Category>> GetCategoriesAsync() =>
        _context.Categories.AsNoTracking().ToListAsync();

    public Task<int> CountAsync() => _context.Events.CountAsync();

    public Task<int> CountUpcomingAsync(DateTime from) =>
        _context.Events.CountAsync(e => e.StartDate >= from);

    public Task<List<Event>> GetUpcomingAsync(DateTime from) =>
        _context.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .Where(e => e.StartDate >= from)
            .OrderBy(e => e.StartDate)
            .AsNoTracking()
            .ToListAsync();

    public async Task AddAsync(Event @event) => await _context.Events.AddAsync(@event);

    public void Remove(Event @event) => _context.Events.Remove(@event);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
