using MySqlConnector;
using Newtonsoft.Json;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.MySql;
using UskokDB.MySql.Attributes;

var connection = new MySqlConnection("Server=localhost;User ID=root;Database=test");
ParameterHandler.UseJsonForUnknownClassesAndStructs = true;
ParameterHandler.JsonWriter = (obj) => obj == null ? null : JsonConvert.SerializeObject(obj);
ParameterHandler.JsonReader = (jsonStr, type) => jsonStr == null ? null : JsonConvert.DeserializeObject(jsonStr, type);
await TestJson.CreateIfNotExistAsync(connection);

var read = await connection.QueryAsync<TestJson>("SELECT * FROM testjson");
Console.WriteLine(JsonConvert.SerializeObject(read));

public class TestJson : MySqlTable<TestJson>
{
    public TestJsonObject Test { get; set; }
}

public class TestJsonObject
{
    public string Test = "312";
}