using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;
[TableName("fileTable")]
[GenerateSqlTableHelpers]


public class MyTable
{
    [Column("da")]
    
    public string SomeColumn { get; set; } = null!;
    public string SomeColumn2 { get; set; } = null!;
    public string SomeColumn3 { get; set; } = null!;
}

[GenerateLinqQueries]
public static class MyTableQueries
{
    public static Task Test() => Task.CompletedTask;
    
    public static Task<List<Test>> GetTablesFromFirstColumn(ShopDbContext dbContext, string id) =>
        dbContext.TestTable.Where(t => t.Id == id).QueryAsync();
}



