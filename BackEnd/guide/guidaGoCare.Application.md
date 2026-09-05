# Guida — `GoCare.Application`

Spiegazioni teoriche raccolte mentre costruiamo `GoCare.Application`: il
progetto che contiene sia l'area Auth sia l'area di dominio (convenzione di
cartelle, non due progetti). Qui la guida copre la **Fase 2 (Persistenza)**,
lato dominio: setup PostgreSQL, `BusinessDbContext`, DI, la modellazione
degli enum di dominio (§5) e delle entità di dominio (§7).

Complementare a `guidaGoCare.Shared.md` e `guidaGoCare.Api.md`. Da rileggere
quando serve.

---

## 1. PostgreSQL: due database, due DbContext, un team

**Decisione presa:** ogni sviluppatore installa PostgreSQL **in locale** sulla
propria macchina e ci crea entrambi i database (`gocare_auth`,
`gocare_business`), anche se si occupa di una sola area. Non si condivide un
server Postgres unico fra i membri del team.

Perché:
- ciò che si **condivide via git** è il codice (i due `DbContext`, le
  migrazioni EF Core), non i dati né il server;
- ogni sviluppatore applica le migrazioni al proprio Postgres locale
  (`dotnet ef database update`) e lavora isolato — nessun rischio di rompere
  il lavoro di un collega toccando lo stesso database;
- le password vere non vanno mai in `appsettings.json` (finisce in git):
  restano su `appsettings.Development.json` (ignorato) o `dotnet user-secrets`.

Due database perché due `DbContext` restano separati anche dentro un solo
progetto: `AuthDbContext` → `gocare_auth`, `BusinessDbContext` →
`gocare_business`. Ogni area ha le sue migrazioni, indipendenti.

---

## 2. Conflitto di versione NuGet (`MSB3277`) e come si risolve

**Il problema:** aggiungendo a `GoCare.Application` sia
`Npgsql.EntityFrameworkCore.PostgreSQL` sia
`Microsoft.EntityFrameworkCore.Design`, il build segnala un conflitto — due
pacchetti diversi richiedono **versioni diverse** di
`Microsoft.EntityFrameworkCore` come dipendenza transitiva:

```
Npgsql.EntityFrameworkCore.PostgreSQL  10.0.3  →  richiede EF Core 10.0.4
Microsoft.EntityFrameworkCore.Design   10.0.11 →  richiede EF Core 10.0.11
```

MSBuild non decide da solo quale usare: **`MSB3277`** (warning di conflitto)
ti avvisa che sceglierà una versione ma non è detto sia quella giusta.

**La regola:** il provider database (Npgsql) è il vincolo più stretto — è
lui che sa con quale versione esatta di EF Core è stato testato e compilato.
Si **pinna** `Design` alla stessa versione che Npgsql richiede (10.0.4), anche
se una versione più recente di `Design` esiste su NuGet:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.4">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
```

**`PrivateAssets="all"` + `IncludeAssets`:** `Microsoft.EntityFrameworkCore.Design`
serve **solo agli strumenti da riga di comando** (`dotnet ef migrations add`,
`dotnet ef database update`) mentre lavori su questo progetto — non a runtime,
non a chi lo referenzia. Senza questi due attributi, il pacchetto
"trapelerebbe" per transitività a `GoCare.Api` (che referenzia
`GoCare.Application`), portandosi dietro un pacchetto inutile lì.
- `PrivateAssets="all"` — "questo pacchetto non deve propagarsi a chi mi
  referenzia" (resta privato di `GoCare.Application`).
- `IncludeAssets="…"` — cosa effettivamente serve *qui*: file di runtime,
  strumenti di build, analyzer. È il pattern standard consigliato da
  Microsoft per pacchetti *design-time-only*.

---

## 3. `BusinessDbContext`

Un `DbContext` è il punto d'accesso di EF Core a un database: tiene traccia
delle entità caricate, traduce query LINQ in SQL, e raggruppa le modifiche in
una singola transazione con `SaveChangesAsync()`.

```csharp
// src/GoCare.Application/Data/BusinessDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace GoCare.Application.Data;

public sealed class BusinessDbContext(DbContextOptions<BusinessDbContext> options)
    : DbContext(options)
{
}
```

- **Costruttore primario che inoltra alla base:** `DbContextOptions<BusinessDbContext>`
  è **generico sul tipo del context stesso** — così, quando nella DI ci sono
  *due* `DbContext` (`AuthDbContext` e `BusinessDbContext`), ognuno riceve
  **le proprie** opzioni (connection string, provider) e non quelle dell'altro.
  Passare `options` al costruttore di `DbContext` è obbligatorio: è lì che EF
  Core legge come collegarsi al database.
- **Vuoto per ora:** nessun `DbSet<T>` — non esiste ancora nessuna entità di
  dominio (`Person`, `TransportRequest`, ...). Un `DbSet<Person>` si aggiunge
  quando l'entità `Person` viene scritta: rappresenta "la tabella `Person`
  vista come collezione di oggetti C#".
- **`sealed`:** stesso motivo di `SystemClock` in `GoCare.Shared` — non è
  pensato per essere derivato.

---

## 4. `AddApplication()` — registrare il DbContext nella DI

Stesso schema di `AddSharedKernel()` in `GoCare.Shared`: un metodo di
estensione che raggruppa tutte le registrazioni DI di `GoCare.Application`, così
`Program.cs` non deve conoscerle una per una.

```csharp
// src/GoCare.Application/DependencyInjection.cs
using GoCare.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCare.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BusinessDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("BusinessDb")));

        return services;
    }
}
```

- **`IConfiguration configuration` come parametro, non letta da un singolo
  globale:** il metodo riceve la configurazione dall'esterno invece di
  chiamare direttamente qualcosa come `Configuration.Current`. È lo stesso
  principio della DI applicato al **metodo di registrazione**: chi chiama
  decide quale configurazione passare (comodo nei test, dove si può passare
  una configurazione finta).
- **`AddDbContext<T>`** — metodo di EF Core che registra `T` nella DI con
  lifetime **`Scoped`** (una nuova istanza per ogni richiesta HTTP,
  automaticamente smaltita a fine richiesta). Ha senso: un `DbContext`
  accumula stato (le entità che tracka) e **non è thread-safe** — condiviso
  fra richieste diverse romperebbe tutto.
- **`options.UseNpgsql(connectionString)`** — dice a EF Core "usa il
  provider Postgres, collegati con questa stringa". Ogni provider
  (`UseSqlServer`, `UseSqlite`, `UseNpgsql`...) ha il suo metodo `Use…`.
- **`configuration.GetConnectionString("BusinessDb")`** — metodo di comodo
  equivalente a `configuration["ConnectionStrings:BusinessDb"]`: legge la
  chiave `BusinessDb` dentro la sezione `ConnectionStrings` di
  `appsettings.json`.
- **Area Auth, non ancora scritta:** stessa forma, un secondo
  `AddDbContext<AuthDbContext>` con `GetConnectionString("AuthDb")` — a cura
  di chi scrive `AuthDbContext`.

### Uso in `Program.cs`

```csharp
builder.Services.AddApplication(builder.Configuration);
```

`builder.Configuration` è l'`IConfiguration` già assemblato dal host
(legge `appsettings.json`, `appsettings.{Environment}.json`, variabili
d'ambiente, user-secrets, in quest'ordine di precedenza) — lo si passa così
com'è al metodo di estensione.

### Build verde non basta: serve una verifica a runtime

`dotnet build` controlla solo che il codice **compili**: `AddDbContext` è
una chiamata sintatticamente valida anche se `Program.cs` la scrivesse a
vuoto o senza argomento. Un errore di **cablaggio DI** (dimenticare di
chiamare `AddApplication`, o chiamarla senza la configuration) si nota solo
quando qualcosa **chiede davvero** `BusinessDbContext` al contenitore — cioè
a runtime, alla prima richiesta che lo usa (o esplicitamente con un piccolo
smoke test: `dotnet run` + una chiamata che forza la risoluzione, guardando i
log per assicurarsi che l'app parta senza eccezioni di DI).

---

## 5. Modellazione degli enum di dominio

Prima di scrivere le entità, si sono fissati gli enum che ne useranno i
campi di stato. Ogni scelta è stata verificata contro
`Project_GoCare_Revisione_v2.pdf` e `PIANO_BACKEND_GoCare.md`, non inventata
a tavolino — dove il documento non elencava un valore esplicito, è segnalato
sotto.

Cartella: `GoCare.Application/Models/Enums/`. Convenzione di nome: prefisso
`E` + stile `Trip*` (non `Transport*`) per gli enum legati al viaggio.

### 5.1 `ETripType` — tipo di viaggio

```csharp
public enum ETripType { Visit, Hospitalization, Discharge, Transfer }
```

`Transfer` (Trasferimento) è **in-scope v0**: verificato in §5 del PDF (tabella
dei tipi di trasporto), dove solo *Trasporto sociale* è segnato fuori
scope (PA-13). Ogni valore corrisponde a un flusso leggermente diverso a
livello di UC (1.5, 4.1), ma tutti condividono lo stesso modello dati
`TransportRequest` — il tipo è solo un campo, non entità separate.

### 5.2 `ETripDirection` — andata/ritorno

```csharp
public enum ETripDirection { OnlyGo, OnlyReturn, RoundTrip }
```

Serve perché un trasporto può richiedere solo l'andata, solo il ritorno,
oppure entrambe le tratte in un'unica richiesta.

### 5.3 `ETripRequestStatus` — stato aggregato della richiesta

```csharp
public enum ETripRequestStatus
{
    Pending, Confirmed, InProgress, Completed, NotCovered, Cancelled
}
```

**Perché 6 valori e non 7:** il piano originale prevedeva anche `Rifiutata`
(Refused). Si è deciso di **rimuoverla**: il rifiuto di una singola
associazione (UC 6, `Decline`) non cambia lo stato aggregato della richiesta
— resta registrato su `TransportRequestRejection`, un'entità separata che
tiene traccia di *chi* ha rifiutato. L'unico esito negativo aggregato è
`NotCovered`, calcolato da `ICoverageEvaluator` quando **nessuna** delle
associazioni destinatarie ha accettato (o il timeout è scaduto senza
risposte).

**Perché `InProgress` è un campo separato, non derivato:** sembra ridondante
con lo stato di avanzamento più fine (`ETripTransitionStatus`, sotto), ma è
un campo **denormalizzato apposta**: query come "dammi tutte le richieste in
corso" (per una dashboard, un filtro lista) sono molto più semplici su un
singolo campo indicizzabile che su un `join`/calcolo sulla cronologia
transizioni. Il prezzo di questa scelta: va **tenuto sincronizzato a mano**
da chi gestisce le transizioni (`TripStatusService`), altrimenti i due
campi divergono.

### 5.4 `ETripTransitionStatus` — stato fine di avanzamento

```csharp
public enum ETripTransitionStatus
{
    Pending, InCharge, Arriving, OnSite, Returning, Completed
}
```

Mappa la macchina a stati di §9 del piano (`TripStateMachine`):
`NonPresoInCarico → PresoInCarico → InArrivo → InVisita → InRitorno →
Concluso`. Tenuto **deliberatamente separato e più fine** di
`ETripRequestStatus.InProgress` (che li riassume tutti in un unico valore
"in corso").

**Perché non si chiama `TripState`:** discusso esplicitamente — "stato" da
solo è troppo generico (si confonderebbe con `ETripRequestStatus`, che è
anch'esso uno "stato" del viaggio, ma sull'esito della richiesta, non
sull'avanzamento fisico). `TransitionStatus` nomina la cosa con precisione:
è lo stato delle **transizioni** fisiche del trasporto (dove si trova
l'accompagnatore adesso).

### 5.5 `EModificationRequestStatus` — richieste di modifica

```csharp
public enum EModificationRequestStatus { PendingApproval, Approved, Rejected }
```

**Perché resta un enum separato**, su un'entità separata
(`TransportModificationRequest`), e **non si accorpa** con
`ETripRequestStatus`: sono due assi indipendenti. Una richiesta `Confirmed`
può avere in contemporanea una modifica `PendingApproval` — se si usasse un
solo enum condiviso, si perderebbe lo stato originale della richiesta nel
momento in cui arriva una modifica.

### 5.6 `EMembershipRole` — ruolo nel gruppo cura

```csharp
public enum EMembershipRole { Caregiver, Assisted }
```

**Non include `Associazione`:** motivo strutturale, non di modellazione
libera — `CareGroupMembership.PersonId` è una FK che punta **solo** a
`Person`. `Association` è un attore completamente diverso (confermato in
§4 del PDF, tabella attori) e non fa mai parte di un `CareGroupMembership`.

### 5.7 `EGroupAdminRole` — ruolo amministrativo nel gruppo

```csharp
public enum EGroupAdminRole { Admin, Member }
```

Per `CareGroupMembership.RuoloAmministrativo`. **Gap del piano originale**:
non era esplicitamente nell'elenco enum del piano — individuato e aggiunto
in questa sessione, poi riportato in `PIANO_BACKEND_GoCare.md`.

### 5.8 `EInvitationGroupStatus` — invito al gruppo cura

```csharp
public enum EInvitationGroupStatus { Pending, Accepted, Refused }
```

**3 valori invece dei 2 originali** (`InAttesa`/`Accettato`): un invito può
anche essere **rifiutato** dall'invitato — un caso che il piano non
elencava esplicitamente ma che è evidentemente necessario (altrimenti un
invito rifiutato resterebbe per sempre `Pending`). Decisione confermata
dopo essere stata sottoposta due volte all'utente.

### 5.9 `ENotificationType` — 10 valori

```
NewRequest, RequestAccepted, RequestNotCovered, TripStatusChanged,
ModificationRequested, ModificationApproved, ModificationRejected,
TripCancelledByUser, TripCancelledByAssociation, TripReminder
```

**Sintetizzato, non citato:** il piano non ha un elenco unico e autoritativo
per questo enum — i valori sono stati ricavati incrociando le menzioni
sparse nei vari UC 4.x (le notifiche che il sistema deve generare). Non è
una lista "presa" dal documento, è una derivazione — segnalato esplicitamente
come tale quando proposta.

### 5.10 `ENotificationChannel` — `[Flags]`

```csharp
[Flags]
public enum ENotificationChannel
{
    None = 0,
    Push = 1,
    Email = 2
}
```

Vedi §6 sotto per la teoria di `[Flags]` e il perché di questa scelta
specifica.

### 5.11 `ENotificationSubject`

```csharp
public enum ENotificationSubject { Association, Person }
```

Per `Notification.DestinatarioType`: una notifica può essere destinata a
una persona o a un'associazione, e serve sapere quale per interpretare
correttamente l'`Id` del destinatario (che altrimenti sarebbe ambiguo fra
le due tabelle).

### 5.12 `EAccreditationStatus` — accreditamento associazione

```csharp
public enum EAccreditationStatus { Pending, Accredited, Rejected }
```

Per `Association.StatoAccreditamento` (PA-11). Nome nuovo, non era nel piano
originale. **3 valori:** un'associazione appena registrata è `Pending` (non
può ancora operare, GoCare deve verificarla manualmente perché accede a dati
sensibili di persone fragili), diventa `Accredited` dopo la verifica, oppure
`Rejected` se la verifica va male — `Rejected` aggiunto dopo conferma
dell'utente, stesso precedente di `Refused` in `EInvitationGroupStatus`.

**Perché è separato da `Account.Stato`** (lato Auth): l'area dominio non
tocca mai `AuthDbContext`, ma ha bisogno di sapere localmente se
un'associazione è operativa per i controlli di autorizzazione e per i match
`TransportRequestRecipient` (PA-05). I due campi (`Account.Stato` in
`gocare_auth`, `Association.StatoAccreditamento` in `gocare_business`) sono
tenuti in sync da un solo metodo di servizio
(`ProfileProvisioningService.AccreditAssociationAsync` /
`RejectAssociationAsync`), non da un join fra database.

### 5.13 `ERejectionKind` — tipo di rifiuto/disdetta

```csharp
public enum ERejectionKind { Declined, CancelledAfterAcceptance }
```

Per `TransportRequestRejection.Kind`. Discrimina due eventi diversi che
finiscono nella stessa tabella:
- `Declined` — un'associazione candidata rifiuta una richiesta ancora
  `InAttesa` (UC 6 `Decline`). `Causale` di norma `null`: la riga serve solo
  a `ICoverageEvaluator` per contare i rifiuti (tutte rifiutano → `NonCoperta`).
- `CancelledAfterAcceptance` — un'associazione che aveva **già accettato** si
  tira indietro (UC 3 `CancelByAssociation`). `Causale` obbligatoria (imposta
  dal Service): l'utente va rinotificato ed è un tema di qualità del servizio.

Un rifiuto "secco" non ha bisogno di una causale — obbligare a scriverne una
sarebbe attrito senza nessuno che legge quel dato. La causale serve solo dove
qualcuno si sfila da un impegno preso.

### 5.14 `EModificationField` — campo di una richiesta di modifica

```csharp
public enum EModificationField { Schedule, Destination }
```

Per `TransportModificationRequest.Field`. **Niente `Companions`**: gli
accompagnatori si modificano sempre in modo diretto (anche a viaggio
`Confermata`), con notifica informativa ma senza approvazione — non toccano
percorso/orario, quindi non c'è niente da far approvare all'associazione.
Il piano elencava `Accompagnatori` fra i valori possibili (riga entità) ma
contraddiceva il flusso UC 2: risolto togliendolo.

### 5.15 Ancora da scrivere

**Area Auth** (`AccountStatus`, `AccountRole`, ecc.) — non ancora iniziata,
a cura del collega.

---

## 6. `[Flags]`: enum come insieme di opzioni combinabili

### Il problema che risolve

Un enum "normale" rappresenta **un valore alla volta** — `ETripType.Visit`
*oppure* `ETripType.Hospitalization`, mai entrambi insieme. Ma un evento di
notifica può dover raggiungere l'utente **su più canali insieme** (push *e*
email per lo stesso evento). Un enum normale non lo permette.

### Come funziona (bit a bit)

Un enum è, sotto, un intero. Se i valori sono scelti come **potenze di 2**
(1, 2, 4, 8, 16, ...), ognuno occupa **un bit diverso**:

```
Push  = 1  = 0000 0001
Email = 2  = 0000 0010
       (il prossimo sarebbe 4 = 0000 0100, poi 8, ecc.)
```

Combinarli con l'operatore **OR bit a bit** (`|`) accende entrambi i bit
senza che si sovrappongano:

```
Push | Email = 0000 0001 | 0000 0010 = 0000 0011   (= 3)
```

Se invece i valori fossero `1, 2, 3` (sequenziali, non potenze di 2), `1 | 2`
darebbe comunque `3` — ma `3` sarebbe *già* un valore a sé (`Email` per
qualcun altro?), quindi non si distinguerebbe più "Push+Email combinati" da
"il valore 3 preso singolarmente". Le potenze di 2 garantiscono che ogni
combinazione produca un intero **unico**, decifrabile.

Per **controllare** se un bit è acceso si usa l'AND bit a bit (`&`) o il
metodo di comodo `HasFlag`:

```csharp
ENotificationChannel canali = ENotificationChannel.Push | ENotificationChannel.Email;

canali.HasFlag(ENotificationChannel.Push);   // true
canali.HasFlag(ENotificationChannel.Email);  // true
canali.HasFlag(ENotificationChannel.None);   // true sempre (None = 0, "sottoinsieme" di tutto)
```

### Cosa fa *davvero* l'attributo `[Flags]`

Punto sottile: la meccanica bit a bit (`|`, `&`, `HasFlag`) **funziona su
qualsiasi enum con base intera**, con o senza `[Flags]` — è aritmetica, non
magia del compilatore. `[Flags]` da solo cambia **una cosa sola**: come
`ToString()` formatta il valore.

```
Senza [Flags]:  (ENotificationChannel)3 → "3"              (numero grezzo, illeggibile)
Con [Flags]:    (ENotificationChannel)3 → "Push, Email"     (elenca i flag accesi)
```

Il resto — richiedere valori potenza di 2, aggiungere `None = 0` — è
**convenzione** che l'attributo segnala come intento, non impone da solo
(il compilatore non impedisce di scrivere valori non potenza di 2 su un
enum `[Flags]`; semplicemente, se lo fai, la combinazione smette di
funzionare in modo prevedibile).

### Perché qui: la riga "morta" nella tabella `Notification`

Discussione chiave: inizialmente si era ipotizzato che l'utente legga la
notifica email **dentro GoCare** — motivazione poi corretta: **non è così**,
l'email la si legge nel proprio client di posta, non nel centro notifiche
dell'app.

La motivazione riformulata, corretta, è più solida: se si generassero **due
righe separate** (`Canale=Push`, `Canale=Email`) per lo stesso evento, la
riga `Canale=Email` non avrebbe **mai** un `LettaAt` sensato — nessuno la
segna "letta" da dentro GoCare, perché nessuno la vede lì. Sarebbe una riga
strutturalmente "morta" in una tabella pensata per `List` (centro
notifiche), `MarkRead`, contatori di non lette: andrebbe sempre filtrata via
per non sporcare quei conteggi.

Con `[Flags]`, un solo evento produce **una sola riga** `Notification` con
`Canali = Push | Email`: la riga rappresenta l'evento, non il canale. Il
job che spedisce le notifiche guarda `HasFlag(Push)` / `HasFlag(Email)` e
decide se mandare una push, un'email, o entrambe — ma `LettaAt` sulla riga
si riferisce sempre e solo all'esperienza in-app (push/centro notifiche),
mai all'email.

---

## 7. Entità di dominio (`Models/Domain/`)

### 7.1 Entità vs value object

`Person` è un'**entità**: ha un'identità propria (`Id`), esiste come riga a sé
nella tabella — due `Person` con dati identici ma `Id` diversi restano due
persone diverse.

Un indirizzo (`Address`) è concettualmente diverso: **non ha identità
propria**. Due indirizzi con gli stessi Via/Numero/CAP/Città/Provincia *sono*
lo stesso indirizzo. In DDD questo si chiama **value object**: un tipo
definito dal *valore* dei suoi campi, non da un id. Conseguenza pratica: è
naturale renderlo **immutabile** — per "cambiare indirizzo" si sostituisce
l'intero oggetto, non si modificano i singoli campi uno per uno.

In EF Core il pattern corrispondente è l'**owned type** (`OwnsOne`):
`Address` non ha una tabella propria né una sua chiave primaria/FK. Le sue
proprietà diventano colonne aggiuntive nella tabella di chi lo possiede (es.
`Person` avrà colonne `IndirizzoDomicilio_Via`, `IndirizzoDomicilio_Cap`, …).
Nessun join, nessuna tabella in più — è zucchero sintattico sul modello C#,
non una relazione vera nel DB. Si configura in `BusinessDbContext.OnModelCreating`
quando arriviamo lì.

Torna utile perché più di un'entità avrà bisogno di un indirizzo: `Person`,
`SavedDestination`, `Association.Sede`, e `TransportRequest` che ne avrà
**due** sulla stessa riga (`IndirizzoPartenza`, `IndirizzoDestinazione`) — con
un tipo riusabile bastano due proprietà dello stesso tipo, niente da
duplicare.

### 7.2 `Address` — perché `record` e non `class`

`Person` è `sealed class` con `private set` perché è un'entità **mutabile nel
tempo**. `Address` è un valore immutabile — stesso caso di `PagedResult<T>` in
`GoCare.Shared`: un **record posizionale** dà gratis immutabilità e
**uguaglianza per valore** (due `Address` coi campi identici risultano
`Equal`).

```csharp
// src/GoCare.Application/Models/Domain/Address.cs
public sealed record Address(
    string Street,
    string Number,
    string Cap,
    string City,
    string Province,
    string Region);
```

- **`Number` è `string`, non `int`**: un numero civico può essere `"12/A"`,
  `"12 bis"`, `"SNC"` — non è un dato aritmetico.
- **`Cap` è `string`, non `int`**: un CAP come `"00100"` (Roma) perderebbe lo
  zero iniziale se fosse un numero.
- Nessun campo nullable: se esiste un `Address`, è completo. L'opzionalità (una
  persona senza indirizzo ancora) si gestisce rendendo nullable il
  **riferimento** (`Address?` su `Person`), non i campi interni.
- **`Region` denormalizzata accanto a `Province`** (PA-05): `Region` è
  ricavabile dalla provincia, ma tenerla come campo separato evita di
  mantenere una mappa statica provincia→regione e permette a un'associazione
  di filtrare le richieste per regione con una query diretta. Scelta fatta
  per il v0: meno codice, meno rischio di bug in una mappa. Vedi §7.5.

### 7.3 `Guid` come id, ovunque

Tutte le entità usano `Guid` come chiave, non `int` auto-increment. Non è
solo stile:

- **Requisito dell'architettura a due database.** `PersonId` viene generato
  in `AuthService.RegisterUserAsync` (area Auth), salvato su `Account` nel DB
  `gocare_auth`, e passato a `ProfileProvisioningService.CreateForAccountAsync`
  che crea `Person` nel DB `gocare_business` **con lo stesso id**. Con un
  `int` assegnato dal database questo è impossibile: i due DB assegnerebbero
  interi indipendenti che non combaciano. Un `Guid` lo generi in memoria
  (`Guid.NewGuid()`) *prima* di salvare, quindi può attraversare il confine.
- **Coerenza del pattern di costruzione.** Tutte le classi hanno `Id` come
  parametro obbligatorio del costruttore, `{ get; }` senza setter — l'oggetto
  nasce già con la sua identità. Un `int` dal DB imporrebbe un ciclo di vita
  a due fasi (costruisci senza id → salva → EF assegna l'id), diverso da
  tutto il resto.
- **Sicurezza.** Un id esposto in una route (`GET /transports/:id`) come
  `Guid` non è enumerabile/indovinabile. Rilevante qui: PA-06 riguarda la
  protezione dei dati di contatto di persone fragili.

Costo accettato: 16 byte per riga invece di 4-8, indici meno compatti —
trascurabile per i volumi di un v0.

### 7.4 Chiave composta vs `Id` surrogato

Non tutte le entità hanno un `Id` proprio. `CareGroupMembership` e
`TransportRequestRecipient` usano una **chiave primaria composta** — la
coppia di FK che le identifica naturalmente:

| Entità | Chiave |
|---|---|
| `CareGroupMembership` | `(CareGroupId, PersonId)` |
| `TransportRequestRecipient` | `(TransportRequestId, AssociationId)` |

Criterio: se **nessuna route e nessun'altra tabella** indirizza mai una riga
di quella entità col suo id singolo — ci si arriva sempre tramite la coppia
(o tramite un token, es. `InvitoToken`) — allora l'`Id` surrogato è peso
morto. La chiave composta dà anche gratis l'invariante "non due righe uguali
per la stessa coppia" (una persona non è membro due volte dello stesso
gruppo), che con un `Id` surrogato richiederebbe comunque un indice unique
a parte.

`TransportRequestRejection` e `Accompagnatore` invece **mantengono un `Id`
proprio**: sono referenziate altrove / hanno senso come entità singole.

La chiave composta si configura in `BusinessDbContext.OnModelCreating`:
`modelBuilder.Entity<T>().HasKey(x => new { x.A, x.B });`.

### 7.5 Snapshot: dati congelati alla creazione

Alcuni campi sono **copie per valore** prese al momento della creazione,
non riferimenti a dati che possono cambiare dopo.

- **`TransportRequest.IndirizzoPartenza` / `IndirizzoDestinazione` /
  contatti di riferimento**: sono `Address` / stringhe copiate dentro la
  riga, non FK a `SavedDestination` o a `Person`. Se la persona poi modifica
  o cancella quella destinazione salvata, o cambia telefono, il trasporto
  già registrato conserva i dati validi *quando è stato creato* — un record
  operativo non si riscrive a posteriori.
- **`TransportRequestRecipient`**: è il risultato **già calcolato** del match
  geografico PA-05. Alla creazione della richiesta, per ogni associazione
  accreditata la cui `CoveredProvinces` contiene la provincia di partenza,
  si scrive una riga `(TransportRequestId, AssociationId)`. Dopo, "questa
  associazione vede questa richiesta?" è una lookup su indice, senza
  rieseguire nessuna logica geografica a ogni caricamento della dashboard.

Perché congelare la lista dei destinatari è un **pro**, non un effetto
collaterale:
- niente "rug-pull" su una richiesta in corso — chi è stato interpellato
  resta interpellato finché la richiesta non è chiusa;
- denominatore stabile per PA-04 ("non coperta" = "N interpellate, M hanno
  rifiutato, timeout scaduto"): se l'insieme cambiasse in corsa, il conteggio
  non vorrebbe più dire niente;
- tracciabilità: si può ricostruire con certezza *quali* associazioni erano
  state interpellate per un dato trasporto, e resta allineato a chi ha
  ricevuto la notifica.

Costo: un'associazione che aggiunge quella provincia *dopo* non vedrà la
richiesta. Finestra minima (le richieste durano poco) → per il v0 non si
ricalcola.

### 7.6 Timestamp e `IClock`: l'entità non legge l'ora

Campi come `CareGroupMembership.CreatedAt`, `TransportRequest.CreatedAt` sono
parametri **obbligatori del costruttore**, passati dal Service. L'entità
**non chiama `DateTimeOffset.UtcNow` da sola**: è il Service (che ha `IClock`
iniettato) a leggere l'ora e passarla.

```csharp
// nel Service, non nell'entità
var now = _clock.UtcNow;
var membership = new CareGroupMembership(groupId, personId, adminRole, role, status, now);
```

Motivo: `IClock` esiste per rendere il tempo **testabile** (un `FakeClock`
con ora fissa nei test). Se l'ora la leggesse l'entità, ogni test avrebbe
timestamp diversi e non verificabili, e ci sarebbero due modi di ottenere
"adesso" nel progetto invece di uno. L'entità resta "dumb": riceve il tempo,
non lo calcola.

### 7.7 `DateOnly` vs `DateTimeOffset`

- **`DateOnly`** — solo la data di calendario, nessuna ora, nessun fuso.
  Per `Person.DataNascita`: una data di nascita non ha un "istante", è
  la stessa in ogni fuso.
- **`DateTimeOffset`** — istante univoco + offset UTC. Per tutto il resto:
  `TransportRequest.DataOraAndata`/`DataOraRitorno` (un trasporto si prenota
  per un'ora precisa), tutti i timestamp. Convenzione: `DateTimeOffset.UtcNow`
  internamente (mai `.Now`), si localizza solo lato client.

Errore tipico intercettato in revisione: `DataOraAndata` scritta come
`DateOnly` — il nome contiene "Ora" ma il tipo non porta nessuna
informazione oraria.

### 7.8 `row_version`: concorrenza ottimistica, non è una proprietà

Nel piano `TransportRequest` ha `row_version`. **Non** va scritto come
proprietà C# nella classe. È una colonna gestita dal database che cambia a
ogni update: quando salvi, EF Core genera `UPDATE ... WHERE Id = @id AND
row_version = @valoreLetto`. Se qualcun altro ha modificato la riga nel
frattempo, l'update non trova nulla e EF lancia
`DbUpdateConcurrencyException` — invece di sovrascrivere silenziosamente.

Serve su `TransportRequest` perché è l'entità con più scritture concorrenti
(due associazioni che accettano lo stesso trasporto nello stesso istante —
"prima accettazione vince"; il `CoverageTimeoutJob` che marca `NonCoperta`
mentre una accetta). Su Postgres si mappa la colonna di sistema `xmin` come
concurrency token in `OnModelCreating` (`.UseXminAsConcurrencyToken()`) — pura
configurazione EF, non tocca `TransportRequest.cs`.

### 7.9 `Person` senza flag di ruolo

`Person` **non ha** `IsCaregiver`/`IsAssisted`. Il ruolo caregiver/assistito
non è un'identità globale della persona: esiste solo come
`CareGroupMembership.RuoloNelGruppo`, deciso per singolo gruppo di cura. Una
persona che si gestisce da sola l'app senza gruppi non ha nessun ruolo, e va
bene — richiede un trasporto per sé come richiedente-e-beneficiario
coincidenti (nessun controllo di gruppo). Decisione ribaltata rispetto a una
precedente, dettagli in `MEMORY.md`.

### 7.10 Avanzamento

| Entità | Stato |
|---|---|
| `Address` | scritta — `record`, campi in inglese + `Region` (§7.2) |
| `Person` | scritta — `sealed class`, costruttore `Id`/`Name`/`Surname`/`BirthDate`/`Email`/`Phone`; `Address?`, `DeletedAt?`, `AnonymizedAt?` fuori dal costruttore |
| `Association` | scritta — `Sede` (`Address`), `CoveredProvinces` (`List<string>`, PA-05), `Status` (`EAccreditationStatus`), `DeletedAt?` |
| `CareGroup` | scritta — costruttore `Id`/`Name`/`CreatedByPersonId` (quest'ultimo obbligatorio: un gruppo non esiste senza creatore) |
| `CareGroupMembership` | scritta — chiave composta `(CareGroupId, PersonId)`, `Role`/`AdminRole`/`Status`, `InvitationEmail?`/`InvitationToken?`, `CreatedAt` |
| `SavedDestinations` | scritta |
| `TransportRequest` | scritta — snapshot indirizzi/contatti, `RequestStatus` default `Pending` interno, `AssignedAssociationId?`/`AcceptedAt?` post-accettazione, `NotCoveredAt?`, `DeletedAt?`/`DeletedBy?`; manca `KmPrevisti`, timestamp di creazione |
| `TransportRequestRecipient` | scritta — chiave composta `(TransportRequestId, AssociationId)`, due sole colonne |
| `TransportRequestRejection` | scritta — `Kind` (`ERejectionKind`) + `Causale?`, `At`; immutabile (riga di log), `Id` proprio (coppia non unica) |
| `Accompagnatore` | scritta (come `Helper`, da rinominare) — immutabile, gestione replace-all |
| `TransportModificationRequest` | scritta — `Field` (`EModificationField`), `PreviousValue`/`ProposedValue` serializzati, `Status` default `PendingApproval` interno; mutabile via `Approve()`/`Reject()` (da scrivere) |
| `TripStatusTransition` | scritta (come `TripStatusTransiction`, typo da correggere) — log append-only immutabile, una riga per cambio di stato, `Timestamp` via `IClock`, `OperatorLabel` testo libero (PA-03) |
| `Notification` | scritta — solo `ReadAt` mutabile (`MarkRead`, prima lettura vince); `RecipientId`/`RelatedEntityId` `Guid` polimorfici, non FK |
| `ContactAccessLog`, `DeviceToken` | non ancora iniziate (elenco completo in `PIANO_BACKEND_GoCare.md`) |

---

## 8. Stato di `GoCare.Application` (Fase 2, lato dominio)

**Fatto:**
- PostgreSQL 17 installato in locale, database `gocare_auth`/`gocare_business`
  creati.
- `GoCare.Application.csproj`: `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 +
  `Microsoft.EntityFrameworkCore.Design` 10.0.4 (pinnato, `PrivateAssets="all"`).
- `Data/BusinessDbContext.cs` — ancora senza `DbSet` (le entità scritte non
  sono ancora state mappate nel context né configurate in `OnModelCreating`).
- `DependencyInjection.cs` — `AddApplication(IServiceCollection, IConfiguration)`,
  registra `BusinessDbContext` via `AddDbContext` + `UseNpgsql`.
- `GoCare.Api/Program.cs` — chiama `AddApplication(builder.Configuration)`;
  verificato sia con `dotnet build` (0/0) sia a runtime (`dotnet run`, nessun
  errore di risoluzione DI).
- **Tutti** gli enum di dominio in `Models/Enums/`, inclusi
  `EAccreditationStatus`, `ERejectionKind`, `EModificationField` (§5).
- Entità in `Models/Domain/` — scritte: `Address`, `Person`, `Association`,
  `CareGroup`, `CareGroupMembership`, `SavedDestinations`, `TransportRequest`,
  `TransportRequestRecipient`, `TransportRequestRejection`, `Accompagnatore`
  (come `Helper`), `TransportModificationRequest`, `TripStatusTransition` (come
  `TripStatusTransiction`), `Notification` (vedi §7.10 per lo stato di
  dettaglio e i punti ancora aperti su ciascuna).

**Da fare:**
- `AuthDbContext` — a cura del collega, non ancora scritto.
- Entità di dominio rimanenti: `ContactAccessLog`, `DeviceToken`.
- Rifiniture sulle entità scritte (vedi §7.10): rinomina `Helper` →
  `Companion`/`Accompagnatore` e `TripStatusTransiction` → `TripStatusTransition`;
  `TransportRequest` senza `KmPrevisti` e senza timestamp di creazione; metodi
  `Approve()`/`Reject()` su `TransportModificationRequest`.
- `DbSet<T>` nel `BusinessDbContext` + `OnModelCreating` (owned type per
  `Address`, chiavi composte per `CareGroupMembership` /
  `TransportRequestRecipient`, `xmin` come concurrency token su
  `TransportRequest`).
- Prima migrazione EF Core di `BusinessDbContext` (bloccata finché non c'è
  almeno un `DbSet`).
