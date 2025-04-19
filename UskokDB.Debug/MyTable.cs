using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;
[TableName("testTable")]
public class MyTable
{
    [Key]
    public Guid Id { get; set; }
    public string Whatever { get; set; }
}


