using MySqlConnector;
using System.Text.Json;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();

Console.WriteLine(await context.TestTable.DeleteAsync(null));


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
    [MaxLength(20), ColumnNotNull, Column("Name")]
    public string? Name { get; set; }
}

public record TestBranch(string Test);
public record TestRecord(TestBranch Branch);
public class TestStatic
{
    public static string Kurac = "KURAC";
}