using MySqlConnector;
using System.Text.Json;
using pikac;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();
context.TestTable.AppendDeleteByKey(
    new ParkingSession()
    {
        SessionId = Guid.NewGuid(),
        Age = 3,
        Name = "Name"
    });
Console.WriteLine(context.BuildQueue());


public class ShopDbContext : DbContext
{
    public DbTable<ParkingSession> TestTable { get; }
    public ShopDbContext() : base(DbType.MySQL,() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        TestTable = new DbTable<ParkingSession>(this);
    }
}