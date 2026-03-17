using System.Text.Json;
using UskokDB;
using UskokDB.Debug;
using UskokDB.Debug.Tables;

UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
await using var dbContext = new ShopDbContext();
await dbContext.InitDb();

var users = await dbContext.Users.Where(u => u.Name == "Luka").QueryAsync();

Console.WriteLine(JsonSerializer.Serialize(users));