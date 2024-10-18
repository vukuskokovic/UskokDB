using MySqlConnector;
using System.Text.Json;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();
await context.TestTable.InsertAsync(new Test()
{
    Id = 1,
    Name = "test"
});
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
