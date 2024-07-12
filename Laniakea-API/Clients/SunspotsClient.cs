namespace Coursework.Clients;
using Constants;

public class SunspotsClient
{
    private readonly HttpClient _client;
    private static string _address;

    public SunspotsClient()
    {
        _address = Constants.SunspotsUrl;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_address);
    }

    public async Task<byte[]> GetSunspots()
    {
        var response = await _client.GetAsync("");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}