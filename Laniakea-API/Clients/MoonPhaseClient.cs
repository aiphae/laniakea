using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coursework.Clients;
using Constants;

public class MoonPhaseClient
{
    private readonly HttpClient _client;
    private static string _address;
    private static string _authKey;

    public MoonPhaseClient()
    {
        _address = Constants.AstronomyApiMoonPhaseUrl;
        _authKey = Constants.AstronomyApiAuth;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_address);
        _client.DefaultRequestHeaders.Add("Authorization", $"Basic {_authKey}");
    }

    public async Task<string> GetMoonPhase(double lon, double lat, string date)
    {
        var requestParameters = new
        {
            format = "png",
            style = new
            {
                moonStyle = "default",
                backgroundStyle = "stars",
                backgroundColor = "black",
                headingColor = "white",
                textColor = "white"
            },
            observer = new
            {
                latitude = lat,
                longitude = lon,
                date = date
            },
            view = new
            {
                type = "portrait-simple",
                orientation = "north-up"
            }
        };
        
        var json = JsonConvert.SerializeObject(requestParameters);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync(_address, content);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JObject.Parse(responseString);

        return responseObject["data"]?["imageUrl"]?.ToString();
    }
}