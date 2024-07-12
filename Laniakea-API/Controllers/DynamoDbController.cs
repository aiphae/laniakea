using Microsoft.AspNetCore.Mvc;

namespace Coursework.Controllers;

using Clients;
using Models;

[ApiController]
[Route("[controller]")]
public class DynamoDbController : ControllerBase
{
    private readonly IDynamoDbClient _dynamoDbClient;

    public DynamoDbController(IDynamoDbClient dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetCoordinates(string id)
    {
        var coordinates = await _dynamoDbClient.GetCoordinatesById(id);
        return Ok(coordinates);
    }

    [HttpPost]
    public async Task<IActionResult> SetCoordinates([FromBody] CoordinatesRequest request)
    {
        await _dynamoDbClient.SetCoordinatesById(request.Id, request.Latitude, request.Longitude);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCoordinates(string id)
    {
        await _dynamoDbClient.ClearCoordinatesById(id);
        return Ok();
    }
}