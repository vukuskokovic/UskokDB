using System.Diagnostics;
using System.Text.Json;
using UskokDB;
using UskokDB.Debug;
using UskokDB.Debug.Tables;

UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
await using var dbContext = new ShopDbContext();
//await dbContext.InitDb();

User u = new User
{
    Birthday = DateTime.MaxValue,
    Email = "email",
    JoinedAt = DateTime.MaxValue,
    Name = "name",
    UserId = Guid.NewGuid(),
    Username = "username"
};
