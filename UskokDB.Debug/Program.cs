using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using MySqlConnector;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using pikac;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.Query;

UskokDb.SetSqlDialect(SqlDialect.MySql);
var dbContext = new ShopDbContext();
await dbContext.InitDb();

public class PlayerVehicleRead
{
    public Guid PlayerId { get; set; }
    public Guid SkinId { get; set; }
}

public class ShopDbContext : DbContext
{
    public DbTable<Skin> Skins { get; }
    public DbTable<Player> Players { get; }
    public DbTable<Vehicle> Vehicles { get; }
    
    public ShopDbContext() : base(() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        Skins = new DbTable<Skin>(this);
        Players = new DbTable<Player>(this);
        Vehicles = new DbTable<Vehicle>(this);
    }
}