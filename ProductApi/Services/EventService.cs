using ProductApi.Models;
using ProductApi.Models.Dtos;
using ProductApi.Repositories;

namespace ProductApi.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IImageUploadService _imageUploadService;

    public EventService(IEventRepository eventRepository, IImageUploadService imageUploadService)
    {
        _eventRepository = eventRepository;
        _imageUploadService = imageUploadService;
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        var events = await _eventRepository.GetAllAsync();
        return events.Select(ToDto).ToList();
    }

    public async Task<EventDto?> GetByIdAsync(int id)
    {
        var @event = await _eventRepository.GetByIdAsync(id);
        return @event is null ? null : ToDto(@event);
    }

    public async Task<EventDto> CreateAsync(CreateEventRequest request)
    {
        await EnsureReferencesExistAsync(request.CategoryId, request.OrganizerId);

        var @event = new Event
        {
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Capacity = request.Capacity,
            Price = request.Price,
            CategoryId = request.CategoryId,
            OrganizerId = request.OrganizerId,
        };

        if (request.Image is not null)
        {
            @event.ImageUrl = await _imageUploadService.SaveEventImageAsync(request.Image);
        }

        await _eventRepository.AddAsync(@event);
        await _eventRepository.SaveChangesAsync();

        return ToDto(@event);
    }

    public async Task<EventDto?> UpdateAsync(int id, UpdateEventRequest request)
    {
        var @event = await _eventRepository.GetByIdAsync(id);
        if (@event is null)
        {
            return null;
        }

        await EnsureReferencesExistAsync(request.CategoryId, request.OrganizerId);

        @event.Title = request.Title;
        @event.Description = request.Description;
        @event.Location = request.Location;
        @event.StartDate = request.StartDate;
        @event.EndDate = request.EndDate;
        @event.Capacity = request.Capacity;
        @event.Price = request.Price;
        @event.CategoryId = request.CategoryId;
        @event.OrganizerId = request.OrganizerId;

        if (request.Image is not null)
        {
            var newImageUrl = await _imageUploadService.SaveEventImageAsync(request.Image);
            _imageUploadService.DeleteEventImage(@event.ImageUrl);
            @event.ImageUrl = newImageUrl;
        }

        await _eventRepository.SaveChangesAsync();
        return ToDto(@event);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var @event = await _eventRepository.GetByIdAsync(id);
        if (@event is null)
        {
            return false;
        }

        _imageUploadService.DeleteEventImage(@event.ImageUrl);
        _eventRepository.Remove(@event);
        await _eventRepository.SaveChangesAsync();
        return true;
    }

    public Task<List<Category>> GetCategoriesAsync() => _eventRepository.GetCategoriesAsync();

    public Task<int> GetCountAsync() => _eventRepository.CountAsync();

    public Task<int> GetUpcomingCountAsync() => _eventRepository.CountUpcomingAsync(DateTime.UtcNow);

    public async Task<List<EventDto>> GetUpcomingAsync()
    {
        var events = await _eventRepository.GetUpcomingAsync(DateTime.UtcNow);
        return events.Select(ToDto).ToList();
    }

    private async Task EnsureReferencesExistAsync(int categoryId, int organizerId)
    {
        if (!await _eventRepository.CategoryExistsAsync(categoryId))
        {
            throw new InvalidOperationException("Category not found.");
        }

        if (!await _eventRepository.OrganizerExistsAsync(organizerId))
        {
            throw new InvalidOperationException("Organizer not found.");
        }
    }

    private static EventDto ToDto(Event @event) => new()
    {
        Id = @event.Id,
        Title = @event.Title,
        Description = @event.Description,
        Location = @event.Location,
        StartDate = @event.StartDate,
        EndDate = @event.EndDate,
        Capacity = @event.Capacity,
        Price = @event.Price,
        CategoryId = @event.CategoryId,
        CategoryName = @event.Category?.Name,
        OrganizerId = @event.OrganizerId,
        OrganizerUsername = @event.Organizer?.Username,
        ImageUrl = @event.ImageUrl,
    };
}
