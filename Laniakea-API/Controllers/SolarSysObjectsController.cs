using Microsoft.AspNetCore.Mvc;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.ImageSharp;
using System.Globalization;

namespace Coursework.Controllers;
using Clients;
using Models;

[ApiController]
[Route("[controller]")]
public class SolarSysObjectsController : Microsoft.AspNetCore.Mvc.ControllerBase
{
    private readonly ILogger<SolarSysObjectsController> _logger;
    private readonly SolarSysObjectsClient _solarSysObjectsClient;

    public SolarSysObjectsController(ILogger<SolarSysObjectsController> logger, SolarSysObjectsClient solarSysObjectsClient)
    {
        _logger = logger;
        _solarSysObjectsClient = solarSysObjectsClient;
    }

    [HttpGet]
public async Task<SolarSysObjectResponse> GetObjectData([FromQuery] double lat, double lon, double elev, 
    string date, string time, string body)
{
    var solarSysObjectData = await _solarSysObjectsClient.GetObjectData(lat, lon, elev, date, date, time, body);
    var initialRow = solarSysObjectData.data.table.rows[0];
    var initialCell = initialRow.cells[0];

    var result = new SolarSysObjectResponse
    {
        Name = initialRow.entry.name,
        Altitude = initialCell.position.horizontal.altitude.@string,
        IsVisible = double.Parse(initialCell.position.horizontal.altitude.degrees, CultureInfo.InvariantCulture) > 0.0,
        Azimuth = initialCell.position.horizontal.azimuth.@string,
        Constellation = initialCell.position.constellation.name,
        Magnitude = initialCell.extraInfo.magnitude,
        HighestAltitudeDegrees = 0
    };

    TimeSpan lowerBound = new TimeSpan(6, 0, 0);
    TimeSpan upperBound = new TimeSpan(20, 0, 0);

    var providedTime = DateTime.ParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture);
    providedTime = providedTime.Date.AddHours(20);
    
    var tasks = new List<Task>();
    var lockObject = new object();

    for (var tempTime = providedTime; tempTime.TimeOfDay < lowerBound || tempTime.TimeOfDay >= upperBound; 
         tempTime = tempTime.AddMinutes(20)) 
    {
        var tempTimeCopy = tempTime;
        tasks.Add(Task.Run(async () =>
        {
            var temp = await _solarSysObjectsClient.GetObjectData(lat, lon, elev, date, date,
                tempTimeCopy.ToString("HH:mm:ss"), body);
            var tempAltitude = double.Parse(temp.data.table.rows[0].cells[0].position.horizontal.altitude.degrees,
                CultureInfo.InvariantCulture);

            lock (lockObject)
            {
                if (tempAltitude > result.HighestAltitudeDegrees)
                {
                    result.HighestAltitude = temp.data.table.rows[0].cells[0].position.horizontal.altitude.@string;
                    result.HighestAltitudeTime = tempTimeCopy.ToString("HH:mm:ss");
                    result.HighestAltitudeDegrees = tempAltitude;
                }
            }
        }));
    }

    await Task.WhenAll(tasks);

    return result;
}

[HttpGet("graph")]
public async Task<IActionResult> GetPlanetGraph([FromQuery] double lat, double lon, double elev, string date,
    string time, string body, bool extended = false)
{
    var dataPoints = new List<(DateTime Time, double Altitude)>();
    var tasks = new List<Task>();
    var lockObject = new object();

    if (extended)
    {
        for (int i = 0; i < 7; i++)
        {
            var currentDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(i * 30);
            var dateString = currentDate.ToString("yyyy-MM-dd");

            var response = await GetObjectData(lat, lon, elev, dateString, "20:00:00", body);
            var altitude = response.HighestAltitudeDegrees;
            lock (lockObject)
            {
                dataPoints.Add((currentDate, altitude));
            }
        }
    }
    else
    {
        DateTime startTime, endTime;
        TimeSpan lowerBound = new TimeSpan(6, 0, 0);
        var currentTime = DateTime.ParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture);

        if (currentTime.TimeOfDay > lowerBound)
        {
            startTime = DateTime.Today.AddHours(20);
            endTime = startTime.AddHours(8);
        }
        else
        {
            startTime = DateTime.Today.AddHours(-4);
            endTime = startTime.AddHours(10);
        }

        var timeSpan = new TimeSpan(0, 20, 0);
        for (var t = startTime; t <= endTime; t += timeSpan)
        {
            var tempTime = t;
            tasks.Add(Task.Run(async () =>
            {
                var response = await _solarSysObjectsClient.GetObjectData(lat, lon, elev, date, date,
                    tempTime.ToString("HH:mm:ss"), body);
                var altitude = double.Parse(response.data.table.rows[0].cells[0].position.horizontal.altitude.degrees,
                    CultureInfo.InvariantCulture);
                lock (lockObject)
                {
                    dataPoints.Add((tempTime, altitude));
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    dataPoints = dataPoints.OrderBy(dp => dp.Time).ToList();

    var plotModel = new PlotModel { Title = extended ? $"{body.ToUpperInvariant()} - 6 Months" : body.ToUpperInvariant() };

    plotModel.DefaultFont = "DejaVu Sans";

    var dateTimeAxis = new DateTimeAxis
    {
        StringFormat = extended ? "MMM dd" : "HH:mm",
        Title = extended ? "Date" : "Time",
        IntervalLength = 30,
        MinorIntervalType = extended ? DateTimeIntervalType.Days : DateTimeIntervalType.Minutes,
        MajorGridlineStyle = LineStyle.Solid,
        FontSize = 10, 
        TitleFontSize = 14
    };

    plotModel.Axes.Add(dateTimeAxis);

    var linearAxis = new LinearAxis
    {
        Title = "Altitude",
        IntervalLength = 20,
        MajorGridlineStyle = LineStyle.Solid,
        MinorGridlineStyle = LineStyle.Dot,
        FontSize = 12,
        TitleFontSize = 14
    };

    plotModel.Axes.Add(linearAxis);

    var lineSeries = new LineSeries
    {
        MarkerType = MarkerType.Circle,
        MarkerSize = 5,
        MarkerStroke = OxyColors.White
    };

    foreach (var point in dataPoints)
    {
        lineSeries.Points.Add(DateTimeAxis.CreateDataPoint(point.Time, point.Altitude));
    }

    plotModel.Series.Add(lineSeries);

    if (extended)
    {
        var polynomialCoefficients = PolynomialFit(dataPoints.Select(p => p.Time.ToOADate()).ToArray(),
            dataPoints.Select(p => p.Altitude).ToArray(), 2);
        var polynomialSeries = new LineSeries
        {
            Color = OxyColors.Red,
        };

        for (var t = dataPoints.MinBy(dp => dp.Time).Time;
             t <= dataPoints.MaxBy(dp => dp.Time).Time;
             t = t.AddDays(1))
        {
            var x = t.ToOADate();
            var y = EvaluatePolynomial(polynomialCoefficients, x);
            polynomialSeries.Points.Add(DateTimeAxis.CreateDataPoint(t, y));
        }

        plotModel.Series.Add(polynomialSeries);
    }

    var stream = new MemoryStream();
    var pngExporter = new PngExporter(600, 400);
    pngExporter.Export(plotModel, stream);
    stream.Seek(0, SeekOrigin.Begin);

    return File(stream, "image/png");
}

    private double[] PolynomialFit(double[] x, double[] y, int degree)
    {
        var vandMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(x.Length, degree + 1);
        for (int i = 0; i < x.Length; i++)
        {
            for (int j = 0; j <= degree; j++)
            {
                vandMatrix[i, j] = Math.Pow(x[i], j);
            }
        }

        var yVec = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(y);
        var pVec = vandMatrix.QR().Solve(yVec);
        return pVec.ToArray();
    }

    private double EvaluatePolynomial(double[] coefficients, double x)
    {
        double result = 0;
        for (int i = 0; i < coefficients.Length; i++)
        {
            result += coefficients[i] * Math.Pow(x, i);
        }

        return result;
    }
}
