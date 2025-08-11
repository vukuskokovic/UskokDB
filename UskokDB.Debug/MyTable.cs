using System.Data.Common;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;

[TableName("typesTable")]
public class TypesTable
{
    [Key]
    public long LongValue { get; set; }
    public char? CharValue { get; set; }
    public float FloatValue { get; set; }
}


[GenerateRequests]
public static class TestRequests
{
    public static Task<List<TypesTable>> GetTables(ShopDbContext dbContext, float floatVal) =>
        dbContext.QueryAsync<TypesTable>("SELECT * FROM typesTable WHERE FloatValue = @FloatVal", new { FloatVal = floatVal});
}