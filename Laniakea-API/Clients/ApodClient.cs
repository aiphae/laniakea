using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Linq;

namespace Coursework.Clients;
using Constants;

public class ApodClient
{
    private readonly HttpClient _client;
    private static string _address;
    private static string _authKey;

    public ApodClient()
    {
        _address = Constants.NasaApodUrl;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_address);
    }

    public async Task<string> GetApod(string date)
    {
        var response = await _client.GetAsync($"?api_key={Constants.NasaApiKey}&" +
                                              $"date={date}&concept_tags=False&hd=True");
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JObject.Parse(responseString);

        return responseObject.ToString();
    }
}