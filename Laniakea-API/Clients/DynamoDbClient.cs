using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Coursework.Clients;

using Constants;
using Models;

public class DynamoDbClient : IDynamoDbClient
{
    public string _tableName;
    private readonly IAmazonDynamoDB _dynamoDb;

    public DynamoDbClient(IAmazonDynamoDB dynamoDb)
    {
        _tableName = Constants.DbName;
        _dynamoDb = dynamoDb;
    }

    public async Task<CoordinatesRequest> GetCoordinatesById(string id)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } }
            }
        };

        var response = await _dynamoDb.GetItemAsync(request);

        var latitude = double.Parse(response.Item["latitude"].N);
        var longitude = double.Parse(response.Item["longitude"].N);

        return new CoordinatesRequest { Id = id, Latitude = latitude, Longitude = longitude };
    }

    public async Task SetCoordinatesById(string id, double latitude, double longitude)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "latitude", new AttributeValue { N = latitude.ToString() } },
                { "longitude", new AttributeValue { N = longitude.ToString() } }
            }
        };

        await _dynamoDb.PutItemAsync(request);
    }

    public async Task ClearCoordinatesById(string id)
    {
        var request = new DeleteItemRequest()
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
            }
        };

        await _dynamoDb.DeleteItemAsync(request);
    }
}