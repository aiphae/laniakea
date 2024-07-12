using Microsoft.AspNetCore.Mvc;

namespace Coursework.Controllers;
using Clients;

[ApiController]
[Route("[controller]")]
public class SunspotsController : ControllerBase
{
    private readonly ILogger<SunspotsController> _logger;
    private readonly SunspotsClient _sunspotsClient;

    public SunspotsController(ILogger<SunspotsController> logger, SunspotsClient sunspotsClient)
    {
        _logger = logger;
        _sunspotsClient = sunspotsClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetSunspots()
    {
        var sunspotsBytes = await _sunspotsClient.GetSunspots();
        return File(sunspotsBytes, "image/png");
    }
}