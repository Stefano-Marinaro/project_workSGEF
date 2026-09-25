using FluentValidation;
using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Errors;
using GoCare.Infrastructure;
using GoCare.Services.Auth;
using GoCare.Services.Provisioning;
using GoCare.Validation;     // ValidationFilter
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);   

// --- Infrastruttura trasversale
builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<ValidationFilter>();

// --- Configurazione tipizzata
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<FrontendOptions>(
    builder.Configuration.GetSection(FrontendOptions.SectionName));

// --- Database
var connectionString = builder.Configuration.GetConnectionString("GoCareDb")
    ?? throw new InvalidOperationException("Connection string 'GoCareDb' mancante.");

builder.Services.AddDbContext<GoCareDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

// --- Servizi applicativi
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<TokenIssuer>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<RefreshService>();
builder.Services.AddScoped<RegisterUserService>();
builder.Services.AddScoped<RegisterAssociationService>();
builder.Services.AddScoped<VerifyEmailService>();
builder.Services.AddScoped<LogoutService>();
builder.Services.AddScoped<ForgotPasswordService>();
builder.Services.AddScoped<ResetPasswordService>();
builder.Services.AddScoped<ResendVerificationEmailService>();
builder.Services.AddScoped<ChangeEmailService>();
builder.Services.AddScoped<ConfirmEmailChangeService>();
builder.Services.AddScoped<ProfileProvisioningService>();
builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();

// --- Validator delle request: registrati in automatico tutti gli AbstractValidator<T> pubblici dell'assembly
// (il ValidationFilter salta i tipi di request senza un validator)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers(options =>          // abilita i controller MVC
{
    options.Filters.AddService<ValidationFilter>(); // esegue ValidationFilter (preso dalla DI) su OGNI azione, globalmente
});

// --- Autenticazione JWT
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Sezione 'Jwt' mancante in configurazione");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            NameClaimType = "sub",
            RoleClaimType = "role",
        };
    });

builder.Services.AddAuthorization();
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

app.UseAuthentication();                            // popola HttpContext.User dal token
app.UseAuthorization();                             // valuta i tag [Authorize]

app.MapControllers();                               // collega le route agli endpoint dei controller

app.Run();                                          // avvia il web server e blocca qui finche l'app non si ferma
