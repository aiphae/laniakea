namespace Coursework.Constants;

public class Constants
{
    public const string WeatherUrl = "";
    
    private const string AstronomyApiAppId = "";
    
    private const string AstronomyApiAppSecret = "";
    
    public static readonly string AstronomyApiAuth = 
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{AstronomyApiAppId}:{AstronomyApiAppSecret}"));
    
    public const string AstronomyApiBodiesPositionsUrl = "";
    
    public const string AstronomyApiMoonPhaseUrl = "";
    
    public const string SunspotsUrl = "g";
    
    public const string DbAccessKey = "E";

    public const string DbSecret = "";

    public const string DbName = "t";

    public const string NasaApiKey = "";

    public const string NasaApodUrl = "";
}
