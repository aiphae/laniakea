using Microsoft.AspNetCore.Mvc;

namespace Coursework.Controllers;
using Clients;

[ApiController]
[Route("[controller]")]
public class MoonPhaseController : ControllerBase
{
    private readonly ILogger<MoonPhaseController> _logger;
    private readonly MoonPhaseClient _moonPhaseClient;

    public MoonPhaseController(ILogger<MoonPhaseController> logger, MoonPhaseClient moonPhaseClient)
    {
        _logger = logger;
        _moonPhaseClient = moonPhaseClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetSunspots(string date)
    {
        var moonPhase = await _moonPhaseClient.GetMoonPhase(45, 45, date);
        return Ok(moonPhase);
    }
}