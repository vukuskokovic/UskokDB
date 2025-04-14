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
