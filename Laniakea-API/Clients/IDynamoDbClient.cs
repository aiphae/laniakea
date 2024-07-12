namespace Coursework.Clients;
using Models;

public interface IDynamoDbClient
{
    public Task<CoordinatesRequest> GetCoordinatesById(string id);
    public Task SetCoordinatesById(string id, double latitude, double longitude);
    public Task ClearCoordinatesById(string id);
}