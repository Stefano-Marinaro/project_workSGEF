# Guida — `GoCare.Api` (host)

Spiegazioni teoriche raccolte mentre costruiamo l'host `GoCare.Api`: il progetto
eseguibile che avvia il web server, monta la pipeline HTTP e mette insieme i pezzi
che le librerie (`GoCare.Shared`, `GoCare.Application`) lasciano "da collegare".

Complementare a `guidaGoCare.Shared.md`. Da rileggere quando serve.

---

## 1. Cos'è `GoCare.Api` e cosa fa la Fase 1

`GoCare.Api` è l'unico progetto **eseguibile** (`Sdk.Web`, ha un `Program.cs` con
`app.Run()`). Le librerie non "partono" da sole: l'host

- crea il `WebApplication` (`builder` → `app`);
- **registra i servizi** nel contenitore DI (`builder.Services.Add…`);
- **compone la pipeline** dei middleware (`app.Use…`);
- fornisce le implementazioni concrete delle porte che le librerie dichiarano
  solo come interfaccia (`ICurrentUser`, `IEmailSender`, i due `DbContext`…).

**Fase 1** = agganciare `GoCare.Shared` all'host:

| # | Cosa | Perché |
|---|------|--------|
| 1 | `CurrentUser : ICurrentUser` | `GoCare.Shared` ha solo l'interfaccia; l'implementazione legge lo `HttpContext` → vive nell'host. |
| 2 | `Program.cs`: `AddSharedKernel()`, `UseExceptionHandler()`, `ValidationFilter` globale, Swagger | attivare e **agganciare alla pipeline** ciò che `AddSharedKernel()` registra. |
| 3 | `appsettings.json`: connection string `AuthDb` e `BusinessDb` | placeholder ora; il DB vero è Fase 2. |

---

## 2. Claims: cosa sono e perché

### Cos'è un claim

Un **claim** ("asserzione") è un'affermazione su un soggetto, nella forma
**`(tipo, valore)`**, rilasciata da un'autorità che ne garantisce la veridicità.

| tipo | valore | traduzione |
|------|--------|-----------|
| `sub` (NameIdentifier) | `a3f1…-9c2e` | "l'id di questo utente è a3f1…" |
| `email` | `mario@rossi.it` | "la sua email è …" |
| `role` | `admin` | "ha il ruolo admin" |
| `association_id` | `77b2…` | "appartiene all'associazione 77b2…" (claim nostro) |

Il punto: **non è l'utente a dichiarare queste cose, è il server** a certificarle
al login e a firmarle. Chi le riceve dopo si fida perché la firma è valida.

### I tre livelli in .NET

```
Claim            → una coppia (tipo, valore)             es. ("role", "admin")
ClaimsIdentity   → insieme di claim da UNA fonte         es. "i claim estratti dal JWT"
                   + AuthenticationType ("Bearer") + IsAuthenticated
ClaimsPrincipal  → l'utente: una o più ClaimsIdentity    es. HttpContext.User
```

`HttpContext.User` è un `ClaimsPrincipal`. Di norma ha una sola identità (quella
del JWT); il modello ne prevede più d'una.

### Il flusso

```
1. LOGIN
   Client → POST /auth/login { email, password }
   Server: verifica le credenziali sul DB, COSTRUISCE la lista di claim
           [ sub=<accountId>, email=…, role=…, association_id=… ],
           la mette in un JWT e lo FIRMA con una chiave segreta.
   Server → { accessToken: "eyJhbGciOi…" }

2. RICHIESTE SUCCESSIVE
   Client → GET /transports   Authorization: Bearer eyJhbGciOi…

3. SUL SERVER, per ogni richiesta
   Il middleware di autenticazione:
     - verifica la FIRMA (token non manomesso), scadenza, issuer, audience
     - se ok: legge i claim e costruisce
       HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
     - IsAuthenticated diventa true

4. NEL CODICE
   CurrentUser.AccountId legge User.FindFirstValue(NameIdentifier)  → il claim "sub"
```

Il DB viene toccato **solo al passo 1**. Dopo, tutto ciò che serve sull'utente è
già nel token, firmato.

### Perché i claim invece di alternative

- **vs sessione lato server** (id di sessione nel cookie, dati sul server): la
  sessione va riletta a ogni richiesta e condivisa fra più server. Il JWT è
  **stateless**: aggiungere un server dietro un load balancer non richiede nulla.
- **vs solo ruoli** (`[Authorize(Roles="admin")]`): troppo rigido. I claim portano
  **dati arbitrari** (quale associazione, quale persona, quali permessi), non solo
  il ruolo.

Vantaggi dei claim + JWT:

1. **Stateless** — il server non ricorda nulla tra le richieste; il token è
   autosufficiente.
2. **Un solo controllo d'identità** — la firma. Non "aspetta che controllo il DB"
   a ogni chiamata.
3. **Estensibile** — serve l'associazione senza query? Ci metti un claim
   `association_id` al login.
4. **Autorizzazione disaccoppiata dall'archiviazione** — i Service dipendono da
   `ICurrentUser`, non da com'è fatta la tabella utenti. Cambi provider di
   identità e il dominio non se ne accorge.
5. **Standard** — è il modello di OpenID Connect / OAuth2.

### Il rovescio (da sapere)

- **Un JWT valido non si revoca prima della scadenza.** Se banni un utente, il suo
  token resta buono fino a scadenza. Mitigazione: scadenza breve (es. 15 min) +
  *refresh token* revocabile (entità `RefreshToken` nell'area Auth).
- **I claim sono leggibili da chiunque abbia il token** — la firma garantisce
  l'integrità, non la segretezza. Dentro solo id e attributi di autorizzazione,
  mai password o dati sensibili.
- **I claim fotografano il login.** Cambio di ruolo → il vecchio token ha ancora
  quello vecchio fino al refresh.

### In GoCare

`ICurrentUser` è l'astrazione con cui i Service leggono l'identità:
- `AccountId` ← claim `sub` — per marcare "creato da" e per i controlli "è roba
  tua?";
- `IsInRole("admin")` ← claim `role` — per gli endpoint di accreditamento
  associazioni;
- in Fase 3 si decide se aggiungere claim custom (`person_id`, `association_id`)
  per evitare una query in più a ogni richiesta.

---

## 3. `CurrentUser` — l'adapter di `ICurrentUser`

`GoCare.Shared` espone la **porta** `ICurrentUser`; `GoCare.Api` fornisce
l'**adapter** che sotto usa lo `HttpContext`. Stesso schema di `IClock`/
`SystemClock`.

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

### Perché `IHttpContextAccessor` e non `HttpContext` diretto

`HttpContext` è diverso per ogni richiesta e non esiste fuori da una richiesta.
`CurrentUser` invece è un oggetto solo. L'`accessor` è un livello di indirezione:
è sempre lo stesso oggetto, ma la sua `.HttpContext` restituisce *lo `HttpContext`
della richiesta che sta girando adesso su questo thread* (o `null` se non siamo
dentro una richiesta: avvio, job in background).

`AddHttpContextAccessor()` lo registra — l'abbiamo già messo in
`AddSharedKernel()`. Il costruttore primario `(IHttpContextAccessor accessor)` lo
fa iniettare dalla DI.

`IHttpContextAccessor` sta nel namespace `Microsoft.AspNetCore.Http`, tra gli
`ImplicitUsings` di un progetto `Sdk.Web` → nessun `using` esplicito.

### `private ClaimsPrincipal? User => accessor.HttpContext?.User;`

Proprietà privata di comodo (evita di ripetere `accessor.HttpContext?.User` tre
volte). `=>` = ricalcolata a ogni accesso, non memorizzata — giusto, la richiesta
corrente cambia. `?.` (null-conditional): se `HttpContext` è `null`, l'intera
espressione è `null` invece di lanciare `NullReferenceException`. Tipo di ritorno
`ClaimsPrincipal?`.

### `AccountId`

Dall'interno all'esterno:

1. `ClaimTypes.NameIdentifier` — costante stringa (un URI lungo). È il "tipo" con
   cui ASP.NET, di default, mappa il claim `sub` (l'id utente). Si usa la costante
   invece dell'URI a mano.
2. `User?.FindFirstValue(...)` — metodo di estensione su `ClaimsPrincipal` (da
   `System.Security.Claims`): primo claim di quel tipo → **valore `string`**, o
   `null` se non c'è. Col `?.`: `User` null → `null`. Risultato: `string?`.
3. `Guid.TryParse(stringaForseNull, out var id)` — prova la conversione **senza
   lanciare eccezioni**: ritorna `bool` (`false` se `null`/vuota/non un GUID) e
   scrive il risultato in `id` (`out var id` dichiara la variabile nella chiamata
   stessa; `var` deduce `Guid`).
4. `... ? id : null` — ternario sul `bool` di `TryParse`: parse riuscito → `id`;
   fallito → `null`. Tipo comune dei due rami: `Guid?`, come la proprietà.

`TryParse` e non `Parse` perché il contenuto del token è **input non fidato**: un
claim mancante o malformato deve dare `null`, non un crash.

### `IsAuthenticated` e `IsInRole`

- `User?.Identity?.IsAuthenticated` ha tipo **`bool?`** (i due `?.` possono
  produrre `null`). `?? false` (null-coalescing: `a ?? b` = "`a` se non null,
  altrimenti `b`") lo converte in `bool`, che l'interfaccia pretende. Traduzione:
  "autenticato ⇔ c'è un'identità e dice di esserlo".
- `IsInRole` è un **metodo** (ha un parametro). `User?.IsInRole(role)` →
  `bool?` → `?? false`: nessun utente ⇒ in nessun ruolo.

### Il quadro d'insieme

`CurrentUser` è un **traduttore**: da un lato ASP.NET (`HttpContext`,
`ClaimsPrincipal`, claim), dall'altro il contratto pulito `ICurrentUser`. Tre
accortezze ricorrenti:

- **`?.`** ovunque: `HttpContext` e le sue parti possono mancare → si degrada a
  "utente sconosciuto", niente crash;
- **`?? false`** per passare da `bool?` a `bool`;
- **`Guid.TryParse`** perché il token è input non fidato.

### Registrazione (in `Program.cs`, non in `AddSharedKernel`)

`AddSharedKernel` **non** registra `ICurrentUser`: l'implementazione sta fuori da
`Shared`. Lo fa l'host:

```csharp
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
```

`Scoped` (una per richiesta HTTP): concettualmente è legato alla richiesta. In
pratica qui sarebbe indifferente — non ha stato proprio, delega tutto
all'`accessor` — ma `Scoped` è la scelta idiomatica per roba "per richiesta" e
non lascia dubbi.

---

## 4. Swagger e `launchSettings.json`

Swagger è la UI web (`/swagger`) che elenca gli endpoint e permette di provarli
dal browser. In GoCare è attivo (`AddSwaggerGen` + `UseSwagger`/`UseSwaggerUI`
dietro `if (app.Environment.IsDevelopment())`), ma **non si apre da solo** e la
root `/` dà 404. Tre cose da sapere:

1. **`launchSettings.json` non è configurazione dell'app.** Sta in
   `Properties/`, vale **solo** per `dotnet run` / F5 in locale, e non viene
   pubblicato. Decide: quali URL ascoltare, quale `ASPNETCORE_ENVIRONMENT`
   impostare, se aprire il browser (`launchBrowser`) e su quale path
   (`launchUrl`). Il template di GoCare ha `launchBrowser: false` e nessun
   `launchUrl` → all'avvio non apre niente. Per aprirlo da solo:

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
   `http://localhost:5161/swagger`, il JSON OpenAPI su
   `/swagger/v1/swagger.json`. Andare sulla root e vedere l'errore non vuol dire
   "l'app non parte".

3. **Swagger solo in `Development`.** Se si lancia il `.dll` compilato senza
   impostare `ASPNETCORE_ENVIRONMENT=Development`, l'ambiente è `Production`, il
   blocco `if (app.Environment.IsDevelopment())` non registra Swagger → `/swagger`
   dà 404. I profili di `launchSettings.json` impostano `Development`; l'avvio
   diretto del `.dll` no.

Nota di versione: dai template .NET 9/10 Microsoft non mette più Swashbuckle di
default (usa `Microsoft.AspNetCore.OpenApi` senza UI). In GoCare Swashbuckle
(`Swashbuckle.AspNetCore` 10.2.3) è stato aggiunto a mano.

---

## 5. Stato di `GoCare.Api`

**Fatto (Fase 1):**
- `Security/CurrentUser.cs` — implementazione di `ICurrentUser`, registrata
  `AddScoped<ICurrentUser, CurrentUser>()` in `Program.cs`.
- `Program.cs`: `AddSharedKernel()`, `AddScoped<ICurrentUser, CurrentUser>()`,
  `AddControllers(o => o.Filters.AddService<ValidationFilter>())`,
  `AddEndpointsApiExplorer()` + `AddSwaggerGen()`; pipeline:
  `UseExceptionHandler()` per primo, Swagger solo in Development,
  `UseHttpsRedirection()`, `MapControllers()`.
- `appsettings.json`: connection string `AuthDb` (`gocare_auth`) e `BusinessDb`
  (`gocare_business`), placeholder `postgres/postgres` — la password vera passerà
  a user-secrets/variabili d'ambiente in Fase 2.
- Pacchetto `Swashbuckle.AspNetCore` 10.2.3 aggiunto a `GoCare.Api.csproj`.
- `launchSettings.json`: profili `http`/`https` con `launchBrowser: true` e
  `launchUrl: "swagger"` → `dotnet run` / F5 aprono direttamente `/swagger`.
- Verifica: `dotnet build` 0/0 sull'intera solution; `dotnet run` avvia l'host,
  `/swagger` e `/swagger/v1/swagger.json` rispondono 200 (nessun errore di
  risoluzione DI a runtime).

**Da fare (Fase 2):** i due `DbContext` (`AuthDbContext`, `BusinessDbContext`) in
`GoCare.Application/Data/`, `AddDbContext` con `UseNpgsql(GetConnectionString(...))`
in `Program.cs`, prime migrazioni.
