# Guida — `GoCare.Shared`

Raccolta delle spiegazioni teoriche viste mentre costruivamo il progetto
`GoCare.Shared`. Da rileggere quando serve.

---

## 1. Cos'è `GoCare.Shared`

È un **progetto libreria** (non ha endpoint HTTP) che contiene il codice "di
servizio" usato da tutto il backend e che **non riguarda il dominio** (niente
trasporti, account, gruppi cura). È l'impianto idraulico, non le stanze.

Sta in un progetto separato perché:
- il progetto applicativo (`GoCare.Application`) lo referenzia → codice condiviso
  in un posto solo;
- tenendolo separato, la logica di dominio non ci finisce dentro per sbaglio.

Contenuto: astrazioni (`IClock`, `IEmailSender`, `ICurrentUser`), eccezioni
tipizzate + gestore globale, filtro di validazione, tipi per la paginazione.

---

## 2. Due concetti di base che tornano ovunque

### Interfaccia
Un **contratto**: l'elenco dei metodi/proprietà che qualcosa espone, **senza il
codice** che li fa funzionare. Una classe "implementa" l'interfaccia scrivendo il
codice vero.

Esempio a parole: l'interfaccia dice *"so mandare un'email"* (`SendAsync`); una
classe concreta dice *come* (via SMTP, via un servizio cloud, o finta nei test).

### Dependency Injection (DI)
Invece di creare da soli gli oggetti che servono con `new`, li si **chiede nel
costruttore** (di solito tipizzati come interfacce) e un "contenitore" del
framework li fornisce già pronti all'avvio.

Vantaggi:
- puoi **sostituire** l'implementazione (una vera in produzione, una finta nei
  test);
- decidi **in un punto solo** cosa viene usato (la registrazione DI).

Messi insieme: il codice dipende dal **cosa** (l'interfaccia), non dal **come**
(la classe concreta). Cambi il come senza toccare chi lo usa.

Esempio concreto (`IClock`):
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
`TransportService` non nomina mai `SystemClock`. Il tipo concreto compare **una
sola volta**, nella registrazione DI:
```csharp
services.AddSingleton<IClock, SystemClock>();
```
In produzione ogni Service riceve un `SystemClock` (ora vera); nei test gli si
passa a mano un `FakeClock` con un istante fisso.

### Porta (port)
Un'interfaccia per una **capacità esterna** (email, database, push, calcolo km).
L'implementazione della porta sta in `Infrastructure/` (dell'host o del progetto
applicativo), **non** in `GoCare.Shared`. Eccezione: un'implementazione banale
come `SystemClock` può stare accanto alla sua interfaccia.

---

## 3. Dettagli di C# incontrati (riferimento rapido)

| Cosa | Significato |
|---|---|
| `namespace X;` (con `;`) | *file-scoped*: tutto il file appartiene a `X`. Il nome = namespace del progetto + cartella. |
| **Implicit usings** | `<ImplicitUsings>enable</ImplicitUsings>` nel `.csproj`: il compilatore importa in automatico `System`, `System.Linq`, `System.Threading.Tasks`, ecc. → niente `using` a mano per quelli. VS li aggiunge comunque nei template: vanno tolti. **Non** copre `Microsoft.AspNetCore.*` in una class library. |
| `public` vs `internal` | `internal` = visibile solo dentro il progetto. Nel *shared kernel* i contratti vanno **`public`** perché usati da altri progetti. |
| `sealed` (su classe) | vieta che altre classi ne ereditino. |
| `abstract` (su classe) | non istanziabile: `new X(...)` vietato; esiste solo come base per sottoclassi. |
| Costruttore primario | `class X(string message)` — i parametri fra parentesi dopo il nome sono catturati e usabili nei membri. |
| `{ get; }` | proprietà di sola lettura: assegnabile solo alla creazione. |
| `{ get; init; }` | assegnabile solo durante la creazione dell'oggetto (object initializer), poi immutabile. |
| `=> espressione` | proprietà/metodo con corpo di espressione (forma breve di `get { return …; }`). |
| `;` al posto di `{ }` | corpo vuoto: la classe/membro non aggiunge nulla. |
| `record` | tipo pensato per contenere dati, con uguaglianza "per valore" e sintassi concisa. |
| `Task` come ritorno | il metodo è **asincrono** (I/O): non blocca il thread. Convenzione: nome in `…Async`. |
| `CancellationToken ct = default` | parametro opzionale per **annullare** l'operazione se la richiesta viene interrotta. Il nome deve **combaciare** con quello dell'interfaccia che si implementa (analyzer CA1725). |
| Nomi dei parametri | **camelCase**: `htmlBody`, non `htmlbody`. |
| `Guid` | *Globally Unique Identifier*: 128 bit, 32 cifre esadecimali. Generato ovunque con `Guid.NewGuid()`, praticamente mai in collisione. Utile quando l'id serve **prima** di salvare sul DB. Per le chiavi delle entità si useranno GUID **v7/sequenziali** (`Guid.CreateVersion7()`); per un id "usa e getta" (evento, ecc.) `Guid.NewGuid()` va bene. `Guid.Empty` = tutto zeri. |
| `exception switch { … }` | *switch expression*: pattern matching sul tipo a runtime, produce un valore. `_` = "qualsiasi altro caso" (default). |
| `x is Tipo variabile` | controlla il tipo **e** assegna alla variabile se combacia, in un colpo solo. `is not Tipo v` per il caso opposto. |
| BOM UTF-8 | i file `.cs` creati da Visual Studio hanno il BOM in testa; quelli scritti "a mano" spesso no. Non rompe la compilazione; `dotnet format` lo uniforma. |
| `_` sui campi privati | `private readonly X _foo;` — l'underscore distingue il **campo** (stato dell'oggetto) da parametri/locali con nome simile, senza dover scrivere `this.`. **Non** si usa su `const` né su `static readonly` usati come costanti: quelli vanno `PascalCase` (`MaxPageSize`), come le proprietà pubbliche. Parametri e variabili locali: `camelCase` **senza** `_`. |
| `cond ? a : b` | *operatore ternario*: se `cond` è vera → `a`, altrimenti → `b`. È un'espressione, produce un valore (non è un `if`). |
| `x is < 1 or > 100` | *pattern relazionale*: "`x` è minore di 1 **oppure** maggiore di 100". Stesso risultato di `x < 1 \|\| x > 100`, ma nomina `x` una volta sola. |
| `(double)x` in una divisione | `int / int` è divisione **intera** (`50 / 20 == 2`, resto buttato). Un cast a `double` sul divisore → `50 / 20.0 == 2.5`. |
| `Math.Ceiling` | arrotonda **sempre verso l'alto** (`2.1 → 3.0`). Restituisce `double`: serve `(int)` davanti per tornare intero. |

---

## 4. I componenti, uno per uno

### 4.1 `IClock` + `SystemClock` (`Abstractions/`)

**Problema:** molte regole dipendono da "adesso" (UC 1.1 "data non nel passato",
PA-04 "meno di 24h alla data", scadenza token). Se il codice chiama direttamente
`DateTimeOffset.UtcNow`, è **impossibile testarlo** in modo affidabile: il test
non può decidere che ore siano.

**Soluzione:** si nasconde "adesso" dietro `IClock` con la proprietà `UtcNow`.
Produzione → `SystemClock` (ora vera del SO). Test → finto con istante fisso.

Dettagli:
- `DateTimeOffset` (non `DateTime`): porta il fuso, è un istante non ambiguo.
  Standard per salvataggio e API. Si lavora **in UTC** internamente; l'ora locale
  si calcola solo ai bordi (UI).
- `UtcNow` è una **proprietà** (lettura pura, niente parametri).
- `SystemClock` è `sealed` (classe banale, non pensata per essere estesa).

L'interfaccia serve **anche al codice vero**, non solo ai test: i Service
dipendono da `IClock`, non da `SystemClock`.

### 4.2 `IEmailSender` (`Abstractions/`)

Una **porta**: *"so mandare un'email"*, senza dire come.

Usata da: area Auth (verifica registrazione, reset password) e area dominio
(notifiche: presa in carico, disdetta, mancata copertura, promemoria, esito
modifica, conferma eliminazione account).

Sta in `GoCare.Shared` perché serve a entrambe le aree. L'implementazione vera
(`SmtpEmailSender`) sta in `Infrastructure/`, **non** qui: è complessa e ha
dipendenze (configurazione, client SMTP). In `Shared` c'è solo il contratto.

Firma:
```csharp
Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
```
`Task` → asincrono (I/O di rete). `CancellationToken ct = default` → parametro
opzionale per annullare.

### 4.3 `Errors/` — eccezioni tipizzate

Un'**eccezione** è un oggetto che rappresenta un errore: si lancia con `throw` e
risale la catena delle chiamate finché qualcuno non la `catch`a. In .NET tutte
derivano da `System.Exception`.

**Perché scriverle noi e non usare quelle di Entity Framework:**
EF lavora sul livello *persistenza*, non sul significato *applicativo*.
- "non trovato" per EF = `null` (`FindAsync`, `FirstOrDefaultAsync`), non
  un'eccezione. È il Service che decide "è `null` → questo è un 404".
- "conflitto" per EF = `DbUpdateException` / `DbUpdateConcurrencyException` /
  `PostgresException` (codice `23505`): eccezioni **di database**. Il Service le
  cattura e le ritraduce in `ConflictException`, oppure controlla la regola prima
  di salvare.
- "forbidden" = autorizzazione: EF non ne sa nulla.

`NotFound()` / `Results.NotFound()` di ASP.NET Core sono **helper per costruire
una risposta** dentro un controller, non eccezioni: userebbero il controller per
la logica "è null?" invece del Service. Il piano fa l'opposto: Service lancia
l'eccezione tipizzata → `GlobalExceptionHandler` la mappa in `ProblemDetails` →
controller sottile.

**La gerarchia:**
```
Exception  (System)
   └─ DomainException  (abstract)   ← si aggancia qui alla gerarchia .NET
        ├─ NotFoundException   (sealed)  → 404
        ├─ ConflictException   (sealed)  → 409
        ├─ ForbiddenException  (sealed)  → 403
        └─ ValidationException (sealed)  → 422  (+ dati per campo)
```

Perché due livelli:
- `DomainException : Exception` — per essere `throw`/`catch` in .NET una classe
  deve discendere da `System.Exception`. `DomainException` è il punto di aggancio.
- le foglie `: DomainException` (non `: Exception` diretto) — per avere **una
  categoria unica**:
  1. un solo `catch (DomainException)` prende tutte le nostre; un `catch
     (Exception)` dopo prende i bug imprevisti → 500;
  2. `ex is DomainException` distingue "errore di business previsto" da "difetto";
  3. comportamento condiviso futuro (codice d'errore, flag) si aggiunge una volta
     sulla base.
- `DomainException` è `abstract`: non si lancia mai un generico "errore di
  dominio", sempre un tipo preciso. Le foglie sono `sealed`: niente sottoclassi.
- Costruttore: `X(string message) : DomainException(message)` — inoltra il
  messaggio al costruttore base, così `ex.Message` funziona. Corpo `;` perché non
  aggiungono nulla (tranne `ValidationException`).

### 4.4 `ValidationException` (`Errors/`)

Diversa dalle altre: oltre a essere un tipo d'errore, **porta dati** — l'elenco
dei problemi di validazione, campo per campo.

```csharp
public sealed class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : DomainException("Uno o più campi non sono validi.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
```

Forma dei dati:
```
IReadOnlyDictionary<string, string[]>
        │              │        │
        │              │        └─ i messaggi per quel campo (più di uno se rompe più regole)
        │              └─ nome del campo, es. "Email"
        └─ sola lettura: chi riceve l'eccezione legge, non modifica
```
`string[]` e non `string` perché un campo può violare più regole insieme
(password troppo corta *e* senza cifre). `Message` è una frase per un umano; i
dettagli strutturati vanno sulla proprietà `Errors` per essere agganciati ai
campi lato client.

**Flusso end to end:**
```
ValidationFilter (o un Service)  →  throw new ValidationException(errors)
        │  (risale la catena, nessuno la cattura nel mezzo)
        ▼
GlobalExceptionHandler  →  switch: è ValidationException?
        ▼
HTTP 422 + ProblemDetails { title, errors: { Email:[...], Password:[...] } }
        ▼
il form React Native mostra ogni messaggio sotto il suo campo
```

Perché un tipo nostro e non `FluentValidation.ValidationException`: per non far
dipendere `GoCare.Shared` e il gestore dalla libreria di validazione (stessa
logica delle eccezioni vs EF). La sua forma (`IEnumerable<ValidationFailure>`) è
anche più scomoda da mappare.

### 4.5 `GlobalExceptionHandler` (`Errors/`)

`IExceptionHandler` è un **gancio** che ASP.NET Core chiama quando un'eccezione
non gestita arriva in cima alla pipeline (previa registrazione con
`AddExceptionHandler` + `UseExceptionHandler` nell'host). Un solo metodo:
```
ValueTask<bool> TryHandleAsync(HttpContext, Exception, CancellationToken)
```
- ritorna `true` = "l'ho gestita io, fermati";
- ritorna `false` = "non è roba mia, passa al prossimo handler".

Cosa fa:
1. **`switch` sull'eccezione** → coppia `(status, title)`. Le nostre 4 →
   404/409/403/422; `_` (default) → 500.
2. **log** se è un 500: `logger.LogError(exception, "…")` con lo stack trace. Le
   `DomainException` sono errori attesi, niente log di errore.
3. **`ProblemDetails`** (RFC 7807): `Status`, `Title`, `Detail`, `Instance`
   (+ `Extensions` per campi custom).
4. per una `ValidationException`, `problem.Extensions["errors"] = ve.Errors`.
5. **`WriteAsJsonAsync`** — serializza in JSON e scrive nel corpo della risposta.

**Sicurezza:** al client **non** si manda mai `exception.Message` per un 500 (può
contenere nomi di tabelle, path, dettagli interni). Per le `DomainException` il
messaggio l'abbiamo scritto noi → si può mostrare (`Detail`).

Il logger arriva col costruttore primario:
`GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)`. `ILogger<T>` è
"etichettato" col nome della classe → filtri i log per sorgente.

Nel metodo, l'**unico** punto che lo conclude è `return true;` in fondo. Tutte le
righe prima sono passi che si eseguono e basta (log, costruzione oggetto,
scrittura risposta): il flusso ci passa attraverso e continua. Non mettere
`return` davanti a `logger.LogError(...)` (ritorna `void`) né davanti a
`problem.Extensions["errors"] = …` (è un'assegnazione, non un `bool`).

### 4.6 `ValidationFilter` (`Validation/`)

Un **action filter** è codice che ASP.NET Core esegue *attorno* all'azione di un
controller:
```
OnActionExecutionAsync(context, next)
   ├─ codice PRIMA dell'azione
   ├─ await next()   → esegue l'azione del controller (e i filtri successivi)
   └─ codice DOPO l'azione
```

Il nostro lavora **prima**: per ogni argomento che l'azione sta per ricevere (i
DTO bindati dal body JSON):
1. costruisce il tipo `IValidator<TipoDelDTO>` e chiede alla DI se ne esiste uno;
2. se sì, lo esegue;
3. se non è valido, raggruppa gli errori per campo e lancia
   `ValidationException` → il gestore la trasforma in 422.

Registrato **una volta** a livello globale, valida ogni richiesta senza che i
controller ci pensino (DRY).

Concetti chiave:
- `typeof(IValidator<>).MakeGenericType(t)` — **reflection**: `IValidator<>` è il
  generico "aperto"; `MakeGenericType(t)` lo chiude su `t` → `IValidator<t>`.
  Serve perché il filtro è generico e non conosce i tipi dei DTO a compile-time.
- `context.HttpContext.RequestServices.GetService(type)` — chiede alla DI un
  servizio dato il suo `Type` (invece di `GetService<T>()`).
- `is not IValidator validator` — nessun validator registrato → `continue`.
- **collisione di nomi**: esiste anche `FluentValidation.ValidationException`.
  L'alias `using ValidationException = GoCare.Shared.Errors.ValidationException;`
  dice "in questo file `ValidationException` è la nostra". Un `using X = Namespace.Tipo;`
  ha la precedenza sui tipi importati da un `using` di namespace.
- LINQ `GroupBy` + `ToDictionary` — da lista di `ValidationFailure` a mappa
  `campo → messaggi[]`.

### 4.7 `ICurrentUser` (`Abstractions/`)

Ogni richiesta autenticata porta un **JWT**. ASP.NET Core lo valida ed espone i
**claim** (coppie `chiave = valore`) su `HttpContext.User`.

I Service devono sapere **chi chiama** per autorizzare ("questo trasporto è
tuo?") e per marcare i record creati. Pescare dentro `HttpContext.User` ovunque è
scomodo, duplicato e poco testabile, e trascina `Microsoft.AspNetCore.*` dentro
classi che dovrebbero essere sola logica → si incapsula dietro `ICurrentUser`,
una **porta** che espone solo lo stretto necessario:

```csharp
public interface ICurrentUser
{
    Guid? AccountId { get; }       // null se richiesta anonima
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
```

- `AccountId` è `Guid?` (nullable): una richiesta può essere anonima (login,
  registrazione). Chi lo usa gestisce il caso `null`.
- `IsAuthenticated` — comodità leggibile (in pratica `AccountId is not null`).
- `IsInRole` è un **metodo**, non una proprietà, perché fa una domanda con un
  parametro; `role` in camelCase.

Stesso schema di `IClock`: interfaccia nel kernel condiviso, implementazione reale
(`CurrentUser`, che legge `IHttpContextAccessor`) registrata a runtime nell'host,
finto con valori fissi nei test.

Solo l'**interfaccia** sta in `GoCare.Shared`; `CurrentUser` la scriveremo con
`GoCare.Api`, per tenere `GoCare.Shared` il più possibile senza dipendenze di
runtime.

**Da decidere in Fase 3 (Auth):** quale claim porta l'id (`sub` standard vs
`ClaimTypes.NameIdentifier`, che ASP.NET a volte rimappa) e se aggiungere
`PersonId` / `AssociationId` (`Guid?` — un account è *o* utente *o* associazione,
mai entrambi) quando `TokenService` li metterà nel token.

### 4.8 `Pagination/` — `PageQuery` e `PagedResult<T>`

Gli endpoint di lista (`GET /notifications`, `GET /transports`) non restituiscono
tutte le righe: il client chiede **una pagina alla volta**
(`?page=3&pageSize=20`). Servono due tipi speculari, uno per direzione.

**`PageQuery` — l'input (client → server).** Rappresenta la coppia
`Page`/`PageSize` **dopo la pulizia**: dei numeri grezzi del client non ci si
fida (`?page=-5&pageSize=999999` metterebbe in ginocchio il DB).

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

- `const` privati (`PascalCase`, niente `_`) per non avere numeri magici sparsi:
  un solo punto da toccare.
- `{ get; }` senza `set`, valorizzate solo nel costruttore → oggetto
  **immutabile** dopo la creazione.
- parametri con default → `new PageQuery()` = pagina 1 da 20; `new PageQuery(3)` =
  pagina 3 da 20.
- `Page` sotto 1 → forzato a 1 (le pagine partono da 1, non da 0).
- `PageSize` fuori da `[1, 100]` → ripiega su 20 (un valore folle è di norma un
  bug del client; dargli il default è più prudente che dargli 100 righe).
- `Skip` — proprietà **calcolata** (`=>`, nessun valore memorizzato): quante righe
  saltare per arrivare all'inizio della pagina. `(Page-1)*PageSize`. È il numero
  da passare a `query.Skip(...).Take(PageSize)` in EF Core.

**`PagedResult<T>` — l'output (server → client).** La pagina corrente + i
metadati per navigare.

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

- **`record` posizionale**: i parametri nell'intestazione diventano proprietà
  `{ get; init; }` + costruttore, generati dal compilatore. Si usa qui (puro
  trasporto dati, nessuna normalizzazione), mentre `PageQuery` ha il corpo scritto
  a mano proprio perché lì serve la logica di pulizia.
- `<T>` generico: lo stesso contenitore vale per `PagedResult<TransportDto>`,
  `PagedResult<NotificationDto>`… Il tipo della riga lo decide il chiamante.
- `IReadOnlyList<T>` per `Items`: sola lettura, il client non deve poter
  aggiungere/togliere righe da una risposta.
- `TotalCount` = righe totali su **tutte** le pagine (richiede un `COUNT(*)`
  separato sul DB). Serve al client per "1–20 di 350" e per sapere quante pagine
  esistono.
- `TotalPages` è **derivato** da `TotalCount`/`PageSize`, quindi sta nel corpo e
  non fra i parametri:
  - `(double)` sul divisore: `int / int` è divisione **intera** (`50/20 == 2`,
    resto buttato); col cast → `50 / 20.0 == 2.5`.
  - `Math.Ceiling` arrotonda **verso l'alto**: 50 righe da 20/pagina → 3 pagine
    (20+20+10). Restituisce `double` → `(int)` davanti.
  - `PageSize <= 0 ? 0 : …` — guardia anti divisione per zero (`PagedResult` può
    essere costruito a mano nei test, senza passare da `PageQuery`).

Entrambi in `GoCare.Shared` perché non dipendono da nulla (né EF né ASP.NET) e
servono a controller e Service di ogni area.

### 4.9 `DependencyInjection.cs` — `AddSharedKernel()`

Un servizio è disponibile per la DI solo se **registrato** nel contenitore. Per
non spargere le registrazioni di `Shared` nel `Program.cs` dell'host, `Shared`
espone **un metodo solo** che le fa tutte:

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

L'host scrive `builder.Services.AddSharedKernel();` e ha tutto. È lo schema di
`AddControllers()`, `AddDbContext()`… ogni libreria porta il suo pacchetto.

**Metodo di estensione:** `static` method in `static class`, primo parametro con
`this` davanti (`this IServiceCollection services`) → lo chiami *come se* fosse un
metodo di `IServiceCollection` (`services.AddSharedKernel()`). Restituisce
`IServiceCollection` per concatenare.

**`IServiceCollection`** è un'**interfaccia** (la `I` maiuscola) definita e
implementata da Microsoft (classe concreta `ServiceCollection`); l'istanza la crea
il framework in `WebApplication.CreateBuilder` (`builder.Services`). Tu la ricevi
già pronta e ci chiami sopra i metodi — non la istanzi né la implementi. È
tipizzata come interfaccia per lo stesso motivo per cui i Service dipendono da
`IClock` e non da `SystemClock`: dipendere dal contratto. È il "carrello" della
DI: la lista di *"quando qualcuno chiede X, dagli Y"*, riempita all'avvio e
congelata da `builder.Build()`.

**Lifetime (durata di vita di un servizio registrato):**

| Metodo | Quante istanze | Quando |
|---|---|---|
| `AddSingleton` | **una** per tutta la vita dell'app | oggetti **senza stato** e **thread safe** (`SystemClock`) |
| `AddScoped` | **una per richiesta HTTP** | roba legata alla richiesta (`DbContext`, `CurrentUser`, un filtro) |
| `AddTransient` | **una nuova a ogni richiesta** del tipo | oggetti leggeri usa-e-getta |

- **Senza stato (stateless):** l'oggetto non ha campi mutabili che ricorda tra una
  chiamata e l'altra. `SystemClock` legge l'orologio del SO e restituisce, non
  salva niente → una sola istanza o mille è identico.
- **Thread safe:** un server serve tante richieste in parallelo, ognuna su un
  thread. Un Singleton è toccato da più thread insieme. È thread safe se questo
  non rompe niente — vero se non c'è stato condiviso da modificare (`++_valore`
  fatto da due thread insieme perde un incremento: leggi-calcola-scrivi non è
  atomico). Un Singleton **con** stato condiviso va protetto (`lock`,
  `Interlocked`, tipi `Concurrent*`) o, meglio, evitato: usa `Scoped`.

**Cosa `AddSharedKernel` NON fa:**
- non registra `IEmailSender` né `ICurrentUser`: le implementazioni
  (`SmtpEmailSender`, `CurrentUser`) stanno fuori da `Shared` → le registra l'host;
- non fa `app.UseExceptionHandler()` né aggancia il `ValidationFilter` alla
  pipeline MVC: quelle toccano l'`app` (middleware) e `AddControllers`, competono
  al `Program.cs`. `AddSharedKernel` tocca **solo** `IServiceCollection`.
- `AddExceptionHandler<GlobalExceptionHandler>()` da solo non basta: senza
  `app.UseExceptionHandler()` nell'host, l'handler non viene mai invocato.

Convenzione alternativa Microsoft: mettere questi metodi nel namespace
`Microsoft.Extensions.DependencyInjection` (così l'host non scrive nemmeno
`using GoCare.Shared;`). Qui si tiene `namespace GoCare.Shared;` per esplicitare
la provenienza. Stile, cambiabile.

---

## 5. Configurazione del progetto

### `FrameworkReference` vs `PackageReference`
- **`PackageReference`** = un pacchetto singolo scaricato da NuGet (es.
  `FluentValidation`).
- **`FrameworkReference`** = un intero *shared framework* già installato con
  l'SDK. `Microsoft.AspNetCore.App` dà con una riga `HttpContext`,
  `IExceptionHandler`, MVC, DI… senza scaricare nulla.

`GoCare.Shared` ha entrambi: `FrameworkReference Microsoft.AspNetCore.App` (per
`GlobalExceptionHandler`, `ValidationFilter`, `CurrentUser`) e
`PackageReference FluentValidation` (per `ValidationFilter`).

### FluentValidation
Libreria per scrivere le regole di validazione di un oggetto in stile leggibile:
```csharp
RuleFor(x => x.Email).NotEmpty().EmailAddress();
RuleFor(x => x.Password).MinimumLength(8);
```
I validator concreti (uno per DTO) vivranno in `GoCare.Application`. In
`GoCare.Shared` c'è solo il `ValidationFilter` che li esegue. Versione installata:
**12.1.1**.

### `Directory.Build.props` (a livello `BackEnd/`)
Vale per tutti i progetti. Punti rilevanti: `net10.0`, `Nullable enable`,
`ImplicitUsings enable`, **warning non bloccanti** (`TreatWarningsAsErrors=false`),
analyzer attivi, `NoWarn` per `CA1716` (namespace "Shared") e `CA1848`
(`LoggerMessage` non serve fuori dai percorsi caldi).

---

## 6. Stato di `GoCare.Shared`

**Fatto:**
- `Abstractions/IClock.cs`, `SystemClock.cs`, `IEmailSender.cs`,
  `ICurrentUser.cs` (interfaccia)
- `Errors/DomainException.cs`, `NotFoundException.cs`, `ConflictException.cs`,
  `ForbiddenException.cs`, `ValidationException.cs`, `GlobalExceptionHandler.cs`
- `Validation/ValidationFilter.cs`
- `Pagination/PageQuery.cs`, `PagedResult.cs`
- `DependencyInjection.cs` — `AddSharedKernel()`

`GoCare.Shared` è **completo lato codice** (build 0/0). I pezzi rimasti vivono
nell'host `GoCare.Api`.

**Da fare (nell'host `GoCare.Api`, Fase 1):**
- `CurrentUser.cs` — implementazione di `ICurrentUser` con `IHttpContextAccessor`
  (in `GoCare.Api` o `GoCare.Application/Infrastructure/`); registrarla nella DI
  dell'host.
- `Program.cs`: `builder.Services.AddSharedKernel()`, `app.UseExceptionHandler()`,
  `AddControllers(o => o.Filters.AddService<ValidationFilter>())`, Swagger.
