using GoCare.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCare.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var businessCs = configuration.GetConnectionString("BusinessDb")
            ?? throw new InvalidOperationException("Connection string 'BusinessDb' mancante.");

        services.AddDbContext<BusinessDbContext>(options =>
            options.UseNpgsql(businessCs).UseSnakeCaseNamingConvention());

        var authCs = configuration.GetConnectionString("AuthDb")
              ?? throw new InvalidOperationException("Connection string 'AuthDb' mancante.");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(authCs).UseSnakeCaseNamingConvention());

        return services;
    }
}
