using UskokDB.Attributes;

namespace UskokDB.Debug.Tables;

[TableName("users")]
public class User
{
    [Key]
    public Guid UserId { get; set; }

    [ColumnNotNull]
    public string Name { get; set; } = null!;
    [Unique, ColumnNotNull, MaxLength(12)]
    public string Username { get; set; } = null!;
    
    public bool Xd { get; set; }
    
    public string? Email { get; set; }
    
    public Post? Post { get; set; }
    
    public DateTime Birthday { get; set; }
    public DateTime JoinedAt { get; set; }
}