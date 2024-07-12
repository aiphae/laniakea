using Microsoft.AspNetCore.Mvc;
using Coursework.Clients;

namespace Coursework.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApodController : ControllerBase
    {
        private readonly ILogger<ApodController> _logger;
        private readonly ApodClient _apodClient;

        public ApodController(ILogger<ApodController> logger, ApodClient apodClient)
        {
            _logger = logger;
            _apodClient = apodClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetApod(string date)
        {
            var response = await _apodClient.GetApod(date);
            return Ok(response);
        }
    }
}