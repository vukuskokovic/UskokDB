using MySqlConnector;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UskokDB;
using UskokDB.MySql;


var table = new MySqlTable<TestSql>("testsql2");
var connection = new MySqlConnection("Server=localhost;User ID=root;Database=test");
await table.CreateIfNotExistAsync(connection);
await table.InsertAsync(connection, new TestSql()
{
    Text = DateTime.Now
});
Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(await connection.QueryAsync("SELECT * FROM testsql2")));
class TestSql
{
    public DateTime Text { get; set; }
}