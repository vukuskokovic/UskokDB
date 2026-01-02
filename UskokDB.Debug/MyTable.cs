using System.Data.Common;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;

public class Player
{
    [Key]
    public Guid PlayerId { get; set; }
    
    public string Name { get; set; }
    [ForeignKey<Skin>("skinId")]
    public Guid SkinId { get; set; }
}

public class Vehicle
{
    [Key]
    public Guid VehicleId { get; set; }
    public string VehicleCode { get; set; }
    [ForeignKey<Player>("playerId")]
    public Guid PlayerId { get; set; }
}

public class Skin
{
    [Key]
    public Guid SkinId { get; set; }
    public string SkinName { get; set; }
    public int Color { get; set; }
}