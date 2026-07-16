using ProductApi.Models;
using ProductApi.Models.Dtos;

namespace ProductApi.Services;

public interface IEventService
{
    Task<List<EventDto>> GetAllAsync();
    Task<EventDto?> GetByIdAsync(int id);
    Task<EventDto> CreateAsync(CreateEventRequest request);
    Task<EventDto?> UpdateAsync(int id, UpdateEventRequest request);
    Task<bool> DeleteAsync(int id);
    Task<List<Category>> GetCategoriesAsync();
    Task<int> GetCountAsync();
    Task<int> GetUpcomingCountAsync();
    Task<List<EventDto>> GetUpcomingAsync();
}
