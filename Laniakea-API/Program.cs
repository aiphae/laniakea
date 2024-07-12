using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Coursework.Clients;
using Coursework.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WeatherClient>();
builder.Services.AddSingleton<SolarSysObjectsClient>();
builder.Services.AddSingleton<SunspotsClient>();
builder.Services.AddSingleton<MoonPhaseClient>();
builder.Services.AddSingleton<ApodClient>();

var credentials = new BasicAWSCredentials(Constants.DbAccessKey, Constants.DbSecret);
var config = new AmazonDynamoDBConfig()
{
    RegionEndpoint = RegionEndpoint.EUNorth1
};
var client = new AmazonDynamoDBClient(credentials, config);
builder.Services.AddSingleton<IAmazonDynamoDB>(client);
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddSingleton<IDynamoDbClient, DynamoDbClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
