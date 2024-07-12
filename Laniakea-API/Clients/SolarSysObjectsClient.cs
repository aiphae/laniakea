namespace Coursework.Clients;
using Newtonsoft.Json;
using Constants;
using Models;

public class SolarSysObjectsClient
{
    private static HttpClient _client;
    private static string _address;
    private static string _authKey;

    public SolarSysObjectsClient()
    {
        _address = Constants.AstronomyApiBodiesPositionsUrl;
        _authKey = Constants.AstronomyApiAuth;

        _client = new HttpClient();
        _client.BaseAddress = new Uri(_address);
        _client.DefaultRequestHeaders.Add("Authorization", $"Basic {_authKey}");
    }
    
    public async Task<SolarSysObjectData> GetObjectData(double lat, double lon, double elev, string from, string to, string time, string body)
    {
        var address = Constants.AstronomyApiBodiesPositionsUrl.Replace(":body", body);
        var url = $"{address}?latitude={lat}&longitude={lon}&elevation={elev}" +
                  $"&from_date={from}&to_date={to}&time={time}";
        
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<SolarSysObjectData>(content);

        return result;
    }
}
