using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProductApi.Exceptions;
using ProductApi.Models.Dtos;
using ProductApi.Services;

namespace ProductApi.Areas.Admin.Pages.Events;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IAuthService _authService;

    public CreateModel(IEventService eventService, IAuthService authService)
    {
        _eventService = eventService;
        _authService = authService;
    }

    [BindProperty]
    public CreateEventRequest Form { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Organizers { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
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
            await _eventService.CreateAsync(Form);
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
