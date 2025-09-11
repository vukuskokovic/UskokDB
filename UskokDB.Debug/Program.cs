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
ShopDbContext context = new ShopDbContext();
await context.ExecuteTableCreationCommand();

var isPhoneUsed = await context.Users.ExistsAsync(x =>
    x.PhoneRegionalCode == "" && x.PhoneNumber == "");
//Console.WriteLine(JsonSerializer.Serialize(list));


/*BenchmarkRunner.Run<BenchmarkTests>();
[MemoryDiagnoser]
public class BenchmarkTests
{
    public ShopDbContext Context = new();
    public BenchmarkTests()
    {
        DbIOOptions.UseJsonForUnknownClassesAndStructs = true;
    }



    [Benchmark]
    public Task Fast()
    {
        return Context.Table.Where("1=1").QueryAsync();
    }
}*/
public class ShopDbContext : DbContext
{
    public DbTable<PKeyTable> PkeyTable { get; }
    public DbTable<User> Users { get; }
    public ShopDbContext() : base(() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        PkeyTable = new DbTable<PKeyTable>(this);
        Users = new DbTable<User>(this);
    }
}