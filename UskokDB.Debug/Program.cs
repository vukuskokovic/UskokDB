using MySqlConnector;
using System.Text.Json;
using UskokDB;
using UskokDB.MySql;
var connection = new MySqlConnection("Server=localhost;User ID=root;Database=test");
await Person.CreateIfNotExistAsync(connection);
FilterBuilder builder = Person.CreateFilterBuilder();

//builder.OrderBy(true, "extension", "field2");
//builder.GroupBy("field3", "field4");
//builder.AddOr("testfuield", new FilterOperand(3, "<"), new FilterOperand(DateTime.Now, ">"), new FilterOperand(DateTime.Now, "is"));
builder.AddAnd("age", new FilterOperand(1, ">"));
//builder.SetLimit(10);
Console.WriteLine(builder.ToString());
Console.WriteLine(JsonSerializer.Serialize(await connection.QueryAsync<Person>(builder.ToString())));