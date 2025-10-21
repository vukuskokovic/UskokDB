using System.Data.Common;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;

public class PKeyTable
{
    [Key]
    public double? Id { get; set; }
}

public class FKeyTable
{
    
    public int? OnlyUpdate { get; set; }
    public int OnlyDelete { get; set; }
    public int? None { get; set; }
    public int Both { get; set; }
}

[TableName("users")]
public class User
{
    [Key] public Guid UserId { get; set; }
    
    [Unique] public string Email { get; set; } = null!;
}