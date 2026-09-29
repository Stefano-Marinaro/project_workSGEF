# Proposta minima – completamento area Auth e provisioning

> Stato: implementato il 25 settembre 2026.
>
> Il codice sorgente e la configurazione sono stati aggiornati; la migrazione EF Core è stata generata ma non applicata al database.
>
> Fuori perimetro: `AnonymizeForDeletedAccountAsync` ed eliminazione account.

---

## 1. Perimetro

Questa proposta implementa esclusivamente:

1. `PATCH /me/profile` per completare o aggiornare il profilo caregiver;
2. `PATCH /association/profile` per completare o aggiornare il profilo associazione;
3. scelta del gate di accreditamento;
4. accreditamento e rifiuto manuale delle associazioni;
5. `POST /auth/verify-email/resend`;
6. cambio e-mail sicuro con richiesta e conferma.

Non vengono introdotti:

- anonimizzazione o cancellazione account;
- `AssociationAccessService`;
- nuovi Service di dominio;
- repository, MediatR o CQRS;
- blocco del login per le associazioni non accreditate;
- un endpoint pubblico per creare amministratori.

---

## 2. Conteggio definitivo

La versione minima richiede:

- **15 file nuovi**;
- **11 file modificati**;
- **26 file coinvolti complessivamente**.

Dei 15 file nuovi:

- 13 sono file sorgente scritti manualmente;
- 2 sono generati da EF Core per la migrazione.

Degli 11 file modificati:

- 10 vengono modificati manualmente;
- 1, `GoCareDbContextModelSnapshot.cs`, viene aggiornato automaticamente da EF Core.

L'unica modifica allo schema del database è la nuova tabella `email_change_tokens`.

---

## 3. File nuovi – 15

### 3.1 Completamento profili – 5 file

| # | File | Scopo |
|---:|---|---|
| 1 | `Dtos/Domain/Requests/CompletePersonProfileRequest.cs` | Contratto HTTP del profilo caregiver |
| 2 | `Dtos/Domain/Requests/CompletePersonProfileRequestValidator.cs` | Validazione dei dati caregiver |
| 3 | `Dtos/Domain/Requests/CompleteAssociationProfileRequest.cs` | Contratto HTTP del profilo associazione e indirizzo |
| 4 | `Dtos/Domain/Requests/CompleteAssociationProfileRequestValidator.cs` | Validazione di associazione e indirizzo |
| 5 | `Controllers/Domain/ProfilesController.cs` | Espone i due endpoint autenticati |

### 3.2 Accreditamento – 1 file

| # | File | Scopo |
|---:|---|---|
| 6 | `Controllers/Domain/Admin/AssociationAccreditationController.cs` | Accredita o rifiuta un'associazione |

Non viene creato un Service aggiuntivo: le operazioni restano in `ProfileProvisioningService`, come previsto dal piano.

### 3.3 Reinvio verifica e-mail – 2 file

| # | File | Scopo |
|---:|---|---|
| 7 | `Dtos/Auth/Requests/ResendVerificationEmailRequest.cs` | Contiene l'e-mail richiesta |
| 8 | `Dtos/Auth/Requests/ResendVerificationEmailRequestValidator.cs` | Valida formato e presenza e-mail |

### 3.4 Cambio e-mail – 5 file

| # | File | Scopo |
|---:|---|---|
| 9 | `Models/Auth/EmailChangeToken.cs` | Token monouso associato alla nuova e-mail |
| 10 | `Dtos/Auth/Requests/ChangeEmailRequest.cs` | Nuova e-mail e password corrente |
| 11 | `Dtos/Auth/Requests/ChangeEmailRequestValidator.cs` | Valida la richiesta iniziale |
| 12 | `Dtos/Auth/Requests/ConfirmEmailChangeRequest.cs` | Token ricevuto dalla nuova casella |
| 13 | `Dtos/Auth/Requests/ConfirmEmailChangeRequestValidator.cs` | Valida il token di conferma |

### 3.5 Migrazione EF Core – 2 file

| # | File | Scopo |
|---:|---|---|
| 14 | `Data/Migrations/<timestamp>_AddEmailChangeToken.cs` | Crea la nuova tabella |
| 15 | `Data/Migrations/<timestamp>_AddEmailChangeToken.Designer.cs` | Metadati generati da EF Core |

Il prefisso `<timestamp>` sarà scelto automaticamente da EF Core.

---

## 4. File modificati – 11

| # | File | Modifiche |
|---:|---|---|
| 1 | `Program.cs` | Claim JWT, ruolo e registrazione dei nuovi validator |
| 2 | `Models/Enums/EAccountRole.cs` | Aggiunta del ruolo `Admin` |
| 3 | `Services/Provisioning/ProfileProvisioningService.cs` | Profili, accreditamento e rifiuto |
| 4 | `Models/Domain/Association.cs` | `CanOperate`, `Accredit`, `Reject`, `ChangeEmail` |
| 5 | `Models/Domain/Person.cs` | `ChangeEmail` |
| 6 | `Services/Auth/AuthService.cs` | Reinvio verifica e cambio e-mail |
| 7 | `Controllers/Auth/AuthController.cs` | Tre nuovi endpoint Auth |
| 8 | `Data/GoCareDbContext.cs` | `DbSet` e mapping di `EmailChangeToken` |
| 9 | `Services/Auth/FrontendOptions.cs` | Nuovo `ChangeEmailUrl` |
| 10 | `appsettings.json` | Valore di `ChangeEmailUrl` |
| 11 | `Data/Migrations/GoCareDbContextModelSnapshot.cs` | Aggiornamento automatico EF Core |

Non vengono modificati:

- `Account.cs`, perché contiene già `ChangeEmail`;
- `TokenService.cs`, perché il generatore di token opachi esistente è riutilizzabile;
- `DependencyInjection.cs`, perché non viene introdotto alcun nuovo Service;
- `EmailVerificationToken.cs`, perché il reinvio usa l'entità esistente;
- `GoCare.csproj`, perché non servono nuovi pacchetti.

---

## 5. Decisione sul gate di accreditamento

La soluzione minima adotta questa regola:

> Un'associazione verificata può effettuare il login, ma non può eseguire operazioni sui trasporti finché non è accreditata.

Il login resta consentito alle associazioni `Pending` o `Rejected` perché devono poter completare il profilo e consultare il proprio stato.

Nel Model `Association` viene aggiunta la proprietà:

```csharp
public bool CanOperate =>
    IsProfileComplete &&
    Status is EAccreditationStatus.Accredited;
```

Quando verranno scritti i Service operativi, il controllo sarà effettuato al loro ingresso:

```csharp
if (!association.CanOperate)
    throw new ForbiddenException(
        "L'associazione non è abilitata a operare.");
```

Non viene creato adesso `AssociationAccessService`, perché al momento non esiste nessun Service operativo che possa utilizzarlo. Il controllo viene predisposto nel Model e sarà applicato nei futuri `AssociationRequestService`, `AcceptedTransportService` e `TripStatusService`.

`AuthService.LoginAsync` non cambia.

---

## 6. Autorizzazione e ruolo Admin

### 6.1 EAccountRole

`EAccountRole.cs` diventa:

```csharp
namespace GoCare.Models.Enums;

public enum EAccountRole
{
    Person,
    Association,
    Admin
}
```

Gli enum vengono salvati come stringa, quindi questa aggiunta non richiede una migrazione.

Il ruolo Admin è necessario perché gli endpoint di accreditamento non possono essere esposti a caregiver e associazioni. Non viene aggiunta una registrazione pubblica per Admin: il relativo account dovrà essere creato mediante seed o procedura controllata.

### 6.2 Configurazione JWT

In `Program.cs`, la configurazione JWT viene resa esplicita:

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

`MapInboundClaims = false` mantiene i nomi originali `sub` e `role` emessi da `TokenService`. `RoleClaimType = "role"` permette a `[Authorize(Roles = ...)]` di funzionare senza conversioni implicite.

---

## 7. Completamento del profilo caregiver

### 7.1 DTO

Nuovo `CompletePersonProfileRequest.cs`:

```csharp
namespace GoCare.Dtos.Domain.Requests;

public sealed record CompletePersonProfileRequest(
    string Name,
    string Surname,
    DateOnly BirthDate,
    string Phone);
```

### 7.2 Validator

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

Le lunghezze sono allineate al mapping corrente del database.

---

## 8. Completamento del profilo associazione

### 8.1 DTO

Nuovo `CompleteAssociationProfileRequest.cs`:

```csharp
namespace GoCare.Dtos.Domain.Requests;

public sealed record CompleteAssociationProfileRequest(
    string Name,
    AddressRequest Headquarter,
    List<string> Phones,
    List<string> CoveredProvinces);

public sealed record AddressRequest(
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province);
```

`AddressRequest` rimane nello stesso file per ridurre il numero di file della versione minima.

### 8.2 Validator

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

internal sealed class AddressRequestValidator
    : AbstractValidator<AddressRequest>
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

Anche il validator dell'indirizzo è nello stesso file per mantenere minima la struttura.

---

## 9. ProfilesController

Nuovo `Controllers/Domain/ProfilesController.cs`:

```csharp
using System.Security.Claims;

using GoCare.Dtos.Domain.Requests;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Provisioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain;

[ApiController]
public sealed class ProfilesController(
    ProfileProvisioningService provisioning) : ControllerBase
{
    [Authorize(Roles = nameof(EAccountRole.Person))]
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

    [Authorize(Roles = nameof(EAccountRole.Association))]
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
                "Il token non contiene un identificativo account valido.");

        return accountId;
    }
}
```

Il controller legge l'identificativo dal token, mappa i DTO nei tipi applicativi e chiama un solo metodo del Service.

---

## 10. ProfileProvisioningService

I metodi di completamento esistenti vengono resi più robusti sostituendo `SingleAsync` con `SingleOrDefaultAsync` e `NotFoundException`:

```csharp
public async Task CompletePersonProfileAsync(
    Guid accountId,
    PersonProfileData data,
    CancellationToken ct)
{
    var person = await db.Persons.SingleOrDefaultAsync(
        item => item.Id == accountId,
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
        item => item.Id == accountId,
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

Nello stesso Service vengono aggiunti:

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

Un profilo incompleto non può essere accreditato. Una transizione incompatibile con lo stato corrente restituisce `409 Conflict`.

---

## 11. Modifiche al Model Association

In `Association.cs` vengono aggiunti:

```csharp
public bool CanOperate =>
    IsProfileComplete &&
    Status is EAccreditationStatus.Accredited;

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

public void ChangeEmail(string newEmail)
{
    Email = newEmail;
}
```

Le transizioni sono protette sia dal Service sia dal Model. `ChangeEmail` mantiene il setter privato e consente una modifica esplicita e controllata.

In `Person.cs` viene aggiunto solamente:

```csharp
public void ChangeEmail(string newEmail)
{
    Email = newEmail;
}
```

---

## 12. Controller amministrativo

Nuovo `Controllers/Domain/Admin/AssociationAccreditationController.cs`:

```csharp
using GoCare.Models.Enums;
using GoCare.Services.Provisioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain.Admin;

[ApiController]
[Authorize(Roles = nameof(EAccountRole.Admin))]
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

Il rifiuto non cambia `Account.Status`: l'associazione continua a poter effettuare il login, ma `CanOperate` resta `false`.

---

## 13. Reinvio della verifica e-mail

### 13.1 DTO e validator

`ResendVerificationEmailRequest.cs`:

```csharp
namespace GoCare.Dtos.Auth.Requests;

public sealed record ResendVerificationEmailRequest(string Email);
```

`ResendVerificationEmailRequestValidator.cs`:

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

### 13.2 AuthService

```csharp
public async Task ResendVerificationEmailAsync(
    string email,
    CancellationToken ct)
{
    var account = await db.Accounts.SingleOrDefaultAsync(
        item => item.Email == email,
        ct);

    if (account is null ||
        account.Status is not EAccountStatus.Unverified)
    {
        return;
    }

    var now = DateTimeOffset.UtcNow;

    var activeTokens = await db.EmailVerificationTokens
        .Where(token =>
            token.AccountId == account.Id &&
            token.ConsumedAt == null &&
            token.ExpiresAt > now)
        .ToListAsync(ct);

    foreach (var activeToken in activeTokens)
        activeToken.Consume(now);

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

Il metodo non rivela se l'account esiste o è già verificato: in entrambi i casi il controller restituisce `204 No Content`. Gli eventuali token precedenti ancora attivi vengono consumati.

### 13.3 AuthController

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

## 14. Cambio e-mail sicuro

Il cambio e-mail viene eseguito in due passaggi:

1. utente autenticato + password corrente richiedono il cambio;
2. il token inviato alla nuova casella conferma il possesso dell'indirizzo.

### 14.1 EmailChangeToken

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

Il token registra `NewEmail`: non è possibile riutilizzare `EmailVerificationToken`, perché quest'ultimo non indica quale nuovo indirizzo sia stato verificato.

### 14.2 DTO e validator

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

### 14.3 GoCareDbContext

Aggiunta del `DbSet`:

```csharp
public DbSet<EmailChangeToken> EmailChangeTokens =>
    Set<EmailChangeToken>();
```

Mapping nella regione Auth:

```csharp
modelBuilder.Entity<EmailChangeToken>(token =>
{
    token.HasKey(item => item.Id);
    token.Property(item => item.NewEmail).HasMaxLength(255);
    token.Property(item => item.Token).HasMaxLength(255);
    token.Property(item => item.ExpiresAt);

    token.HasOne<Account>()
        .WithMany()
        .HasForeignKey(item => item.AccountId)
        .OnDelete(DeleteBehavior.Cascade);

    token.HasIndex(item => item.Token).IsUnique();
});
```

### 14.4 FrontendOptions e appsettings

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

### 14.5 AuthService – richiesta

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

    if (string.Equals(
            account.Email,
            newEmail,
            StringComparison.OrdinalIgnoreCase))
    {
        throw new ConflictException(
            "La nuova e-mail coincide con quella attuale.");
    }

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

La password corrente protegge la modifica in caso di furto temporaneo dell'access token. L'e-mail effettiva non cambia fino alla conferma.

### 14.6 AuthService – conferma

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

    if (account.Role is EAccountRole.Person)
    {
        var person = await db.Persons.SingleOrDefaultAsync(
            item => item.Id == account.Id,
            ct)
            ?? throw new NotFoundException(
                "Profilo caregiver non trovato.");

        person.ChangeEmail(token.NewEmail);
    }
    else if (account.Role is EAccountRole.Association)
    {
        var association = await db.Associations.SingleOrDefaultAsync(
            item => item.Id == account.Id,
            ct)
            ?? throw new NotFoundException(
                "Profilo associazione non trovato.");

        association.ChangeEmail(token.NewEmail);
    }

    token.Consume(now);

    var activeSessions = await db.RefreshTokens
        .Where(item =>
            item.AccountId == account.Id &&
            item.RevokedAt == null)
        .ToListAsync(ct);

    foreach (var activeSession in activeSessions)
        activeSession.Revoke(now);

    await db.SaveChangesAsync(ct);
}
```

Account, profilo, token e sessioni vengono modificati dallo stesso `DbContext` e salvati con un unico `SaveChangesAsync`.

### 14.7 AuthController

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

L'endpoint di conferma è pubblico perché il possesso del token monouso inviato alla nuova casella costituisce la prova richiesta.

---

## 15. Registrazione dei validator

In `Program.cs`:

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

Il `ValidationFilter` cerca i validator nel container DI, quindi la sola esistenza delle classi non è sufficiente.

---

## 16. Migrazione

Dopo avere aggiunto `EmailChangeToken` e il mapping:

```powershell
dotnet ef migrations add AddEmailChangeToken `
    --project BackEnd/src/GoCare/GoCare.csproj `
    --startup-project BackEnd/src/GoCare/GoCare.csproj `
    --output-dir Data/Migrations
```

La tabella risultante conterrà:

```text
email_change_tokens
├─ id            uuid, primary key
├─ account_id    uuid, foreign key -> accounts.id
├─ new_email     varchar(255)
├─ token         varchar(255), unique
├─ expires_at    timestamptz
└─ consumed_at   timestamptz, nullable
```

L'eliminazione fisica dell'account comporterebbe la cancellazione in cascata dei token, coerentemente con gli altri token Auth. Questa configurazione non implementa l'endpoint di eliminazione account.

---

## 17. Route finali

| Metodo | Route | Autorizzazione | Risultato |
|---|---|---|---|
| `PATCH` | `/me/profile` | Ruolo `Person` | Completa o aggiorna il caregiver |
| `PATCH` | `/association/profile` | Ruolo `Association` | Completa o aggiorna l'associazione |
| `POST` | `/admin/associations/{id}/accredit` | Ruolo `Admin` | Porta Pending ad Accredited |
| `POST` | `/admin/associations/{id}/reject` | Ruolo `Admin` | Porta Pending a Rejected |
| `POST` | `/auth/verify-email/resend` | Pubblico | Genera e invia un nuovo token |
| `POST` | `/auth/change-email` | Autenticato | Avvia il cambio e-mail |
| `POST` | `/auth/change-email/confirm` | Token monouso | Applica la nuova e-mail |

---

## 18. Test minimi richiesti

### Profili

- caregiver completa il proprio profilo;
- associazione completa il proprio profilo;
- ruoli incrociati ricevono 403;
- dati invalidi ricevono 422;
- profilo inesistente riceve 404.

### Accreditamento

- Admin accredita un'associazione Pending completa;
- Admin rifiuta un'associazione Pending;
- non Admin riceve 403;
- profilo incompleto non viene accreditato;
- una seconda transizione produce 409;
- il rifiuto non blocca il login;
- `CanOperate` è vero solo per associazioni complete e accreditate.

### Reinvio verifica

- account Unverified riceve un nuovo token;
- i precedenti token attivi vengono consumati;
- account inesistente e account già verificato producono la stessa risposta 204.

### Cambio e-mail

- password errata produce 403;
- e-mail già occupata produce 409;
- l'e-mail non cambia prima della conferma;
- token scaduto o già consumato viene rifiutato;
- la conferma aggiorna Account e profilo;
- la conferma revoca i refresh token;
- una seconda conferma con lo stesso token viene rifiutata.

---

## 19. Riepilogo conclusivo

La versione minima coinvolge esattamente:

```text
15 file nuovi
├─ 5 completamento profili
├─ 1 controller accreditamento
├─ 2 reinvio verifica
├─ 5 cambio e-mail
└─ 2 migrazione EF Core

11 file modificati
├─ 10 modifiche manuali
└─ 1 snapshot EF aggiornato automaticamente
```

Totale: **26 file coinvolti**.

Nessun file relativo ad anonimizzazione o eliminazione account è incluso.
