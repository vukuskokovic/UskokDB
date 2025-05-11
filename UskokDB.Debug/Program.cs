using MySqlConnector;
using System.Text.Json;
using pikac;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();

context.TestTable.Update((t) => new ParkingSession()
{
    Radius = t.Radius + 3,
    SpotLatitude = t.SpotLatitude + 3,
}, (t) => t.Radius > 3 && t.SpotLatitude < 2);
context.TestTable.Insert(new ParkingSession(){});
Console.WriteLine(context.BuildQueue());


public class ShopDbContext : DbContext
{
    public DbTable<ParkingSession> TestTable { get; }
    public ShopDbContext() : base(DbType.MySQL,() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        TestTable = new DbTable<ParkingSession>(this);
    }
}