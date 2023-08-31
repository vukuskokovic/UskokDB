using MySqlConnector;
using Newtonsoft.Json;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.MySql;
using UskokDB.MySql.Attributes;

ParameterHandler.UseJsonForUnknownClassesAndStructs = true;
ParameterHandler.JsonReader = (str, type) => str == null ? null : JsonConvert.DeserializeObject(str, type);
ParameterHandler.JsonWriter = obj => obj == null ? null : JsonConvert.SerializeObject(obj);

var connection = new MySqlConnection("Server=localhost;User ID=root;Database=test");
await TestEnum.CreateIfNotExistAsync(connection);
await TestEnum.InsertAsync(connection, new TestEnum()
{
    Test = TestEnumType.Test,
    TestPest = TestEnumType.Test2
});

var all = await connection.QueryAsync<TestEnum>("SELECT * FROM testenum");

Console.WriteLine(JsonConvert.SerializeObject(all));

public class TestEnum : MySqlTable<TestEnum>
{
    public TestEnumType Test { get; set; }
    public TestEnumType TestPest { get; set; }
}

public enum TestEnumType
{
    Test,
    Test1,
    Test2
}