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
var stuff = dbContext.Clock.Query().OrderBy<ClockTable>((c) => c.Date).Select<ClockTable, ClockTable>((c) =>
    new ClockTable()
    {
        Date = c.Date,
        End = c.End,
        Kurac = c.Kurac,
        Start = c.Start,
        Test = c.Test
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