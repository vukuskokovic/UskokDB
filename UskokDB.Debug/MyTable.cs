using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;
[TableName("testTable")]
public class ParkingSession
{
    [Key]
    public Guid SessionId {get;set;}
    [Key]
    public string Name { get; set; }
    public int Age { get; set; }
}

