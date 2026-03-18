using UskokDB.Attributes;

namespace UskokDB.Debug.Tables;

public class UserRelation
{
    [Key]
    public Guid RelationId { get; set; }
    [ForeignKey<User>("userId", OnDelete = ForeignKeyAction.Cascade)]
    public Guid Initiator { get; set; }
    [ForeignKey<User>("userId", OnDelete = ForeignKeyAction.Cascade)]
    public Guid Receiver { get; set; }

    [ColumnNotNull]
    public string Status { get; set; } = null!;

    public DateTime InitiatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}