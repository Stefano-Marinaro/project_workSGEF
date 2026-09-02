using GoCare.Api.Security;          // CurrentUser
using GoCare.Shared;                // AddSharedKernel()
using GoCare.Shared.Abstractions;   // ICurrentUser
using GoCare.Shared.Validation;     // ValidationFilter

var builder = WebApplication.CreateBuilder(args);   // crea il builder: config, logging, contenitore DI

builder.Services.AddSharedKernel();                 // registra i servizi di GoCare.Shared (IClock, handler eccezioni, ProblemDetails, ValidationFilter, IHttpContextAccessor)

builder.Services.AddScoped<ICurrentUser, CurrentUser>();  // "chi chiede ICurrentUser riceve un CurrentUser", uno per richiesta HTTP

builder.Services.AddControllers(options =>          // abilita i controller MVC
{
    options.Filters.AddService<ValidationFilter>(); // esegue ValidationFilter (preso dalla DI) su OGNI azione, globalmente
});

builder.Services.AddEndpointsApiExplorer();         // raccoglie i metadati degli endpoint per OpenAPI
builder.Services.AddSwaggerGen();                   // genera il documento OpenAPI (Swagger)

var app = builder.Build();                          // costruisce l'app: il contenitore DI si congela

app.UseExceptionHandler();                          // 1o middleware: cattura le eccezioni non gestite -> GlobalExceptionHandler -> ProblemDetails

if (app.Environment.IsDevelopment())               // solo in ambiente Development
{
    app.UseSwagger();                              // espone il JSON OpenAPI su /swagger/v1/swagger.json
    app.UseSwaggerUI();                            // espone la pagina web Swagger su /swagger
}

app.UseHttpsRedirection();                          // redirige le richieste http:// verso https://

app.MapControllers();                               // collega le route agli endpoint dei controller

app.Run();                                          // avvia il web server e blocca qui finche l'app non si ferma
