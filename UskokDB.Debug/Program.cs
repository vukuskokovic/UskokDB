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
var arr = new bool[] { true, false };
var stuff = dbContext.Clock.Where
    (c => Sql.Exists(dbContext.Clock.Where((y) => y.TestBool == c.TestBool)))
    .QueryAsync(printToConsole:true);


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