# Documentation is outdated and missing a lot of new key features new documentation yet to come

# UskokDB
#### UskokDB is a .NET orm designed with simplicity and ease of use in mind its great for idea testing fast prototyping and could even be production ready. It supports MySql, SqlLite, PostgreSql with full table creation strings, LinqToSql, and normal raw queries. You can define your tables and in 2 lines of codes generate your full db, you can write WHERE queries with linq in seconds and get the list of items from a generic function, you dont have to worry abount reading from db or writing, inserting is a one-liner. Updating/Deleting data with linq also is very easy. Let's get started.

## Getting started
### First you have to define your tables
```csharp

[TableName("people")] // This is not required the default name would be 'Person'
public class Person
{
    [Key]
    public Guid PersonId { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime TimeOfBirth { get; set; }
}
```
### Next you define a DbContext
```csharp
public class MyDbContext : DbContext
{
    public DbTable<Person> PeopleTable { get; }
                                          //Any ADO.NET driver will work
    public MyDbContext() : base(() => new MySqlConnection("<your_connection_string>"))
    {
        PeopleTable = new DbTable<Person>(this);
    }
}
```
### Now here is an example of how it would look like if you were to use it ASP.NET
```csharp
//In your Startup.cs/Program.cs
builder.Services.AddTransient<MyDbContext>((_) => new MyDbContext());

//And here is an example get request

[HttpGet("adults")]
public async Task<IActionResult> GetAllAdultsAsync(MyDbContext dbContext)
{
    List<PeopleTable> allAdults = await dbContext.PeopleTable.Where(person => person.Age >= 18).QueryAsync();
    return Ok(allAdults);
}
```
### Or here is a more detailed request
```csharp
//I know this is not how it should be done but for the sake of simplicity
[HttpGet("withName/{name}/{age:int}")]
public async Task<IActionResult> GetWithParamsAsync(MyDbContext dbContext, string name, int age)
{
    List<PeopleTable> people = await dbContext.PeopleTable.Where(person => person.Age == age && person.Name == name).QueryAsync();
    return Ok(people);
}
```
## Table generation
I will use the same table & context from above<br/>
In order to get table generation you need to specify the sql language
```csharp
//In program.cs/startup.cs
UskokDb.SqlDialect = SqlDialect.MySql;
builder.Services.AddTransient<MyDbContext>((_) => new MyDbContext());

await using(var context = new MyDbContext())
{
    await context.ExecuteTableCreationCommand();
}

//You can also get the string like this
string fullCommandText = context.GetTableCreationString();
```
### This will execute this command
```mysql
CREATE TABLE IF NOT EXISTS people (personId VARCHAR(36), name TEXT, age INT, timeOfBirth DATETIME, PRIMARY KEY (personId));
```
### If you were to have more tables, more tables would be added to this
## Table attributes
1. <b>TableNameAttribute</b> - Used on the table class to specify the name manually
2. <b>ColumnAttribute</b> - Used to specify the name of the column/property manually
3. <b>NotMappedAttribute</b> - To tell the framework to not store/read this property
4. <b>AutoIncrementAttribute</b> - Pretty self-explanatory
5. <b>MaxLengthAttribute</b> - Only for strings to specify the max length(VARCHAR)
6. <b>KeyAttribute</b> - Used for primary keys
7. <b>ForeignKeyAttribute(T)</b> - Used for foreign keys the generic type is another table and the string paramter is the column name
8. <b>UniqueAttribute</b> - Used for sql unique attribute
## Inserting
```csharp
await dbContext.PeoplTable.InsertAsync(new Person()
{
    PersonId = Guid.NewGuid(),
    Age = 20,
    Name = "Charlie",
    TimeOfBirth = new DateTime(2005, 1, 2)
});

//You can also insert an array
Person[] people = GeneratePeopleArrayFromSomewhere();
await dbContext.PeoplTable.InsertAsync(people);
```
## Deleting/Update
```csharp
//Updating without row context
await dbContext.PeoplTable.UpdateAsync(
    //This is the update statement
    () => new Person()
    {
        Age = 20
    },
    //This is the where statement
    (person) => person.Name == "SomeNameYouWantToChangeAge"
);

//Updating with row context
await dbContext.PeoplTable.UpdateAsync(
    //This is the update statement
    (person) => new Person()
    {
        Age = person.Age + 1
    },
    //This is the where statement
    (person) => person.PersonId = BirthdayPersonId
);

//Deleting
await dbContext.PeoplTable.DeleteAsync((person) => person.PersonId == personToDeleteId);
/*
BE AWARE YOU IF THE WHERE PART OF THE DELETE STATEMENT IS LEFT AS NULL IT WILL DELETE THE WHOLE TABLE
*/
```
## Multiple commands
### This allows you to batch more commands in one transaction like delete insert and update all at once
### These commands are executed in the order you add them to the list
```csharp
dbContext.PeopleTable.AppendDelete((person) => person.PersonId == somePersonId);
dbContext.PeopleTable.AppendInsert(newPerson);
dbContext.PeopleTable.AppendUpdate(() => new Person(){Age = 22}, (p) => p.PersonId = anotherPersonId);
await dbContext.ExecuteQueueAsync();
```
## Json support(to be desired)
In short, it works will get the job done for simple stuff
```csharp
class SomeDto
{
    public string Text {get;set;}
}

class SomeTable
{
    public SomeDto SomeDto {get;set;}
    public SomeDto[] SomeDtoArray {get;set;}
}

//Before using json you first must enable it
DbIOOptions.UseJsonForUnknownClassesAndStructs = true;
//Then when inserting/reading this values will be treated as strings in db and will be serilized/deserilized on input/output
//You can also override the default json parser(mandatory for .netstandard 2.0)
//Default parser is system.text.json
DbIOOptions.JsonWriter = (someObject) => (string)Myjsonparser(someObject);
DbIOOptions.JsonReader = (jsonStr, expectedType) => MyjsonReader(jsonStr, expectedType);
```
## Custom reader/writer
### You can make a custom type that will be stored to your liking in the db here's how
```csharp
int WritingFunc(SomeClass value) => value.GetIntToBeWrittenToDb();
SomeClass ReadingFunc(int valueFromDb) => SomeClass.FromDb(valueFromDb);


DbIOOptions.ParameterConverters[typeof(SomeClass)] = 
                                //This is your type you want to add custom reading/writing to
new DefaultColumnValueConverter<SomeClass, 
                                //This is the type that will be stored in the db
                                int>(WritingFunc, ReadingFunc);
```
### There are also two optional arguments maxLength, and typeName
1. maxLength - Used if the db column type is string to specify the max length of the string
2. typeName - In case you want to set the custom like "TIMESTAMP" or something
