using System.Data.Common;
using System.Diagnostics;
using MySqlConnector;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using pikac;
using UskokDB;
using UskokDB.Attributes;

UskokDb.SetSqlDialect(SqlDialect.MySql);
var dbContext = new ShopDbContext();
await dbContext.ExecuteTableCreationCommand();
var allUsers = await dbContext.Users.QueryAllAsync();
Console.WriteLine(JsonSerializer.Serialize(allUsers));

var command = await dbContext.Users.QueryWhere("userId IN @UserIds", new
{
    UserIds = (new [] {Guid.Parse("0d9315ad-40a7-4a34-b1fc-72e2d03a12ad")}).ToList()
});
Console.WriteLine(JsonSerializer.Serialize(command));


public class ShopDbContext : DbContext
{
    public DbTable<User> Users { get; }
    public ShopDbContext() : base(() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        Users = new DbTable<User>(this);
    }
}