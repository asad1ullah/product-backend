using ProductApi.Models.Dtos;

namespace ProductApi.Services;

public interface IWeatherService
{
    Task<WeatherDto?> GetWeatherAsync(string city);
}
