using System.Data.Common;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;

[TableName("tedawdwad")]
public class ClockTable
{
    public Guid Test { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public DateTime? Date { get; set; }
}