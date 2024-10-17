##### * *For UskokDB 1.0-2.x look at older commits* *

# UskokDB
A .NET ORM designed for quick and simple development. Supports **MySQL**, **SQLite**, and **PostgreSQL**
#### Connection drivers are not included in the package but any ADO.NET is supported

## **DbContext**
### This class will contain all of your tables in the DB, the db type(MYSql, SQLIte, PostgreSQL) and the ADO.NET connection factory
### DbContext can create a creating string for your database using reflections

## **DbTable&lt;T&gt;**
### Contains functions to help query the table
# Attributes
### **NotMappedAttribute** - used to describe property that is not mapped to the db
### **KeyAttribute** - used to describe primary keys in the table(can be multiple in the same type)
### **MaxLengthAttribute** - used to turn string from TEXT type in db to VARCHAR([specifiedLength])
### **ForeignKeyAttribute&lt;T&gt;** - Points to the primary key in another table
### **ColumnNotNullAttribute** - marks the column as *NOT NULL*
### **AutoIncrementAttribute** - Adds auto increment to the column
### **TableNameAttribute** - Used for the actuall table class just overwrites the default name for the table(default is the type name)
### **ColumnAttribute** - Used for giving a custom name for the column (default is lower case the property name)
### Example usage

## There is somewhat limited custom Linq to Sql implementation

## NOTE: Only async queries are supported for now

```cs
public class ShopDbContext : DbContext
{
    public DbTable<Shop> ShopTable { get; }
    // In the constructor of the context you need to provide the db type and the connection factory (use your respective driver for your DB)
    public ShopDbContext() : base(DbType.MySQL, () => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        ShopTable = new DbTable<Shop>(this);
    }
}

[TableName("shop")]
public class Shop
{
    [Key]
    public int Id { get; set; }
    [MaxLength(20)]
    public string Name { get; set; }
    public string ChainName {get;set;}
    public int Personel { get; set; }
}

ShopDbContext context = new ShopDbContext();

List<Shop> over10Personel = await context.ShopTable.Where(shop => shop.Personel > 10).QueryAsync();
Shop withId = await context.ShopTable.Where(shop => shop.Id == 3 && shop.Name == "<Some shop name>").QuerySingleAsync();
Shop stringQuery = await context.ShopTable.Where("id=@Id", new {Id=3}).QuerySingleAsync();
List<Shop> groupAndOrder = await context.ShopTable.OrderBy("personel ASC", "id DESC").GroupBy("chainName");
// There is also support for IAsyncEnumerable
await foreach(var shop in context.ShopTable.Where(shop => shop.Personel > 2).QueryAsyncEnumerable())
{
    Console.WriteLine(shop.Name);
}
 
// For now linq to sql and inline queries such as OrderBy(...).Limit(..).Where(..)
// Are compiled every time they are executed if you want the fastest perfomance you could compile it only once

string compiledQuery = context.ShopTable.Where(shop => shop.Personel > 30).Limit(10).OrderBy("personel DESC").CompileQuery();
List<Shop> first10Above30Personel = await context.QueryAsync<Shop>(compiledQuery);
// SELECT * FROM shop WHERE personel > 30 ORDER BY personel DESC LIMIT 10
Console.WriteLine(compiledQuery);


// In case you have query parameters
string compiledQuery = context.ShopTable.Where("personel > @Personel").Limit(10).OrderBy("personel DESC").CompileQuery();
// SELECT * FROM shop WHERE personel > @Personel ORDER BY personel DESC LIMIT 10
Console.WriteLine(compiledQuery);
List<Shop> first10Above30Personel = await context.QueryAsync<Shop>(compiledQuery, new { Personel = 30 });
```