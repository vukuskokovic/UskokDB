# UskokDB
    A simple C# ORM which adds extension methods on IDbConnection and DbConnection classes
## Quering
#### Lets supose we have a model specified like:
```cs
class Person
{
    public string Name { get; set; }
    public string LastName { get; set; }
}

//Getting a connection instance from some factory
IDbConnection connection = SomeConnectionFactory.CreateConnection();
```
### Sync quering
```cs Sync
List<dynamic> people = connection.Query("Select * From people Where name=@Name", new { 
    Name = "Vuk" 
})
List<Person> people = connection.Query<Person>("Select * From people Where name=@Name", new { 
    Name = "Vuk" 
})
dynamic person = connection.QuerySingle("Select * From people Where name=@Name", new { 
    Name = "Vuk" 
})
Person person = connection.QuerySingle<Person>("Select * From people Where name=@Name", new { 
    Name = "Vuk" 
})
```
### Async Quering
```cs Async
List<dynamic> people = await connection.QueryAsync("Select * From people Where name=@Name", new { 
    Name = "Vuk" 
})
List<Person> people = await connection.QueryAsync<Person>("Select * From people Where name=@Name", new { 
    Name = "Vuk" 
})
dynamic person = await connection.QuerySingleAsync("Select * From people Where name=@Name", new { 
    Name = "Vuk" 
})
Person person = await connection.QuerySingleAsync<Person>("Select * From people Where name=@Name", new { 
    Name = "Vuk" 
})
```
## `IParamterConverter`
### The `ParameterHandler` class has a static dictionary `Dictionary<Type, IParamterConverter> ParamterConverters` which is used as a map to create db values from custom types the prime example of this is `Guid` demostrated here
```cs
[typeof(Guid)] = new DefaultParamterConverter<Guid, string>((guid) => guid.ToString(), Guid.Parse, Guid.Empty.ToString().Length)
```
### The `DefaultParamterConverter` takes in a function of when writing to a database and a function when reading it along side with the `int? maxLength` and `string? typeName`, maxLength is used to determine the max length in case the writen type is string in which case this is and it is `Guid.Empty.ToString().Length` and typeName is used in the extension library `UskokDB.MySql` to know what type should be written in case of a for example `DateTime` this parameter would be `DATETIME`.
## Note! Primtive types cannot be changed (int, uint, char, bool, short, ushort, enum, string, double, float, decimal, etc...)
### `IParamterConverter` is used for the type `Guid` so that the model can be represented like this
```cs
class Person 
{
    public Guid Id;
    public string Name;
    public string LastName;
}
```
### This can be just any type so lets create an example
```cs
//This is a kind of dumb example but shows the way it used
const int MaxNameLength = 20;
class NameClass
{
    public string Value; 
}
class Person 
{
    public Guid Id;
    public NameClass Name;
    public NameClass LastName;
}
ParameterHandler.ParamterConverters[typeof(NameClass)] = new DefaultParamterConverter<NameClass, string>(nameClass => nameClass.Value, (str) => new NameClass { Value = str }, MaxNameLength)
```
## Column Attribute
### You can use the column attribute in order to specify a name for a certain property and here is an example of this
```cs
class Person {
    [Column("name")]
    public string FullName {get; set;}
}

Person p = connection.QuerySingle<Person>("Select * From people where name='Somename'")
```
### As you can see the column name in the table is name but we can still name the property as we wish as long as we specify the actuall name of the column with the attribute

## NotMapped Attribute
### You can ignore a specific property with this attribute
```cs
class Person {
    [NotMapped]
    public string FullName {get; set;}
    
    public int Age {get; set;}
}
```

# Json
    You can configure the library to use json for unkown structs and classes this is however 
    bypassed if a speicifc way of converting is specified in the convert dictionary

```csharp
//This sets the usage
ParameterHandler.UseJsonForUnknownClassesAndStructs = true;

//And here is an example with Newtonsoft.Json but you can use any library as you wish
ParameterHandler.JsonWriter = (object? someInstance) => 
    someInstance == null? null : JsonConvert.SerializeObject(someInstance);
    //If you return null, null value will be stored in the column
    
ParameterHandler.JsonReader = (string? jsonString, Type columnType) =>
    jsonString == null? null : JsonConvert.DeserializeObject(jsonString, columnType);
```
