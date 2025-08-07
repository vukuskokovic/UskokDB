using MySqlConnector;
using System.Text.Json;
using pikac;
using UskokDB;
using UskokDB.Attributes;

var context = new ShopDbContext();


//[CompileSql]
public static class ShopControllerQueries
{
    public static Task<List<ParkingSession>> GetSessions(ShopDbContext context, String id) =>
        context.TestTable.Where((session) => session.SessionId == Guid.Parse(id)).OrderBy("sessionId").GroupBy("sessionId").Limit(3).QueryAsync();
}

public static class ShopControllerQueriesGenerated
{
    public static Task<List<ParkingSession>> GetSessions(ShopDbContext context, String id) =>
        context.QueryAsync<ParkingSession>(
            $"SELECT * FROM tableName WHERE sessionId = {context.DbIo.WriteValue(id)} ORDER BY sessionId GROUP BY sessionId LIMIT 3");
}


public class ShopDbContext : DbContext
{
    public DbTable<ParkingSession> TestTable { get; }
    public ShopDbContext() : base(DbType.MySQL,() => new MySqlConnection("Server=localhost;User ID=root;Database=test"))
    {
        TestTable = new DbTable<ParkingSession>(this);
    }
}