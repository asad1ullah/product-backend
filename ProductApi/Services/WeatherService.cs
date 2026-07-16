using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Logging;
using ProductApi.Exceptions;
using ProductApi.Models.Dtos;

namespace ProductApi.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WeatherService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<WeatherDto?> GetWeatherAsync(string city)
    {
        var section = _configuration.GetSection("OpenWeatherMap");
        var apiKey = section["ApiKey"];
        var baseUrl = section["BaseUrl"] ?? "https://api.openweathermap.org/data/2.5/weather";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("OpenWeatherMap:ApiKey is not configured.");
            throw new WeatherServiceException("Weather service is not configured.");
        }

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["q"] = city;
        query["appid"] = apiKey;
        query["units"] = "metric";
        var requestUri = $"{baseUrl}?{query}";

        var client = _httpClientFactory.CreateClient("OpenWeatherMap");

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(requestUri);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach OpenWeatherMap for city '{City}'.", city);
            throw new WeatherServiceException("Could not reach the weather service. Please try again later.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "OpenWeatherMap request timed out for city '{City}'.", city);
            throw new WeatherServiceException("The weather service timed out. Please try again later.", ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("City '{City}' not found on OpenWeatherMap.", city);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenWeatherMap returned {StatusCode} for city '{City}'.",
                response.StatusCode,
                city);
            throw new WeatherServiceException($"The weather service returned an unexpected error ({(int)response.StatusCode}).");
        }

        try
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var payload = await JsonSerializer.DeserializeAsync<OpenWeatherMapResponse>(stream);

            if (payload is null)
            {
                _logger.LogError("OpenWeatherMap returned an empty response for city '{City}'.", city);
                throw new WeatherServiceException("The weather service returned an empty response.");
            }

            return new WeatherDto
            {
                City = payload.Name,
                Country = payload.Sys?.Country ?? string.Empty,
                TemperatureCelsius = payload.Main?.Temp ?? 0,
                FeelsLikeCelsius = payload.Main?.FeelsLike ?? 0,
                Humidity = payload.Main?.Humidity ?? 0,
                WindSpeed = payload.Wind?.Speed ?? 0,
                Description = payload.Weather?.FirstOrDefault()?.Description ?? string.Empty,
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenWeatherMap response for city '{City}'.", city);
            throw new WeatherServiceException("Failed to parse the weather service response.", ex);
        }
    }

    private class OpenWeatherMapResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("main")]
        public MainInfo? Main { get; set; }

        [JsonPropertyName("wind")]
        public WindInfo? Wind { get; set; }

        [JsonPropertyName("sys")]
        public SysInfo? Sys { get; set; }

        [JsonPropertyName("weather")]
        public List<WeatherInfo>? Weather { get; set; }
    }

    private class MainInfo
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; }

        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }
    }

    private class WindInfo
    {
        [JsonPropertyName("speed")]
        public double Speed { get; set; }
    }

    private class SysInfo
    {
        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }

    private class WeatherInfo
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
