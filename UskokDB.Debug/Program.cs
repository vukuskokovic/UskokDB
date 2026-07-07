using System.Text.Json;
using UskokDB;
using UskokDB.Debug;
using UskokDB.Debug.Tables;
using UskokDB.Query.QueryFunctions;


UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
await using var dbContext = new ShopDbContext();
await dbContext.InitDb();


Console.WriteLine(JsonSerializer.Serialize(res));