using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProductApi.Services;

namespace ProductApi.Areas.Admin.Pages;

[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IAuthService _authService;

    public DashboardModel(IEventService eventService, IAuthService authService)
    {
        _eventService = eventService;
        _authService = authService;
    }

    public int TotalEvents { get; set; }
    public int TotalUsers { get; set; }
    public int UpcomingEvents { get; set; }

    public async Task OnGetAsync()
    {
        TotalEvents = await _eventService.GetCountAsync();
        UpcomingEvents = await _eventService.GetUpcomingCountAsync();
        TotalUsers = await _authService.GetUserCountAsync();
    }
}
