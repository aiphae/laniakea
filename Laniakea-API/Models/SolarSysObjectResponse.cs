namespace Coursework.Models;

public class SolarSysObjectResponse
{
    public string Name { get; set; }
    public bool IsVisible { get; set; }
    public string? Altitude { get; set; }
    public string? Azimuth { get; set; }
    public string Constellation { get; set; }
    public double Magnitude { get; set; }
    public string? HighestAltitude { get; set; }
    public double HighestAltitudeDegrees { get; set; }
    public string? HighestAltitudeTime { get; set; }
}