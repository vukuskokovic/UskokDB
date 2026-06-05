using System.Text.Json;
using UskokDB;
using UskokDB.Debug;
using UskokDB.Debug.Tables;


UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
await using var dbContext = new ShopDbContext();
await dbContext.InitDb();

var res = await dbContext.Users.Where(x => x.Email == "312").SelectOneAsync<User, bool?>((u) => u.Xd, true);

Console.WriteLine(JsonSerializer.Serialize(res));