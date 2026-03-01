using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using MySqlConnector;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using pikac;
using UskokDB;
using UskokDB.Attributes;
using UskokDB.Query;
using UskokDB.Query.QueryFunctions;


UskokDb.SetSqlDialect(SqlDialect.MySql);
UskokDb.InitLinqMethodRegistry();
var dbContext = new ShopDbContext();
var t = TimeSpan.Zero;
Guid d = Guid.NewGuid();
var arr = new bool[] { true, false };
var admins = await dbContext.AdminUser
    .Join(dbContext.AdminPermission, (a, ap) => a.AdminId == ap.AdminId)
    .GroupBy(x => x.AdminId)
    .Select<AdminRead, AdminUser, AdminPermissions>((a, ap) => new AdminRead()
    {
        AdminId = a.AdminId,
        Permissions = Sql.JsonCreateArray<AdminTestRead, AdminPermissions>((ad) => new AdminTestRead()
        {
            AdminId = ad.AdminId,
            Permission = ad.Permission
        }),
        Username = a.Username
    });

Console.WriteLine(JsonSerializer.Serialize(admins, new JsonSerializerOptions()
{
    WriteIndented = true
}));


public class AdminTestRead
{
    public Guid AdminId { get; set; }
    public string Permission { get; set; }
}
public class AdminRead
{
    public Guid AdminId { get; set; }
    public string Username { get; set; }
    public AdminTestRead[] Permissions { get; set; }
}
public class ShopDbContext : DbContext
{
    public DbTable<AdminUser> AdminUser { get; }
    public DbTable<AdminPermissions> AdminPermission { get; }
    public ShopDbContext() : base(() => new MySqlConnection(""))
    {
        AdminUser = new DbTable<AdminUser>(this);
        AdminPermission = new DbTable<AdminPermissions>(this);
    }
}