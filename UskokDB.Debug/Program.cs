using MySqlConnector;
using System.Text.Json;
using pikac;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();
context.TestTable.Update(() => new MyTable(){Whatever = "3"}, where: t => t.Whatever == "Test");
await context.ExecuteQueue();
Console.WriteLine(context.BuildQueue());


public class ShopDbContext : DbContext
{
    public DbTable<MyTable> TestTable { get; }
    public ShopDbContext() : base(DbType.MySQL,() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        TestTable = new DbTable<MyTable>(this);
    }
}