using MySqlConnector;
using System.Text.Json;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();
await foreach(var item in context.TestTable.Where(x => x.Id == 3).QueryAsyncEnumerable())
{

}
Console.WriteLine(context.GetTableCreationString());
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
    [MaxLength(20), ColumnNotNull]
    public string? Name { get; set; }
}
