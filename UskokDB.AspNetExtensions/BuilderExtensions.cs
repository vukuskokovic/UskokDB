using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace UskokDB.AspNetExtensions;

public static class BuilderExtensions
{
    public static void AddDbContext<T>(this WebApplicationBuilder builder, Func<IServiceProvider, T> dbContextFactory) where T : DbContext
    {
        builder.Services.AddTransient(dbContextFactory);
    }

}
