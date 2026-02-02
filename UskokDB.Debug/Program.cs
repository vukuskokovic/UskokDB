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
UskokDb.InitLinqMethodRegistry();
var dbContext = new ShopDbContext();
var t = TimeSpan.Zero;
Guid d = Guid.NewGuid();
var stuff = dbContext.Clock.Query().Select<ClockTable, ClockTable>((clock) =>
    new ClockTable()
    {
        Date = clock.Date,
        End = clock.End,
        Kurac = clock.Kurac + clock.End,
        Start = clock.Start < t? clock.Start : t,
        Test = d,
        Str = clock.Str.Substring(0, 1),
        TestBool = clock.Str.Substring(0, 1) == null,
        TestInt = 3
    }, true);


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