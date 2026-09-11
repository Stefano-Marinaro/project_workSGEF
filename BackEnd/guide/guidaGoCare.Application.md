# Guida — `GoCare.Application`

Teoria raccolta mentre si costruisce `GoCare.Application` (area Auth + area
dominio nello stesso progetto, separate per convenzione di cartelle). Copre la
**Fase 2 (Persistenza)** lato dominio: PostgreSQL, `BusinessDbContext`, DI,
enum di dominio (§5), entità di dominio (§7).

Complementare a `guidaGoCare.Shared.md` e `guidaGoCare.Api.md`.

---

## 1. PostgreSQL: due database, due DbContext

Ogni sviluppatore installa PostgreSQL **in locale** e crea entrambi i database
(`gocare_auth`, `gocare_business`), anche se lavora su una sola area. Non si
condivide un server Postgres di team.

- Si condivide via git il **codice** (i due `DbContext`, le migrazioni EF
  Core), non i dati né il server.
- Ognuno applica le migrazioni al proprio Postgres (`dotnet ef database
  update`) e lavora isolato.
- Le password vere non vanno in `appsettings.json` (finisce in git): stanno in
  `appsettings.Development.json` (ignorato) o `dotnet user-secrets`.

Due database perché i due `DbContext` restano separati anche in un progetto
solo: `AuthDbContext` → `gocare_auth`, `BusinessDbContext` → `gocare_business`,
migrazioni indipendenti.

---

## 2. Conflitto di versione NuGet (`MSB3277`)

Aggiungendo insieme `Npgsql.EntityFrameworkCore.PostgreSQL` e
`Microsoft.EntityFrameworkCore.Design`, i due pacchetti richiedono versioni
transitive diverse di `Microsoft.EntityFrameworkCore`:

```
Npgsql.EntityFrameworkCore.PostgreSQL  10.0.3  →  EF Core 10.0.4
Microsoft.EntityFrameworkCore.Design   10.0.11 →  EF Core 10.0.11
```

MSBuild non sceglie da solo: `MSB3277` avvisa che la versione scelta potrebbe
non essere quella giusta.

**Regola:** il provider database (Npgsql) è il vincolo più stretto — sa con
quale EF Core esatto è stato testato. Si pinna `Design` alla stessa versione
(10.0.4), anche se su NuGet ne esiste una più recente:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.4">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
```

`Microsoft.EntityFrameworkCore.Design` serve **solo agli strumenti da riga di
comando** (`dotnet ef …`), non a runtime né a chi referenzia il progetto:

- `PrivateAssets="all"` — non si propaga a chi referenzia `GoCare.Application`
  (altrimenti "trapelerebbe" a `GoCare.Api`).
- `IncludeAssets="…"` — cosa serve *qui*: runtime, build, analyzer. È il
  pattern Microsoft per pacchetti *design-time-only*.

---

## 3. `BusinessDbContext`

Un `DbContext` è il punto d'accesso di EF Core a un database: traccia le
entità caricate, traduce LINQ in SQL, raggruppa le modifiche in una
transazione con `SaveChangesAsync()`.

```csharp
// src/GoCare.Application/Data/BusinessDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace GoCare.Application.Data;

public sealed class BusinessDbContext(DbContextOptions<BusinessDbContext> options)
    : DbContext(options)
{
}
```

- **`DbContextOptions<BusinessDbContext>` generico sul tipo del context:** con
  due `DbContext` nella DI, ognuno riceve **le proprie** opzioni (connection
  string, provider). Passare `options` alla base è obbligatorio — è lì che EF
  legge come collegarsi.
- **Vuoto per ora:** nessun `DbSet<T>` finché non esistono le entità. Un
  `DbSet<Person>` = "la tabella `Person` vista come collezione di oggetti C#".
- **`sealed`:** non è pensato per essere derivato.

---

## 4. `AddApplication()` — registrare il DbContext nella DI

Stesso schema di `AddSharedKernel()`: un metodo di estensione che raggruppa le
registrazioni DI di `GoCare.Application`.

```csharp
// src/GoCare.Application/DependencyInjection.cs
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

- **`IConfiguration` come parametro:** chi chiama decide quale configurazione
  passare (comodo nei test con una configurazione finta).
- **`AddDbContext<T>`:** registra `T` con lifetime **`Scoped`** (una istanza
  per richiesta HTTP). Un `DbContext` accumula stato e non è thread-safe:
  condividerlo fra richieste romperebbe tutto.
- **`options.UseNpgsql(…)`:** provider Postgres + connection string. Ogni
  provider ha il suo `Use…`.
- **`GetConnectionString("BusinessDb")`:** comodo per
  `configuration["ConnectionStrings:BusinessDb"]`.
- **Area Auth:** stessa forma, un secondo `AddDbContext<AuthDbContext>` con
  `GetConnectionString("AuthDb")`.

Uso in `Program.cs`:

```csharp
builder.Services.AddApplication(builder.Configuration);
```

**Build verde non basta.** `dotnet build` verifica solo che compili. Un errore
di cablaggio DI (dimenticare `AddApplication`, o passarla senza configuration)
si manifesta solo a runtime, alla prima richiesta che usa `BusinessDbContext`
— o con uno smoke test (`dotnet run` + una chiamata che forza la risoluzione,
controllando i log all'avvio).

---

## 5. Enum di dominio

Cartella `Models/Enums/`. Nome: prefisso `E` + stile `Trip*` (non
`Transport*`) per gli enum legati al viaggio. I valori sono verificati contro
`Project_GoCare_Revisione_v2.pdf` e `PIANO_BACKEND_GoCare.md`.

### 5.1 `ETripType`

```csharp
public enum ETripType { Visit, Hospitalization, Discharge, Transfer }
```

`Transfer` è in-scope v0 (§5 del PDF; solo *Trasporto sociale* è fuori scope,
PA-13). Ogni valore è un flusso UC leggermente diverso ma condivide lo stesso
`TransportRequest`: il tipo è un campo, non entità separate.

### 5.2 `ETripDirection`

```csharp
public enum ETripDirection { OnlyGo, OnlyReturn, RoundTrip }
```

Un trasporto può richiedere solo andata, solo ritorno, o entrambe le tratte in
un'unica richiesta.

### 5.3 `ETripRequestStatus` — stato aggregato della richiesta

```csharp
public enum ETripRequestStatus
{
    Pending, Confirmed, InProgress, Completed, NotCovered, Cancelled
}
```

**6 valori, non 7:** niente `Refused`. Il rifiuto di una singola associazione
(UC 6 `Decline`) non cambia lo stato aggregato — resta su
`TransportRequestRejection`, che traccia *chi* ha rifiutato. L'unico esito
negativo aggregato è `NotCovered`, calcolato da `ICoverageEvaluator` quando
nessuna associazione ha accettato (o timeout scaduto).

**`InProgress` denormalizzato apposta:** ridondante con
`ETripTransitionStatus` (sotto), ma "tutte le richieste in corso" è una query
molto più semplice su un campo indicizzato che su un calcolo sulla cronologia
transizioni. Prezzo: `TripStatusService` lo tiene sincronizzato a mano.

### 5.4 `ETripTransitionStatus` — avanzamento fine

```csharp
public enum ETripTransitionStatus
{
    Pending, InCharge, Arriving, OnSite, Returning, Completed
}
```

Mappa la macchina a stati di §9 del piano (`NonPresoInCarico → PresoInCarico →
InArrivo → InVisita → InRitorno → Concluso`). Separato e più fine di
`ETripRequestStatus.InProgress`, che li riassume tutti.

Non `TripState`: "stato" da solo si confonde con `ETripRequestStatus`.
`TransitionStatus` = stato delle transizioni fisiche (dov'è l'accompagnatore
adesso).

### 5.5 `EModificationRequestStatus`

```csharp
public enum EModificationRequestStatus
{
    PendingApproval, Approved, Rejected, Withdrawn
}
```

Enum separato su entità separata (`TransportModificationRequest`), **non**
accorpato con `ETripRequestStatus`: assi indipendenti. Una richiesta
`Confirmed` può avere in parallelo una modifica `PendingApproval`; con un enum
solo si perderebbe lo stato originale. `Withdrawn` = il richiedente ritira la
modifica prima che sia approvata o rifiutata (A6).

### 5.6 `EGroupRole` — ruolo nel gruppo cura

```csharp
public enum EGroupRole { Caregiver, Assisted }
```

Niente `Associazione`: `CareGroupMembership.PersonId` è una FK verso **solo**
`Person`. `Association` è un attore diverso, mai parte di un
`CareGroupMembership`.

### 5.7 `EAdminGroupRole`

```csharp
public enum EAdminGroupRole { Admin, Member }
```

Per `CareGroupMembership.RuoloAmministrativo`. Non era nell'elenco enum del
piano originale — aggiunto e riportato in `PIANO_BACKEND_GoCare.md`.

### 5.8 `EInvitationGroupStatus`

```csharp
public enum EInvitationGroupStatus { Pending, Accepted, Refused }
```

3 valori: un invito può anche essere **rifiutato** dall'invitato (altrimenti
resterebbe per sempre `Pending`).

### 5.9 `ENotificationType`

```
NewRequest, RequestAccepted, RequestNotCovered, TripStatusChanged,
ModificationRequested, ModificationApproved, ModificationRejected,
CompanionUpdated, TripCancelledByUser, TripCancelledByAssociation, TripReminder
```

Il piano non ha un elenco unico: i valori sono ricavati incrociando le
notifiche menzionate nei vari UC 4.x.

### 5.10 `ENotificationChannel` — `[Flags]`

```csharp
[Flags]
public enum ENotificationChannel { None = 0, Push = 1, Email = 2 }
```

Teoria di `[Flags]` in §6.

### 5.11 `ENotificationSubject`

```csharp
public enum ENotificationSubject { Association, Person }
```

Per `Notification.SubjectType` e `DeviceToken.SubjectType`: il destinatario è
una persona o un'associazione, e serve saperlo per interpretare `SubjectId`
(altrimenti ambiguo fra le due tabelle).

### 5.12 `EAccreditationStatus`

```csharp
public enum EAccreditationStatus { Pending, Accredited, Rejected }
```

Per `Association.StatoAccreditamento` (PA-11). Un'associazione appena
registrata è `Pending` (non opera: GoCare la verifica a mano perché accede a
dati sensibili), poi `Accredited` o `Rejected`.

**Separato da `Account.Stato`** (lato Auth): l'area dominio non tocca
`AuthDbContext` ma deve sapere localmente se un'associazione è operativa (per
autorizzazioni e per il match `TransportRequestCandidate`, PA-05). I due campi
(`Account.Stato` in `gocare_auth`, `Association.StatoAccreditamento` in
`gocare_business`) sono tenuti in sync da un solo metodo
(`ProfileProvisioningService.AccreditAssociationAsync` / `RejectAssociationAsync`),
non da un join fra database.

### 5.13 `ERejectionKind`

```csharp
public enum ERejectionKind { Declined, CancelledAfterAcceptance }
```

Per `TransportRequestRejection.Kind`. Due eventi nella stessa tabella:

- `Declined` — associazione candidata rifiuta una richiesta ancora `InAttesa`
  (UC 6). `Causale` di norma `null`: la riga serve solo a `ICoverageEvaluator`
  per contare i rifiuti.
- `CancelledAfterAcceptance` — associazione che **aveva già accettato** si
  tira indietro (UC 3). `Causale` obbligatoria (imposta dal Service):
  l'utente va rinotificato.

Un rifiuto secco non ha bisogno di causale; la causale serve solo dove
qualcuno si sfila da un impegno preso.

### 5.14 `EModificationField`

```csharp
public enum EModificationField { Schedule, Destination }
```

Per `TransportModificationRequest.Field`. **Niente `Companions`**: gli
accompagnatori si modificano sempre in modo diretto (anche a viaggio
`Confermata`), con notifica informativa ma senza approvazione — non toccano
percorso/orario. Il piano li elencava fra i valori ma contraddiceva UC 2:
risolto togliendoli.

### 5.15 `EContactDataKind`

```csharp
public enum EContactDataKind { Requester, Beneficiary }
```

Per `ContactAccessLog.DataKind` (PA-06). Le due parti di cui una richiesta
porta i recapiti; `GetRequesterContactsAsync` scrive una riga di log per ogni
parte i cui contatti restituisce.

### 5.16 `EDevicePlatform`

```csharp
public enum EDevicePlatform { Ios, Android, Web }
```

Per `DeviceToken.Platform`. Insieme chiuso e piccolo → enum, non stringa:
guida lo `switch` in `IPushSender` (APNs / FCM) ed evita refusi
(`"android"`/`"Android"`). Opposto di `Address.Provincia`, che resta `string`
perché il set è ampio e aperto.

### 5.17 Ancora da scrivere

Area Auth (`AccountStatus`, `AccountRole`, …) — a cura del collega.

---

## 6. `[Flags]`: enum come insieme di opzioni combinabili

**Problema.** Un enum normale rappresenta un valore alla volta —
`ETripType.Visit` *oppure* `ETripType.Hospitalization`. Ma una notifica può
dover raggiungere l'utente su più canali insieme (push *e* email).

**Meccanismo (bit a bit).** Se i valori sono **potenze di 2**, ognuno occupa
un bit diverso:

```
Push  = 1  = 0000 0001
Email = 2  = 0000 0010
Push | Email = 0000 0011   (= 3)
```

Con valori sequenziali `1, 2, 3`, `1 | 2` darebbe `3` — ma `3` sarebbe già un
valore a sé, indistinguibile da "Push+Email combinati". Le potenze di 2
garantiscono che ogni combinazione sia un intero unico e decifrabile.

Per leggere un bit: `&` o `HasFlag`:

```csharp
var canali = ENotificationChannel.Push | ENotificationChannel.Email;
canali.HasFlag(ENotificationChannel.Push);   // true
canali.HasFlag(ENotificationChannel.None);   // true sempre (None = 0)
```

**Cosa fa `[Flags]`.** La meccanica bit a bit funziona su qualsiasi enum a
base intera, con o senza attributo. `[Flags]` cambia **solo** `ToString()`:

```
Senza [Flags]:  (ENotificationChannel)3 → "3"
Con [Flags]:    (ENotificationChannel)3 → "Push, Email"
```

Valori potenza di 2 e `None = 0` sono **convenzione** che l'attributo segnala
come intento, non impone.

**Perché qui.** Se si generassero due righe `Notification` separate
(`Canale=Push`, `Canale=Email`) per lo stesso evento, la riga `Email` non
avrebbe mai un `LettaAt` sensato — nessuno la segna letta da dentro GoCare
(l'email si legge nel client di posta). Sarebbe una riga morta in una tabella
pensata per `List` / `MarkRead` / contatori non lette. Con `[Flags]` un evento
= una riga con `Canali = Push | Email`: la riga rappresenta l'evento, non il
canale. Il job di invio guarda `HasFlag(Push)` / `HasFlag(Email)`; `LettaAt`
si riferisce sempre e solo all'esperienza in-app.

---

## 7. Entità di dominio (`Models/Domain/`)

### 7.1 Entità vs value object

`Person` è un'**entità**: identità propria (`Id`), riga a sé — due `Person` coi
dati identici ma `Id` diversi restano due persone.

`Address` **non ha identità**: due indirizzi coi campi uguali *sono* lo stesso
indirizzo. È un **value object** — definito dal valore dei campi, non da un id
— quindi naturale renderlo **immutabile** (per "cambiare indirizzo" si
sostituisce l'intero oggetto).

In EF Core: **owned type** (`OwnsOne`). `Address` non ha tabella né chiave
propria; le sue proprietà diventano colonne nella tabella di chi lo possiede
(`Person` avrà `IndirizzoDomicilio_Via`, `IndirizzoDomicilio_Cap`, …). Nessun
join. Si configura in `OnModelCreating`. Serve perché più entità hanno un
indirizzo — `Person`, `SavedDestination`, `Association.Sede`, e
`TransportRequest` che ne ha **due o tre** sulla stessa riga (`StartAddress`,
`EndAddress` e `ReturnEndAddress?` — opzionale, solo `RoundTrip` con
destinazione di ritorno diversa dalla partenza).

### 7.2 `Address` — `record`, non `class`

`Person` è `sealed class` con `private set` perché mutabile nel tempo.
`Address` è un valore immutabile: un **record posizionale** dà immutabilità e
uguaglianza per valore.

```csharp
// src/GoCare.Application/Models/Domain/Address.cs
public sealed record Address(
    string Street,
    string Number,
    string PostalCode,
    string City,
    string Province);
```

- **`Number` è `string`:** `"12/A"`, `"12 bis"`, `"SNC"` — non è aritmetica.
- **`PostalCode` è `string`:** `"00100"` perderebbe lo zero iniziale come
  `int`.
- Nessun campo nullable: se un `Address` esiste, è completo. L'opzionalità
  (persona senza indirizzo) sta sul **riferimento** (`Address?` su `Person`,
  `TransportRequest.ReturnEndAddress` — `null` ⇒ il ritorno torna a
  `StartAddress`).
- **Niente `Region`:** PA-05 filtra sulla sola `Province`
  (`Association.CoveredProvinces` è una lista di province, confrontata con la
  `Province` di partenza). Nessun rollup regionale da mantenere.

### 7.3 `Guid` come id, ovunque

Tutte le entità usano `Guid`, non `int` auto-increment:

- **Architettura a due database.** `PersonId` è generato in
  `AuthService.RegisterUserAsync`, salvato su `Account` (`gocare_auth`) e
  passato a `ProfileProvisioningService.CreateForAccountAsync` che crea
  `Person` (`gocare_business`) **con lo stesso id**. Con un `int` dal database
  i due DB assegnerebbero interi indipendenti che non combaciano. Un `Guid` lo
  generi in memoria prima di salvare.
- **Pattern di costruzione uniforme.** `Id` è parametro obbligatorio del
  costruttore, `{ get; }` senza setter — l'oggetto nasce con la sua identità.
  Un `int` dal DB imporrebbe un ciclo a due fasi.
- **Sicurezza.** Un `Guid` in una route non è enumerabile (rilevante per
  PA-06, dati di persone fragili).

Costo: 16 byte per riga invece di 4-8 — trascurabile per un v0.

### 7.4 Chiave composta vs `Id` surrogato

`CareGroupMembership` e `TransportRequestCandidate` usano una **chiave
primaria composta**:

| Entità | Chiave |
|---|---|
| `CareGroupMembership` | `(CareGroupId, PersonId)` |
| `TransportRequestCandidate` | `(TransportRequestId, AssociationId)` |

Criterio: se nessuna route e nessun'altra tabella indirizza mai una riga col
suo id singolo (ci si arriva sempre dalla coppia, o da un token come
`InvitoToken`), l'`Id` surrogato è peso morto. La chiave composta dà anche
gratis "non due righe uguali per la stessa coppia".

`TransportRequestRejection` e `Companion` **mantengono un `Id` proprio**: sono
referenziate altrove / hanno senso come entità singole.

Configurazione: `modelBuilder.Entity<T>().HasKey(x => new { x.A, x.B });`.

### 7.5 Snapshot: dati congelati alla creazione

Alcuni campi sono **copie per valore** prese alla creazione, non riferimenti a
dati che cambiano:

- **`TransportRequest.StartAddress` / `EndAddress` / `ReturnEndAddress?` / contatti:**
  `Address` / stringhe copiate nella riga, non FK a `SavedDestination` o
  `Person`. Se la persona poi modifica quella destinazione o cambia telefono,
  il trasporto registrato conserva i dati validi *quando è stato creato*.
- **`TransportRequestCandidate`:** risultato **già calcolato** del match PA-05.
  Alla creazione, per ogni associazione accreditata la cui `CoveredProvinces`
  contiene la provincia di partenza si scrive una riga. Dopo, "questa
  associazione vede questa richiesta?" è una lookup su indice.

Congelare la lista dei candidati è un **pro**:

- niente "rug-pull" su una richiesta in corso;
- denominatore stabile per PA-04 ("non coperta" = N interpellate, M rifiutate,
  timeout scaduto): se l'insieme cambiasse in corsa il conteggio non
  significherebbe più niente;
- tracciabilità di *quali* associazioni erano state interpellate, allineata a
  chi ha ricevuto la notifica.

Costo: un'associazione che aggiunge quella provincia *dopo* non vedrà la
richiesta. Finestra minima → in v0 non si ricalcola.

### 7.6 Timestamp e `IClock`: l'entità non legge l'ora

`CreatedAt` e simili sono parametri **obbligatori del costruttore**, passati
dal Service. L'entità non chiama `DateTimeOffset.UtcNow`:

```csharp
// nel Service, non nell'entità
var now = _clock.UtcNow;
var membership = new CareGroupMembership(groupId, personId, adminRole, role, status, now);
```

`IClock` esiste per rendere il tempo **testabile** (`FakeClock` con ora fissa).
Se l'ora la leggesse l'entità, i test avrebbero timestamp non verificabili e ci
sarebbero due modi di ottenere "adesso". L'entità resta "dumb": riceve il
tempo, non lo calcola.

### 7.7 `DateOnly` vs `DateTimeOffset`

- **`DateOnly`** — data di calendario, niente ora né fuso. `Person.DataNascita`:
  una data di nascita non ha un istante.
- **`DateTimeOffset`** — istante univoco + offset UTC. Tutto il resto:
  `TransportRequest.DataOraPartenza` / `DataOraRitorno`, ogni timestamp.
  Convenzione: `UtcNow` internamente (mai `.Now`), si localizza lato client.

Errore tipico da revisione: un campo con "Ora" nel nome scritto come
`DateOnly`.

### 7.8 `row_version`: concorrenza ottimistica, non è una proprietà

Nel piano `TransportRequest` ha `row_version`. **Non** è una proprietà C#: è
una colonna gestita dal DB che cambia a ogni update. Al salvataggio EF genera
`UPDATE … WHERE Id = @id AND row_version = @valoreLetto`; se un altro ha
modificato la riga nel frattempo, l'update non trova nulla e EF lancia
`DbUpdateConcurrencyException` invece di sovrascrivere in silenzio.

Serve su `TransportRequest` perché è l'entità con più scritture concorrenti
(due associazioni che accettano lo stesso trasporto — "prima accettazione
vince"; il `CoverageTimeoutJob` che marca `NonCoperta` mentre una accetta). Su
Postgres si mappa la colonna di sistema `xmin` come concurrency token in
`OnModelCreating` (`.UseXminAsConcurrencyToken()`) — pura configurazione EF.

### 7.9 `Person` senza flag di ruolo

`Person` non ha `IsCaregiver`/`IsAssisted`. Il ruolo caregiver/assistito non è
un'identità globale della persona: esiste solo come
`CareGroupMembership.RuoloNelGruppo`, per singolo gruppo. Una persona che usa
l'app senza gruppi non ha ruolo — richiede un trasporto per sé come
richiedente-e-beneficiario coincidenti (nessun controllo di gruppo). Dettagli
in `MEMORY.md`.

### 7.10 Metodi sulle entità vs Service

Regola: **se il metodo tocca solo `this` e i suoi campi → entità. Se gli serve
qualcosa da fuori (un'altra riga, un port, il clock, l'utente corrente, il DB)
→ Service**, che chiama l'entità per il pezzo puro.

- **Sull'entità:** metodi sincroni che proteggono un invariante del singolo
  oggetto — `Notification.MarkRead(at)`, `DeviceToken.Deactivate(at)`,
  `TransportModificationRequest.Approve/Reject/Withdraw(at)`, un costruttore
  che fa `throw` su combinazioni incoerenti. Niente `await`, niente
  `DbContext`, niente `IClock`; il timestamp arriva come parametro.
- **Nel Service:** orchestrazione — load/save, regole fra entità ("chi chiama
  è l'associazione assegnata?"), scelta del ramo (`InAttesa` → diretto vs
  `Confermata` → `TransportModificationRequest`), `_clock.UtcNow`, notifiche.
  Schema: `load → controlli cross-entity → entity.FaiLaCosa(dati, now) →
  SaveChanges → notifica`.

Test rapido: se ha bisogno di `await`, non è un metodo d'entità.

`private set` + metodo sull'entità = l'entità è l'unica porta d'ingresso a una
transizione di stato, e non può finire in uno stato non valido perché un
Service ha dimenticato un controllo.

### 7.11 Niente navigation property

Le entità hanno solo `Guid` FK nudi (`RequestedById`, `CareGroupId`, …), mai
`public Person RequestedBy { get; }`. Scelta deliberata, coerente con entità
snelle + due DB + filosofia snapshot. Conseguenze:

- **EF non inferisce nessuna relazione.** Ogni FK in `OnModelCreating` è o un
  vincolo esplicito — `HasOne<Person>().WithMany().HasForeignKey(x => x.RequestedById)`
  (senza lambda: non c'è nav né nav inversa) `.OnDelete(DeleteBehavior.Restrict)` —
  oppure resta solo colonna + indice, integrità garantita dal codice.
- **Il DbContext è la mappa completa:** ogni relazione, chiave, vincolo è scritto
  lì, non nascosto nelle entità.
- **Caricare un correlato = seconda query esplicita nel Service** (per id).
  Nessun `.Include`, da nessuna parte.
- **Collezioni figlie** (`Companions`, `Candidates`, `Memberships`) si leggono
  con `_db.Set<T>().Where(c => c.ParentId == id)`, non via collection nav.
- **Confine Auth↔dominio invariato:** una nav non può attraversare due
  `DbContext` comunque; il legame resta `Guid` + `IAccountReader` (§6.x).

Costo: più righe in `OnModelCreating`, più verbosità ai call site. In cambio:
zero query nascoste, niente lazy loading, niente cascade da ragionare, niente
cicli di serializzazione.

---

## 8. Stato — Fase 2, lato dominio

**Fatto:**

- PostgreSQL locale, database `gocare_auth`/`gocare_business` creati.
- `GoCare.Application.csproj`: Npgsql EF Core 10.0.3 + EF Core Design 10.0.4
  (pinnato, `PrivateAssets="all"`).
- `BusinessDbContext` scritto e registrato in `DependencyInjection.AddApplication`
  (`AddDbContext` + `UseNpgsql`); ancora senza `DbSet` né `OnModelCreating`.
- `Program.cs` chiama `AddApplication(builder.Configuration)` — verificato a
  build (0/0) e a runtime.
- **Tutti** gli enum di dominio in `Models/Enums/` (§5).
- **Tutte** le entità di dominio in `Models/Domain/` scritte: `Address`,
  `Person`, `Association`, `CareGroup`, `CareGroupMembership`,
  `SavedDestination`, `TransportRequest`, `TransportRequestCandidate`,
  `TransportRequestRejection`, `Companion`, `TransportModificationRequest`,
  `TripStatusTransition`, `Notification`, `ContactAccessLog`, `DeviceToken`.

**Da fare:**

- `AuthDbContext` + entità Auth — a cura del collega.
- Metodi ancora da aggiungere (vedi §7.10): `Approve()`/`Reject()`/`Withdraw()`
  su `TransportModificationRequest`; transizioni di stato + corpo del
  costruttore con invariante direzione↔orari su `TransportRequest`;
  `Invite(...)` / `Accept()` / `Decline()` su `CareGroupMembership`;
  `Accredit()`/`Reject()` su `Association`; soft-delete / anonimizzazione dove
  previsto.
- Rifiniture: `TransportRequest` senza timestamp di creazione;
  `EModificationRequestStatus` ha un valore chiamato `Fallback` da rinominare
  `Withdrawn`; `TransportModificationRequest.DeletedAt` da togliere (il ritiro
  è un valore di `Status`); refuso `Association.AvaibilityHours` →
  `AvailabilityHours`.
- `DbSet<T>` + `OnModelCreating`: owned type per `Address`, chiavi composte per
  `CareGroupMembership` / `TransportRequestCandidate`, `xmin` su
  `TransportRequest`, query filter globale per il soft-delete, conversione enum
  (`HasConversion<string>()`) e `List<string>`, relazioni esplicite senza nav
  property (§7.11) — `HasOne<T>().WithMany().HasForeignKey(...)` o colonna+indice.
- Prima migrazione EF Core di `BusinessDbContext` (bloccata finché non c'è un
  `DbSet`).
