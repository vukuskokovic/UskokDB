using MySqlConnector;
using UskokDB;
using UskokDB.MySql;
using UskokDB.MySql.Attributes;


var table = new MySqlTable<TestSql>("test1");
var table2 = new MySqlTable<TestSql2>("test2");
var connection = new MySqlConnection("Server=localhost;User ID=root;Database=test");
await table.CreateIfNotExistAsync(connection);
await table2.CreateIfNotExistAsync(connection);
class TestSql
{
    [Key]
    public Guid Id { get; }
}

class TestSql2
{
    [ForeignKey("test1", "Id")]
    public Guid Fk { get; }
}