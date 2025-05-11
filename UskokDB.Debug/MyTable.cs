using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;
[TableName("testTable")]
public class ParkingSession
{
    [Key]
    public Guid SessionId {get;set;}
    public DateTime PairedAt {get;set;}
    [Unique]
    public Guid CreatorId {get;set;}
    [Unique]
    public Guid FinderId {get;set;}
    public double SpotLatitude {get;set;}
    public double SpotLongitude {get;set;}
    public string Address {get;set;} = null!;
    public double Radius {get;set;}
    public Guid? TestGuid { get; set; }
    public DateTime? FinishedAt {get;set;}
}

