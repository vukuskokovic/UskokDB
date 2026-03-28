using System.Diagnostics;
using System.Text.Json;
using UskokDB;
using UskokDB.Debug;
using UskokDB.Debug.Tables;

UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
await using var dbContext = new ShopDbContext();
//await dbContext.InitDb();

var count = await dbContext.QuerySingleAsync<Query>("SELECT COUNT(*) from users");
Console.WriteLine(count?.Count);

class Query
{
    public short Count { get; set; }
}