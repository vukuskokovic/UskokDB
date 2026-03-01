using System.Data.Common;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.Generator;

namespace pikac;

[TableName("admin_users")]
public class AdminUser
{
    [Key] public Guid AdminId { get; set; }
    [MaxLength(100), Unique, ColumnNotNull] public string Username { get; set; } = null!;
    [MaxLength(100), ColumnNotNull] public string AdminName { get; set; } = null!;

    [ColumnNotNull]
    public string HashedPassword { get; set; } = null!;
}

[TableName("admin_permissions")]
public class AdminPermissions
{
    [ForeignKey<AdminUser>("adminId", OnDelete = ForeignKeyAction.Cascade)]
    public Guid AdminId { get; set; }
    [MaxLength(60), ColumnNotNull]
    public string Permission { get; set; } = null!;
}