using System.Diagnostics;
using System.Text.Json;
using UskokDB;
using UskokDB.Debug;
using UskokDB.Debug.Tables;
using UskokDB.Query.QueryFunctions;

UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
await using var dbContext = new ShopDbContext();
await dbContext.InitDb();

var list = await dbContext.NullableTest.QueryAllAsync();

Console.WriteLine(JsonSerializer.Serialize(list));