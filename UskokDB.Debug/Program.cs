using MySqlConnector;
using System.Text.Json;
using pikac;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();
await context.ExecuteAsync(context.GetTableCreationString());
var doesExist = await context.TestTable.ExistsAsync(r => r.Id == "testId");
Console.WriteLine(doesExist);
await context.TestTable.InsertAsync(new Test(){
    Id = "testId"
});
doesExist = await context.TestTable.ExistsAsync(r => r.Id == "testId");
Console.WriteLine(doesExist);
Console.WriteLine("Test");


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