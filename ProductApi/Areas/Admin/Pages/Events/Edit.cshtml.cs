using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProductApi.Exceptions;
using ProductApi.Models.Dtos;
using ProductApi.Services;

namespace ProductApi.Areas.Admin.Pages.Events;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IAuthService _authService;

    public EditModel(IEventService eventService, IAuthService authService)
    {
        _eventService = eventService;
        _authService = authService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public UpdateEventRequest Form { get; set; } = new();

    public string? CurrentImageUrl { get; set; }
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Organizers { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var @event = await _eventService.GetByIdAsync(Id);
        if (@event is null)
        {
            return NotFound();
        }

        Form = new UpdateEventRequest
        {
            Title = @event.Title,
            Description = @event.Description,
            Location = @event.Location,
            StartDate = @event.StartDate,
            EndDate = @event.EndDate,
            Capacity = @event.Capacity,
            Price = @event.Price,
            CategoryId = @event.CategoryId,
            OrganizerId = @event.OrganizerId,
        };
        CurrentImageUrl = @event.ImageUrl;

        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadOptionsAsync();
            return Page();
        }

        try
        {
            var updated = await _eventService.UpdateAsync(Id, Form);
            if (updated is null)
            {
                return NotFound();
            }

            return RedirectToPage("/Events/Index", new { area = "Admin" });
        }
        catch (ImageValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }

        await LoadOptionsAsync();
        return Page();
    }

    private async Task LoadOptionsAsync()
    {
        var categories = await _eventService.GetCategoriesAsync();
        Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();

        var users = await _authService.GetAllUsersAsync();
        Organizers = users.Select(u => new SelectListItem(u.Username, u.Id.ToString())).ToList();
    }
}
