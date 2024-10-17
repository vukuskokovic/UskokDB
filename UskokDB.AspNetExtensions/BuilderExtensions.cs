using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace UskokDB.AspNetExtensions;

public static class BuilderExtensions
{
    public static void AddDbContext<T>(this IServiceCollection services, Func<IServiceProvider, T> dbContextFactory) where T : DbContext
    {
        services.AddTransient(dbContextFactory);
    }

}
