using MySqlConnector;
using Newtonsoft.Json;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.MySql;
using UskokDB.MySql.Attributes;

var connection = new MySqlConnection("Server=localhost;User ID=root;Database=test");
await TestPrimryKey.CreateIfNotExistAsync(connection);
await TestForeignKEYYYY.CreateIfNotExistAsync(connection);

public class TestForeignKEYYYY : MySqlTable<TestForeignKEYYYY>
{
    [ForeignKey<TestPrimryKey>]
    public int Test { get; set; }
}

public class TestPrimryKey : MySqlTable<TestPrimryKey>
{
    [Key] public int Test { get; set; }
}