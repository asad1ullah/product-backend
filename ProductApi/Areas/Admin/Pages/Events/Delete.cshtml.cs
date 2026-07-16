using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductApi.Models.Dtos;
using ProductApi.Services;

namespace ProductApi.Areas.Admin.Pages.Events;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly IEventService _eventService;

    public DeleteModel(IEventService eventService)
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

    public async Task<IActionResult> OnPostAsync()
    {
        await _eventService.DeleteAsync(Id);
        return RedirectToPage("/Events/Index", new { area = "Admin" });
    }
}
