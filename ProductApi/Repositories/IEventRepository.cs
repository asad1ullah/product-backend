using ProductApi.Models;

namespace ProductApi.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id);
    Task<List<Event>> GetAllAsync();
    Task<bool> CategoryExistsAsync(int categoryId);
    Task<bool> OrganizerExistsAsync(int organizerId);
    Task<List<Category>> GetCategoriesAsync();
    Task<int> CountAsync();
    Task<int> CountUpcomingAsync(DateTime from);
    Task<List<Event>> GetUpcomingAsync(DateTime from);
    Task AddAsync(Event @event);
    void Remove(Event @event);
    Task SaveChangesAsync();
}
