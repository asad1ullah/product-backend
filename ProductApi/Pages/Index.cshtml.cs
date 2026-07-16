using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductApi.Models.Dtos;
using ProductApi.Services;

namespace ProductApi.Pages;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;

    public IndexModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public List<EventDto> Events { get; set; } = new();

    public async Task OnGetAsync()
    {
        Events = await _eventService.GetUpcomingAsync();
    }
}
