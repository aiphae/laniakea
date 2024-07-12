using Microsoft.AspNetCore.Mvc;

namespace Coursework.Controllers;
using Clients;

[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    private readonly ILogger<WeatherController> _logger;
    private readonly WeatherClient _weatherClient;

    public WeatherController(ILogger<WeatherController> logger, WeatherClient weatherClient)
    {
        _logger = logger;
        _weatherClient = weatherClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetWeather([FromQuery] double lon, double lat)
    {
        var weatherBytes = await _weatherClient.GetWeather(lon, lat);
        return File(weatherBytes, "image/png");
    }
}
