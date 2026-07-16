using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductApi.Models.Dtos;
using ProductApi.Services;

namespace ProductApi.Pages.Events;

public class DetailsModel : PageModel
{
    private readonly IEventService _eventService;

    public DetailsModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public EventDto? Event { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Event = await _eventService.GetByIdAsync(Id);
        return Event is null ? NotFound() : Page();
    }
}
