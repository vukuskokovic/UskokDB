using System.Data.Common;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;

[TableName("t4")]
public class User
{
    [Key] public Guid UserId { get; set; }
    
    [DbEnum("male", "female")] public string Gender { get; set; } = null!;
}