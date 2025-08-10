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
DbIOOptions.UseJsonForUnknownClassesAndStructs = true;
ShopDbContext dbContext = new ShopDbContext();
var cmd = dbContext.Table.BuildDeleteCommand((t) => t.FloatValue > 3 && (t.LongValue < 3 || t.CharValue == 'A'));
cmd.Disposed += (_, _) =>
{
    Console.WriteLine("dispoed");
};

await dbContext.ExecuteAsync(cmd);
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
    public DbTable<TypesTable> Table { get; }
    public ShopDbContext() : base(() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        Table = new DbTable<TypesTable>(this);
    }
}