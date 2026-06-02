using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortManagement.Application.Common.Interfaces;

namespace ResortManagement.WebApi.Controllers.Mvc;

[Route("api/weather")]
[ApiController]
public class WeatherApiController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly HttpClient _httpClient;

    public WeatherApiController(IApplicationDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ResortManagementApp");
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentWeather()
    {
        try
        {
            var resort = await _context.Resorts.FirstOrDefaultAsync();
            if (resort == null)
            {
                return Ok(new { temp = 15, city = "Geneva", country = "CH", icon = "wb_sunny", desc = "Clear" });
            }

            string cityName = resort.Address.Split(',')[0].Trim();

            var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(cityName)}&count=1";
            var geoResponse = await _httpClient.GetFromJsonAsync<JsonElement>(geoUrl);
            
            if (geoResponse.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
            {
                var location = results[0];
                var lat = location.GetProperty("latitude").GetDouble();
                var lon = location.GetProperty("longitude").GetDouble();
                var countryCode = location.TryGetProperty("country_code", out var cc) ? cc.GetString() : "CH";

                var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
                var weatherResponse = await _httpClient.GetFromJsonAsync<JsonElement>(weatherUrl);

                if (weatherResponse.TryGetProperty("current_weather", out var current))
                {
                    var temp = current.GetProperty("temperature").GetDouble();
                    var weatherCode = current.GetProperty("weathercode").GetInt32();

                    var (icon, desc) = MapWeatherCode(weatherCode);

                    return Ok(new
                    {
                        temp = Math.Round(temp),
                        city = cityName,
                        country = countryCode?.ToUpper() ?? "CH",
                        icon = icon,
                        desc = desc
                    });
                }
            }
            
            return Ok(new { temp = 12, city = cityName, country = "CH", icon = "cloud", desc = "Overcast" });
        }
        catch (Exception)
        {
            return Ok(new { temp = 5, city = "Zermatt", country = "CH", icon = "ac_unit", desc = "Snow" });
        }
    }

    private (string icon, string desc) MapWeatherCode(int code)
    {
        return code switch
        {
            0 => ("wb_sunny", "Clear Sky"),
            1 or 2 or 3 => ("cloud_queue", "Partly Cloudy"),
            45 or 48 => ("foggy", "Foggy"),
            51 or 53 or 55 => ("rainy", "Drizzle"),
            61 or 63 or 65 => ("rainy", "Rain"),
            71 or 73 or 75 => ("ac_unit", "Snowing"),
            77 => ("ac_unit", "Snow Grains"),
            80 or 81 or 82 => ("rainy", "Rain Showers"),
            85 or 86 => ("ac_unit", "Snow Showers"),
            95 or 96 or 99 => ("thunderstorm", "Thunderstorm"),
            _ => ("cloud", "Cloudy")
        };
    }
}
