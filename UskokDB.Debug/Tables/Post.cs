using UskokDB.Attributes;

namespace UskokDB.Debug.Tables;

[TableName("posts")]
public class Post
{
    [Key]
    public Guid PostId { get; set; }
    [ForeignKey<User>("userId", OnDelete = ForeignKeyAction.Cascade)]
    public Guid UserId { get; set; }

    [ColumnNotNull, MaxLength(40)] public string Title { get; set; } = null!;
    [ColumnNotNull, MaxLength(200)] public string Text { get; set; } = null!;
}