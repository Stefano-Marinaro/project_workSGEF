using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GoCare.Shared.Errors;
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException   => (StatusCodes.Status404NotFound,            "Risorsa non trovata"),
            ConflictException   => (StatusCodes.Status409Conflict,            "Conflitto"),
            ForbiddenException  => (StatusCodes.Status403Forbidden,           "Operazione non consentita"),
            ValidationException => (StatusCodes.Status422UnprocessableEntity, "Dati non validi"),
            _                   => (StatusCodes.Status500InternalServerError, "Errore interno")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Eccezione non gestita"); 

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception is DomainException ? exception.Message : null,
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validation)
           problem.Extensions["errors"] = validation.Errors;  
        
        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
