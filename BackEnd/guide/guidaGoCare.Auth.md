# Guida — `PasswordService`, `TokenService`, `AuthService`, `AccountService`

Istruzioni per costruire i 4 Service dell'area Auth (`Services/Auth/` in `GoCare.Application`).
Riferimento: `docs/PIANO_BACKEND_GoCare.md` §5. Ordine consigliato: segui le sezioni nell'ordine
in cui sono scritte — ognuna dipende dalla precedente.

---

## 0. Prerequisiti — metodi sulle entità

I Service qui sotto chiamano metodi che devono già esistere su `Account` e sui 3 token.
Se non li hai ancora scritti, questi sono i segnali (già discussi):

**`Account`** (`Models/Auth/Account.cs`):
```csharp
public void Verify(DateTimeOffset at);                              // Unverified -> Active, EmailVerifiedAt = at
public void ChangePassword(string newPasswordHash);
public void ChangeEmail(string newEmail, DateTimeOffset verifiedAt); // solo dopo verifica del nuovo indirizzo
public void Suspend(DateTimeOffset at);
public void Reinstate();
public void MarkDeleted(DateTimeOffset at);
public bool CanLogIn { get; }                                        // Status == Active
```

**`EmailVerificationToken` / `PasswordResetToken`** (`Models/Auth/`):
```csharp
public void Consume(DateTimeOffset at);          // guardia: non scaduto, non già consumato
public bool IsUsable(DateTimeOffset now);         // ConsumedAt is null && !this.IsExpired(now)
```

**`RefreshToken`**:
```csharp
public void Revoke(DateTimeOffset at);            // idempotente: RevokedAt ??= at
public bool IsActive(DateTimeOffset now);         // RevokedAt is null && !this.IsExpired(now)
```

Se mancano, scrivili prima (stesso stile delle altre entità: guardia → muta, vedi le
conversazioni precedenti su `Consume`/`Revoke`).

---

## 1. Configurazione JWT

Aggiungi a `src/GoCare.Api/appsettings.json` (o `appsettings.Development.json` per un valore
locale diverso):

```json
{
  "Jwt": {
    "SigningKey": "una-chiave-lunga-almeno-32-caratteri-cambiala-in-produzione",
    "Issuer": "GoCare",
    "Audience": "GoCare.Client",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 30
  }
}
```

Classe di opzioni, in `GoCare.Application` (es. `Services/Auth/JwtOptions.cs`):

```csharp
namespace GoCare.Application.Services.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenMinutes { get; set; }
    public int RefreshTokenDays { get; set; }
}
```

Registrazione in `DependencyInjection.AddApplication`:
```csharp
services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
```

## 2. NuGet da aggiungere a `GoCare.Application.csproj`

```xml
<PackageReference Include="Microsoft.Extensions.Identity.Core" Version="10.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />
```

(`Microsoft.Extensions.Identity.Core`, **non** `...Identity.EntityFrameworkCore`: quello
trascina tutte le tabelle di ASP.NET Identity, che non usiamo — il nostro `Account` è
un'entità nostra, non `IdentityUser`.)

La pipeline di **validazione** del JWT in ingresso (host, `Program.cs`) userà invece
`Microsoft.AspNetCore.Authentication.JwtBearer` — non serve qui, è Fase 4 del piano.

---

## 3. `PasswordService`

```csharp
// Services/Auth/IPasswordService.cs
namespace GoCare.Application.Services.Auth;

public interface IPasswordService
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string hash);
}
```

```csharp
// Services/Auth/PasswordService.cs
using GoCare.Application.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace GoCare.Application.Services.Auth;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<Account> _hasher = new();

    public string Hash(string plainPassword) =>
        _hasher.HashPassword(user: null!, password: plainPassword);

    public bool Verify(string plainPassword, string hash)
    {
        var result = _hasher.VerifyHashedPassword(user: null!, hashedPassword: hash, providedPassword: plainPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
```

`user: null!` è sicuro: `PasswordHasher<T>` non legge davvero l'istanza, serve solo per la
firma generica. `SuccessRehashNeeded` = password corretta ma algoritmo da aggiornare — per
v0 lo trattiamo come successo (rehash automatico è un miglioramento futuro).

---

## 4. `TokenService`

Responsabilità: **generare** stringhe (JWT di accesso + valore opaco di refresh). **Non**
tocca il database — la persistenza del `RefreshToken` (entità) è compito di `AuthService`,
che ha accesso al `DbContext`. Questo lo tiene puro e testabile senza EF.

```csharp
// Services/Auth/ITokenService.cs
using GoCare.Application.Models.Enums;

namespace GoCare.Application.Services.Auth;

public interface ITokenService
{
    string CreateAccessToken(Guid accountId, EAccountRole role, bool emailVerified, DateTimeOffset now);
    string GenerateRefreshTokenValue();
}
```

```csharp
// Services/Auth/TokenService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GoCare.Application.Models.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GoCare.Application.Services.Auth;

public sealed class TokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public string CreateAccessToken(Guid accountId, EAccountRole role, bool emailVerified, DateTimeOffset now)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new Claim("role", role.ToString()),
            new Claim("email_verified", emailVerified ? "true" : "false"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.UtcDateTime.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Stringa opaca, non un JWT: il refresh token non viene mai decodificato lato client,
    // solo confrontato col valore salvato in tabella (RefreshToken.Token, indicizzato unique).
    public string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
```

Note:
- `role` nel claim: stringa (`"Person"`/`"Association"`), coerente col ruolo dei controller.
- `sub` = `Account.Id`, che è anche l'id del profilo di dominio (PK condivisa — vedi
  `[[project_account_shared_pk_with_profile]]`): il resto dell'app non ha bisogno di altri
  claim id.
- Il refresh token è deliberatamente **opaco** (stringa random, non JWT): se fosse un JWT,
  basterebbe conoscerne il contenuto per capirne la validità senza consultare il DB, e non
  potresti revocarlo davvero prima della scadenza. Con un valore opaco, la revoca (`RefreshToken.Revoke`)
  è l'unica fonte di verità.

---

## 5. `AuthService`

Orchestratore: registrazione, login, refresh, logout. Dipende da `PasswordService`,
`TokenService`, `AuthDbContext`, `IClock` (da `GoCare.Shared`), e da un servizio del
**dominio** non ancora scritto — `IProfileProvisioningService.CreateForAccountAsync` (crea
`Person`/`Association` con lo stesso id dell'`Account`). Per ora basta l'interfaccia:

```csharp
// Services/IProfileProvisioningService.cs (dominio — solo la firma che serve qui)
namespace GoCare.Application.Services;

public interface IProfileProvisioningService
{
    Task CreateForAccountAsync(Guid accountId, PersonProvisioningData data, CancellationToken ct);
    Task CreateForAccountAsync(Guid accountId, AssociationProvisioningData data, CancellationToken ct);
}

public sealed record PersonProvisioningData(string Name, string Surname, DateOnly BirthDate, string Email, string Phone);
public sealed record AssociationProvisioningData(string Name, /* indirizzo, telefoni, ... */ string Email);
```

(La implementazione vera, che scrive su `BusinessDbContext`, è lavoro dell'area dominio —
fuori scope di questa guida. Qui basta poterlo iniettare e chiamare.)

```csharp
// Services/Auth/IAuthService.cs
namespace GoCare.Application.Services.Auth;

public interface IAuthService
{
    Task<(string accessToken, string refreshToken)> RegisterUserAsync(
        string email, string password, PersonProvisioningData profile, CancellationToken ct);

    Task<(string accessToken, string refreshToken)> RegisterAssociationAsync(
        string email, string password, AssociationProvisioningData profile, CancellationToken ct);

    Task<(string accessToken, string refreshToken)> LoginAsync(string email, string password, CancellationToken ct);

    Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshTokenValue, CancellationToken ct);

    Task LogoutAsync(string refreshTokenValue, CancellationToken ct);
}
```

```csharp
// Services/Auth/AuthService.cs
using GoCare.Application.Data;
using GoCare.Application.Models.Auth;
using GoCare.Application.Models.Enums;
using GoCare.Shared.Abstractions;
using GoCare.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Application.Services.Auth;

public sealed class AuthService(
    AuthDbContext db,
    IPasswordService passwordService,
    ITokenService tokenService,
    IProfileProvisioningService provisioning,
    IClock clock,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<(string accessToken, string refreshToken)> RegisterUserAsync(
        string email, string password, PersonProvisioningData profile, CancellationToken ct)
    {
        if (await db.Accounts.AnyAsync(a => a.Email == email, ct))
            throw new ConflictException("Email già registrata.");

        var now = clock.UtcNow;
        var id = Guid.NewGuid(); // stesso id per Account e Person (PK condivisa)

        var account = new Account(id, email, passwordService.Hash(password), EAccountRole.Person, now);
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);

        await provisioning.CreateForAccountAsync(id, profile, ct);

        // TODO: creare EmailVerificationToken + invio e-mail (§5.2 del piano)

        return IssueTokens(account, now);
    }

    public async Task<(string accessToken, string refreshToken)> RegisterAssociationAsync(
        string email, string password, AssociationProvisioningData profile, CancellationToken ct)
    {
        if (await db.Accounts.AnyAsync(a => a.Email == email, ct))
            throw new ConflictException("Email già registrata.");

        var now = clock.UtcNow;
        var id = Guid.NewGuid();

        var account = new Account(id, email, passwordService.Hash(password), EAccountRole.Association, now);
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);

        await provisioning.CreateForAccountAsync(id, profile, ct);

        return IssueTokens(account, now);
    }

    public async Task<(string accessToken, string refreshToken)> LoginAsync(string email, string password, CancellationToken ct)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Email == email, ct)
            ?? throw new ForbiddenException("Credenziali non valide."); // messaggio generico: non rivelare se l'email esiste

        if (!passwordService.Verify(password, account.PasswordHash))
            throw new ForbiddenException("Credenziali non valide.");

        if (!account.CanLogIn)
            throw new ForbiddenException("Account non attivo."); // copre Unverified, Suspended, Deleted

        return IssueTokens(account, clock.UtcNow);
    }

    public async Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshTokenValue, CancellationToken ct)
    {
        var now = clock.UtcNow;

        var stored = await db.RefreshTokens.SingleOrDefaultAsync(t => t.Token == refreshTokenValue, ct)
            ?? throw new ForbiddenException("Refresh token non valido.");

        if (!stored.IsActive(now))
            throw new ForbiddenException("Refresh token scaduto o revocato.");

        var account = await db.Accounts.SingleAsync(a => a.Id == stored.AccountId, ct);

        stored.Revoke(now); // rotazione: il vecchio non è più utilizzabile

        return IssueTokens(account, now);
    }

    public async Task LogoutAsync(string refreshTokenValue, CancellationToken ct)
    {
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(t => t.Token == refreshTokenValue, ct);
        if (stored is null)
            return; // logout su token già invalido: no-op, non un errore

        stored.Revoke(clock.UtcNow);
        await db.SaveChangesAsync(ct);
    }

    private (string accessToken, string refreshToken) IssueTokens(Account account, DateTimeOffset now)
    {
        var accessToken = tokenService.CreateAccessToken(account.Id, account.Role, account.EmailVerifiedAt is not null, now);
        var refreshValue = tokenService.GenerateRefreshTokenValue();

        var refreshToken = new RefreshToken(
            Guid.NewGuid(), account.Id, refreshValue,
            expiresAt: now.AddDays(_jwt.RefreshTokenDays), createdAt: now);

        db.RefreshTokens.Add(refreshToken);
        db.SaveChanges(); // sincrono qui perché i metodi pubblici sopra fanno già await altrove;
                           // se preferisci, rendi IssueTokens async e usa SaveChangesAsync(ct)

        return (accessToken, refreshValue);
    }
}
```

Punti da notare:
- `RegisterUserAsync`/`RegisterAssociationAsync` fanno **due** `SaveChangesAsync` (uno per
  l'`Account`, uno dentro `provisioning.CreateForAccountAsync` sul `BusinessDbContext`):
  sono due database, non c'è transazione atomica fra i due — coerente con la decisione già
  presa (`[[project_account_shared_pk_with_profile]]`, `ProfileReconciliationJob` per il
  caso di fallimento a metà).
- `ForbiddenException` con messaggio identico per "email inesistente" e "password sbagliata":
  non rivelare quale dei due è sbagliato (piano §5.2, UC 10.2).
- `RefreshAsync` fa **rotazione**: revoca il token usato, ne emette uno nuovo. Se un
  refresh token rubato viene riusato dopo essere stato ruotato, risulterà già revocato →
  il legittimo proprietario e l'attaccante lo scoprono entrambi al prossimo tentativo
  (rilevamento di riuso — miglioramento per v1, non implementato qui).
- `IssueTokens` è privato e sincrono per semplicità dell'esempio: valuta di renderlo
  `async` con `SaveChangesAsync(ct)` per coerenza con il resto (```private async
  Task<(string, string)> IssueTokensAsync(Account account, DateTimeOffset now, CancellationToken ct)```).

---

## 6. `AccountService`

Verifica e-mail, cambio e-mail, cancellazione account.

```csharp
// Services/Auth/IAccountService.cs
namespace GoCare.Application.Services.Auth;

public interface IAccountService
{
    Task VerifyEmailAsync(string token, CancellationToken ct);
    Task ResendVerificationAsync(string email, CancellationToken ct);
    Task ChangeEmailAsync(Guid accountId, string newEmail, string verificationToken, CancellationToken ct);
    Task DeleteAsync(Guid accountId, string password, CancellationToken ct);
}
```

```csharp
// Services/Auth/AccountService.cs
using GoCare.Application.Data;
using GoCare.Application.Models.Auth;
using GoCare.Shared.Abstractions;
using GoCare.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace GoCare.Application.Services.Auth;

public sealed class AccountService(
    AuthDbContext db,
    IPasswordService passwordService,
    IProfileProvisioningService provisioning,
    IClock clock) : IAccountService
{
    public async Task VerifyEmailAsync(string token, CancellationToken ct)
    {
        var now = clock.UtcNow;

        var record = await db.EmailVerificationTokens.SingleOrDefaultAsync(t => t.Token == token, ct)
            ?? throw new NotFoundException("Token non valido.");

        if (!record.IsUsable(now))
            throw new DomainException("Token scaduto o già usato.");

        var account = await db.Accounts.SingleAsync(a => a.Id == record.AccountId, ct);

        account.Verify(now);
        record.Consume(now);

        await db.SaveChangesAsync(ct);
    }

    public async Task ResendVerificationAsync(string email, CancellationToken ct)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Email == email, ct);
        if (account is null || account.EmailVerifiedAt is not null)
            return; // risposta neutra: non riveli se l'account esiste o è già verificato

        // TODO: genera EmailVerificationToken, salvalo, invia l'e-mail (IEmailSender da GoCare.Shared)
    }

    public async Task ChangeEmailAsync(Guid accountId, string newEmail, string verificationToken, CancellationToken ct)
    {
        var now = clock.UtcNow;

        var record = await db.EmailVerificationTokens.SingleOrDefaultAsync(t => t.Token == verificationToken, ct)
            ?? throw new NotFoundException("Token non valido.");

        if (!record.IsUsable(now))
            throw new DomainException("Token scaduto o già usato.");

        var account = await db.Accounts.SingleAsync(a => a.Id == accountId, ct);

        account.ChangeEmail(newEmail, now);
        record.Consume(now);

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid accountId, string password, CancellationToken ct)
    {
        var account = await db.Accounts.SingleAsync(a => a.Id == accountId, ct);

        if (!passwordService.Verify(password, account.PasswordHash))
            throw new ForbiddenException("Password errata.");

        // Veto: nessun viaggio futuro attivo. Il controllo vero vive nel dominio;
        // qui si chiama un metodo del provisioning/dominio che lo verifica e lancia
        // ConflictException se il veto scatta — da definire insieme al dominio.
        await provisioning.AnonymizeForDeletedAccountAsync(accountId, ct);

        account.MarkDeleted(clock.UtcNow);

        // Invalida tutte le sessioni: revoca ogni RefreshToken attivo dell'account.
        var activeTokens = await db.RefreshTokens
            .Where(t => t.AccountId == accountId && t.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var t in activeTokens)
            t.Revoke(clock.UtcNow);

        await db.SaveChangesAsync(ct);
    }
}
```

Nota: `DeleteAsync` chiama `provisioning.AnonymizeForDeletedAccountAsync(accountId, ct)` —
aggiungi questa firma a `IProfileProvisioningService` (§5 sopra) quando arrivi a scriverla;
qui basta che il tipo compili.

---

## 7. Registrazione in `DependencyInjection.AddApplication`

```csharp
services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

services.AddScoped<IPasswordService, PasswordService>();
services.AddScoped<ITokenService, TokenService>();
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IAccountService, AccountService>();
```

---

## 8. Cosa resta fuori da questa guida (prossimi passi)

- **Controller** (`AuthController`, `EmailVerificationController`, `PasswordController`,
  `AccountController`): solo bind DTO → chiamata al Service → `ActionResult`, nessuna logica.
- **`PasswordService` per il reset** (`RequestResetAsync`/`ResetAsync`, UC 10.3): stesso
  pattern di `VerifyEmailAsync` ma su `PasswordResetToken`, con l'aggiunta finale
  `account.ChangePassword(...)` + invalidazione di tutte le sessioni (come in `DeleteAsync`).
  Può stare in `AccountService` o in un `IPasswordResetService` dedicato — a te la scelta.
  Ricorda `FailedLoginAttempt` per il rate limiting su login (registra un tentativo fallito
  in `LoginAsync` quando le credenziali sono sbagliate, e controlla il conteggio recente
  prima di procedere — vedi la conversazione su `FailedLoginAttempt`).
- **Pipeline JWT nell'host** (`GoCare.Api/Program.cs`): `AddAuthentication().AddJwtBearer(...)`
  con la stessa `SigningKey`/`Issuer`/`Audience` di qui — Fase 4 del piano, fuori scope Service.
- **`IProfileProvisioningService`** vera (lato dominio): la firma qui è solo quella che serve
  ad `AuthService`/`AccountService` per compilare.
