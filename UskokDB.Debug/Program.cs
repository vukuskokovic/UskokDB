using MySqlConnector;
using System.Text.Json;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();

TestRecord rec = new TestRecord(new TestBranch("312"));
string kurac = "312";
Console.WriteLine(context.TestTable.Where(x => x.Name == kurac || x.Name == rec.Branch.Test || x.Name == null).CompileQuery());

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

public record TestBranch(string Test);
public record TestRecord(TestBranch Branch);
public class TestStatic
{
    public static string Kurac = "KURAC";
}