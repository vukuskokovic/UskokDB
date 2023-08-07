# UskokDB
And extension library for `UskokDB` used for runtime table creating and inserting/replacing to the table, some examples:

## Example
```csharp
[TableName("tableName");
class Person : MySqlTable<Person>
{
    public string Name { get; set; }
    public string LastName { get; set; }
}
Console.WriteLine(Person.MySqlTableInitString);
//Same as above since it is a static property
Console.WriteLine(MySqlTable<Person>.MySqlTableInitString);
```
The resulting mysql string is
```mysql
Create Table IF NOT EXISTS `tableName` (Name Text, LastName Text)
```

## Example 2
```csharp
//If no TableName attribute is found type name is used
class Person : MySqlTable<Person>
{
    [Key]
    public Guid Id;

    [MaxLength(20)]
    public string Name { get; set; }
    //Unless the length is specified the 
    public string LastName { get; set; }

    [AutoIncrement]//not how ages works but...
    public int Age {get;set;}

    [NotMapped]
    public string FullName
    {
        get 
        {
            return $"{Name} {LastName}"
        }
    }
}
//
Person person1 = new Person {
    Id = Guid.NewGuid(),
    Name = "Vuk",
    LastName = "Uskokovic",
    Age = 0
};
Person person2 = new Person {
    Id = Guid.NewGuid(),
    Name = "Vuk",
    LastName = "Uskokovic",
    Age = 0
};
using var connection = SomeConnectionFactory.New();
table.Insert(connection, person1);
await table.InsertAsync(connection, person2);
person1.Age = 21;//Lets say I aged 21 years
person2.Age = 22;
await table.ReplaceAsync(connection, person1);
table.Replace(connection, person2);
```
The resulting mysql for the table is
```mysql
Create Table IF NOT EXISTS `Person` (Id VARHCAR(36) PRIMARY KEY, Name VARCHAR(20), LastName TEXT, Age INT AUTO_INCREMENT)
```

## Example foreign key
```csharp
[TableName("people")]
class Person : MySqlTable<Person>
{
    [Key]
    public Guid PersonKey { get; set; }
    
    public string Name {get; set; }
}

class Child : MySqlTable<Child>
{
    [ForeignKey<Person>]
    public Guid ParentId { get; set; }
    public string Name { get; set; }
}
```
The sql insert for Child type would be
```mysql
CREATE TABLE IF NOT EXISTS `Child` (ParentId VARCHAR(36), Name TEXT, FOREIGN KEY (ParentId) REFERENCES people(PersonKey))
```