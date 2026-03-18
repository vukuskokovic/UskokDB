using MySqlConnector;
using UskokDB.Debug.Tables;

namespace UskokDB.Debug;

public class ShopDbContext : DbContext
{
    public DbTable<User> Users { get; }
    public DbTable<Post> Posts { get; }
    public DbTable<UserRelation> UserRelations { get; }
    public ShopDbContext() : base(() => new MySqlConnection("Server=localhost;User ID=test;Database=test;Password=test"))
    {
        Users = new DbTable<User>(this);
        Posts = new DbTable<Post>(this);
        UserRelations = new DbTable<UserRelation>(this);
    }
}