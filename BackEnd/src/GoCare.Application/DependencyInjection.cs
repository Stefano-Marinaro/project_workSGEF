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
        services.AddDbContext<BusinessDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("BusinessDb")));

        // Area Auth: stessa forma, altro context e altra connection string
        //   services.AddDbContext<AuthDbContext>(options =>
        //       options.UseNpgsql(configuration.GetConnectionString("AuthDb")));

        return services;
    }
}
