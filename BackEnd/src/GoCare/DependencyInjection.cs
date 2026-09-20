using GoCare.Data;
using GoCare.Infrastructure;
using GoCare.Services.Auth;
using GoCare.Services.Provisioning;
using GoCare.Abstractions;
using GoCare.Errors;
using GoCare.Validation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCare;

public static class DependencyInjection
{
    public static IServiceCollection AddGoCare(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Kernel trasversale (ex GoCare.Shared)
        services.AddHttpContextAccessor();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddScoped<ValidationFilter>();

        // Applicazione (ex GoCare.Application)
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        services.Configure<FrontendOptions>(
            configuration.GetSection(FrontendOptions.SectionName));

        services.AddScoped<PasswordService>();
        services.AddScoped<TokenService>();
        services.AddScoped<AuthService>();
        services.AddScoped<ProfileProvisioningService>();
        services.AddScoped<IEmailSender, ConsoleEmailSender>();

        var connectionString = configuration.GetConnectionString("GoCareDb")
            ?? throw new InvalidOperationException("Connection string 'GoCareDb' mancante.");

        services.AddDbContext<GoCareDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        return services;
    }
}
