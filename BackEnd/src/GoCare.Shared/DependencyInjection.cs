using GoCare.Shared.Abstractions;
using GoCare.Shared.Errors;
using GoCare.Shared.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace GoCare.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services) // this IServiceCollection services == "questo metodo si può chiamare su qualsiasi oggetto che sia un IServiceCollection"
    {
        services.AddSingleton<IClock, SystemClock>(); // Singleton = per tutto il ciclo di vita dell'app, si usa per oggetti senza stato, thread-safe (è sicuro che piu thread lo tocchino)

        services.AddHttpContextAccessor();  // Helper del framework Microsoft.AspNetCore.Http

        services.AddExceptionHandler<GlobalExceptionHandler>(); // regiostra il nostro handler di gestione delle eccezioni
        services.AddProblemDetails();                           // Servizio che serializza gli errori in formato ProblemDetails

        services.AddScoped<ValidationFilter>();                 // Rende il filtro da noi creato risolvibile

        return services;

    }
}
