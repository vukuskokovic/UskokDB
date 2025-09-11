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
    [Unique, MaxLength(128)] public string? FirebaseUid { get; set; }
                            public string? HashedPassword { get; set; }
    [Unique] public string Email { get; set; } = null!;
    [MaxLength(4)] public string PhoneRegionalCode { get; set; } = null!;
    [MaxLength(15)] public string PhoneNumber { get; set; } = null!;
    [MaxLength(50)] public string FirstName { get; set; } = null!;
    [MaxLength(50)] public string LastName { get; set; } = null!;
    [MaxLength(1)] public string Gender { get; set; } = null!;
                    public DateTime Birthday { get; set; }
                    public double Balance { get; set; } = 0.0;
}