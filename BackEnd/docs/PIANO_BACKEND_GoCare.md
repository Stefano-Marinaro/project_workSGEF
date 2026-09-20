# Piano di sviluppo – Backend GoCare

> Documento di pianificazione allineato a `Project_GoCare.md` (guida funzionale del progetto) e al codice reale in `src/GoCare/`.
> **Non contiene codice**: è la sequenza di step da eseguire per costruire il backend.
> Ogni step è pensato per essere spuntato man mano. In caso di conflitto, il codice vince sempre su questo documento.

---

## 0. Premesse e decisioni architetturali fissate

| Aspetto | Scelta |
|---|---|
| Linguaggio / runtime | C# (.NET, ASP.NET Core Web API, **Controller MVC**) |
| Database | PostgreSQL, **un solo database** (`gocare`) |
| ORM | EF Core + provider Npgsql, **un solo `DbContext`** (`GoCareDbContext`) |
| Architettura | **Un solo progetto** (`src/GoCare`), organizzato **per livello tecnico** (Controllers/Dtos/Models/Services/Data/…), con sottocartelle `Auth`/`Domain` dentro Controllers, Dtos, Models e Services dove serve distinguere le due aree |
| Livelli | `Controllers/` → `Services/` → `Data/`; `Dtos/` (contratti verso il client) e `Models/` (entità EF/dominio) separati per cartella, stesso progetto |
| Interfacce sui Service | **Niente `I…Service`**: i Service sono classi concrete, registrate in DI per tipo concreto. Un'interfaccia si scrive solo se serve davvero (secondo implementatore reale, contratto polimorfico) — vedi §3 |
| Repository | **Non usati**: i Service leggono/scrivono `GoCareDbContext` direttamente |
| Comunicazione Auth ↔ dominio | **Diretta**: un solo `DbContext`, i Service di dominio leggono `Account` come qualsiasi altra tabella. Nessun `IAccountReader` (aveva senso solo con due database separati) |
| Notifiche | Push applicative (es. Firebase Cloud Messaging) + e-mail transazionali |
| Real-time stato viaggio | Push per aggiornamento immediato + polling alla riapertura schermata (SignalR opzionale, non richiesto in v0) |
| Cancellazioni | Soft delete + anonimizzazione dove previsto |
| Modello di prenotazione | A **richiesta** dell'utente (PA-01), non a slot |
| Validazione | FluentValidation (un validator per DTO di Request) + `ValidationFilter` globale sui controller |
| Errori | Eccezioni tipizzate + `GlobalExceptionHandler` → `ProblemDetails` (RFC 7807) |
| Mediator / CQRS | **Non usati**: i controller chiamano direttamente i Service |

### Attori / ruoli da modellare
- **Caregiver** – unico ruolo utente registrato lato "famiglia"; gestisce uno o più **Assistiti** (che non hanno account) e prenota i loro trasporti.
- **Assistito** (`AssistedPerson`) – destinatario del trasporto; nessun account, nessun login, gestito interamente dal/dai caregiver collegati.
- **Associazione** – riceve, accetta/rifiuta, gestisce i trasporti in carico.
- **Operatore dell'associazione** – aggiorna lo stato del viaggio tramite un link **scoped al singolo trasporto** (`TripOperatorLink`), generato e inoltrato dall'associazione. Non ha un account proprio, non fa login; la sua identità è solo un'etichetta testuale sulla transizione di stato (PA-03, risolto).
- **GoCare (sistema)** – instrada richieste, genera notifiche push/e-mail, conserva lo storico.

---

## 0.1 Struttura interna del progetto

**`src/GoCare`** — unico progetto, dipendenza da ASP.NET Core MVC:
- `Controllers/Auth/`, `Controllers/Domain/` – classi `[ApiController]`, una per area funzionale. Fanno solo: bind del DTO, chiamata a **un** metodo di Service, mapping del risultato in `ActionResult`. Nessuna logica, nessun `DbContext`.
- `Dtos/Auth/{Requests,Responses}/`, `Dtos/Domain/{Requests,Responses}/` – record `…Request` / `…Response`, con il relativo Validator FluentValidation accanto alla Request (stesso file/cartella). I `Models` di dominio non escono mai verso il client.
- `Models/Auth/`, `Models/Domain/`, `Models/Enums/` – entità EF; in v0 fanno anche da modello di dominio.
- `Services/Auth/`, `Services/Provisioning/`, `Services/Domain/` – logica applicativa: validazione, `SaveChangesAsync`, orchestrazione. Classi concrete, non interfacce. I metodi pubblici accettano parametri primitivi o tipi di dominio, **mai** un DTO HTTP (es. `AuthService.LoginAsync(string email, string password, CancellationToken ct)`, non `LoginAsync(LoginRequest request)`).
- `Data/` – `GoCareDbContext` (configurazione EF **inline** in `OnModelCreating`, nessuna classe `IEntityTypeConfiguration` separata), `Data/Migrations/`.
- `Errors/` – eccezioni tipizzate + `GlobalExceptionHandler`.
- `Validation/` – `ValidationFilter`.
- `Abstractions/` – solo `IEmailSender` (le altre astrazioni previste in origine, `IClock`/`ICurrentUser`, sono state rimosse: nessun secondo implementatore reale, `DateTimeOffset.UtcNow` e i claim del JWT si leggono direttamente dove servono).
- `Infrastructure/` – implementazioni delle Port (`ConsoleEmailSender` oggi, `SmtpEmailSender` quando serve davvero).
- `Pagination/` – `PagedResult<T>` + `PageQuery`, pronti ma non ancora usati (nessun endpoint di lista scritto finora).
- `Program.cs` – DI, pipeline (autenticazione/autorizzazione JWT, `GlobalExceptionHandler`, `ValidationFilter`), Swagger.

**Un solo database `gocare`.** Alla registrazione si genera **un** `Guid`, usato come chiave primaria sia dell'`Account` sia del profilo di dominio (`Person` o `Association`) — con una **foreign key reale** (`Person.Id`/`Association.Id` → `Account.Id`, `OnDelete(Cascade)`), possibile proprio perché tutto vive nello stesso database. Registrazione in due tempi: alla `POST /auth/register/*` si crea solo `Account` + un profilo "scheletro" (`Person(id, email)` / `Association(id, email)`, il resto dei campi nullable); il profilo si completa in un secondo momento con `CompleteProfile(...)` (Service già scritto, endpoint ancora da esporre — §5.3).

Non si introducono: MediatR/CQRS, `Result<T>`, livello `Manager` separato, eventi di integrazione / bus, repository.

---

## 1. Struttura della solution (stato reale)

```
BackEnd/
├─ src/
│  └─ GoCare/                                  # unico progetto
│     ├─ Controllers/
│     │  ├─ Auth/
│     │  │  └─ AuthController.cs               # register user/association, verify-email, login,
│     │  │                                     # refresh, logout, forgot/reset-password (TUTTI fatti)
│     │  └─ Domain/                            # da scrivere (vedi §8)
│     │     ├─ AssistedPeopleController.cs         # UC 9
│     │     ├─ TransportsController.cs             # UC 1, 3, 5
│     │     ├─ AssociationRequestsController.cs    # UC 6
│     │     ├─ AcceptedTransportsController.cs     # UC 7
│     │     ├─ TripStatusController.cs             # UC 4.10/4.11, TripOperatorLink
│     │     ├─ ProfilesController.cs               # UC 8.1–8.2, 8.5–8.6
│     │     ├─ SavedDestinationsController.cs      # UC 8.3
│     │     ├─ NotificationsController.cs          # UC 4
│     │     ├─ DevicesController.cs                # registrazione push token
│     │     └─ Admin/AssociationAccreditationController.cs  # PA-05
│     ├─ Dtos/
│     │  ├─ Auth/{Requests,Responses}/          # tutti fatti: Login, RegisterUser, RegisterAssociation,
│     │  │                                     # VerifyEmail, ForgotPassword, ResetPassword, Logout,
│     │  │                                     # Refresh, RegisterResponse, AuthTokenResponse
│     │  └─ Domain/{Requests,Responses}/        # da fare
│     ├─ Models/
│     │  ├─ Auth/         Account, EmailVerificationToken, PasswordResetToken,
│     │  │                RefreshToken, FailedLoginAttempt (entità fatta, non ancora usata in LoginAsync),
│     │  │                IExpirable + ExpirableExtensions
│     │  ├─ Domain/       Address, Person, Association, AssistedPerson, CaregiverAssistedLink,
│     │  │                SavedDestination, TransportRequest, TransportRequestCandidate,
│     │  │                TransportRequestRejection, Companion, TripStatusTransition,
│     │  │                TripOperatorLink, Notification, ContactAccessLog, DeviceToken
│     │  └─ Enums/        Auth: EAccountStatus, EAccountRole
│     │                   Dominio: ETripType, ETripDirection, ETripRequestStatus,
│     │                   ETripTransitionStatus (con Suspended, PA "sospeso" risolto per v0),
│     │                   ENotificationType, ENotificationChannel ([Flags]), ENotificationSubject,
│     │                   EAccreditationStatus, ERejectionKind, EContactDataKind, EDevicePlatform
│     ├─ Services/
│     │  ├─ Auth/
│     │  │  ├─ AuthService.cs        # login, register (user/association), verify, refresh,
│     │  │  │                       # logout, forgot/reset-password — TUTTI fatti
│     │  │  ├─ PasswordService.cs    # PasswordHasher<Account> (hash/verify) — fatto
│     │  │  ├─ TokenService.cs       # JWT access token + generatore di token opachi — fatto
│     │  │  └─ JwtOptions.cs
│     │  ├─ Provisioning/
│     │  │  └─ ProfileProvisioningService.cs
│     │  │           # CreatePersonSkeletonAsync / CreateAssociationSkeletonAsync (fatti)
│     │  │           # CompletePersonProfileAsync / CompleteAssociationProfileAsync (fatti, nessun controller li chiama ancora)
│     │  │           # AccreditAssociationAsync / RejectAssociationAsync / AnonymizeForDeletedAccountAsync (da fare)
│     │  └─ Domain/                  # da scrivere (vedi §8): TransportService, AssociationRequestService,
│     │                              # AcceptedTransportService, TripStatusService, AssistedPersonService,
│     │                              # ProfileService, SavedDestinationService, NotificationService
│     ├─ Data/
│     │  ├─ GoCareDbContext.cs       # configurazione EF inline in OnModelCreating, tutte le entità sopra
│     │  └─ Migrations/              # migrazione corrente applicata
│     ├─ Errors/                     # NotFoundException, ConflictException, ForbiddenException,
│     │                              # ValidationException, DomainException, GlobalExceptionHandler
│     ├─ Validation/                 # ValidationFilter
│     ├─ Abstractions/               # IEmailSender
│     ├─ Infrastructure/             # ConsoleEmailSender (stub dev); SmtpEmailSender da fare
│     ├─ Pagination/                 # PagedResult<T>, PageQuery — pronti, non ancora usati
│     ├─ Program.cs
│     └─ appsettings*.json
│
├─ tests/                            # da ricreare (i progetti test precedenti sono stati rimossi con la
│                                     # consolidazione in un progetto unico)
│
├─ Directory.Build.props
├─ docker-compose.yml                # da fare: PostgreSQL (un database) + MailHog
└─ GoCare.slnx
```

---

## 1.1 Flusso di una richiesta (esempio "crea trasporto")

```
POST /transports
 └─ TransportsController.Create(CreateTransportRequest dto)
     └─ TransportService.CreateAsync(dto, currentAccountId)
         1. validazione di dominio (data non nel passato, coerenza direzione↔orari — già garantita
            dal costruttore di TransportRequest, EnsureScheduleConsistency)
         2. verifica che il beneficiario (AssistedPerson) sia collegato al caregiver chiamante
            (CaregiverAssistedLink)
         3. costruisce TransportRequest (stato Pending) + Companions + Candidates (per area, PA-03)
         4. se destinazione nuova e confermata → crea SavedDestination
         5. GoCareDbContext.SaveChangesAsync()
         6. notifica → push/email alle associazioni candidate (UC 4)
         7. ritorna TransportDetailResponse
 └─ 201 Created
```

- Il **Validator** FluentValidation della Request gira nel `ValidationFilter` prima di entrare nell'action.
- Le eccezioni tipizzate (`NotFoundException`, `ConflictException`, `ForbiddenException`) risalgono al `GlobalExceptionHandler` che le mappa in `ProblemDetails` con lo status corretto (404 / 409 / 403).

---

## 2. Fase 0 – Ambiente e tooling

- [x] Installare .NET SDK, verificare versione target (net10.0).
- [x] Consolidare la solution in un progetto unico `src/GoCare` (i precedenti `GoCare.Api`/`GoCare.Application`/`GoCare.Shared` sono stati rimossi).
- [x] `Directory.Build.props`: nullable enabled, `LangVersion` latest.
- [x] `.gitignore` per .NET.
- [x] Connection string verso `gocare` in `appsettings.json`.
- [ ] `docker-compose.yml` con PostgreSQL (un database) e MailHog.
- [ ] User-secrets per chiavi JWT/SMTP/push in sviluppo (oggi la chiave JWT di sviluppo è in chiaro in `appsettings.Development.json`, accettabile solo perché non reale).
- [ ] Configurazione tipizzata: un `IOptions<T>` per gruppo di impostazioni (fatto per `JwtOptions`), validata all'avvio.

---

## 3. Fase 1 – Kernel trasversale

- [x] **`Abstractions/IEmailSender`** – porta e-mail transazionali. Implementazione dev: `Infrastructure/ConsoleEmailSender` (scrive nel log invece di mandare mail vere). `SmtpEmailSender` da fare quando serve un invio reale.
- [x] **`Pagination/PagedResult<T>` + `PageQuery`** – pronti, da usare nei primi endpoint di lista (UC 5, 6, 7).
- [x] **`Errors/`** – eccezioni tipizzate + `GlobalExceptionHandler` → `ProblemDetails` (404/409/403/422) + 500 pulito senza stack trace per le eccezioni non previste.
- [x] **`Validation/ValidationFilter`** – esegue il validator FluentValidation registrato per il tipo di Request prima dell'action. **Nota operativa**: ogni nuovo validator va anche registrato esplicitamente in `Program.cs` (`AddScoped<IValidator<T>, TValidator>()`) — non è automatico; è già capitato che un validator scritto restasse silenziosamente non collegato.
- [x] Swagger/OpenAPI base (`AddSwaggerGen`/`UseSwaggerUI`, solo in Development).
- [ ] Swagger: gruppi per area (Auth/dominio), schema di sicurezza Bearer, esempi di request.
- [x] **Rimosse per YAGNI** (nessun secondo implementatore, nessun test che le richiedesse): `IClock`/`SystemClock`, `ICurrentUser`. `DateTimeOffset.UtcNow` si chiama direttamente nei Service; i claim del JWT (quando servirà leggerli in un controller) si leggono da `HttpContext.User` senza wrapper dedicato finché non emerge un bisogno reale di sostituirli in test.

---

## 4. Fase 2 – Persistenza e database

- [x] Npgsql EF Core Provider + `EFCore.NamingConventions` (snake_case) sul progetto `GoCare`.
- [x] **Un solo database**: `GoCareDbContext` → DB `gocare`. Configurazione EF **inline in `OnModelCreating`**, organizzata in `#region` per area (`AUTH`, `PERSON AND ASSISTED`, `TRANSPORT`, `NOTIFICATION`) — nessuna classe `IEntityTypeConfiguration` separata, decisione presa esplicitamente.
- [x] FK reali `Person.Id`/`Association.Id` → `Account.Id` (`OnDelete(Cascade)`), possibili solo perché tutto vive nello stesso database — prima della consolidazione (due database) erano solo una convenzione applicativa, non un vincolo reale.
- [x] Convenzioni comuni: enum con prefisso `E` salvati come stringa (`ConfigureConventions` → `Properties<Enum>().HaveConversion<string>()`, override a `int` solo per `Notification.Channels` perché `[Flags]`). Concorrenza ottimistica sulla colonna di sistema Postgres `xmin` (shadow property su `TransportRequest`); nessun `row_version` CLR.
- [x] Indici: univoci su `Account.Email` e sui vari `Token` (`EmailVerificationToken`, `PasswordResetToken`, `RefreshToken`, `TripOperatorLink`), indice su `CaregiverAssistedLink.AssistedPersonId`, su `TripOperatorLink.TransportRequestId`.
- [ ] **Soft delete**: colonna `deleted_at` presente sulle entità che la richiedono; **query filter globale ancora da configurare** (oggi va filtrato a mano `Where(x => x.DeletedAt == null)` in ogni query).
- [x] Prima migrazione generata e applicata (`Data/Migrations/`); regenerata una volta per correggere un bug reale di EF Core (v. nota sotto) e una seconda volta per il passaggio al provisioning in due tempi.
- [ ] Migrazioni: auto-apply in Development, script SQL versionati per ambienti superiori.
- [ ] Seed minimo (associazione demo, dati enum se serve).
- [ ] Health check DB.

> **Nota tecnica per chi tocca `GoCareDbContext`**: durante la consolidazione è emerso un bug reale di EF Core 10 — proprietà interamente immutabili (`{ get; }`, nessun setter) di tipo `enum` o `DateTimeOffset` non nullable, su entità che hanno *anche* altre proprietà di tipo "complesso", venivano escluse **silenziosamente** dal modello (nessun errore, nessuna colonna creata). Fix applicato: configurazione esplicita `entity.Property(x => x.Campo)` per ogni proprietà del genere. Se in futuro un campo sembra "sparire" dal database senza errori, controllare prima questo.

### Entità (in `Models/`)

**Area Auth:**
- `Account` — `Id`, `Email` (univoca), `PasswordHash`, `Role` (`EAccountRole`: Person | Association), `Status` (`EAccountStatus`: Unverified | Active | Suspended | Deleted), `CanLogIn` (computata: `Status == Active`), `CreatedAt`, `EmailVerifiedAt?`, `SuspendedAt?`, `DeletedAt?`. `Account.Id` è lo **stesso `Guid`** di `Person.Id` / `Association.Id` (PK condivisa, FK reale). **L'accreditamento non è uno stato dell'account**: è `Association.Status` (dominio) — un'associazione registrata e verificata ha account `Active` come un utente qualsiasi, l'accreditamento è un gate separato.
- `EmailVerificationToken` / `PasswordResetToken` / `RefreshToken` — implementano `IExpirable`, scadenza controllata con l'estensione `IsExpired(now)`. Tutti monouso (`Consume`/`Revoke`).
- `FailedLoginAttempt` — entità e tabella pronte, **non ancora usata** da `AuthService.LoginAsync` (nessun conteggio/blocco tentativi oggi).

**Area dominio:**
- `Person` — `Id` (= `Account.Id`), `Name?`, `Surname?`, `BirthDate?` (`DateOnly?`), `Email`, `Phone?`, `HomeAddress?` (owned type), `DeletedAt`, `AnonymizedAt`. Campi profilo nullable: popolati da `CompleteProfile(...)` dopo la registrazione minima (email+password).
- `Association` — `Id` (= `Account.Id`), `Name?`, `Headquarter?` (owned type), `Phones?` (`List<string>`), `Email`, `AvailabilityHours?`, `Status` (`EAccreditationStatus`: Pending | Accredited | Rejected), `DeletedAt`, `CoveredProvinces?` (`List<string>`, PA-03). Stessi campi profilo nullable + `CompleteProfile(...)` di `Person`.
- `AssistedPerson` — `Id`, `Name`, `Surname`, `BirthDate`, `Phone`, `HomeAddress?`, `CreatedBy` (`Person.Id` del caregiver che l'ha creato), `DeletedAt`, `AnonymizedAt`. Nessun account, nessun login.
- `CaregiverAssistedLink` — chiave primaria composta (`CaregiverId`, `AssistedPersonId`), nessun `Id` surrogato. `CreatedAt`, `DeletedAt?`. Collegamento **sempre diretto e subito attivo in v0** (nessuna accettazione richiesta, anche per un secondo caregiver) — il flusso di invito con consenso è pianificato per v1 (vedi `Project_GoCare.md` §14).
- `SavedDestination` — `Id`, `PersonId`, `PlaceName`, `SavedAddress` (owned type), `Note?`.
- `TransportRequest` — `Id`, `RequestedById` (`Person`), `BeneficiaryId` (`AssistedPerson`), `TripType` (`ETripType`), `TripDirection` (`ETripDirection`), `DepartureDateHour`, `ReturnDateHour?`, `StartAddress`, `EndAddress`, `ReturnEndAddress?` (valorizzato solo se `RoundTrip` con destinazione di ritorno diversa dall'andata), `ReferencePhone` + `ReferenceEmail` (snapshot), `CreatedAt`, `RequestStatus` (`ETripRequestStatus`: Pending | Confirmed | InProgress | Completed | NotCovered | Cancelled), `AssignedAssociationId?`, `AcceptedAt?`, `NotCoveredAt?`, `DeletedBy?`, `DeletedAt?`. Il costruttore valida da solo la coerenza direzione↔orari (`EnsureScheduleConsistency`, lancia se incoerente — non serve ripeterlo nel Validator FluentValidation). Concorrenza ottimistica via shadow property `xmin`. Metodi: `Accept` / `ChangeSchedule` / `ChangeDestination` / `RequestNotCovered` / `Cancel`. **Nessuna modifica post-conferma in v0**: `TransportModificationRequest` è stato rimosso, la modifica del viaggio è pianificata per v1 (§14) con un disegno da rifare da zero.
- `TransportRequestCandidate` — chiave primaria composta (`TransportRequestId`, `AssociationId`), nessun altro campo. Righe immutabili, calcolate alla creazione (PA-03).
- `TransportRequestRejection` — `Id`, `TransportRequestId`, `AssociationId`, `Kind` (`ERejectionKind`: Declined | CancelledAfterAcceptance), `Reason?`, `RejectedAt`.
- `Companion` — `Id`, `TransportRequestId`, `Name`, `Surname`, `Relationship`, `Phone` (tutte `{ get; }`, immutabile — replace-all per modificarla).
- `TripStatusTransition` — `Id`, `TransportRequestId`, `Status` (`ETripTransitionStatus`: Pending | InCharge | Arriving | OnSite | Returning | **Suspended** | Completed — `Suspended` raggiungibile da qualsiasi stato di esecuzione attivo, notifica push+email come evento di esito), `MadeByAssociationId`, `OperatorLabel` (`string`), `OccurredAt`. Log append-only, stato corrente = riga con `OccurredAt` più recente.
- `TripOperatorLink` — `Id`, `TransportRequestId` (l'unico trasporto a cui dà accesso), `Token`, `ExpiresAt` (calcolata automaticamente da `DepartureDateHour + 24h`, non scelta dall'associazione), `CreatedAt`, `RevokedAt?`. Implementa `IExpirable`. Generato/rigenerabile dall'associazione (il vecchio link non viene revocato automaticamente alla rigenerazione, resta valido fino a scadenza naturale o revoca esplicita). L'operatore lo usa per aggiornare `TripStatusTransition` senza login.
- `Notification` — `Id`, `SubjectType` (`ENotificationSubject`: Association | Person), `SubjectId`, `Type` (`ENotificationType`), `Title`, `Body`, `Channels` (`[Flags] ENotificationChannel`), `RelatedEntityId?`, `CreatedAt`, `ReadAt?`. `SubjectId`/`RelatedEntityId` sono `Guid` **polimorfici, senza FK**: possono puntare a tabelle diverse a seconda di `SubjectType`, e Postgres non permette una FK verso più tabelle — l'integrità qui è responsabilità del codice applicativo, non del database.
- `ContactAccessLog` — `Id`, `TransportRequestId`, `AssociationId`, `DataKind` (`EContactDataKind`: Requester | Beneficiary), `AccessedAt` (PA-04).
- `DeviceToken` — `Id`, `SubjectType` (`ENotificationSubject`), `SubjectId` (stesso discorso polimorfico di `Notification`), `PushToken`, `Platform` (`EDevicePlatform`), `CreatedAt`, `DeactivatedAt?`.

---

## 5. Fase 3 – Area Autenticazione

> Cartelle `Controllers/Auth/`, `Services/Auth/`, `Services/Provisioning/`. **Flusso completo e funzionante end-to-end.**

### 5.1 Fondamenta dell'area — tutto fatto
- [x] `GoCareDbContext` con tutte le entità Auth configurate (chiavi, FK verso `Account` con `Cascade`, indici univoci sui `Token`).
- [x] `PasswordService` — `PasswordHasher<Account>` (`Microsoft.AspNetCore.Identity`, PBKDF2-HMACSHA256). Arriva gratis con `Microsoft.NET.Sdk.Web`, nessun pacchetto NuGet aggiuntivo, nessun'altra parte del framework Identity in uso (né `UserManager`, né `IdentityDbContext`: il resto dell'autenticazione — JWT, refresh, macchina a stati dell'account — è scritto a mano).
- [x] `TokenService`: access token JWT (claim `sub` = `Account.Id`, `role`, `email_verified`); token opachi (`RandomNumberGenerator`, 64 byte → Base64) riusati per refresh token, verifica email e reset password — stesso generatore, scopi diversi.
- [x] Configurazione autenticazione JWT in `Program.cs` (`AddAuthentication().AddJwtBearer(...)`, `UseAuthentication`/`UseAuthorization`).
- [ ] Policy di autorizzazione per ruolo (`Role=Person`, `Role=Association`) — nessun controller di dominio ancora scritto, quindi nessuna policy applicata finora.
- [ ] Rate limiting su login e forgot-password.
- [ ] `FailedLoginAttempt`: entità pronta, logica di conteggio/blocco da scrivere.

### 5.2 Endpoint — tutti fatti e verificati

| Endpoint | Route | Service · metodo | Note |
|---|---|---|---|
| Registrazione utente | `POST /auth/register/user` | `AuthService.RegisterUserAsync(email, password)` | Solo email+password: crea `Account` `Unverified` + `Person` scheletro (`CreatePersonSkeletonAsync`). Profilo completo via `CompleteProfile` (§5.3, endpoint da fare). |
| Registrazione associazione | `POST /auth/register/association` | `AuthService.RegisterAssociationAsync(email, password)` | Stesso schema, crea `Association` `Status = Pending` scheletro. |
| Verifica e-mail | `POST /auth/verify-email` | `AuthService.VerifyAccount(token)` | Token nel **body**, non nella route: un link e-mail apre una pagina frontend (GET), che poi chiama questa POST — un link e-mail non può innescare una POST da solo. |
| Login | `POST /auth/login` | `AuthService.LoginAsync(email, password)` | Verifica credenziali + `Account.CanLogIn` (`Status == Active`). **Non controlla ancora l'accreditamento dell'associazione** (PA-05, gate da aggiungere quando si scrive quel Service). |
| Refresh | `POST /auth/refresh` | `AuthService.RefreshAsync(refreshToken)` | **Rotazione**: il token usato viene revocato e se ne emette uno nuovo insieme al nuovo access token (best practice OAuth2 — rileva il riuso di un token rubato). |
| Logout | `POST /auth/logout` | `AuthService.LogoutAsync(refreshToken)` | Nessun `[Authorize]`: il possesso del refresh token è la prova sufficiente. Idempotente — token sconosciuto o già revocato non è un errore. |
| Richiesta reset password | `POST /auth/forgot-password` | `AuthService.ForgotPasswordAsync(email)` | Risposta **sempre uguale** che l'email esista o no (niente enumerazione utenti). |
| Conferma reset password | `POST /auth/reset-password` | `AuthService.ResetPasswordAsync(token, newPassword)` | Cambia la password e **revoca tutte le sessioni attive** dell'account (tutti i `RefreshToken` non ancora revocati). |

Tutti i Validator FluentValidation corrispondenti sono scritti **e registrati** in `Program.cs`.

### 5.3 Cosa manca ancora in quest'area
- [ ] **Endpoint di complementazione profilo**: `ProfileProvisioningService.CompletePersonProfileAsync`/`CompleteAssociationProfileAsync` esistono ma nessun controller li richiama. Servono due endpoint autenticati (es. `PATCH /me/profile`, `PATCH /association/profile` — probabile sovrapposizione con UC 8, da unificare quando si scrive `ProfilesController`).
- [ ] **Gate di accreditamento sul login/operatività associazione** (PA-05): oggi un'associazione può fare login subito dopo la verifica email, indipendentemente da `Association.Status`. Da decidere se bloccare il login o solo l'operatività (candidatura/accettazione trasporti) finché non è `Accredited`.
- [ ] `AccreditAssociationAsync` / `RejectAssociationAsync` in `ProfileProvisioningService` + `Admin/AssociationAccreditationController`.
- [ ] `AnonymizeForDeletedAccountAsync` + endpoint di eliminazione account (UC 10.4, PA-06).
- [ ] `resend verifica email`, `change-email`.

---

## 6. Fase 4 – Concerns trasversali nell'host

- [x] **Composizione**: `Program.cs` con pipeline JWT, `GlobalExceptionHandler`, `ValidationFilter` globale, Swagger.
- [ ] **Autorizzazione**: policy per ruolo dai claim JWT. I controlli a livello di risorsa ("è l'associazione assegnataria", "il richiedente è collegato all'assistito") staranno nei Service di dominio, che leggono `GoCareDbContext` direttamente (un solo database, nessuna indirezione).
- [x] **Validazione**: FluentValidation, un validator per DTO — tutti quelli Auth fatti e registrati.
- [ ] **Logging/telemetria**: Serilog strutturato, correlation id.
- [x] **E-mail transazionali**: `IEmailSender` + `ConsoleEmailSender` (stub dev, scrive nel log). Mancano i template reali e `SmtpEmailSender`.
- [ ] **Push**: implementazione `IPushSender` (adapter FCM/APNs) — interfaccia ancora da introdurre quando servirà davvero (per ora nessun codice la richiede).
- [ ] **Job schedulati** (nessuno scritto ancora):
  - [ ] `TripReminderJob` – promemoria viaggio del giorno successivo (UC 4).
  - [ ] `CoverageTimeoutJob` – richiesta senza accettazione entro soglia (PA-02) → `NotCovered`.
  - [ ] `TokenCleanupJob` – pulizia token scaduti (email verification, password reset, refresh).
- [x] **Concorrenza**: shadow property `xmin` su `TransportRequest`, già configurata — da usare in `AssociationRequestService.AcceptAsync` per "prima accettazione vince" (UC 6.3, non ancora scritto).
- [ ] **Audit**: `ContactAccessLog` da scrivere quando un'associazione vede i contatti di un richiedente (UC 6.5 / PA-04).
- [ ] **Paginazione/sorting/filtri**: `PageQuery`/`PagedResult<T>` pronti, da usare nei primi endpoint di lista.

---

## 7. Fase 5 – Helper interni e Infrastructure

- [ ] **`Services/Domain/Internal/TripStateMachine.cs`** – funzione pura `CanTransition(current, target, direction) → (bool, motivo)`. Regole in §9. Nessuna dipendenza da EF: testabile in isolamento.
- [ ] **`Services/Domain/Internal/NotificationDispatcher.cs`** – dato (destinatario, tipo, dati), crea la `Notification`, invia push e/o e-mail secondo le regole di canale (push per cambi stato intermedi; push + e-mail per eventi di esito, incluso `Suspended`).
- [ ] **`Services/Domain/Internal/CoverageEvaluator.cs`** – decide se una richiesta è `NotCovered` (tutte le associazioni candidate hanno rifiutato, oppure sotto soglia ore alla data — PA-02). Usato da `DeclineAsync` e da `CoverageTimeoutJob`.
- [ ] **`Infrastructure/SmtpEmailSender.cs`** — quando serve un invio e-mail reale (oggi c'è solo `ConsoleEmailSender`).
- [x] **`Services/Provisioning/ProfileProvisioningService.cs`**: `CreatePersonSkeletonAsync`/`CreateAssociationSkeletonAsync` (fatti), `CompletePersonProfileAsync`/`CompleteAssociationProfileAsync` (fatti, nessun endpoint li chiama ancora — §5.3). Da fare: `AccreditAssociationAsync`, `RejectAssociationAsync`, `AnonymizeForDeletedAccountAsync`.
- [ ] Interfacce (`IPushSender`, un eventuale `IKmCalculator` per V1): **da introdurre solo quando esiste un secondo scenario reale** che le richiede (test con fake, o un secondo provider) — coerente con la scelta di non avere interfacce "per principio" (§0).

---

## 8. Fase 6 – Controller + Service per Use Case

> Ogni voce: azione del controller → metodo di Service → cosa fa. Nessun controller di dominio scritto finora — quest'intera fase è da fare.

### UC 1 – Richiedi trasporto — `TransportsController` / `TransportService`
- [ ] `Create` — `POST /transports` → `CreateAsync`
  - Verifica che il beneficiario (`AssistedPerson`) sia collegato al caregiver chiamante (`CaregiverAssistedLink`).
  - Copia indirizzi sulla richiesta (il costruttore di `TransportRequest` valida da solo la coerenza direzione↔orari); salva accompagnatori.
  - Se destinazione nuova e confermata → crea `SavedDestination`.
  - Crea `TransportRequest` `Pending` + `TransportRequestCandidate` per area (PA-03: associazioni accreditate con `CoveredProvinces` che contiene la provincia di partenza).
  - Notifica "nuova richiesta" alle associazioni candidate.

### UC 3 – Annulla trasporto
- [ ] `CancelByUser` — `POST /transports/:id/cancel` → `TransportService.CancelByUserAsync` (soft delete, `Cancel(...)`; se già preso in carico → notifica associazione)
- [ ] `CancelByAssociation` — `POST /association/transports/:id/cancel` → `AcceptedTransportService.CancelAsync`
  - Valida: `Confirmed`, chiamante = `AssignedAssociationId`, causale non vuota.
  - `TransportRequestRejection` con `Kind = CancelledAfterAcceptance` + `Reason`.
  - Resetta la richiesta (`AssignedAssociationId = null`, torna `Pending`); `CoverageEvaluator` → eventuale `NotCovered` immediato.
  - Notifica utente + "cerca un'alternativa". I `TransportRequestCandidate` restano congelati, la richiesta ricompare da sola.

### UC 4 – Stato e notifiche — `NotificationsController`/`NotificationService`, `TripStatusController`/`TripStatusService`, `DevicesController`
- [ ] `List` / `MarkRead` / `MarkAllRead` / `Counters` — `GET /notifications`, `POST /notifications/:id/read`, `POST /notifications/read-all`, `GET /notifications/counters`.
- [ ] `RegisterDevice` / `UnregisterDevice` — `POST /devices` · `DELETE /devices/:id`.
- [ ] `GenerateOperatorLink` — `POST /association/transports/:id/operator-link` → `TripStatusService.GenerateOperatorLinkAsync` — crea/rigenera `TripOperatorLink` (`ExpiresAt = DepartureDateHour + 24h`, calcolata, non scelta).
- [ ] `GetOperatorScreen` — `GET /operatore/:token` → legge il trasporto tramite il link, nessun login.
- [ ] `AdvanceStatus` — `POST /operatore/:token/status` → `TripStatusService.AdvanceAsync` — valida il link (`IsUsable`), valida la transizione con `TripStateMachine`, registra `TripStatusTransition`, notifica push (+ email su `Completed`/`Suspended`).
- [ ] `GetTimeline` — `GET /transports/:id/status-timeline`.

### UC 5 – "I miei viaggi" — `TransportsController`/`TransportService`
- [ ] `ListMine` — `GET /transports?scope=upcoming|history` (tutti gli assistiti collegati al caregiver).
- [ ] `GetDetail` — `GET /transports/:id` (contatti associazione solo dopo presa in carico; timeline stato).
- [ ] `GetAssociationPublicContacts` — `GET /associations/:id/contacts`.

### UC 6 – Richieste pendenti, associazione — `AssociationRequestsController`/`AssociationRequestService`
- [ ] `ListPending` — `GET /association/requests` (solo `Pending` candidate a quell'associazione, esclude quelle già rifiutate da lei; dati minimi assistito — PA-04).
- [ ] `GetDetail` — `GET /association/requests/:id`.
- [ ] `Accept` — `POST /association/requests/:id/accept` → `AcceptAsync` (`Pending`→`Confirmed`, "prima accettazione vince" con `xmin`).
- [ ] `Decline` — `POST /association/requests/:id/decline` → `DeclineAsync` (`TransportRequestRejection` con `Kind = Declined`; `CoverageEvaluator` → eventuale `NotCovered`).
- [ ] `GetRequesterContacts` — `GET /association/requests/:id/contacts` (recapiti completi solo dopo accettazione; scrive `ContactAccessLog`).

### UC 7 – Trasporti accettati, associazione — `AcceptedTransportsController`/`AcceptedTransportService`
- [ ] `List` — `GET /association/transports` (assegnati `Confirmed`/`InProgress`).
- [ ] `GetDetail` — `GET /association/transports/:id`.
- [ ] `GetHistory` — `GET /association/history` (stati terminali, filtri periodo/esito).

### UC 8 – Profilo — `ProfilesController`/`ProfileService`, `SavedDestinationsController`/`SavedDestinationService`
- [ ] `GetMine` / `UpdateMine` — `GET|PUT /me/profile` (per il caregiver — qui va probabilmente agganciata anche la complementazione profilo di §5.3, da valutare se unificare in un solo endpoint o tenerli separati).
- [ ] `List`/`Create`/`Update`/`Delete` destinazioni — `GET|POST|PUT|DELETE /me/destinations[/:id]`.
- [ ] `GetAssociation` / `UpdateAssociation` — `GET|PUT /association/profile` (almeno un telefono e una e-mail obbligatori; include `CoveredProvinces` — PA-03).

### UC 9 – Gestione assistiti — `AssistedPeopleController`/`AssistedPersonService`
- [ ] `Create` — `POST /assisted` → crea `AssistedPerson` + `CaregiverAssistedLink` diretto verso il caregiver chiamante (v0: nessuna accettazione richiesta).
- [ ] `List` — `GET /assisted` (tutti gli assistiti collegati al caregiver chiamante).
- [ ] `GetDetail` — `GET /assisted/:id` (dati, caregiver collegati, viaggi).
- [ ] `Update` — `PUT /assisted/:id`.
- [ ] `AddCaregiver` — `POST /assisted/:id/caregivers` → nuovo `CaregiverAssistedLink` diretto (v0). *(v1: invito con consenso per il secondo caregiver — `Project_GoCare.md` §14).*
- [ ] `RemoveCaregiver` — `DELETE /assisted/:id/caregivers/:caregiverId` → `CaregiverAssistedLink.Revoke(at)`.

### Accreditamento associazioni — `Admin/AssociationAccreditationController` (PA-05)
- [ ] `Accredit` — `POST /admin/associations/:id/accredit` → `ProfileProvisioningService.AccreditAssociationAsync` (`Association.Status` → `Accredited`; sblocca l'operatività nei Service di dominio).
- [ ] `Reject` — `POST /admin/associations/:id/reject` → `RejectAssociationAsync` (`Status` → `Rejected`; non tocca l'`Account`, che resta `Active` — decisione da confermare in fase di scrittura).

---

## 9. Fase 7 – `TripStateMachine`

- [ ] **Stati della richiesta** (`ETripRequestStatus`): `Pending` → `Confirmed` → `InProgress` → `Completed`. Terminali alternativi: `NotCovered`, `Cancelled`. Il rifiuto di una singola associazione non è uno stato della richiesta: resta in `TransportRequestRejection`; l'esito aggregato negativo è sempre `NotCovered` (`CoverageEvaluator`).
- [ ] **Stati di esecuzione** (`ETripTransitionStatus`, aggiornati dall'operatore via `TripOperatorLink`): `Pending` → `InCharge` → `Arriving` → `OnSite` → `Returning` → `Completed`, con **`Suspended`** raggiungibile da qualsiasi stato di esecuzione attivo (`InCharge`/`Arriving`/`OnSite`/`Returning`) e ripristinabile allo stesso stato di provenienza — guasto/imprevisto durante il viaggio, notifica push+email come evento di esito (PA "sospeso", risolto per v0).
- [ ] **Regole (in `Services/Domain/Internal/TripStateMachine.cs`, testate una per una)**:
  - Sequenza obbligata: nessun salto avanti, nessun ritorno indietro (eccetto il rientro da `Suspended` allo stato di provenienza).
  - `OnlyGo` (ricovero, trasferimento) → salta `Returning`.
  - `OnlyReturn` (dimissione) → salta `OnSite`.
  - Ogni transizione registra stato + timestamp + `OperatorLabel`.
  - Accoppiamento dei due assi: la prima transizione di esecuzione porta `RequestStatus` da `Confirmed` a `InProgress`; la transizione a `Completed` porta `RequestStatus` a `Completed`.
  - `Completed` chiude il viaggio, lo rende non modificabile, lo sposta negli storici.
  - Notifiche: push per i cambi di stato intermedi; push + e-mail per gli eventi di esito (`Completed`, `Suspended`).

---

## 10. Fase 8 – Sicurezza, privacy e conformità

- [ ] Accettazione informativa privacy in registrazione + versione salvata.
- [ ] Politica esposizione contatti (PA-04): dati minimi in valutazione, recapiti completi post-accettazione, `ContactAccessLog`.
- [ ] Anonimizzazione post-eliminazione account (PA-06): rimuovere dati personali, mantenere dati di servizio del viaggio nello storico associazione.
- [ ] Accreditamento associazioni (PA-05): `Association.Status`, endpoint admin, gate operatività nei Service di dominio + (da decidere) sul login.
- [ ] Hardening: HTTPS obbligatorio (`UseHttpsRedirection` già attivo), header di sicurezza, CORS ristretto all'app, secret fuori dal repo (oggi la chiave JWT dev è in chiaro), rotazione chiavi JWT, rate limiting login/forgot-password, lockout progressivo (`FailedLoginAttempt`).
- [ ] Autorizzazione a livello di risorsa in ogni Service (proprietà viaggio, collegamento caregiver↔assistito, associazione assegnataria/candidata).

---

## 11. Fase 9 – Testing

- [ ] **Unit test dei Service** con `GoCareDbContext` InMemory o SQLite + fake `IEmailSender`.
- [ ] **Unit test `TripStateMachine`** (tutte le direzioni, incluso `Suspended`) e `CoverageEvaluator`.
- [ ] **Unit test validator** FluentValidation.
- [ ] **Integration test** (`WebApplicationFactory` + Testcontainers PostgreSQL) sui flussi §12 di `Project_GoCare.md`:
  - 12.1 Registrazione (e-mail duplicata, token scaduto, login non verificato, profilo scheletro→completo).
  - 12.2 Login (credenziali errate, account non verificato).
  - 12.3 Recupero password (risposta neutra, token monouso, invalidazione sessioni).
  - 12.4 Richiesta → accettazione (rifiuto, doppia accettazione concorrente, timeout → non coperta).
  - 12.6 Annullamento utente e disdetta associazione (rientro fra le pendenti).
  - 12.7 Ciclo di stato completo per ogni direzione di viaggio, incluso `Suspended`.
  - 12.8 Gestione assistiti (collegamento diretto, secondo caregiver).
- [ ] Test dei job (`TripReminderJob`, `CoverageTimeoutJob`, `TokenCleanupJob`).
- [ ] Test di autorizzazione negativi (accesso a risorse altrui).
- [ ] Progetti test da ricreare (rimossi con la consolidazione in progetto unico — `tests/GoCare.Tests` unico, non più due).

---

## 12. Fase 10 – Osservabilità, configurazione, delivery

- [ ] Health check (`/health/live`, `/health/ready`) con il database e le dipendenze esterne.
- [ ] Logging strutturato + correlation id + log degli eventi di dominio chiave.
- [ ] Metriche: richieste create, tasso di copertura, tempo medio di presa in carico, notifiche inviate/fallite.
- [ ] Configurazione per ambiente con `IOptions` validati all'avvio.
- [ ] Dockerfile dell'API; `docker-compose` per stack locale completo (un database, non più due).
- [ ] CI: build, `dotnet format --verify`, test, migrazioni verificate, pubblicazione artefatto.
- [ ] Migrazioni in produzione via script SQL applicati in deploy (non auto-migrate).
- [ ] Backup DB e retention.

---

## 13. Fase 11 – Dati demo e onboarding sviluppo

- [ ] Seeder: 1 associazione accreditata, 2 caregiver, 2 assistiti collegati, richieste in vari stati.
- [ ] README per avviare l'ambiente locale in un comando.
- [ ] Collezione richieste (`.http` / Bruno / Postman) allineata alla tabella endpoint §14.

---

## 14. Tabella route backend (sintesi)

| Area | Metodo + path | Ruolo | Stato |
|---|---|---|---|
| Auth | `POST /auth/register/user` | Pubblico | ✅ fatto |
| Auth | `POST /auth/register/association` | Pubblico | ✅ fatto |
| Auth | `POST /auth/verify-email` | Pubblico | ✅ fatto |
| Auth | `POST /auth/login` | Pubblico | ✅ fatto |
| Auth | `POST /auth/refresh` | Pubblico | ✅ fatto |
| Auth | `POST /auth/logout` | Pubblico (possesso token) | ✅ fatto |
| Auth | `POST /auth/forgot-password` | Pubblico | ✅ fatto |
| Auth | `POST /auth/reset-password` | Pubblico | ✅ fatto |
| Auth | `PATCH /me/profile` (o equivalente) — completa profilo Person | Autenticato | ⬜ da fare (§5.3) |
| Auth | `PATCH /association/profile` (o equivalente) — completa profilo Association | Autenticato | ⬜ da fare (§5.3) |
| Auth | `POST /auth/verify-email/resend` | Pubblico | ⬜ da fare |
| Auth | `POST /auth/change-email` | Autenticato | ⬜ da fare |
| Auth | `DELETE /auth/account` | Autenticato | ⬜ da fare |
| Dominio | `POST /assisted` | Caregiver | ⬜ da fare |
| Dominio | `GET /assisted` | Caregiver | ⬜ da fare |
| Dominio | `GET /assisted/:id` | Caregiver | ⬜ da fare |
| Dominio | `PUT /assisted/:id` | Caregiver | ⬜ da fare |
| Dominio | `POST /assisted/:id/caregivers` | Caregiver | ⬜ da fare |
| Dominio | `DELETE /assisted/:id/caregivers/:caregiverId` | Caregiver | ⬜ da fare |
| Dominio | `POST /transports` | Caregiver | ⬜ da fare |
| Dominio | `GET /transports` | Caregiver | ⬜ da fare |
| Dominio | `GET /transports/:id` | Caregiver | ⬜ da fare |
| Dominio | `POST /transports/:id/cancel` | Caregiver | ⬜ da fare |
| Dominio | `GET /transports/:id/status-timeline` | Caregiver | ⬜ da fare |
| Dominio | `GET /association/requests` | Associazione | ⬜ da fare |
| Dominio | `GET /association/requests/:id` | Associazione | ⬜ da fare |
| Dominio | `POST /association/requests/:id/accept` | Associazione | ⬜ da fare |
| Dominio | `POST /association/requests/:id/decline` | Associazione | ⬜ da fare |
| Dominio | `GET /association/requests/:id/contacts` | Associazione | ⬜ da fare |
| Dominio | `GET /association/transports` | Associazione | ⬜ da fare |
| Dominio | `GET /association/transports/:id` | Associazione | ⬜ da fare |
| Dominio | `POST /association/transports/:id/operator-link` | Associazione | ⬜ da fare |
| Dominio | `GET /operatore/:token` | Pubblico (solo con link) | ⬜ da fare |
| Dominio | `POST /operatore/:token/status` | Pubblico (solo con link) | ⬜ da fare |
| Dominio | `POST /association/transports/:id/cancel` | Associazione | ⬜ da fare |
| Dominio | `GET /association/history` | Associazione | ⬜ da fare |
| Dominio | `GET|PUT /association/profile` | Associazione | ⬜ da fare |
| Dominio | `GET|PUT /me/profile` | Caregiver | ⬜ da fare |
| Dominio | `GET|POST|PUT|DELETE /me/destinations[/:id]` | Caregiver | ⬜ da fare |
| Dominio | `GET /associations/:id/contacts` | Caregiver | ⬜ da fare |
| Dominio | `GET /notifications` | Tutti | ⬜ da fare |
| Dominio | `POST /notifications/:id/read` · `read-all` | Tutti | ⬜ da fare |
| Dominio | `GET /notifications/counters` | Tutti | ⬜ da fare |
| Dominio | `POST /devices` · `DELETE /devices/:id` | Tutti | ⬜ da fare |
| Admin | `POST /admin/associations/:id/accredit` | Admin GoCare | ⬜ da fare |
| Admin | `POST /admin/associations/:id/reject` | Admin GoCare | ⬜ da fare |

---

## 15. Punti aperti che bloccano/condizionano il backend

Numerazione allineata a `Project_GoCare.md` §13 (rinumerata: i punti chiusi o rinviati a v1 non sono più qui, vedi sotto).

| ID | Punto | Impatto sul backend | Service condizionati |
|---|---|---|---|
| PA-01 | Modello a richiesta vs a slot | Strutturale: entità e flusso principale (già deciso: a richiesta) | UC 1, 4, 5, 6 |
| PA-02 | Soglia "non coperta" | Regola di `CoverageEvaluator` + `CoverageTimeoutJob` | UC 4, 6 |
| PA-03 | Visibilità richieste per area | `Association.CoveredProvinces` confrontata con la provincia di partenza + `TransportRequestCandidate` | UC 6, 8 |
| PA-04 | Esposizione dati contatto assistito | Dati minimi vs completi + `ContactAccessLog` | UC 6 |
| PA-05 | Accreditamento associazioni | `Association.Status` (dominio); gate operatività nei Service; aperto se debba bloccare anche il login | Admin, AuthService.LoginAsync, Service di dominio |
| PA-06 | Retention dati post-eliminazione | Logica di anonimizzazione | `AnonymizeForDeletedAccountAsync` |

**Punti chiusi durante questa fase** (non più aperti): chi aggiorna lo stato viaggio e come accede (`TripOperatorLink`, niente account operatore) — ruoli/permessi nel gruppo cura (non più applicabile: `CareGroup` è stato sostituito da `AssistedPerson`/`CaregiverAssistedLink`, collegamento diretto in v0, senza ruoli amministrativi) — gestione guasti in corsa (`Suspended` nell'enum, non più opzionale).

**Fuori scope v0, pianificati per v1** (`Project_GoCare.md` §14): invito con consenso per il secondo caregiver, modifica della richiesta di viaggio, calcolo km previsti, trasporto sociale, trasporti ricorrenti, posizione GPS del mezzo, flusso API bidirezionale.

---

## 16. Ordine di esecuzione consigliato (milestone)

1. **M0 – Fondamenta** ✅ *(fatto)*: progetto unico, database unico, migrazione iniziale.
2. **M1 – Autenticazione** ✅ *(fatto)*: registrazione (scheletro) → verifica → login → refresh → logout → forgot/reset password, tutto end-to-end.
3. **M1.5 – Completamento profilo** *(prossimo passo naturale)*: endpoint `CompleteProfile` per Person e Association (§5.3) — senza questo, i profili restano scheletri per sempre.
4. **M2 – Assistiti**: UC 9 (`AssistedPerson`, `CaregiverAssistedLink` diretto).
5. **M3 – Ciclo richiesta trasporto**: UC 1 → UC 6 → UC 7 (dopo PA-02, PA-03, PA-04).
6. **M4 – Stato viaggio e notifiche**: `TripStateMachine`, `TripOperatorLink`, UC 4 (centro notifiche, push, e-mail, job).
7. **M5 – Annullamenti**: UC 3.
8. **M6 – Profilo e destinazioni**: UC 8.
9. **M7 – Accreditamento**: gate PA-05, controller admin.
10. **M8 – Hardening**: Fase 8, Fase 10, suite di test, seed demo.

---

## 17. Definition of Done del backend v0

- [ ] Tutti gli endpoint della tabella §14 implementati e coperti da test.
- [ ] Tutti i flussi §12 di `Project_GoCare.md` verificati da test di integrazione.
- [ ] `TripStateMachine` conforme alle regole §9 per ogni direzione di viaggio, incluso `Suspended`.
- [ ] Notifiche push + e-mail secondo le regole di canale.
- [ ] Job schedulati attivi (`TripReminderJob`, `CoverageTimeoutJob`, `TokenCleanupJob`).
- [ ] Un solo progetto, un solo database, un solo `DbContext`.
- [ ] Ogni controller sottile (solo HTTP); tutta la logica nei Service; DTO e Model mai confusi.
- [ ] Punti aperti (§15) chiusi o parcheggiati con un default documentato.
