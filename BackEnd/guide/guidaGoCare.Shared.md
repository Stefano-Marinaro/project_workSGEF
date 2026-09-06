# Guida — `GoCare.Shared`

Teoria vista costruendo `GoCare.Shared`. Da rileggere quando serve.

---

## 1. Cos'è `GoCare.Shared`

Un **progetto libreria** (nessun endpoint HTTP) col codice "di servizio" usato
da tutto il backend e che **non riguarda il dominio** (niente trasporti,
account, gruppi). È l'impianto idraulico, non le stanze.

Progetto separato perché: `GoCare.Application` lo referenzia → codice condiviso
in un posto solo; e tenendolo isolato la logica di dominio non ci finisce
dentro per sbaglio.

Contenuto: astrazioni (`IClock`, `IEmailSender`, `ICurrentUser`), eccezioni
tipizzate + gestore globale, filtro di validazione, tipi per la paginazione.

---

## 2. Concetti di base

### Interfaccia
Un **contratto**: l'elenco dei metodi/proprietà che qualcosa espone, senza il
codice che li fa funzionare. Una classe la "implementa" scrivendo il codice
vero. L'interfaccia dice *"so mandare un'email"* (`SendAsync`); una classe
concreta dice *come* (SMTP, servizio cloud, finta nei test).

### Dependency Injection (DI)
Invece di creare gli oggetti con `new`, li si **chiede nel costruttore**
(tipizzati come interfacce) e un "contenitore" del framework li fornisce
pronti. Così puoi **sostituire** l'implementazione (vera in produzione, finta
nei test) e decidi **in un punto solo** cosa si usa (la registrazione DI). Il
codice dipende dal **cosa** (l'interfaccia), non dal **come**.

```csharp
public sealed class TransportService(IClock clock)   // chiede IClock, non SystemClock
{
    public void Create(DateTimeOffset dataViaggio)
    {
        if (dataViaggio < clock.UtcNow)
            throw new ValidationException(/* "data nel passato" */);
    }
}
```

Il tipo concreto compare **una sola volta**, nella registrazione:

```csharp
services.AddSingleton<IClock, SystemClock>();
```

In produzione ogni Service riceve un `SystemClock`; nei test gli si passa a
mano un `FakeClock` con istante fisso.

### Porta (port)
Un'interfaccia per una **capacità esterna** (email, database, push).
L'implementazione sta in `Infrastructure/` (host o progetto applicativo),
**non** in `GoCare.Shared`. Eccezione: un'implementazione banale come
`SystemClock` può stare accanto alla sua interfaccia.

---

## 3. Dettagli di C# incontrati (riferimento rapido)

| Cosa | Significato |
|---|---|
| `namespace X;` (con `;`) | *file-scoped*: tutto il file appartiene a `X`. Nome = namespace del progetto + cartella. |
| **Implicit usings** | `<ImplicitUsings>enable</ImplicitUsings>`: importa in automatico `System`, `System.Linq`, `System.Threading.Tasks`, ecc. I template VS li aggiungono comunque: vanno tolti. **Non** copre `Microsoft.AspNetCore.*` in una class library. |
| `public` vs `internal` | `internal` = visibile solo dentro il progetto. Nel kernel condiviso i contratti vanno **`public`**. |
| `sealed` (classe) | vieta l'ereditarietà. |
| `abstract` (classe) | non istanziabile: esiste solo come base per sottoclassi. |
| Costruttore primario | `class X(string message)` — i parametri dopo il nome sono catturati e usabili nei membri. |
| `{ get; }` | sola lettura: assegnabile solo alla creazione. |
| `{ get; init; }` | assegnabile solo durante la creazione (object initializer), poi immutabile. |
| `=> espressione` | corpo di espressione (forma breve di `get { return …; }`). |
| `;` al posto di `{ }` | corpo vuoto: non aggiunge nulla. |
| `record` | tipo per dati, uguaglianza "per valore", sintassi concisa. |
| `Task` come ritorno | metodo **asincrono** (I/O): non blocca il thread. Nome in `…Async`. |
| `CancellationToken ct = default` | parametro opzionale per **annullare** l'operazione. Il nome deve combaciare con quello dell'interfaccia implementata (CA1725). |
| Nomi parametri | **camelCase**: `htmlBody`, non `htmlbody`. |
| `Guid` | 128 bit, 32 esadecimali. `Guid.NewGuid()` praticamente mai in collisione. Utile quando l'id serve **prima** di salvare. Chiavi entità: GUID **v7/sequenziali** (`Guid.CreateVersion7()`); id usa-e-getta: `Guid.NewGuid()`. `Guid.Empty` = tutto zeri. |
| `exception switch { … }` | *switch expression*: pattern matching sul tipo, produce un valore. `_` = default. |
| `x is Tipo v` | controlla il tipo **e** assegna a `v` se combacia. `is not Tipo v` per il caso opposto. |
| BOM UTF-8 | i `.cs` da Visual Studio hanno il BOM; quelli scritti a mano spesso no. Non rompe niente; `dotnet format` uniforma. |
| `_` sui campi privati | `private readonly X _foo;` — distingue il campo da parametri/locali senza `this.`. **Non** su `const` né `static readonly` costanti: quelli `PascalCase`. Parametri e locali: `camelCase` senza `_`. |
| `cond ? a : b` | ternario: espressione, produce un valore (non è un `if`). |
| `x is < 1 or > 100` | pattern relazionale: come `x < 1 \|\| x > 100`, ma nomina `x` una volta. |
| `(double)x` in una divisione | `int / int` è divisione **intera** (`50 / 20 == 2`). Cast a `double` → `50 / 20.0 == 2.5`. |
| `Math.Ceiling` | arrotonda **verso l'alto** (`2.1 → 3.0`). Restituisce `double`: serve `(int)` davanti. |

---

## 4. I componenti

### 4.1 `IClock` + `SystemClock` (`Abstractions/`)

Molte regole dipendono da "adesso" (UC 1.1, PA-04, scadenza token). Chiamare
`DateTimeOffset.UtcNow` direttamente rende il codice **non testabile**: il test
non può decidere che ore siano.

Si nasconde "adesso" dietro `IClock.UtcNow`. Produzione → `SystemClock` (ora
del SO). Test → finto con istante fisso. L'interfaccia serve **anche al codice
vero**: i Service dipendono da `IClock`.

- `DateTimeOffset` (non `DateTime`): porta il fuso, istante non ambiguo. Si
  lavora in UTC internamente; l'ora locale solo ai bordi (UI).
- `UtcNow` è una **proprietà** (lettura pura).
- `SystemClock` è `sealed`.

### 4.2 `IEmailSender` (`Abstractions/`)

Una **porta**: *"so mandare un'email"*, senza dire come. Usata da area Auth
(verifica, reset) e area dominio (notifiche di esito). Sta in `Shared` perché
serve a entrambe; l'implementazione (`SmtpEmailSender`) sta in
`Infrastructure/` — qui solo il contratto.

```csharp
Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
```

`Task` → asincrono (I/O di rete). `ct = default` → parametro opzionale per
annullare.

### 4.3 `Errors/` — eccezioni tipizzate

Un'**eccezione** rappresenta un errore: `throw` la lancia, risale la catena
finché qualcuno la `catch`a. In .NET tutte derivano da `System.Exception`.

**Perché scriverle noi e non usare quelle di EF:** EF è al livello
*persistenza*, non al significato *applicativo*.

- "non trovato" per EF = `null`, non un'eccezione. È il Service a decidere
  "`null` → 404".
- "conflitto" per EF = `DbUpdateException` / `DbUpdateConcurrencyException` /
  `PostgresException` (`23505`): eccezioni **di database**. Il Service le
  ritraduce in `ConflictException`, o controlla la regola prima di salvare.
- "forbidden" = autorizzazione: EF non ne sa nulla.

`Results.NotFound()` di ASP.NET Core è un helper per **costruire una risposta**
in un controller, non un'eccezione: sposterebbe la logica "è null?" nel
controller. Il piano fa l'opposto: Service lancia l'eccezione tipizzata →
`GlobalExceptionHandler` la mappa in `ProblemDetails` → controller sottile.

```
Exception  (System)
   └─ DomainException  (abstract)
        ├─ NotFoundException   (sealed)  → 404
        ├─ ConflictException   (sealed)  → 409
        ├─ ForbiddenException  (sealed)  → 403
        └─ ValidationException (sealed)  → 422  (+ dati per campo)
```

Perché due livelli:

- `DomainException : Exception` — per `throw`/`catch` una classe deve
  discendere da `System.Exception`. È il punto d'aggancio.
- foglie `: DomainException` (non `: Exception`) — per avere **una categoria
  unica**: un solo `catch (DomainException)` prende tutte le nostre, un `catch
  (Exception)` dopo prende i bug → 500; `ex is DomainException` distingue
  "errore di business previsto" da "difetto"; comportamento condiviso futuro si
  aggiunge una volta sulla base.
- `DomainException` è `abstract` (mai un generico "errore di dominio"); le
  foglie `sealed`.
- Costruttore: `X(string message) : DomainException(message)` inoltra il
  messaggio alla base. Corpo `;` perché non aggiungono nulla (tranne
  `ValidationException`).

### 4.4 `ValidationException` (`Errors/`)

Oltre a essere un tipo d'errore, **porta dati**: i problemi di validazione
campo per campo.

```csharp
public sealed class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : DomainException("Uno o più campi non sono validi.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
```

- `IReadOnlyDictionary<string, string[]>`: chiave = nome campo (`"Email"`),
  valore = messaggi per quel campo; sola lettura.
- `string[]` e non `string` perché un campo può violare più regole insieme.
- `Message` è per un umano; i dettagli strutturati vanno su `Errors`, agganciati
  ai campi lato client.

Flusso: `throw new ValidationException(errors)` → risale → `GlobalExceptionHandler`
→ `HTTP 422 + ProblemDetails { errors: { Email:[...], Password:[...] } }` → il
form React Native mostra ogni messaggio sotto il suo campo.

Tipo nostro e non `FluentValidation.ValidationException`: per non far dipendere
`Shared` e il gestore dalla libreria di validazione (stessa logica delle
eccezioni vs EF); la sua forma è anche più scomoda da mappare.

### 4.5 `GlobalExceptionHandler` (`Errors/`)

`IExceptionHandler` è un **gancio** che ASP.NET Core chiama quando un'eccezione
non gestita arriva in cima alla pipeline (serve `AddExceptionHandler` +
`UseExceptionHandler` nell'host). Un metodo:

```
ValueTask<bool> TryHandleAsync(HttpContext, Exception, CancellationToken)
```

`true` = "l'ho gestita, fermati"; `false` = "non è roba mia, prossimo handler".

Cosa fa:

1. **`switch` sull'eccezione** → `(status, title)`. Le nostre 4 →
   404/409/403/422; `_` → 500.
2. **log** solo se 500 (`logger.LogError(exception, …)` con stack trace). Le
   `DomainException` sono attese: niente log di errore.
3. **`ProblemDetails`** (RFC 7807): `Status`, `Title`, `Detail`, `Instance`.
4. per `ValidationException`: `problem.Extensions["errors"] = ve.Errors`.
5. **`WriteAsJsonAsync`** — serializza nel corpo della risposta.

**Sicurezza:** al client non si manda mai `exception.Message` per un 500 (può
contenere nomi di tabelle, path). Per le `DomainException` il messaggio l'hai
scritto tu → si può mostrare (`Detail`).

Logger dal costruttore primario: `GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)`.
`ILogger<T>` è "etichettato" col nome della classe → filtri i log per sorgente.

Nel metodo, l'unico punto che lo conclude è `return true;` in fondo. Le righe
prima sono passi che si eseguono e basta: non mettere `return` davanti a
`logger.LogError(...)` (ritorna `void`) né a `problem.Extensions["errors"] = …`
(è un'assegnazione).

### 4.6 `ValidationFilter` (`Validation/`)

Un **action filter** gira *attorno* all'azione di un controller:

```
OnActionExecutionAsync(context, next)
   ├─ codice PRIMA dell'azione
   ├─ await next()   → esegue l'azione (e i filtri successivi)
   └─ codice DOPO l'azione
```

Il nostro lavora **prima**: per ogni DTO bindato dal body,

1. costruisce `IValidator<TipoDelDTO>` e chiede alla DI se esiste;
2. se sì, lo esegue;
3. se non valido, raggruppa gli errori per campo e lancia `ValidationException`
   → il gestore la trasforma in 422.

Registrato **una volta** a livello globale (DRY).

Concetti:

- `typeof(IValidator<>).MakeGenericType(t)` — **reflection**: `IValidator<>` è
  il generico "aperto"; `MakeGenericType(t)` lo chiude su `t`. Serve perché il
  filtro non conosce i tipi dei DTO a compile-time.
- `context.HttpContext.RequestServices.GetService(type)` — chiede alla DI un
  servizio dato il `Type`.
- `is not IValidator validator` → nessun validator → `continue`.
- **collisione di nomi** con `FluentValidation.ValidationException`: l'alias
  `using ValidationException = GoCare.Shared.Errors.ValidationException;` ha la
  precedenza sui tipi importati da un `using` di namespace.
- LINQ `GroupBy` + `ToDictionary` — da lista di `ValidationFailure` a mappa
  `campo → messaggi[]`.

### 4.7 `ICurrentUser` (`Abstractions/`)

Ogni richiesta autenticata porta un **JWT**; ASP.NET Core lo valida ed espone i
**claim** su `HttpContext.User`. I Service devono sapere **chi chiama** per
autorizzare e marcare i record. Pescare in `HttpContext.User` ovunque è
scomodo, duplicato, poco testabile, e trascina `Microsoft.AspNetCore.*` in
classi che dovrebbero essere sola logica → si incapsula dietro `ICurrentUser`:

```csharp
public interface ICurrentUser
{
    Guid? AccountId { get; }       // null se richiesta anonima
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
```

- `AccountId` è `Guid?`: una richiesta può essere anonima (login,
  registrazione).
- `IsAuthenticated` — comodità (`AccountId is not null`).
- `IsInRole` è un **metodo** (domanda con parametro); `role` in camelCase.

Stesso schema di `IClock`: interfaccia nel kernel, implementazione reale
(`CurrentUser`, legge `IHttpContextAccessor`) registrata nell'host, finta nei
test. Solo l'interfaccia sta in `Shared`; `CurrentUser` si scrive con
`GoCare.Api`.

**Da decidere in Fase 3 (Auth):** quale claim porta l'id (`sub` vs
`ClaimTypes.NameIdentifier`) e se aggiungere `PersonId` / `AssociationId`
(`Guid?` — un account è *o* utente *o* associazione).

### 4.8 `Pagination/` — `PageQuery` e `PagedResult<T>`

Gli endpoint di lista restituiscono **una pagina alla volta**
(`?page=3&pageSize=20`). Due tipi speculari, uno per direzione.

**`PageQuery` — input (client → server).** La coppia `Page`/`PageSize` **dopo
la pulizia**: dei numeri grezzi del client non ci si fida
(`?page=-5&pageSize=999999`).

```csharp
public sealed record PageQuery
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public int Page { get; }
    public int PageSize { get; }

    public PageQuery(int page = 1, int pageSize = DefaultPageSize)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize is < 1 or > MaxPageSize ? DefaultPageSize : pageSize;
    }

    public int Skip => (Page - 1) * PageSize;
}
```

- `const` privati (`PascalCase`) per non avere numeri magici sparsi.
- `{ get; }` senza `set`, valorizzate solo nel costruttore → immutabile.
- default → `new PageQuery()` = pagina 1 da 20.
- `Page` < 1 → 1 (le pagine partono da 1). `PageSize` fuori da `[1,100]` →
  ripiega su 20 (un valore folle è di norma un bug del client).
- `Skip` — proprietà **calcolata** (`=>`): righe da saltare per arrivare alla
  pagina. Va a `query.Skip(...).Take(PageSize)`.

**`PagedResult<T>` — output (server → client).** Pagina corrente + metadati.

```csharp
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages =>
        PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

- **`record` posizionale**: i parametri diventano `{ get; init; }` + costruttore
  generati. Qui va bene (puro trasporto); `PageQuery` ha il corpo a mano perché
  lì serve la pulizia.
- `<T>` generico: stesso contenitore per `PagedResult<TransportDto>`, ecc.
- `IReadOnlyList<T>` per `Items`: il client non modifica una risposta.
- `TotalCount` = righe su **tutte** le pagine (un `COUNT(*)` separato). Serve
  per "1–20 di 350".
- `TotalPages` è **derivato** → nel corpo, non fra i parametri:
  - `(double)` sul divisore: senza, `50/20 == 2`.
  - `Math.Ceiling` verso l'alto: 50 righe da 20 → 3 pagine. Restituisce
    `double` → `(int)`.
  - `PageSize <= 0 ? 0 : …` — guardia anti divisione per zero (`PagedResult` si
    può costruire a mano nei test).

Entrambi in `Shared` perché non dipendono da nulla (né EF né ASP.NET).

### 4.9 `DependencyInjection.cs` — `AddSharedKernel()`

Un servizio è disponibile per la DI solo se **registrato**. Per non spargere le
registrazioni di `Shared` nel `Program.cs`, `Shared` espone **un metodo solo**:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddHttpContextAccessor();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddScoped<ValidationFilter>();
        return services;
    }
}
```

L'host scrive `builder.Services.AddSharedKernel();` e ha tutto. Schema di
`AddControllers()`, `AddDbContext()`…

**Metodo di estensione:** `static` in `static class`, primo parametro con
`this` → lo chiami come metodo di `IServiceCollection`. Restituisce
`IServiceCollection` per concatenare.

**`IServiceCollection`** è un'interfaccia Microsoft (classe concreta
`ServiceCollection`); l'istanza la crea il framework
(`WebApplication.CreateBuilder` → `builder.Services`). La ricevi già pronta. È
il "carrello" della DI (*"quando qualcuno chiede X, dagli Y"*), riempito
all'avvio e congelato da `builder.Build()`.

**Lifetime:**

| Metodo | Quante istanze | Quando |
|---|---|---|
| `AddSingleton` | **una** per tutta la vita dell'app | oggetti **senza stato** e **thread safe** (`SystemClock`) |
| `AddScoped` | **una per richiesta HTTP** | roba legata alla richiesta (`DbContext`, `CurrentUser`, un filtro) |
| `AddTransient` | **una nuova a ogni richiesta** del tipo | oggetti leggeri usa-e-getta |

- **Senza stato:** niente campi mutabili ricordati fra chiamate. `SystemClock`
  legge l'orologio e restituisce → una istanza o mille è identico.
- **Thread safe:** un Singleton è toccato da più thread insieme. Ok se non c'è
  stato condiviso da modificare (`++_valore` da due thread perde un
  incremento). Un Singleton con stato va protetto (`lock`, `Interlocked`,
  `Concurrent*`) o, meglio, evitato: usa `Scoped`.

**Cosa `AddSharedKernel` NON fa:**

- non registra `IEmailSender` né `ICurrentUser`: le implementazioni stanno
  fuori da `Shared` → le registra l'host;
- non fa `app.UseExceptionHandler()` né aggancia il `ValidationFilter` alla
  pipeline MVC: quelle toccano l'`app` e `AddControllers`, competono al
  `Program.cs`. `AddSharedKernel` tocca **solo** `IServiceCollection`.
- `AddExceptionHandler<…>()` senza `app.UseExceptionHandler()` nell'host non
  basta: l'handler non verrebbe mai invocato.

---

## 5. Configurazione del progetto

### `FrameworkReference` vs `PackageReference`

- **`PackageReference`** = un pacchetto singolo da NuGet (`FluentValidation`).
- **`FrameworkReference`** = un intero *shared framework* già con l'SDK.
  `Microsoft.AspNetCore.App` dà con una riga `HttpContext`, `IExceptionHandler`,
  MVC, DI… senza scaricare nulla.

`GoCare.Shared` ha entrambi: `FrameworkReference Microsoft.AspNetCore.App` (per
`GlobalExceptionHandler`, `ValidationFilter`, `CurrentUser`) e
`PackageReference FluentValidation` (per `ValidationFilter`).

### FluentValidation

Regole di validazione in stile leggibile:

```csharp
RuleFor(x => x.Email).NotEmpty().EmailAddress();
RuleFor(x => x.Password).MinimumLength(8);
```

I validator concreti (uno per DTO) vivono in `GoCare.Application`. In `Shared`
c'è solo il `ValidationFilter` che li esegue. Versione: **12.1.1**.

### `Directory.Build.props` (a `BackEnd/`)

Vale per tutti i progetti: `net10.0`, `Nullable enable`, `ImplicitUsings
enable`, **warning non bloccanti** (`TreatWarningsAsErrors=false`), analyzer
attivi, `NoWarn` per `CA1716` (namespace "Shared") e `CA1848` (`LoggerMessage`
non serve fuori dai percorsi caldi).

---

## 6. Stato di `GoCare.Shared`

**Fatto** — build 0/0, `GoCare.Shared` completo lato codice:

- `Abstractions/`: `IClock.cs`, `SystemClock.cs`, `IEmailSender.cs`,
  `ICurrentUser.cs`
- `Errors/`: `DomainException.cs`, `NotFoundException.cs`, `ConflictException.cs`,
  `ForbiddenException.cs`, `ValidationException.cs`, `GlobalExceptionHandler.cs`
- `Validation/ValidationFilter.cs`
- `Pagination/PageQuery.cs`, `PagedResult.cs`
- `DependencyInjection.cs` — `AddSharedKernel()`

**Da fare (nell'host `GoCare.Api`):** vedi `guidaGoCare.Api.md`.

- `Security/CurrentUser.cs` — implementazione di `ICurrentUser` con
  `IHttpContextAccessor`; `AddScoped<ICurrentUser, CurrentUser>()`.
- `Program.cs`: `AddSharedKernel()`, `app.UseExceptionHandler()`,
  `AddControllers(o => o.Filters.AddService<ValidationFilter>())`, Swagger.
- `appsettings.json`: connection string `AuthDb` e `BusinessDb`.
