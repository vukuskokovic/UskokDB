# UskokDB
And extension library for `UskokDB` used for runtime table creating and inserting/replacing to the table some examples:

## Example 1
```cs
class Person 
{
    public string Name {get;set;}
    public string LastName {get;set;}
}

MySqlTable<Person> table = new MySqlTable<Person>("tablename");
table.CreateIfNotExist();
await table.CreateIfNotExistAsync();
```
The resulting mysql string is
```sql
Create Table IF NOT EXISTS `tablename` (Name Text, LastName Text)
```

## Example
```cs
class Person 
{
    [PrimaryKey]
    public Guid Id;

    [MaxLength(20)]
    public string Name {get;set;}
    //Unless the length is specified the 
    public string LastName {get;set;}

    [AutoIncrement]//not how ages works but...
    public int Age {get;set;}

    [NotMapped]
    public string FullName => $"{Name} {LastName}";
}

MySqlTable<Person> table = new MySqlTable<Person>("tablename");
// Creation string shown in the example
table.CreateIfNotExist();
await table.CreateIfNotExistAsync();
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
table.Insert(person1);
await table.InsertAsync(person2);
person1.Age = 21;//Lets say I aged 21 years
person2.Age = 22;
await table.ReplaceAsync(person1);
table.Replace(person2);
```
The resulting mysql for the table is
```sql
Create Table IF NOT EXISTS `tablename` (Id VARHCAR(36) PRIMARY KEY, Name VARCHAR(20), LastName TEXT, Age INT AUTO_INCREMENT)