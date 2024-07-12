namespace Coursework.Clients;
using Constants;

public class WeatherClient
{
    private readonly HttpClient _client;
    private static string _address;

    public WeatherClient()
    {
        _address = Constants.WeatherUrl;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_address);
    }

    public async Task<byte[]> GetWeather(double lon, double lat)
    {
        var response = await _client.GetAsync(
            $"?lon={lon}&lat={lat}&ac=0&lang=en&unit=metric&output=internal&tzshift=0");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}