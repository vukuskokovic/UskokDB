using MySqlConnector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using UskokDB;
using UskokDB.MySql;

var connection = new MySqlConnection("server=localhost;username=root;database=test");
Stopwatch watch = new Stopwatch();
watch.Start();
var t1 = new MySqlTable<Person>("pikara");
var t2 = new MySqlTable<Person>("dikara");
var t3 = new MySqlTable<Person>("likara");
var t4 = new MySqlTable<Person>("lokara");
await t1.CreateIfNotExistAsync(connection);
await t2.CreateIfNotExistAsync(connection);
await t3.CreateIfNotExistAsync(connection);
await t4.CreateIfNotExistAsync(connection);
var all = await connection.QueryAsync<Person>("SELECT * FROM pikara");
watch.Stop();
Console.WriteLine(watch.ElapsedMilliseconds);
Console.WriteLine(JsonSerializer.Serialize(all));

class Person
{
    [Column("name")]
    public string Name { get; set; }
    [Column("lastname")]
    [MaxLength(100)]
    [ColumnNotNull]
    [Key]
    public string LastName { get; set; }

    public EnumTest Test {get; set; }

    [Column("id")]
    public Guid Id { get; set; }
}

public enum EnumTest : ushort
{
    Kitac,
    pitac
}