using MySqlConnector;
using System.Text.Json;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();
var builder = context.TestTable.Where(t => t.Id > 2).OrderByDesc("id");
Console.WriteLine(builder.CompileQuery());
var list = await builder.QueryAsync();
Console.WriteLine(JsonSerializer.Serialize(list));
public class ShopDbContext : DbContext
{
    public DbTable<Test> TestTable { get; }
    public ShopDbContext() : base(DbType.MySQL,() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        TestTable = new DbTable<Test>(this);
    }
}

[TableName("test")]
public class Test
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}
