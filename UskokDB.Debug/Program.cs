using MySqlConnector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UskokDB;
using UskokDB.MySql;

var table = new MySqlTable<TestTable>("table1");
var connection = new MySqlConnection("Server=localhost;User ID=root;Database=test");
await table.CreateIfNotExistAsync(connection);
Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(await table.GetByKeyAsync(connection, "312")));

class TestTable
{
    [Key, MaxLength(20)] 
    public string Text { get; set; }
}