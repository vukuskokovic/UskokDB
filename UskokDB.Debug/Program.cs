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
Console.WriteLine(context.GetTableCreationString());
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
    public DbTable<FKeyTable> FkeyTable { get; }
    public ShopDbContext() : base(() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        PkeyTable = new DbTable<PKeyTable>(this);
        FkeyTable = new DbTable<FKeyTable>(this);
    }
}