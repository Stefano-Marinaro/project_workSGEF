using GoCare.Application.Data;
using GoCare.Application.Services.Auth;

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
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();

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
