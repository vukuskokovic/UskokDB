using System.Data.Common;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;

[TableName("tedawdwad312")]
public class ClockTable : TimedModelItem
{
    public Guid Test { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public DateTime Date { get; set; }
    public string Str { get; set; }
    public bool TestBool { get; set; }
    public int TestInt { get; set; }
}

public abstract class TimedModelItem
{
    public DateTime Kurac { get; set; }
}