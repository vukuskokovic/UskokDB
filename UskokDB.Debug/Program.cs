using System.Text.Json;
using UskokDB;
using UskokDB.Debug;
using UskokDB.Debug.Tables;

UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
await using var dbContext = new ShopDbContext();
await dbContext.InitDb();

var userId = Guid.Parse("057273f6-e0e7-4f75-ba7b-54e964918edc");
var targetedUserId = Guid.Parse("057273f6-e0e7-4f75-ba7b-54e964918edc");
var now = DateTime.MaxValue;
await dbContext.UserRelations.UpdateAsync(() => new UserRelation()
{
    Initiator = userId,
    Receiver = targetedUserId,
    UpdatedAt = now,
    Status = "status"
}, ur => ur.RelationId == targetedUserId, true);