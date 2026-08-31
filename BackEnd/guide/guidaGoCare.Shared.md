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

### 4.7 `ICurrentUser` (`Abstractions/`) — *interfaccia da scrivere*

Ogni richiesta autenticata porta un **JWT**. ASP.NET Core lo valida ed espone i
**claim** (coppie `chiave = valore`) su `HttpContext.User`. Nel nostro token
(piano §5.1): `sub` (id account), `role`, `person_id`, `association_id`,
`email_verified`.

I Service devono sapere **chi chiama** per autorizzare ("questo trasporto è
tuo?") e per marcare i record creati. Pescare dentro `HttpContext.User` ovunque è
scomodo e poco testabile → si incapsula dietro `ICurrentUser` con proprietà
pulite. I Service dipendono da `ICurrentUser`; nei test un finto con valori
fissi. Stesso schema di `IClock`.

`PersonId` e `AssociationId` sono `Guid?` (nullable): un account è *o* un utente
(ha `PersonId`) *o* un'associazione (ha `AssociationId`), mai entrambi.

L'implementazione `CurrentUser` (da fare) userà `IHttpContextAccessor` per
raggiungere l'`HttpContext` della richiesta corrente da una classe non-controller.

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
- `Abstractions/IClock.cs`, `SystemClock.cs`, `IEmailSender.cs`
- `Errors/DomainException.cs`, `NotFoundException.cs`, `ConflictException.cs`,
  `ForbiddenException.cs`, `ValidationException.cs`, `GlobalExceptionHandler.cs`
- `Validation/ValidationFilter.cs`

**Da fare:**
- `Abstractions/ICurrentUser.cs` (interfaccia) + `CurrentUser.cs` (implementazione
  con `IHttpContextAccessor`)
- `Pagination/PageQuery.cs` + `PagedResult<T>.cs`
- `DependencyInjection.cs` — `AddSharedKernel()` che registra `IClock`,
  `ICurrentUser`, `GlobalExceptionHandler`, `AddProblemDetails()`,
  `AddHttpContextAccessor()`, `ValidationFilter`.
- Nell'host: agganciare `UseExceptionHandler()`, il `ValidationFilter` globale,
  Swagger.
