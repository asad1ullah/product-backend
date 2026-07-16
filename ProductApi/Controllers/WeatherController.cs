using Microsoft.AspNetCore.Mvc;
using ProductApi.Exceptions;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> GetByCity(string city)
    {
        try
        {
            var weather = await _weatherService.GetWeatherAsync(city);
            return weather is null
                ? NotFound(new { message = $"Weather data not found for city '{city}'." })
                : Ok(weather);
        }
        catch (WeatherServiceException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }
}
