using UskokDB.MySql;
using UskokDB.MySql.Attributes;

[TableName("shop")]
public class Shop : MySqlTable<Shop>
{
    public int Test {get;set;}
    public double Test1 {get;set;}
}

[TableName("shop1")]
public class Shop1 : MySqlTable<Shop1>
{
    public int Test {get;set;}
    public double Test1 {get;set;}
}