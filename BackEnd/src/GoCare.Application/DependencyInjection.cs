using GoCare.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCare.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var authConnection = configuration.GetConnectionString("AuthDb")
            ?? throw new InvalidOperationException("Connection string 'AuthDb' is missing.");
        services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(authConnection,
            npgsql => npgsql.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention());
        return services;
    }
}
