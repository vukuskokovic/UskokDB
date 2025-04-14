using MySqlConnector;
using System.Text.Json;
using pikac;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();
await context.ExecuteAsync(context.GetTableCreationString());
for (var i = 0; i < 10; i++)
{
    await context.TestTable.Where(r => r.Id == "testId").QueryAsync();
}

public class ShopDbContext : DbContext
{
    public DbTable<Test> TestTable { get; }
    public ShopDbContext() : base(DbType.MySQL,() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        TestTable = new DbTable<Test>(this);
    }
}

[TableNameAttribute("testTable")]
public class Test
{
    [Key, MaxLength(10)]
    public string Id {get;set;} = null!;
}