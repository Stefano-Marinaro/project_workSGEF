# Guida — `GoCare.Api` (host)

Teoria vista costruendo l'host `GoCare.Api`: il progetto eseguibile che avvia
il web server, monta la pipeline HTTP e collega i pezzi che le librerie
(`GoCare.Shared`, `GoCare.Application`) lasciano "da collegare".

Complementare a `guidaGoCare.Shared.md`.

---

## 1. Cos'è `GoCare.Api` e cosa fa la Fase 1

`GoCare.Api` è l'unico progetto **eseguibile** (`Sdk.Web`, `Program.cs` con
`app.Run()`). Le librerie non partono da sole: l'host

- crea il `WebApplication` (`builder` → `app`);
- **registra i servizi** nella DI (`builder.Services.Add…`);
- **compone la pipeline** dei middleware (`app.Use…`);
- fornisce le implementazioni concrete delle porte che le librerie dichiarano
  come interfaccia (`ICurrentUser`, `IEmailSender`, i due `DbContext`…).

**Fase 1** = agganciare `GoCare.Shared` all'host:

| # | Cosa | Perché |
|---|------|--------|
| 1 | `CurrentUser : ICurrentUser` | `Shared` ha solo l'interfaccia; l'implementazione legge lo `HttpContext` → vive nell'host. |
| 2 | `Program.cs`: `AddSharedKernel()`, `UseExceptionHandler()`, `ValidationFilter` globale, Swagger | attivare e agganciare alla pipeline ciò che `AddSharedKernel()` registra. |
| 3 | `appsettings.json`: connection string `AuthDb` e `BusinessDb` | placeholder ora; il DB vero è Fase 2. |

---

## 2. Claims

### Cos'è un claim

Un'affermazione su un soggetto nella forma **`(tipo, valore)`**, rilasciata da
un'autorità che ne garantisce la veridicità.

| tipo | valore | traduzione |
|------|--------|-----------|
| `sub` (NameIdentifier) | `a3f1…-9c2e` | "l'id di questo utente è a3f1…" |
| `email` | `mario@rossi.it` | "la sua email è …" |
| `role` | `admin` | "ha il ruolo admin" |
| `association_id` | `77b2…` | "appartiene all'associazione 77b2…" (claim nostro) |

Il punto: **non è l'utente a dichiararle, è il server** a certificarle al login
e a firmarle. Chi le riceve dopo si fida perché la firma è valida.

### I tre livelli in .NET

```
Claim            → una coppia (tipo, valore)             es. ("role", "admin")
ClaimsIdentity   → insieme di claim da UNA fonte + AuthenticationType + IsAuthenticated
ClaimsPrincipal  → l'utente: una o più ClaimsIdentity    es. HttpContext.User
```

`HttpContext.User` è un `ClaimsPrincipal`, di norma con una sola identità
(quella del JWT).

### Il flusso

```
1. LOGIN
   Client → POST /auth/login { email, password }
   Server: verifica le credenziali sul DB, costruisce la lista di claim
           [ sub=<accountId>, email=…, role=…, association_id=… ],
           la mette in un JWT e lo firma con una chiave segreta.
   Server → { accessToken: "eyJhbGciOi…" }

2. RICHIESTE SUCCESSIVE
   Client → GET /transports   Authorization: Bearer eyJhbGciOi…

3. SUL SERVER, per ogni richiesta
   Il middleware di autenticazione:
     - verifica firma (token non manomesso), scadenza, issuer, audience
     - se ok: legge i claim e costruisce
       HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
     - IsAuthenticated → true

4. NEL CODICE
   CurrentUser.AccountId legge User.FindFirstValue(NameIdentifier)  → il claim "sub"
```

Il DB si tocca **solo al passo 1**. Dopo, tutto ciò che serve sull'utente è già
nel token, firmato.

### Perché claim + JWT

- **vs sessione lato server:** la sessione va riletta a ogni richiesta e
  condivisa fra più server. Il JWT è **stateless**: un server dietro un load
  balancer non richiede nulla in più.
- **vs solo ruoli** (`[Authorize(Roles="admin")]`): troppo rigido. I claim
  portano dati arbitrari (quale associazione, quale persona, quali permessi).

In più: un solo controllo d'identità (la firma, niente query al DB a ogni
chiamata); estensibile (aggiungi un claim al login); autorizzazione
disaccoppiata dall'archiviazione (i Service dipendono da `ICurrentUser`);
standard OpenID Connect / OAuth2.

### Il rovescio

- **Un JWT valido non si revoca prima della scadenza.** Se banni un utente il
  token resta buono fino a scadenza. Mitigazione: scadenza breve (~15 min) +
  *refresh token* revocabile (entità `RefreshToken`).
- **I claim sono leggibili da chiunque abbia il token** — la firma garantisce
  integrità, non segretezza. Dentro solo id e attributi di autorizzazione, mai
  password o dati sensibili.
- **I claim fotografano il login.** Cambio di ruolo → il vecchio token ha
  quello vecchio fino al refresh.

### In GoCare

`ICurrentUser` è l'astrazione con cui i Service leggono l'identità:

- `AccountId` ← claim `sub` — per marcare "creato da" e i controlli "è roba
  tua?";
- `IsInRole("admin")` ← claim `role` — per gli endpoint di accreditamento;
- in Fase 3 si decide se aggiungere claim custom (`person_id`,
  `association_id`) per evitare una query in più a ogni richiesta.

---

## 3. `CurrentUser` — l'adapter di `ICurrentUser`

`GoCare.Shared` espone la **porta** `ICurrentUser`; `GoCare.Api` fornisce
l'**adapter** che sotto usa lo `HttpContext`. Stesso schema di
`IClock`/`SystemClock`.

```csharp
using System.Security.Claims;
using GoCare.Shared.Abstractions;

namespace GoCare.Api.Security;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public Guid? AccountId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : null;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
```

### `IHttpContextAccessor` e non `HttpContext` diretto

`HttpContext` è diverso per ogni richiesta e non esiste fuori da una richiesta;
`CurrentUser` è un oggetto solo. L'`accessor` è un livello di indirezione:
sempre lo stesso oggetto, ma `.HttpContext` restituisce *lo `HttpContext` della
richiesta che sta girando adesso su questo thread* (o `null` fuori da una
richiesta: avvio, job in background).

`AddHttpContextAccessor()` lo registra (già in `AddSharedKernel()`). Il
costruttore primario lo fa iniettare. `IHttpContextAccessor` è tra gli
`ImplicitUsings` di un progetto `Sdk.Web` → nessun `using` esplicito.

### `private ClaimsPrincipal? User => accessor.HttpContext?.User;`

Proprietà privata di comodo. `=>` = ricalcolata a ogni accesso (la richiesta
corrente cambia). `?.`: se `HttpContext` è `null`, l'espressione è `null`
invece di lanciare `NullReferenceException`.

### `AccountId`, da dentro a fuori

1. `ClaimTypes.NameIdentifier` — costante stringa con cui ASP.NET, di default,
   mappa il claim `sub` (id utente). Si usa la costante, non l'URI a mano.
2. `User?.FindFirstValue(...)` — primo claim di quel tipo → **`string`**, o
   `null`. Col `?.`: `User` null → `null`.
3. `Guid.TryParse(stringaForseNull, out var id)` — conversione **senza
   eccezioni**: `bool` (`false` se null/vuota/non GUID) e scrive in `id`.
4. `... ? id : null` — parse riuscito → `id`; fallito → `null`. Tipo comune:
   `Guid?`.

`TryParse` e non `Parse` perché il contenuto del token è **input non fidato**:
un claim mancante o malformato deve dare `null`, non un crash.

### `IsAuthenticated` e `IsInRole`

- `User?.Identity?.IsAuthenticated` è **`bool?`** (i `?.` possono dare `null`).
  `?? false` lo porta a `bool`. "Autenticato ⇔ c'è un'identità e dice di
  esserlo".
- `IsInRole` è un **metodo** (ha un parametro). `User?.IsInRole(role)` →
  `bool?` → `?? false`: nessun utente ⇒ in nessun ruolo.

### Quadro d'insieme

`CurrentUser` è un **traduttore**: da un lato ASP.NET (`HttpContext`,
`ClaimsPrincipal`, claim), dall'altro il contratto pulito `ICurrentUser`. Tre
accortezze: `?.` ovunque (parti mancanti → "utente sconosciuto", niente
crash); `?? false` per `bool?` → `bool`; `Guid.TryParse` perché il token è
input non fidato.

### Registrazione (in `Program.cs`, non in `AddSharedKernel`)

`AddSharedKernel` non registra `ICurrentUser` (l'implementazione sta fuori da
`Shared`). Lo fa l'host:

```csharp
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
```

`Scoped` è la scelta idiomatica per roba "per richiesta". Qui sarebbe
indifferente (nessuno stato proprio, delega all'`accessor`), ma non lascia
dubbi.

---

## 4. Swagger e `launchSettings.json`

Swagger è la UI web (`/swagger`) che elenca gli endpoint e li fa provare dal
browser. In GoCare è attivo (`AddSwaggerGen` + `UseSwagger`/`UseSwaggerUI`
dietro `if (app.Environment.IsDevelopment())`), ma non si apre da solo e la
root `/` dà 404.

1. **`launchSettings.json` non è configurazione dell'app.** Sta in
   `Properties/`, vale solo per `dotnet run` / F5 in locale, non è pubblicato.
   Decide URL da ascoltare, `ASPNETCORE_ENVIRONMENT`, se aprire il browser
   (`launchBrowser`) e su quale path (`launchUrl`). Per aprire Swagger
   all'avvio:

   ```json
   "http": {
     "commandName": "Project",
     "dotnetRunMessages": true,
     "launchBrowser": true,
     "launchUrl": "swagger",
     "applicationUrl": "http://localhost:5161",
     "environmentVariables": { "ASPNETCORE_ENVIRONMENT": "Development" }
   }
   ```

2. **La root `/` è 404**: non c'è endpoint sulla radice. Swagger è su
   `/swagger`, il JSON OpenAPI su `/swagger/v1/swagger.json`. Vedere il 404
   sulla root non vuol dire "l'app non parte".

3. **Swagger solo in `Development`.** Lanciando il `.dll` compilato senza
   `ASPNETCORE_ENVIRONMENT=Development`, l'ambiente è `Production`, il blocco
   `if (IsDevelopment())` non registra Swagger → `/swagger` dà 404. I profili
   di `launchSettings.json` impostano `Development`; l'avvio diretto del `.dll`
   no.

Nota di versione: dai template .NET 9/10 Microsoft non mette più Swashbuckle di
default. In GoCare `Swashbuckle.AspNetCore` 10.2.3 è aggiunto a mano.

---

## 5. Stato di `GoCare.Api`

**Fatto (Fase 1):**

- `Security/CurrentUser.cs` — `ICurrentUser`, registrata
  `AddScoped<ICurrentUser, CurrentUser>()` in `Program.cs`.
- `Program.cs`: `AddSharedKernel()`, `AddScoped<ICurrentUser, CurrentUser>()`,
  `AddControllers(o => o.Filters.AddService<ValidationFilter>())`,
  `AddEndpointsApiExplorer()` + `AddSwaggerGen()`; pipeline:
  `UseExceptionHandler()` per primo, Swagger solo in Development,
  `UseHttpsRedirection()`, `MapControllers()`.
- `appsettings.json`: connection string `AuthDb` (`gocare_auth`) e `BusinessDb`
  (`gocare_business`), placeholder `postgres/postgres` — la password vera
  passerà a user-secrets in Fase 2.
- `Swashbuckle.AspNetCore` 10.2.3 in `GoCare.Api.csproj`.
- `launchSettings.json`: profili `http`/`https` con `launchBrowser: true` e
  `launchUrl: "swagger"`.
- Verifica: `dotnet build` 0/0 sull'intera solution; `dotnet run` avvia l'host,
  `/swagger` e `/swagger/v1/swagger.json` rispondono 200.

**Fatto (Fase 2, lato dominio):** `AddApplication(builder.Configuration)` in
`Program.cs` — registra `BusinessDbContext`. Dettagli in
`guidaGoCare.Application.md`.

**Da fare:** `AuthDbContext` (a cura del collega) e la sua registrazione in
`AddApplication`; prime migrazioni EF Core.
