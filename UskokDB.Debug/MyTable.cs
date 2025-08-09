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
