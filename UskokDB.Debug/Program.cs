using System.Text.Json;
using UskokDB;
using UskokDB.Debug;
using UskokDB.Debug.Tables;


UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
await using var dbContext = new ShopDbContext();
await dbContext.InitDb();

var res = await dbContext.Users.Select((User u) => u.Name, true);

Console.WriteLine(JsonSerializer.Serialize(res));