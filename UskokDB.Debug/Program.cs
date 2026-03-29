using System.Diagnostics;
using System.Text.Json;
using UskokDB;
using UskokDB.Debug;
using UskokDB.Debug.Tables;
using UskokDB.Query.QueryFunctions;

UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
await using var dbContext = new ShopDbContext();
//await dbContext.InitDb();

var ids = new Guid[]
{
    Guid.NewGuid(),
    Guid.NewGuid(),
};
var count = await dbContext.Users.Where(x => Sql.In(x.UserId, ids)).QueryAsync();
Console.WriteLine(count.Count);

class Query
{
    public short Count { get; set; }
}