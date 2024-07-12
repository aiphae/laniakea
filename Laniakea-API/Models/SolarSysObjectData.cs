namespace Coursework.Models;

public class SolarSysObjectData
{
    public Data data { get; set; }
}

public class Altitude
{
    public string degrees { get; set; }
    public string @string { get; set; }
}

public class Azimuth
{
    public string @string { get; set; }
}

public class Cell
{
    public Position position { get; set; }
    public ExtraInfo extraInfo { get; set; }
}

public class Constellation
{
    public string name { get; set; }
}

public class Data
{
    public Table table { get; set; }
}

public class Entry
{
    public string name { get; set; }
}

public class ExtraInfo
{
    public double magnitude { get; set; }
}

public class Horizontal
{
    public Altitude altitude { get; set; }
    public Azimuth azimuth { get; set; }
}

public class Position
{
    public Horizontal horizontal { get; set; }
    public Constellation constellation { get; set; }
}

public class Row
{
    public Entry entry { get; set; }
    public List<Cell> cells { get; set; }
}

public class Table
{
    public List<Row> rows { get; set; }
}
