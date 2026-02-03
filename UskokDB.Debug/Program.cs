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
using UskokDB.Query.QueryFunctions;

UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
var dbContext = new ShopDbContext();
var t = TimeSpan.Zero;
Guid d = Guid.NewGuid();
var list = new List<bool>() { false, true };
var stuff = dbContext.Clock.GroupBy((c) => c.TestBool).GroupBy((c) => c.Date).Where(c => Sql.In(c.TestBool, list)).Limit(3).QueryAsync(printToConsole:true);


public class PlayerVehicleRead
{
    public Guid PlayerId { get; set; }
    public Guid SkinId { get; set; }
}

public class ShopDbContext : DbContext
{
    public DbTable<ClockTable> Clock { get; }
    public ShopDbContext() : base(() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        Clock = new DbTable<ClockTable>(this);
    }
}