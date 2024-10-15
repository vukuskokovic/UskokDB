
using UskokDB;
using UskokDB.Attributes;

[TableName("test")]
public class Person
{
    public int Age {get;set;}
    public string Name { get; set; }
    public int Height { get; set; }
}