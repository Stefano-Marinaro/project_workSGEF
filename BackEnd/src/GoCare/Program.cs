using FluentValidation;
using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Errors;
using GoCare.Infrastructure;
using GoCare.Services.Auth;
using GoCare.Services.Destinations;
using GoCare.Services.Devices;
using GoCare.Services.Domain;
using GoCare.Services.Notifications;
using GoCare.Services.Profile;
using GoCare.Services.Provisioning;
using GoCare.Services.Transport;
using GoCare.Validation;     // ValidationFilter
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Override locali e non condivisi (es. l'IP LAN del proprio PC per i link via e-mail):
// ogni sviluppatore ha il proprio file, mai committato (vedi .gitignore).
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);   

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
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(SmtpOptions.SectionName));

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
builder.Services.AddScoped<CreateTransportService>();
builder.Services.AddScoped<DeleteTransportService>();
builder.Services.AddScoped<TripDetailService>();
builder.Services.AddScoped<ListTransportsService>();
builder.Services.AddScoped<GenerateOperatorLinkService>();
builder.Services.AddScoped<TimelineService>();
builder.Services.AddScoped<GetTripExecutionStatusService>();
builder.Services.AddScoped<AdvanceTripStatusService>();
builder.Services.AddScoped<NotificationDispatcher>();
builder.Services.AddScoped<AcceptTransportRequestService>();
builder.Services.AddScoped<IPushSender, ConsolePushSender>();
builder.Services.AddScoped<AssistedPersonService>();
builder.Services.AddScoped<GenerateCaregiverInviteLinkService>();
builder.Services.AddScoped<GetCaregiverInvitePreviewService>();
builder.Services.AddScoped<AcceptCaregiverInviteService>();
builder.Services.AddScoped<SearchComuneService>(); // mockup di verifica, vedi SearchComuneService
builder.Services.AddScoped<SearchAddressService>();
builder.Services.AddHttpClient<IGeocodingService, NominatimGeocodingService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "GoCare/1.0 (progetto universitario)");
});
builder.Services.AddScoped<GetPersonProfileService>();
builder.Services.AddScoped<UpdatePersonProfileService>();
builder.Services.AddScoped<GetAssociationProfileService>();
builder.Services.AddScoped<UpdateAssociationProfileService>();
builder.Services.AddScoped<ListAcceptedTransportsService>();
builder.Services.AddScoped<AcceptedTransportDetailService>();
builder.Services.AddScoped<AssociationHistoryService>();
builder.Services.AddScoped<ListNotificationsService>();
builder.Services.AddScoped<MarkNotificationReadService>();
builder.Services.AddScoped<MarkAllNotificationsReadService>();
builder.Services.AddScoped<NotificationCountersService>();
builder.Services.AddScoped<RegisterDeviceService>();
builder.Services.AddScoped<UnregisterDeviceService>();
builder.Services.AddScoped<CreateDestinationService>();
builder.Services.AddScoped<ListDestinationsService>();
builder.Services.AddScoped<UpdateDestinationService>();
builder.Services.AddScoped<DeleteDestinationService>();
builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();

// --- Validator delle request: registrati in automatico tutti gli AbstractValidator<T> pubblici dell'assembly
// (il ValidationFilter salta i tipi di request senza un validator)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // In sviluppo l'IP del frontend web cambia da PC a PC (e nel tempo):
        // accettiamo qualunque origine invece di elencarne una fissa, per non
        // doverlo risistemare ogni volta che cambia la rete di qualcuno.
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins("http://localhost:8081")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

builder.Services.AddControllers(options =>          // abilita i controller MVC
{
    options.Filters.AddService<ValidationFilter>(); // esegue ValidationFilter (preso dalla DI) su OGNI azione, globalmente
})
.AddJsonOptions(options =>
{
    // Enum come stringa nel JSON (sia in risposta che in richiesta), non come numero.
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
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

builder.Services.AddSwaggerGen(options =>           // genera il documento OpenAPI (Swagger)
{
    // Dichiara che l'API usa un token JWT "Bearer": fa comparire il pulsante Authorize
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Incolla l'accessToken ottenuto da POST /auth/login (solo il token, senza 'Bearer')."
    });

    // Dice a Swagger di inviare quel token nelle richieste
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();                          // costruisce l'app: il contenitore DI si congela

// --- Migrazioni EF Core: applicate automaticamente all'avvio (crea le tabelle se non esistono).
using (var migrationScope = app.Services.CreateScope())
{
    var db = migrationScope.ServiceProvider.GetRequiredService<GoCareDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();                          // 1o middleware: cattura le eccezioni non gestite -> GlobalExceptionHandler -> ProblemDetails
app.UseCors();

if (app.Environment.IsDevelopment())               // solo in ambiente Development
{
    app.UseSwagger();                              // espone il JSON OpenAPI su /swagger/v1/swagger.json
    app.UseSwaggerUI();                            // espone la pagina web Swagger su /swagger
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();                      // in dev niente redirect forzato: il cert è valido solo per localhost, non per l'IP di LAN
}

app.UseAuthentication();                            // popola HttpContext.User dal token
app.UseAuthorization();                             // valuta i tag [Authorize]

app.MapControllers();                               // collega le route agli endpoint dei controller

app.Run();                                          // avvia il web server e blocca qui finche l'app non si ferma
