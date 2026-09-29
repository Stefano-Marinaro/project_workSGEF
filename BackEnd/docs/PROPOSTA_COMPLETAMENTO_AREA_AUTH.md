# Proposta di completamento dell'area autenticazione e provisioning

> Stato del documento: proposta tecnica, non implementata.
>
> Questo documento descrive il codice da aggiungere o modificare, ma non implica che tali modifiche siano già presenti nel progetto.
>
> Esclusione esplicita: `AnonymizeForDeletedAccountAsync` e l'endpoint di eliminazione dell'account non fanno parte di questa proposta.

---

## 1. Obiettivi

La proposta copre le seguenti funzionalità ancora mancanti nella sezione 5.3 del piano backend:

1. completamento del profilo caregiver;
2. completamento del profilo associazione;
3. accreditamento o rifiuto manuale di un'associazione;
4. blocco delle funzioni operative per le associazioni non accreditate;
5. reinvio dell'e-mail di verifica;
6. cambio sicuro dell'indirizzo e-mail.

Non viene modificata la scelta architetturale corrente:

- un solo progetto ASP.NET Core;
- un solo `GoCareDbContext`;
- controller sottili;
- logica applicativa nei Service;
- nessun repository;
- nessun MediatR/CQRS;
- DTO HTTP separati dai Model EF;
- Service concreti registrati direttamente nella dependency injection.

---

## 2. Decisioni funzionali

### 2.1 Login e accreditamento delle associazioni

Un'associazione con account verificato può effettuare il login anche quando il suo stato di accreditamento è `Pending` o `Rejected`.

Il login deve rimanere disponibile perché l'associazione deve poter:

- completare o correggere il proprio profilo;
- consultare il proprio stato di accreditamento;
- ricevere eventuali comunicazioni amministrative.

Lo stato di accreditamento diventa invece un gate per le operazioni di dominio. Soltanto un'associazione con:

- account attivo;
- profilo completo;
- `Association.Status == EAccreditationStatus.Accredited`;

può visualizzare richieste operative, accettare o rifiutare trasporti e gestire viaggi.

Il gate non viene quindi inserito in `AuthService.LoginAsync`. Viene applicato nei Service di dominio tramite un componente riutilizzabile.

### 2.2 Rifiuto dell'accreditamento

Il rifiuto cambia solamente `Association.Status` in `Rejected`.

Non cambia `Account.Status`: l'account resta attivo e l'associazione può continuare ad autenticarsi, ma non può operare sui trasporti.

### 2.3 Completamento e aggiornamento del profilo

Gli endpoint di completamento usano `PATCH`:

```text
PATCH /me/profile
PATCH /association/profile
```

I metodi esistenti `CompleteProfile` assegnano tutti i campi ricevuti e possono essere richiamati nuovamente. Di conseguenza questi endpoint possono essere riutilizzati anche come aggiornamento completo dei medesimi campi, evitando di creare ora una seconda coppia di endpoint quasi identici per UC 8.

### 2.4 Cambio e-mail

Il cambio e-mail non deve sostituire immediatamente l'indirizzo dell'account. La nuova casella deve essere verificata prima della modifica definitiva.

Il flusso proposto è:

1. l'utente autenticato invia nuova e-mail e password corrente;
2. il backend verifica password e unicità dell'indirizzo;
3. il backend crea un `EmailChangeToken` associato alla nuova e-mail;
4. il link viene inviato alla nuova casella;
5. il frontend legge il token dal link e chiama l'endpoint di conferma;
6. il backend aggiorna atomicamente `Account.Email` e l'e-mail del profilo;
7. le sessioni refresh esistenti vengono revocate.

Il normale `EmailVerificationToken` non è sufficiente perché contiene l'account destinatario, ma non contiene la nuova e-mail richiesta.

---

## 3. Inventario dei file

Seguendo la convenzione corrente di un DTO e un validator per file, l'implementazione completa richiederebbe 19 file nuovi e 12 file modificati.

### 3.1 File nuovi

| # | File | Responsabilità |
|---:|---|---|
| 1 | `Authorization/AuthorizationPolicies.cs` | Nomi centralizzati delle policy |
| 2 | `Dtos/Domain/Requests/AddressRequest.cs` | Indirizzo ricevuto via HTTP |
| 3 | `Dtos/Domain/Requests/AddressRequestValidator.cs` | Validazione dell'indirizzo |
| 4 | `Dtos/Domain/Requests/CompletePersonProfileRequest.cs` | Dati del caregiver |
| 5 | `Dtos/Domain/Requests/CompletePersonProfileRequestValidator.cs` | Validazione profilo caregiver |
| 6 | `Dtos/Domain/Requests/CompleteAssociationProfileRequest.cs` | Dati dell'associazione |
| 7 | `Dtos/Domain/Requests/CompleteAssociationProfileRequestValidator.cs` | Validazione profilo associazione |
| 8 | `Controllers/Domain/ProfilesController.cs` | Endpoint profilo caregiver/associazione |
| 9 | `Controllers/Domain/Admin/AssociationAccreditationController.cs` | Endpoint amministrativi di accreditamento |
| 10 | `Services/Domain/AssociationAccessService.cs` | Gate operativo delle associazioni |
| 11 | `Dtos/Auth/Requests/ResendVerificationEmailRequest.cs` | Richiesta di reinvio verifica |
| 12 | `Dtos/Auth/Requests/ResendVerificationEmailRequestValidator.cs` | Validazione reinvio verifica |
| 13 | `Models/Auth/EmailChangeToken.cs` | Token per confermare la nuova e-mail |
| 14 | `Dtos/Auth/Requests/ChangeEmailRequest.cs` | Avvio del cambio e-mail |
| 15 | `Dtos/Auth/Requests/ChangeEmailRequestValidator.cs` | Validazione avvio cambio e-mail |
| 16 | `Dtos/Auth/Requests/ConfirmEmailChangeRequest.cs` | Conferma del cambio e-mail |
| 17 | `Dtos/Auth/Requests/ConfirmEmailChangeRequestValidator.cs` | Validazione token di conferma |
| 18 | `Data/Migrations/<timestamp>_AddEmailChangeToken.cs` | Migrazione generata da EF Core |
| 19 | `Data/Migrations/<timestamp>_AddEmailChangeToken.Designer.cs` | Metadati della migrazione EF Core |

### 3.2 File modificati

| # | File | Modifica |
|---:|---|---|
| 1 | `Program.cs` | Claim JWT, policy e validator |
| 2 | `DependencyInjection.cs` | Registrazione `AssociationAccessService` |
| 3 | `Models/Enums/EAccountRole.cs` | Aggiunta del ruolo `Admin` |
| 4 | `Services/Provisioning/ProfileProvisioningService.cs` | Completamento robusto, accredita, rifiuta |
| 5 | `Models/Domain/Association.cs` | Transizioni di stato e cambio e-mail |
| 6 | `Models/Domain/Person.cs` | Cambio e-mail |
| 7 | `Services/Auth/AuthService.cs` | Reinvio verifica e cambio e-mail |
| 8 | `Controllers/Auth/AuthController.cs` | Nuovi endpoint Auth |
| 9 | `Data/GoCareDbContext.cs` | Mapping di `EmailChangeToken` |
| 10 | `Services/Auth/FrontendOptions.cs` | URL frontend per cambio e-mail |
| 11 | `appsettings.json` | Configurazione dell'URL |
| 12 | `Data/Migrations/GoCareDbContextModelSnapshot.cs` | Aggiornamento automatico EF Core |

L'unica funzionalità che modifica lo schema del database è il cambio e-mail, tramite la nuova tabella dei token.

---

## 4. Policy di autorizzazione

### 4.1 Costanti delle policy

Nuovo file `Authorization/AuthorizationPolicies.cs`:

```csharp
namespace GoCare.Authorization;

public static class AuthorizationPolicies
{
    public const string Person = "Person";
    public const string Association = "Association";
    public const string Admin = "Admin";
}
```

La classe evita stringhe duplicate nei controller e riduce il rischio di errori di battitura.

### 4.2 Ruolo amministratore

Modifica di `Models/Enums/EAccountRole.cs`:

```csharp
namespace GoCare.Models.Enums;

public enum EAccountRole
{
    Person,
    Association,
    Admin
}
```

Il ruolo è salvato come stringa, quindi aggiungere `Admin` non richiede una migrazione. Non deve essere creato un endpoint pubblico di registrazione amministratore. Il primo account Admin dovrà essere creato tramite seed controllato o procedura operativa separata.

### 4.3 Configurazione JWT e policy

In `Program.cs` vanno aggiunti:

```csharp
using GoCare.Authorization;
using GoCare.Models.Enums;
```

La configurazione JWT viene resa esplicita:

```csharp
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });
```

`MapInboundClaims = false` evita la conversione implicita dei nomi dei claim da parte del middleware. Il codice può quindi leggere direttamente `sub` e `role`, esattamente come sono stati emessi da `TokenService`.

Le policy diventano:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.Person,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireClaim("sub")
            .RequireRole(nameof(EAccountRole.Person)));

    options.AddPolicy(
        AuthorizationPolicies.Association,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireClaim("sub")
            .RequireRole(nameof(EAccountRole.Association)));

    options.AddPolicy(
        AuthorizationPolicies.Admin,
        policy => policy
            .RequireAuthenticatedUser()
            .RequireClaim("sub")
            .RequireRole(nameof(EAccountRole.Admin)));
});
```

Queste policy verificano il tipo di account, non l'accreditamento. L'accreditamento dipende dal database e viene controllato nei Service di dominio.

---

## 5. Completamento del profilo

### 5.1 DTO indirizzo

Nuovo `Dtos/Domain/Requests/AddressRequest.cs`:

```csharp
namespace GoCare.Dtos.Domain.Requests;

public sealed record AddressRequest(
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province);
```

Il DTO HTTP non riutilizza direttamente il Model `Address`. Il controller effettua il mapping esplicito dal contratto esterno al tipo di dominio.

Nuovo `AddressRequestValidator.cs`:

```csharp
using FluentValidation;

namespace GoCare.Dtos.Domain.Requests;

public sealed class AddressRequestValidator : AbstractValidator<AddressRequest>
{
    public AddressRequestValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(120);
    }
}
```

Le lunghezze corrispondono ai limiti già configurati nel `GoCareDbContext`.

### 5.2 Profilo caregiver

Nuovo `CompletePersonProfileRequest.cs`:

```csharp
namespace GoCare.Dtos.Domain.Requests;

public sealed record CompletePersonProfileRequest(
    string Name,
    string Surname,
    DateOnly BirthDate,
    string Phone);
```

Nuovo `CompletePersonProfileRequestValidator.cs`:

```csharp
using FluentValidation;

namespace GoCare.Dtos.Domain.Requests;

public sealed class CompletePersonProfileRequestValidator
    : AbstractValidator<CompletePersonProfileRequest>
{
    public CompletePersonProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Surname)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La data di nascita deve essere nel passato.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(32);
    }
}
```

### 5.3 Profilo associazione

Nuovo `CompleteAssociationProfileRequest.cs`:

```csharp
namespace GoCare.Dtos.Domain.Requests;

public sealed record CompleteAssociationProfileRequest(
    string Name,
    AddressRequest Headquarter,
    List<string> Phones,
    List<string> CoveredProvinces);
```

Nuovo `CompleteAssociationProfileRequestValidator.cs`:

```csharp
using FluentValidation;

namespace GoCare.Dtos.Domain.Requests;

public sealed class CompleteAssociationProfileRequestValidator
    : AbstractValidator<CompleteAssociationProfileRequest>
{
    public CompleteAssociationProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Headquarter)
            .NotNull()
            .SetValidator(new AddressRequestValidator());

        RuleFor(x => x.Phones)
            .NotEmpty()
            .WithMessage("Inserire almeno un numero di telefono.");

        RuleForEach(x => x.Phones)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.CoveredProvinces)
            .NotEmpty()
            .WithMessage("Inserire almeno una provincia coperta.");

        RuleForEach(x => x.CoveredProvinces)
            .NotEmpty()
            .MaximumLength(120);
    }
}
```

### 5.4 ProfilesController

Nuovo `Controllers/Domain/ProfilesController.cs`:

```csharp
using System.Security.Claims;

using GoCare.Authorization;
using GoCare.Dtos.Domain.Requests;
using GoCare.Models.Domain;
using GoCare.Services.Provisioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain;

[ApiController]
public sealed class ProfilesController(
    ProfileProvisioningService provisioning) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.Person)]
    [HttpPatch("/me/profile")]
    public async Task<IActionResult> CompletePersonProfile(
        CompletePersonProfileRequest request,
        CancellationToken ct)
    {
        await provisioning.CompletePersonProfileAsync(
            GetAccountId(),
            new PersonProfileData(
                request.Name,
                request.Surname,
                request.BirthDate,
                request.Phone),
            ct);

        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Association)]
    [HttpPatch("/association/profile")]
    public async Task<IActionResult> CompleteAssociationProfile(
        CompleteAssociationProfileRequest request,
        CancellationToken ct)
    {
        var address = new Address(
            request.Headquarter.Street,
            request.Headquarter.Number,
            request.Headquarter.PostalCode,
            request.Headquarter.City,
            request.Headquarter.Province);

        await provisioning.CompleteAssociationProfileAsync(
            GetAccountId(),
            new AssociationProfileData(
                request.Name,
                address,
                request.Phones,
                request.CoveredProvinces),
            ct);

        return NoContent();
    }

    private Guid GetAccountId()
    {
        var subject = User.FindFirstValue("sub");

        if (!Guid.TryParse(subject, out var accountId))
            throw new InvalidOperationException(
                "Il token autenticato non contiene un identificativo valido.");

        return accountId;
    }
}
```

Il controller svolge solo responsabilità HTTP:

- applica la policy corretta;
- legge l'account dal claim `sub`;
- converte i DTO nei tipi applicativi e di dominio;
- chiama un solo metodo di Service;
- restituisce `204 No Content`.

### 5.5 Miglioramento del ProfileProvisioningService

I metodi esistenti usano `SingleAsync`. Se il profilo non esistesse, EF lancerebbe un'eccezione generica trasformata in HTTP 500. È preferibile restituire un errore applicativo 404:

```csharp
public async Task CompletePersonProfileAsync(
    Guid accountId,
    PersonProfileData data,
    CancellationToken ct)
{
    var person = await db.Persons.SingleOrDefaultAsync(
        person => person.Id == accountId,
        ct)
        ?? throw new NotFoundException("Profilo caregiver non trovato.");

    person.CompleteProfile(
        data.Name,
        data.Surname,
        data.BirthDate,
        data.Phone);

    await db.SaveChangesAsync(ct);
}

public async Task CompleteAssociationProfileAsync(
    Guid accountId,
    AssociationProfileData data,
    CancellationToken ct)
{
    var association = await db.Associations.SingleOrDefaultAsync(
        association => association.Id == accountId,
        ct)
        ?? throw new NotFoundException("Profilo associazione non trovato.");

    association.CompleteProfile(
        data.Name,
        data.Headquarter,
        data.Phones,
        data.CoveredProvinces);

    await db.SaveChangesAsync(ct);
}
```

---

## 6. Accreditamento delle associazioni

### 6.1 Transizioni nel Model

In `Models/Domain/Association.cs`:

```csharp
public void Accredit()
{
    if (Status is not EAccreditationStatus.Pending)
        throw new InvalidOperationException(
            "Solo un'associazione in attesa può essere accreditata.");

    Status = EAccreditationStatus.Accredited;
}

public void Reject()
{
    if (Status is not EAccreditationStatus.Pending)
        throw new InvalidOperationException(
            "Solo un'associazione in attesa può essere rifiutata.");

    Status = EAccreditationStatus.Rejected;
}
```

Le guardie vengono ripetute anche nel Model per impedire transizioni invalide da qualunque punto del codice. Il Service esegue prima i controlli e produce eccezioni applicative traducibili in `ProblemDetails`.

### 6.2 Metodi di provisioning

In `ProfileProvisioningService`:

```csharp
public async Task AccreditAssociationAsync(
    Guid associationId,
    CancellationToken ct)
{
    var association = await db.Associations.SingleOrDefaultAsync(
        item => item.Id == associationId,
        ct)
        ?? throw new NotFoundException("Associazione non trovata.");

    if (!association.IsProfileComplete)
        throw new ConflictException(
            "Il profilo deve essere completato prima dell'accreditamento.");

    if (association.Status is not EAccreditationStatus.Pending)
        throw new ConflictException(
            "Solo un'associazione in attesa può essere accreditata.");

    association.Accredit();
    await db.SaveChangesAsync(ct);
}

public async Task RejectAssociationAsync(
    Guid associationId,
    CancellationToken ct)
{
    var association = await db.Associations.SingleOrDefaultAsync(
        item => item.Id == associationId,
        ct)
        ?? throw new NotFoundException("Associazione non trovata.");

    if (association.Status is not EAccreditationStatus.Pending)
        throw new ConflictException(
            "Solo un'associazione in attesa può essere rifiutata.");

    association.Reject();
    await db.SaveChangesAsync(ct);
}
```

`409 Conflict` descrive correttamente il tentativo di eseguire una transizione incompatibile con lo stato corrente.

### 6.3 Controller amministrativo

Nuovo `Controllers/Domain/Admin/AssociationAccreditationController.cs`:

```csharp
using GoCare.Authorization;
using GoCare.Services.Provisioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain.Admin;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Admin)]
[Route("admin/associations")]
public sealed class AssociationAccreditationController(
    ProfileProvisioningService provisioning) : ControllerBase
{
    [HttpPost("{associationId:guid}/accredit")]
    public async Task<IActionResult> Accredit(
        Guid associationId,
        CancellationToken ct)
    {
        await provisioning.AccreditAssociationAsync(associationId, ct);
        return NoContent();
    }

    [HttpPost("{associationId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid associationId,
        CancellationToken ct)
    {
        await provisioning.RejectAssociationAsync(associationId, ct);
        return NoContent();
    }
}
```

Entrambi gli endpoint sono inaccessibili agli account caregiver e associazione.

---

## 7. Gate operativo delle associazioni

Nuovo `Services/Domain/AssociationAccessService.cs`:

```csharp
using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Domain;

public sealed class AssociationAccessService(GoCareDbContext db)
{
    public async Task EnsureCanOperateAsync(
        Guid associationId,
        CancellationToken ct)
    {
        var association = await db.Associations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == associationId,
                ct)
            ?? throw new NotFoundException("Associazione non trovata.");

        if (!association.IsProfileComplete)
            throw new ForbiddenException(
                "Completare il profilo prima di utilizzare le funzioni operative.");

        if (association.Status is EAccreditationStatus.Pending)
            throw new ForbiddenException(
                "L'associazione è ancora in attesa di accreditamento.");

        if (association.Status is EAccreditationStatus.Rejected)
            throw new ForbiddenException(
                "L'accreditamento dell'associazione è stato rifiutato.");
    }
}
```

Il servizio usa `AsNoTracking` perché effettua solo una verifica. Va registrato in `DependencyInjection.cs`:

```csharp
services.AddScoped<AssociationAccessService>();
```

I futuri Service operativi lo richiameranno all'inizio dei metodi protetti:

```csharp
public async Task AcceptAsync(
    Guid associationId,
    Guid transportRequestId,
    CancellationToken ct)
{
    await associationAccess.EnsureCanOperateAsync(associationId, ct);

    // Logica di accettazione della richiesta.
}
```

Il gate va applicato alle funzioni operative, non al login e non al completamento del profilo.

---

## 8. Reinvio della verifica e-mail

### 8.1 DTO e validator

Nuovo `ResendVerificationEmailRequest.cs`:

```csharp
namespace GoCare.Dtos.Auth.Requests;

public sealed record ResendVerificationEmailRequest(string Email);
```

Nuovo `ResendVerificationEmailRequestValidator.cs`:

```csharp
using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class ResendVerificationEmailRequestValidator
    : AbstractValidator<ResendVerificationEmailRequest>
{
    public ResendVerificationEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
```

### 8.2 AuthService

```csharp
public async Task ResendVerificationEmailAsync(
    string email,
    CancellationToken ct)
{
    var account = await db.Accounts.SingleOrDefaultAsync(
        item => item.Email == email,
        ct);

    if (account is null || account.Status is not EAccountStatus.Unverified)
        return;

    var now = DateTimeOffset.UtcNow;

    var previousTokens = await db.EmailVerificationTokens
        .Where(token =>
            token.AccountId == account.Id &&
            token.ConsumedAt == null &&
            token.ExpiresAt > now)
        .ToListAsync(ct);

    foreach (var previousToken in previousTokens)
        previousToken.Consume(now);

    var newToken = new EmailVerificationToken(
        Guid.NewGuid(),
        account.Id,
        tokenService.GenerateRefreshTokenValue(),
        now.AddHours(24));

    db.EmailVerificationTokens.Add(newToken);
    await db.SaveChangesAsync(ct);

    await emailSender.SendAsync(
        account.Email,
        "Verifica il tuo account GoCare",
        $"<a href=\"{_frontend.VerifyEmailUrl}?token={newToken.Token}\">" +
        "Clicca qui per verificare il tuo account</a>",
        ct);
}
```

La risposta è deliberatamente neutra: se l'account non esiste o è già verificato, il metodo termina senza errore. Questo evita l'enumerazione degli account registrati.

I precedenti token ancora validi vengono consumati prima di crearne uno nuovo. In questo modo rimane valido un solo link alla volta.

### 8.3 Endpoint

Da aggiungere ad `AuthController`:

```csharp
[HttpPost("verify-email/resend")]
public async Task<IActionResult> ResendVerificationEmail(
    ResendVerificationEmailRequest request,
    CancellationToken ct)
{
    await authservice.ResendVerificationEmailAsync(request.Email, ct);
    return NoContent();
}
```

---

## 9. Cambio e-mail

### 9.1 EmailChangeToken

Nuovo `Models/Auth/EmailChangeToken.cs`:

```csharp
namespace GoCare.Models.Auth;

public sealed class EmailChangeToken : IExpirable
{
    private EmailChangeToken() { }

    public EmailChangeToken(
        Guid id,
        Guid accountId,
        string newEmail,
        string token,
        DateTimeOffset expiresAt)
    {
        Id = id;
        AccountId = accountId;
        NewEmail = newEmail;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; }
    public Guid AccountId { get; }
    public string NewEmail { get; } = null!;
    public string Token { get; } = null!;
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset? ConsumedAt { get; private set; }

    public bool IsUsable(DateTimeOffset now) =>
        ConsumedAt is null && !this.IsExpired(now);

    public void Consume(DateTimeOffset at)
    {
        if (!IsUsable(at))
            throw new InvalidOperationException(
                "Il token non è più utilizzabile.");

        ConsumedAt = at;
    }
}
```

`NewEmail` è indispensabile: lega il token all'indirizzo che è stato effettivamente verificato.

### 9.2 Mapping EF

In `GoCareDbContext`:

```csharp
public DbSet<EmailChangeToken> EmailChangeTokens => Set<EmailChangeToken>();
```

Dentro la regione Auth di `OnModelCreating`:

```csharp
modelBuilder.Entity<EmailChangeToken>(token =>
{
    token.HasKey(item => item.Id);

    token.Property(item => item.NewEmail)
        .HasMaxLength(255);

    token.Property(item => item.Token)
        .HasMaxLength(255);

    token.Property(item => item.ExpiresAt);

    token.HasOne<Account>()
        .WithMany()
        .HasForeignKey(item => item.AccountId)
        .OnDelete(DeleteBehavior.Cascade);

    token.HasIndex(item => item.Token)
        .IsUnique();
});
```

EF Core genererà la tabella e aggiornerà il model snapshot.

### 9.3 DTO e validator

`ChangeEmailRequest.cs`:

```csharp
namespace GoCare.Dtos.Auth.Requests;

public sealed record ChangeEmailRequest(
    string NewEmail,
    string CurrentPassword);
```

`ChangeEmailRequestValidator.cs`:

```csharp
using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class ChangeEmailRequestValidator
    : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.CurrentPassword)
            .NotEmpty();
    }
}
```

`ConfirmEmailChangeRequest.cs`:

```csharp
namespace GoCare.Dtos.Auth.Requests;

public sealed record ConfirmEmailChangeRequest(string Token);
```

`ConfirmEmailChangeRequestValidator.cs`:

```csharp
using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class ConfirmEmailChangeRequestValidator
    : AbstractValidator<ConfirmEmailChangeRequest>
{
    public ConfirmEmailChangeRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
```

La password corrente protegge il cambio e-mail nel caso in cui qualcuno ottenga temporaneamente un access token dell'utente.

### 9.4 Sincronizzazione dell'e-mail nei Model

In `Person`:

```csharp
public void ChangeEmail(string newEmail)
{
    Email = newEmail;
}
```

In `Association`:

```csharp
public void ChangeEmail(string newEmail)
{
    Email = newEmail;
}
```

Questi metodi evitano di esporre setter pubblici e mantengono le modifiche del Model attraverso operazioni esplicite.

### 9.5 Configurazione frontend

In `FrontendOptions`:

```csharp
public string ChangeEmailUrl { get; set; } = null!;
```

In `appsettings.json`:

```json
"Frontend": {
  "VerifyEmailUrl": "http://localhost:3000/verifica-email",
  "ResetPasswordUrl": "http://localhost:3000/reimposta-password",
  "ChangeEmailUrl": "http://localhost:3000/conferma-cambio-email"
}
```

Il link apre il frontend. Sarà il frontend a inviare il token al backend tramite POST, come già avviene per la verifica dell'account.

### 9.6 Avvio del cambio e-mail

Da aggiungere ad `AuthService`:

```csharp
public async Task RequestEmailChangeAsync(
    Guid accountId,
    string newEmail,
    string currentPassword,
    CancellationToken ct)
{
    var account = await db.Accounts.SingleOrDefaultAsync(
        item => item.Id == accountId,
        ct)
        ?? throw new NotFoundException("Account non trovato.");

    if (!account.CanLogIn)
        throw new ForbiddenException("Account non attivo.");

    if (!passwordService.Verify(currentPassword, account.PasswordHash))
        throw new ForbiddenException("Password non valida.");

    if (string.Equals(account.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        throw new ConflictException("La nuova e-mail coincide con quella attuale.");

    if (await db.Accounts.AnyAsync(
            item => item.Email == newEmail && item.Id != accountId,
            ct))
    {
        throw new ConflictException("E-mail già registrata.");
    }

    var now = DateTimeOffset.UtcNow;

    var previousTokens = await db.EmailChangeTokens
        .Where(token =>
            token.AccountId == accountId &&
            token.ConsumedAt == null &&
            token.ExpiresAt > now)
        .ToListAsync(ct);

    foreach (var previousToken in previousTokens)
        previousToken.Consume(now);

    var token = new EmailChangeToken(
        Guid.NewGuid(),
        accountId,
        newEmail,
        tokenService.GenerateRefreshTokenValue(),
        now.AddHours(1));

    db.EmailChangeTokens.Add(token);
    await db.SaveChangesAsync(ct);

    await emailSender.SendAsync(
        newEmail,
        "Conferma la nuova e-mail GoCare",
        $"<a href=\"{_frontend.ChangeEmailUrl}?token={token.Token}\">" +
        "Conferma il cambio di e-mail</a>",
        ct);
}
```

In questa fase l'indirizzo corrente non viene modificato. Il token ha validità di un'ora e gli eventuali token precedenti ancora attivi vengono invalidati.

### 9.7 Conferma del cambio e-mail

Sempre in `AuthService`:

```csharp
public async Task ConfirmEmailChangeAsync(
    string tokenValue,
    CancellationToken ct)
{
    var token = await db.EmailChangeTokens.SingleOrDefaultAsync(
        item => item.Token == tokenValue,
        ct)
        ?? throw new NotFoundException(
            "Token di cambio e-mail non trovato.");

    var now = DateTimeOffset.UtcNow;

    if (!token.IsUsable(now))
        throw new ForbiddenException(
            "Token di cambio e-mail non valido o scaduto.");

    var account = await db.Accounts.SingleOrDefaultAsync(
        item => item.Id == token.AccountId,
        ct)
        ?? throw new NotFoundException("Account non trovato.");

    if (!account.CanLogIn)
        throw new ForbiddenException("Account non attivo.");

    if (await db.Accounts.AnyAsync(
            item => item.Email == token.NewEmail && item.Id != account.Id,
            ct))
    {
        throw new ConflictException("E-mail già registrata.");
    }

    account.ChangeEmail(token.NewEmail, now);

    switch (account.Role)
    {
        case EAccountRole.Person:
        {
            var person = await db.Persons.SingleOrDefaultAsync(
                item => item.Id == account.Id,
                ct)
                ?? throw new NotFoundException(
                    "Profilo caregiver non trovato.");

            person.ChangeEmail(token.NewEmail);
            break;
        }

        case EAccountRole.Association:
        {
            var association = await db.Associations.SingleOrDefaultAsync(
                item => item.Id == account.Id,
                ct)
                ?? throw new NotFoundException(
                    "Profilo associazione non trovato.");

            association.ChangeEmail(token.NewEmail);
            break;
        }
    }

    token.Consume(now);

    var sessions = await db.RefreshTokens
        .Where(item =>
            item.AccountId == account.Id &&
            item.RevokedAt == null)
        .ToListAsync(ct);

    foreach (var session in sessions)
        session.Revoke(now);

    await db.SaveChangesAsync(ct);
}
```

Un unico `SaveChangesAsync` rende atomiche le modifiche tracciate dal `DbContext`: account, profilo, token e sessioni vengono aggiornati insieme.

La verifica dell'unicità viene ripetuta al momento della conferma perché un altro account potrebbe avere occupato l'indirizzo durante l'attesa. Il vincolo univoco sul database resta comunque l'ultima protezione contro richieste concorrenti.

### 9.8 Endpoint di cambio e-mail

Da aggiungere ad `AuthController`:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
```

```csharp
[Authorize]
[HttpPost("change-email")]
public async Task<IActionResult> ChangeEmail(
    ChangeEmailRequest request,
    CancellationToken ct)
{
    var subject = User.FindFirstValue("sub");

    if (!Guid.TryParse(subject, out var accountId))
        throw new InvalidOperationException(
            "Identificativo account non valido.");

    await authservice.RequestEmailChangeAsync(
        accountId,
        request.NewEmail,
        request.CurrentPassword,
        ct);

    return NoContent();
}

[HttpPost("change-email/confirm")]
public async Task<IActionResult> ConfirmEmailChange(
    ConfirmEmailChangeRequest request,
    CancellationToken ct)
{
    await authservice.ConfirmEmailChangeAsync(request.Token, ct);
    return NoContent();
}
```

Il primo endpoint richiede una sessione autenticata. Il secondo è pubblico perché il possesso del token monouso inviato alla nuova casella costituisce la prova richiesta.

---

## 10. Registrazione dei validator

Il `ValidationFilter` non scopre automaticamente i validator. Tutti devono essere registrati esplicitamente in `Program.cs`:

```csharp
builder.Services.AddScoped<
    IValidator<CompletePersonProfileRequest>,
    CompletePersonProfileRequestValidator>();

builder.Services.AddScoped<
    IValidator<CompleteAssociationProfileRequest>,
    CompleteAssociationProfileRequestValidator>();

builder.Services.AddScoped<
    IValidator<ResendVerificationEmailRequest>,
    ResendVerificationEmailRequestValidator>();

builder.Services.AddScoped<
    IValidator<ChangeEmailRequest>,
    ChangeEmailRequestValidator>();

builder.Services.AddScoped<
    IValidator<ConfirmEmailChangeRequest>,
    ConfirmEmailChangeRequestValidator>();
```

`AddressRequestValidator` non deve essere registrato globalmente perché viene richiamato esplicitamente dal validator del profilo associazione.

---

## 11. Route risultanti

| Metodo | Route | Accesso | Effetto |
|---|---|---|---|
| `PATCH` | `/me/profile` | Caregiver autenticato | Completa o aggiorna il profilo caregiver |
| `PATCH` | `/association/profile` | Associazione autenticata | Completa o aggiorna il profilo associazione |
| `POST` | `/admin/associations/{id}/accredit` | Admin | Accredita un'associazione Pending |
| `POST` | `/admin/associations/{id}/reject` | Admin | Rifiuta un'associazione Pending |
| `POST` | `/auth/verify-email/resend` | Pubblico | Invia un nuovo token con risposta neutra |
| `POST` | `/auth/change-email` | Autenticato | Avvia il cambio e-mail |
| `POST` | `/auth/change-email/confirm` | Pubblico con token | Conferma e applica la nuova e-mail |

---

## 12. Migrazione necessaria

Dopo l'aggiunta di `EmailChangeToken` e del relativo mapping:

```powershell
dotnet ef migrations add AddEmailChangeToken \
    --project BackEnd/src/GoCare/GoCare.csproj \
    --startup-project BackEnd/src/GoCare/GoCare.csproj \
    --output-dir Data/Migrations
```

La migrazione deve creare una tabella concettualmente equivalente a:

```text
email_change_tokens
├─ id                 uuid, PK
├─ account_id         uuid, FK -> accounts.id, cascade
├─ new_email          varchar(255)
├─ token              varchar(255), unique
├─ expires_at         timestamptz
└─ consumed_at        timestamptz, nullable
```

Prima di applicarla bisogna controllare il file generato e il model snapshot.

---

## 13. Test consigliati

### Completamento profilo

- caregiver autenticato completa il proprio profilo;
- associazione autenticata completa il proprio profilo;
- caregiver non può modificare il profilo associazione;
- associazione non può modificare il profilo caregiver;
- token senza `sub` viene rifiutato;
- richiesta con campi mancanti restituisce 422;
- profilo inesistente restituisce 404.

### Accreditamento

- Admin accredita un'associazione Pending con profilo completo;
- non Admin riceve 403;
- profilo incompleto produce 409;
- accreditamento ripetuto produce 409;
- rifiuto ripetuto produce 409;
- il rifiuto non sospende l'Account.

### Gate operativo

- associazione Accredited e completa può operare;
- associazione Pending riceve 403;
- associazione Rejected riceve 403;
- associazione con profilo incompleto riceve 403;
- il gate non impedisce il login o il completamento del profilo.

### Reinvio verifica

- account Unverified riceve un nuovo token;
- i vecchi token attivi vengono invalidati;
- account inesistente produce comunque 204;
- account già verificato produce comunque 204;
- token generato scade dopo 24 ore.

### Cambio e-mail

- password errata produce 403;
- nuova e-mail già usata produce 409;
- nuova e-mail uguale alla corrente produce 409;
- l'indirizzo non cambia prima della conferma;
- token scaduto o consumato viene rifiutato;
- la conferma aggiorna sia Account sia profilo;
- la conferma consuma il token;
- la conferma revoca tutte le sessioni refresh;
- un secondo utilizzo del token viene rifiutato;
- una collisione sull'indice univoco non lascia dati parzialmente aggiornati.

---

## 14. Ordine di implementazione suggerito

1. policy e ruolo Admin;
2. DTO, validator e controller dei profili;
3. miglioramento dei metodi esistenti di provisioning;
4. transizioni `Accredit`/`Reject` e controller Admin;
5. `AssociationAccessService`;
6. reinvio verifica e-mail;
7. modello e flusso di cambio e-mail;
8. migrazione EF;
9. test automatici;
10. aggiornamento del piano backend soltanto dopo verifica completa.

Questo ordine permette di verificare separatamente ogni gruppo funzionale e lascia la modifica dello schema verso la fine, quando il comportamento applicativo è già definito.
