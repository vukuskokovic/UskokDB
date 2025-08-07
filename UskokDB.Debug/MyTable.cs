using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;
[TableName("testTableee")]
public class ParkingSession
{
    [Key]
    public Guid SessionId {get;set;}
    public string Name { get; set; }
    public int Age { get; set; }
}

