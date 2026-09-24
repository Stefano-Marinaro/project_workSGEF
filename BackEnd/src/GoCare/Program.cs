using GoCare;                   // AddGoCare()
using GoCare.Dtos.Auth.Requests; // LoginRequest, LoginRequestValidator
using GoCare.Services.Auth;
using GoCare.Validation;     // ValidationFilter
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);   // crea il builder: config, logging, contenitore DI

builder.Services.AddGoCare(builder.Configuration);  // registra kernel trasversale + servizi applicativi + i due DbContext

builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:8081")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddControllers(options =>          // abilita i controller MVC
{
    options.Filters.AddService<ValidationFilter>(); // esegue ValidationFilter (preso dalla DI) su OGNI azione, globalmente
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Sezione 'Jwt' mancante in configurazione");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
        };
    });
builder.Services.AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterAssociationRequest>, RegisterAssociationValidator>();
builder.Services.AddScoped<IValidator<VerifyEmailRequest>, VerifyEmailValidator>();
builder.Services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordValidator>();
builder.Services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordValidator>();
builder.Services.AddScoped<IValidator<LogoutRequest>, LogoutValidator>();
builder.Services.AddScoped<IValidator<RefreshRequest>, RefreshValidator>();

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();         // raccoglie i metadati degli endpoint per OpenAPI
builder.Services.AddSwaggerGen();                   // genera il documento OpenAPI (Swagger)

var app = builder.Build();                          // costruisce l'app: il contenitore DI si congela

app.UseExceptionHandler();                          // 1o middleware: cattura le eccezioni non gestite -> GlobalExceptionHandler -> ProblemDetails
app.UseCors();

if (app.Environment.IsDevelopment())               // solo in ambiente Development
{
    app.UseSwagger();                              // espone il JSON OpenAPI su /swagger/v1/swagger.json
    app.UseSwaggerUI();                            // espone la pagina web Swagger su /swagger
}

app.UseHttpsRedirection();                          // redirige le richieste http:// verso https://

app.UseAuthentication();                            // popola HttpContext.User dal token
app.UseAuthorization();                             // valuta i tag [Authorize] 

app.MapControllers();                               // collega le route agli endpoint dei controller

app.Run();                                          // avvia il web server e blocca qui finche l'app non si ferma
