using UskokDB.MySql;
using UskokDB.MySql.Attributes;

[TableName("test")]
public class Person : MySqlTable<Person>
{
    public int Age {get;set;}
    public string Name { get; set; }
    public int Height { get; set; }
}