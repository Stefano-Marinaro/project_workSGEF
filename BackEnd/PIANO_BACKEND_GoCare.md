# Piano di sviluppo – Backend GoCare

> Documento di pianificazione estratto da `Project_GoCare_Revisione_v2.pdf`.
> **Non contiene codice**: è la sequenza di step da eseguire per costruire il backend.
> Ogni step è pensato per essere spuntato man mano.

---

## 0. Premesse e decisioni architetturali fissate

| Aspetto | Scelta |
|---|---|
| Linguaggio / runtime | C# (.NET, ASP.NET Core Web API, **Controller MVC**) |
| Database | PostgreSQL (da confermare nel doc, assunto qui) |
| ORM | EF Core + provider Npgsql |
| Architettura | **A livelli (layered / N-tier) organizzata come modular monolith** |
| Separazione moduli | **Autenticazione e logica di business in due progetti separati**, con schemi DB separati e nessun riferimento incrociato oltre `GoCare.Auth.Contracts` |
| Livelli per modulo | `Controllers/` → `Services/` → `Data/`; `Dtos/` (contratti verso il client) e `Models/` (entità EF/dominio) separati |
| Comunicazione fra moduli | Contratti espliciti + eventi di integrazione (`AccountRegistered`, `AccountDeleted`…) |
| Notifiche | Push applicative (es. Firebase Cloud Messaging) + e-mail transazionali |
| Real-time stato viaggio | Push per aggiornamento immediato + polling alla riapertura schermata (SignalR opzionale, non richiesto in v0) |
| Cancellazioni | Soft delete + anonimizzazione dove previsto |
| Modello di prenotazione | A **richiesta** dell'utente (PA-01), non a slot |
| Validazione | FluentValidation + filtro globale sui controller |
| Errori | Eccezioni tipizzate + `GlobalExceptionHandler` → `ProblemDetails` (RFC 7807) |
| Mediator / CQRS | **Non usati**: i controller chiamano direttamente i Service |

### Attori / ruoli da modellare
- **Assistito** – destinatario del trasporto, può registrarsi da solo.
- **Caregiver** – prenota per gli assistiti del proprio gruppo cura.
- **Associazione** – riceve, accetta/rifiuta, gestisce i trasporti in carico.
- **Operatore dell'associazione** – aggiorna lo stato del viaggio. In v0 **non ha account proprio**: i cambi di stato passano dall'account associazione (PA-03), ma va registrata l'identità (label testuale) dell'operatore.
- **GoCare (sistema)** – instrada richieste, genera notifiche push/e-mail, conserva lo storico.

---

## 0.1 Razionale architetturale (variante C – layered / N-tier modulare)

**Organizzazione per livello tecnico dentro ogni modulo:**
- `Controllers/` – classi `[ApiController]`, una per area funzionale. Fanno **solo**: bind del DTO, chiamata a **un** metodo di Service, mapping del risultato in `ActionResult`. Nessuna logica, nessun accesso a `DbContext`.
- `Dtos/` – record `…Request` / `…Response`. Sono i contratti verso il client React Native. Al client non escono mai i `Models`.
- `Models/` – entità EF; in v0 fanno anche da modello di dominio (niente classi separate). Vivono solo dentro il modulo.
- `Services/` – la logica applicativa: validazione di dominio, transazioni (`SaveChangesAsync`), orchestrazione, chiamate agli helper interni e alle Port. Un'interfaccia `I…Service` + implementazione per area. **È il livello che in altri progetti si chiama "Manager".**
- `Data/` – `DbContext`, `IEntityTypeConfiguration`, migrazioni; repository **opzionali** (in v0 il Service usa `DbContext` direttamente).
- `Infrastructure/` – implementazioni delle Port (push, e-mail, calcolo km, lettura account cross-modulo).

**Il "verticale" sopravvive al confine di modulo:** `GoCare.Auth` e `GoCare.Business` restano due progetti indipendenti, due `DbContext`, due schemi PostgreSQL, due set di migrazioni. Comunicano solo tramite `GoCare.Auth.Contracts` + eventi di integrazione.

**Cosa NON introduciamo:** MediatR/CQRS, pattern `Result<T>`, un livello `Manager` separato dai Service, mapper generici oltre a Mapster/estensioni manuali.

---

## 1. Struttura della solution (obiettivo finale)

```
BE_GoCare/
├─ src/
│  ├─ GoCare.Api/                          # Host: Program.cs, DI, middleware, JWT, Swagger, ApplicationParts
│  │  ├─ Program.cs
│  │  └─ appsettings*.json
│  │
│  ├─ GoCare.Auth/                         # ── MODULO AUTENTICAZIONE ──────────────────────
│  │  ├─ Controllers/
│  │  │  ├─ AuthController.cs              # register (user/association), login, refresh, logout
│  │  │  ├─ EmailVerificationController.cs # verify-email, resend
│  │  │  ├─ PasswordController.cs          # forgot-password, reset-password
│  │  │  └─ AccountController.cs           # change-email, delete account
│  │  ├─ Dtos/
│  │  │  ├─ Requests/                      # RegisterUserRequest, RegisterAssociationRequest,
│  │  │  │                                 # LoginRequest, RefreshTokenRequest,
│  │  │  │                                 # ForgotPasswordRequest, ResetPasswordRequest,
│  │  │  │                                 # ChangeEmailRequest, DeleteAccountRequest
│  │  │  └─ Responses/                     # AuthTokensResponse, AccountResponse
│  │  ├─ Models/
│  │  │  ├─ Account.cs
│  │  │  ├─ EmailVerificationToken.cs
│  │  │  ├─ PasswordResetToken.cs
│  │  │  ├─ RefreshToken.cs
│  │  │  ├─ FailedLoginAttempt.cs
│  │  │  └─ Enums/                         # AccountStatus, AccountRole
│  │  ├─ Services/
│  │  │  ├─ IAuthService.cs     / AuthService.cs        # registrazione, login, refresh, logout
│  │  │  ├─ IPasswordService.cs / PasswordService.cs    # forgot/reset, hashing
│  │  │  ├─ ITokenService.cs    / TokenService.cs       # emissione/rotazione JWT
│  │  │  └─ IAccountService.cs  / AccountService.cs     # verify-email, change-email, delete
│  │  ├─ Data/
│  │  │  ├─ AuthDbContext.cs               # schema "auth"
│  │  │  ├─ Configurations/               # IEntityTypeConfiguration<T>
│  │  │  ├─ Repositories/  (opzionale)
│  │  │  └─ Migrations/
│  │  ├─ Mapping/                          # estensioni DTO↔Model (o profili Mapster)
│  │  └─ DependencyInjection.cs            # AddAuthModule(IServiceCollection, IConfiguration)
│  │
│  ├─ GoCare.Auth.Contracts/               # progetto minimo referenziato da GoCare.Business
│  │  ├─ IAccountReader.cs                 # stato/ruolo/email_verified di un account
│  │  └─ Events/                           # AccountRegistered, AccountEmailVerified,
│  │                                       # AssociationAccredited, AccountDeleted
│  │
│  ├─ GoCare.Business/                     # ── MODULO LOGICA DI BUSINESS ──────────────────
│  │  ├─ Controllers/
│  │  │  ├─ TransportsController.cs             # UC 1, 2 (utente), 3.1, 5
│  │  │  ├─ TransportModificationsController.cs # UC 2.3
│  │  │  ├─ AssociationRequestsController.cs    # UC 6
│  │  │  ├─ AcceptedTransportsController.cs     # UC 7, 3.2
│  │  │  ├─ TripStatusController.cs             # UC 4.10 / 4.11
│  │  │  ├─ CareGroupsController.cs             # UC 9
│  │  │  ├─ ProfilesController.cs               # UC 8.1–8.3, 8.5–8.6
│  │  │  ├─ SavedDestinationsController.cs      # UC 8.4
│  │  │  ├─ NotificationsController.cs          # UC 4 (centro notifiche, counters)
│  │  │  ├─ DevicesController.cs                # registrazione push token
│  │  │  └─ Admin/AssociationAccreditationController.cs   # PA-11
│  │  ├─ Dtos/
│  │  │  ├─ Requests/                      # CreateTransportRequest, UpdateDestinationRequest,
│  │  │  │                                 # UpdateScheduleRequest, UpdateCompanionsRequest,
│  │  │  │                                 # CancelTransportRequest, DeclineRequestRequest,
│  │  │  │                                 # ApproveModificationRequest, RejectModificationRequest,
│  │  │  │                                 # AdvanceTripStatusRequest, CreateCareGroupRequest,
│  │  │  │                                 # InviteMemberRequest, UpdateProfileRequest,
│  │  │  │                                 # SavedDestinationRequest, RegisterDeviceRequest
│  │  │  └─ Responses/                     # TransportListItemResponse, TransportDetailResponse,
│  │  │                                    # PendingRequestResponse, AcceptedTransportResponse,
│  │  │                                    # CareGroupResponse, CareGroupBookingResponse,
│  │  │                                    # ProfileResponse, NotificationResponse, CountersResponse
│  │  ├─ Models/
│  │  │  ├─ Person.cs, Association.cs
│  │  │  ├─ CareGroup.cs, CareGroupMembership.cs
│  │  │  ├─ TransportRequest.cs, TransportRequestRecipient.cs, TransportRequestRejection.cs
│  │  │  ├─ Accompagnatore.cs, TransportModificationRequest.cs, TripStatusTransition.cs
│  │  │  ├─ SavedDestination.cs, Notification.cs, DeviceToken.cs, ContactAccessLog.cs
│  │  │  └─ Enums/                         # TripType, TripDirection, RequestStatus,
│  │  │                                    # ExecutionStatus, MembershipRole, InvitationStatus,
│  │  │                                    # NotificationType
│  │  ├─ Services/
│  │  │  ├─ ITransportService.cs             / TransportService.cs
│  │  │  ├─ ITransportModificationService.cs / TransportModificationService.cs
│  │  │  ├─ IAssociationRequestService.cs    / AssociationRequestService.cs
│  │  │  ├─ IAcceptedTransportService.cs     / AcceptedTransportService.cs
│  │  │  ├─ ITripStatusService.cs            / TripStatusService.cs      # usa TripStateMachine
│  │  │  ├─ ICareGroupService.cs             / CareGroupService.cs
│  │  │  ├─ IProfileService.cs               / ProfileService.cs
│  │  │  ├─ ISavedDestinationService.cs      / SavedDestinationService.cs
│  │  │  ├─ INotificationService.cs          / NotificationService.cs
│  │  │  └─ Internal/                        # helper di dominio, non esposti ai controller
│  │  │     ├─ TripStateMachine.cs           # transizioni ammesse (§9 / §12.7)
│  │  │     ├─ INotificationDispatcher.cs / NotificationDispatcher.cs   # crea Notification + push/email
│  │  │     ├─ ICoverageEvaluator.cs / CoverageEvaluator.cs             # regola "non coperta" (PA-04)
│  │  │     ├─ IKmCalculator.cs              # port
│  │  │     ├─ IPushSender.cs                # port
│  │  │     └─ IEmailSender.cs               # port (o da GoCare.Shared)
│  │  ├─ Data/
│  │  │  ├─ BusinessDbContext.cs           # schema "business"
│  │  │  ├─ Configurations/
│  │  │  ├─ Repositories/  (opzionale)
│  │  │  └─ Migrations/
│  │  ├─ Infrastructure/                   # implementazioni delle port
│  │  │  ├─ KmCalculator.cs
│  │  │  ├─ PushSender.cs                  # FCM/APNs
│  │  │  ├─ EmailSender.cs                 # SMTP
│  │  │  └─ AccountReaderClient.cs         # consuma GoCare.Auth.Contracts.IAccountReader
│  │  ├─ Jobs/
│  │  │  ├─ TripReminderJob.cs             # promemoria giorno prima (UC 4.3 / 4.7)
│  │  │  ├─ CoverageTimeoutJob.cs          # richiesta → NonCoperta (PA-04)
│  │  │  └─ TokenCleanupJob.cs
│  │  ├─ Integrations/                     # consumer degli eventi Auth
│  │  │  ├─ AccountRegisteredHandler.cs    # crea Person / Association
│  │  │  ├─ AssociationAccreditedHandler.cs
│  │  │  └─ AccountDeletedHandler.cs       # anonimizza, esce dai gruppi
│  │  ├─ Mapping/
│  │  └─ DependencyInjection.cs            # AddBusinessModule(...)
│  │
│  └─ GoCare.Shared/                       # cross-cutting comune ai due moduli
│     ├─ Abstractions/                     # IClock, ICurrentUser, IEmailSender, IEventBus
│     ├─ Pagination/                       # PagedResult<T>, PageQuery
│     ├─ Errors/                           # NotFoundException, ConflictException,
│     │                                    # ForbiddenException, ValidationException,
│     │                                    # GlobalExceptionHandler
│     ├─ Events/                           # IntegrationEvent (base), InProcessEventBus
│     └─ Validation/                       # ValidationFilter (aggancio FluentValidation ai controller)
│
├─ tests/
│  ├─ GoCare.Auth.Tests/                   # unit test dei Service (DbContext InMemory/SQLite + fake)
│  ├─ GoCare.Business.Tests/
│  └─ GoCare.Api.IntegrationTests/         # WebApplicationFactory + Testcontainers PostgreSQL
│
├─ docker-compose.yml                      # PostgreSQL + MailHog
└─ GoCare.sln
```

**Riferimenti fra progetti:**
`GoCare.Api` → tutti. `GoCare.Auth` → `GoCare.Shared`, `GoCare.Auth.Contracts`. `GoCare.Business` → `GoCare.Shared`, `GoCare.Auth.Contracts` (mai `GoCare.Auth`). I controller stanno in class library: l'host li registra con `AddControllers().AddApplicationPart(typeof(...).Assembly)`.

---

## 1.1 Flusso di una richiesta (esempio "crea trasporto")

```
POST /transports
 └─ TransportsController.Create(CreateTransportRequest dto)
     └─ ITransportService.CreateAsync(dto, currentUser)
         1. validazione di dominio (data non nel passato, coerenza direzione↔orari)
         2. se beneficiario ≠ richiedente → verifica appartenenza al gruppo cura
         3. IKmCalculator.CalculateAsync(partenza, destinazione)
         4. costruisce TransportRequest (stato InAttesa) + Accompagnatori + Recipients (per area, PA-05)
         5. se destinazione nuova e confermata → crea SavedDestination
         6. BusinessDbContext.SaveChangesAsync()
         7. INotificationDispatcher.NotifyNewRequest(...) → push/email alle associazioni (UC 4.7)
         8. ritorna TransportDetailResponse
 └─ 201 Created
```

- Il **Validator** FluentValidation della Request gira nel `ValidationFilter` prima di entrare nell'action.
- Le eccezioni tipizzate (`NotFoundException`, `ConflictException`, `ForbiddenException`) risalgono al `GlobalExceptionHandler` che le mappa in `ProblemDetails` con lo status corretto (404 / 409 / 403).
- Il mapping `Dto`↔`Model` avviene nel Service (estensioni in `Mapping/` o profili Mapster).

---

## 2. Fase 0 – Ambiente e tooling

- [ ] Installare .NET SDK, verificare versione target (LTS).
- [ ] Creare `GoCare.sln` e i progetti della struttura §1 (`GoCare.Api`, `GoCare.Auth`, `GoCare.Auth.Contracts`, `GoCare.Business`, `GoCare.Shared`, i 3 progetti di test).
- [ ] `Directory.Build.props`: nullable enabled, warnings as errors, `LangVersion` latest, analyzers.
- [ ] `.editorconfig` + `dotnet format`.
- [ ] `docker-compose.yml` con PostgreSQL e MailHog.
- [ ] `.gitignore` per .NET; inizializzare git (la cartella non è ancora un repo).
- [ ] `appsettings.json` + `appsettings.Development.json` + user-secrets: connection string, chiavi JWT, config SMTP, chiavi push, soglie di dominio (ore per "non coperta", scadenza token).
- [ ] Configurazione tipizzata: un `IOptions<T>` per gruppo di impostazioni, validato all'avvio.

---

## 3. Fase 1 – `GoCare.Shared` (cross-cutting comune)

> Da costruire per prima: i due moduli ne dipendono. Nessun concetto di GoCare qui dentro.

- [ ] **`Abstractions/IClock` + `SystemClock`** – astrazione del tempo iniettata al posto di `DateTime.UtcNow`; serve per testare "data non nel passato" (UC 1.1) e la soglia "non coperta" (PA-04).
- [ ] **`Abstractions/ICurrentUser`** – estrae dai claim del token JWT: `AccountId`, `Ruolo`, `AssociationId?`, `PersonId?`. Iniettata nei Service e nei controller.
- [ ] **`Abstractions/IEmailSender`** – porta e-mail transazionali (riusata da Auth e Business).
- [ ] **`Abstractions/IEventBus` + `Events/InProcessEventBus`** – pubblicazione/consumo eventi di integrazione in memoria; interfaccia pronta per un broker futuro.
- [ ] **`Events/IntegrationEvent`** – classe base degli eventi.
- [ ] **`Pagination/PagedResult<T>` + `PageQuery`** – contenitore `{ items, page, pageSize, totalCount }` e parametri `?page=&pageSize=&sort=` per tutte le liste (UC 5, 6, 7, storici).
- [ ] **`Errors/`** – eccezioni tipizzate (`NotFoundException`, `ConflictException`, `ForbiddenException`, `ValidationException`, `DomainException`) + `GlobalExceptionHandler` che le mappa in `ProblemDetails` (404 / 409 / 403 / 400 / 422) e cattura le eccezioni non previste → 500 pulito senza stack trace.
- [ ] **`Validation/ValidationFilter`** – filtro `IActionFilter`/endpoint filter che esegue i validator FluentValidation registrati per il tipo di Request prima dell'action.
- [ ] Setup **Swagger/OpenAPI** nell'host: gruppi per modulo (Auth / Business), schema di sicurezza Bearer, esempi di request.

---

## 4. Fase 2 – Persistenza e database

- [ ] Aggiungere Npgsql EF Core Provider ai due moduli.
- [ ] `AuthDbContext` (schema `auth`) e `BusinessDbContext` (schema `business`), ciascuno in `Data/` del proprio modulo.
- [ ] Decidere: **un solo database PostgreSQL con due schemi** (consigliato v0) vs due database. Migrazioni separate per modulo in entrambi i casi.
- [ ] Entità in `Models/`; configurazioni `IEntityTypeConfiguration` in `Data/Configurations/`.
- [ ] Convenzioni comuni: chiavi `Guid` (v7/sequenziali), `created_at`/`updated_at`, `row_version` per concorrenza ottimistica, naming snake_case.
- [ ] **Soft delete**: colonna `deleted_at` + query filter globale; gli storici restano leggibili.
- [ ] Migrazioni: `dotnet ef migrations` per modulo, auto-apply in Development, script SQL versionati per ambienti superiori.
- [ ] Seed minimo (enum se non gestiti come `enum` C#, associazione demo).
- [ ] Health check DB.
- [ ] Repository: **opzionali** in v0. Se adottati, uno per aggregato (`IAccountRepository`, `ITransportRequestRepository`, `ICareGroupRepository`) in `Data/Repositories/`; altrimenti il Service usa `DbContext` direttamente.

### Entità (in `Models/` dei rispettivi moduli)

**Modulo Auth (`schema auth`):**
- `Account` — Id, Email (univoca), PasswordHash, Ruolo (Utente | Associazione), Stato (`NonVerificato`, `Attivo`, `InAttesaAccreditamento`, `Sospeso`, `Eliminato`), PersonId? / AssociationId? (riferimento logico all'entità di business), timestamps.
- `EmailVerificationToken` — Id, AccountId, Token, ScadenzaAt, UsatoAt?.
- `PasswordResetToken` — Id, AccountId, Token, ScadenzaAt, UsatoAt? (monouso).
- `RefreshToken` — Id, AccountId, Token, ScadenzaAt, RevocatoAt?, UserAgent/IP (facoltativo).
- `FailedLoginAttempt` — contatore per rate limiting e blocco temporaneo.

**Modulo Business (`schema business`):**
- `Person` — Id, Nome, Cognome, DataNascita, IndirizzoDomicilio, Telefono, Email di contatto, flag capacità (è caregiver / è assistito), `deleted_at`, `anonymized_at`.
- `Association` — Id, Denominazione, Sede, AreaOperativita (PA-05), Telefoni[], Email, OrariReperibilita, StatoAccreditamento, `deleted_at`.
- `CareGroup` — Id, Nome (univoco per creatore), Descrizione?, CreatoDaPersonId, `deleted_at`.
- `CareGroupMembership` — Id, CareGroupId, PersonId, RuoloNelGruppo (Caregiver | Assistito), RuoloAmministrativo (Admin | Membro), StatoInvito (InAttesa | Accettato), InvitoEmail, InvitoToken, timestamps.
- `SavedDestination` — Id, PersonId, Etichetta, IndirizzoCompleto, Note?.
- `TransportRequest` — Id, RichiedentePersonId, BeneficiarioPersonId, CareGroupId?, TipoViaggio, Direzione, DataOraAndata, DataOraRitorno?, IndirizzoPartenza (snapshot), IndirizzoDestinazione (snapshot), KmPrevisti, ContattiRiferimento (snapshot), Stato (`InAttesa`, `Confermata`, `InEsecuzione`, `Conclusa`, `Rifiutata`, `NonCoperta`, `Annullata`), AssociazioneAssegnatariaId?, CausaleAnnullamento?, AnnullatoDa?, timestamps, `row_version`.
- `TransportRequestRecipient` — Id, TransportRequestId, AssociationId (associazioni destinatarie calcolate per area – PA-05).
- `TransportRequestRejection` — Id, TransportRequestId, AssociationId, Causale?, At.
- `Accompagnatore` — Id, TransportRequestId, Nome, Cognome, Parentela, Contatto.
- `TransportModificationRequest` — Id, TransportRequestId, Campo (DataOrario | Destinazione | Accompagnatori), ValorePrecedente, ValoreProposto, Stato (`InAttesaApprovazione`, `Approvata`, `Rifiutata`), MessaggioEsito?, timestamps.
- `TripStatusTransition` — Id, TransportRequestId, Stato (`NonPresoInCarico`, `PresoInCarico`, `InArrivo`, `InVisita`, `InRitorno`, `Concluso`, + eventuale `SospesoImprevisto` – PA-07), Timestamp, EseguitoDaAssociationId, OperatoreLabel.
- `Notification` — Id, DestinatarioType (Person | Association), DestinatarioId, Tipo, Titolo, Corpo, RelatedEntityId?, Canali (Push | Email), LettaAt?, CreatedAt.
- `ContactAccessLog` — Id, TransportRequestId, AssociationId, DatiAccedutiTipo, At (PA-06).
- `DeviceToken` — Id, DestinatarioType, DestinatarioId, PushToken, Piattaforma, DisattivatoAt?.

---

## 5. Fase 3 – Modulo Autenticazione (`GoCare.Auth`)

> Progetto separato. Espone endpoint pubblici (senza header) e genera l'identità che il modulo Business consuma.

### 5.1 Fondamenta del modulo
- [ ] `AuthDbContext`, configurazioni EF, prima migrazione.
- [ ] `PasswordService` con algoritmo di hashing moderno (Argon2/PBKDF2).
- [ ] `TokenService`: access token breve con claim `sub`, `role`, `person_id`/`association_id`, `email_verified`; refresh token persistito e revocabile.
- [ ] Configurazione autenticazione JWT nell'host + policy di autorizzazione (`Ruolo=Utente`, `Ruolo=Associazione`, `AssociazioneAccreditata`).
- [ ] Rate limiting su login e forgot-password.
- [ ] `DependencyInjection.AddAuthModule`: registra `DbContext`, i Service, i validator, l'`ApplicationPart` dei controller.

### 5.2 Controller e metodi di Service

| Controller · action | Route | Service · metodo | UC |
|---|---|---|---|
| `AuthController.RegisterUser` | `POST /auth/register/user` | `AuthService.RegisterUserAsync` – valida, unicità e-mail, hash password, crea `Account` `NonVerificato`, pubblica `AccountRegistered`, genera token, invia e-mail | 10.1 / CG-01 |
| `AuthController.RegisterAssociation` | `POST /auth/register/association` | `AuthService.RegisterAssociationAsync` – come sopra; dopo verifica resta `InAttesaAccreditamento` (PA-11) | 10.1 / AS-01 |
| `EmailVerificationController.Verify` | `GET /auth/verify-email/:token` | `AccountService.VerifyEmailAsync` – valida token, `email_verified=true`, porta ad `Attivo` (o `InAttesaAccreditamento`) | 10.1 |
| `EmailVerificationController.Resend` | `POST /auth/verify-email/resend` | `AccountService.ResendVerificationAsync` | 10.1 |
| `AuthController.Login` | `POST /auth/login` | `AuthService.LoginAsync` – verifica credenziali e stato, blocca `NonVerificato`, limita tentativi, emette access+refresh, restituisce ruolo | 10.2 / CG-02, AS-02 |
| `AuthController.Refresh` | `POST /auth/refresh` | `AuthService.RefreshAsync` – ruota il refresh token, revoca il precedente | 10.2 |
| `AuthController.Logout` | `POST /auth/logout` | `AuthService.LogoutAsync` – revoca refresh token / sessione | 10.2 |
| `PasswordController.Forgot` | `POST /auth/forgot-password` | `PasswordService.RequestResetAsync` – risposta **sempre neutra**; se l'account esiste genera `PasswordResetToken` monouso e invia e-mail | 10.3 |
| `PasswordController.Reset` | `POST /auth/reset-password` | `PasswordService.ResetAsync` – valida token, aggiorna password, invalida token e **tutte le sessioni attive** | 10.3 |
| `AccountController.ChangeEmail` | `POST /auth/change-email` | `AccountService.ChangeEmailAsync` – aggiorna e-mail di login solo dopo verifica del nuovo indirizzo | 8.2 |
| `AccountController.Delete` | `DELETE /auth/account` | `AccountService.DeleteAsync` – re-inserimento password; chiede al Business (evento con veto) se ci sono viaggi/trasporti futuri attivi; soft delete + anonimizzazione; invalida sessioni; e-mail di conferma | 10.4 / CG-05, AS-04 |

### 5.3 Contratti ed eventi (`GoCare.Auth.Contracts`)
- [ ] `IAccountReader` – stato/ruolo/email_verified per i controlli del Business senza toccare lo schema `auth`. Implementato in `GoCare.Auth` (legge `AuthDbContext`), registrato nell'host.
- [ ] Eventi: `AccountRegistered`, `AccountEmailVerified`, `AssociationAccredited`, `AccountDeleted`.

---

## 6. Fase 4 – Concerns trasversali nell'host (`GoCare.Api`)

- [ ] **Composizione**: `Program.cs` chiama `AddAuthModule` + `AddBusinessModule`, registra i due `ApplicationPart` dei controller, la pipeline JWT, `GlobalExceptionHandler`, `ValidationFilter` globale, Swagger.
- [ ] **Autorizzazione**: policy per ruolo + requirement `AssociazioneAccreditata`. I controlli a livello di risorsa ("è l'associazione assegnataria", "è amministratore del gruppo", "il richiedente può prenotare per l'assistito") stanno **nei Service**.
- [ ] **Validazione**: FluentValidation, un validator per DTO di Request; regole ricorrenti (data non nel passato, coerenza direzione↔orari, campi obbligatori).
- [ ] **Logging/telemetria**: Serilog strutturato, correlation id, OpenTelemetry opzionale.
- [ ] **E-mail transazionali**: `IEmailSender` + template (verifica, reset, esito richiesta, presa in carico, disdetta, mancata copertura, promemoria, esito modifica, eliminazione account). Dev → MailHog.
- [ ] **Push**: `IPushSender` (adapter FCM/APNs); `DevicesController` per i `DeviceToken`.
- [ ] **Job schedulati** (`GoCare.Business/Jobs/`, Hosted Service / Hangfire / Quartz):
  - [ ] `TripReminderJob` – promemoria viaggio del giorno successivo (UC 4.3 / 4.7).
  - [ ] `CoverageTimeoutJob` – richiesta senza accettazione entro soglia (PA-04) → `NonCoperta` + notifica UC 4.1.
  - [ ] `TokenCleanupJob` – pulizia token scaduti.
- [ ] **Concorrenza**: `row_version` su `TransportRequest`; `AssociationRequestService.AcceptAsync` implementa "prima accettazione vince" (UC 6.3).
- [ ] **Audit**: `AssociationRequestService.GetRequesterContactsAsync` scrive `ContactAccessLog` (UC 6.5 / PA-06); ogni transizione di stato registra l'autore.
- [ ] **Paginazione/sorting/filtri**: via `PageQuery` di `GoCare.Shared` in tutti i metodi di lista.

---

## 7. Fase 5 – Modulo Business: `Models/`, helper interni, `Infrastructure/`

- [ ] `Models/` + `Enums/` (vedi §4) + configurazioni EF in `Data/Configurations/` + prima migrazione di `BusinessDbContext`.
- [ ] **`Services/Internal/TripStateMachine.cs`** – funzione pura `CanTransition(current, target, direction) → (bool, motivo)`. Regole in §9. Nessuna dipendenza da EF: testabile in isolamento.
- [ ] **`Services/Internal/NotificationDispatcher.cs`** (`INotificationDispatcher`) – dato (destinatario, tipo, dati), crea la `Notification`, poi invia push e/o e-mail secondo le regole di canale (push per cambi stato intermedi; push + e-mail per eventi di esito). Usato dai Service che generano notifiche.
- [ ] **`Services/Internal/CoverageEvaluator.cs`** (`ICoverageEvaluator`) – decide se una richiesta è `NonCoperta` (tutte le associazioni hanno rifiutato **oppure** < soglia ore alla data). Usato da `DeclineAsync` e da `CoverageTimeoutJob`.
- [ ] **Port** (`Services/Internal/`): `IKmCalculator`, `IPushSender`, `IEmailSender`.
- [ ] **`Infrastructure/`**: implementazioni `KmCalculator`, `PushSender`, `EmailSender`, `AccountReaderClient` (consuma `GoCare.Auth.Contracts.IAccountReader`).
- [ ] **`Integrations/`**: consumer degli eventi Auth (`AccountRegisteredHandler` crea `Person`/`Association`; `AssociationAccreditedHandler`; `AccountDeletedHandler` anonimizza ed esce dai gruppi, con veto se ci sono viaggi futuri attivi).
- [ ] `DependencyInjection.AddBusinessModule`: registra `DbContext`, tutti i Service + helper interni, i validator, i job, gli handler di integrazione, l'`ApplicationPart` dei controller.

---

## 8. Fase 6 – Business: Controller + Service per Use Case

> Ogni voce: azione del controller → metodo di Service → cosa fa. L'accesso dati è nel Service, via `BusinessDbContext`.

### UC 1 – Richiedi trasporto — `TransportsController` / `ITransportService`
- [ ] `Create` — `POST /transports` → `CreateAsync`
  - Valida campi e data non nel passato; se beneficiario ≠ richiedente verifica appartenenza al gruppo cura.
  - Copia indirizzi sulla richiesta; se destinazione nuova e confermata crea `SavedDestination` (UC 1.6 / 8.4).
  - Valida coerenza direzione↔orari (UC 1.3); salva tipo viaggio e direzione; salva accompagnatori (UC 1.5).
  - `IKmCalculator` → `KmPrevisti`.
  - Crea `TransportRequest` `InAttesa` con doppio riferimento richiedente/beneficiario; calcola `TransportRequestRecipient` per area (PA-05).
  - `INotificationDispatcher` → notifica "nuova richiesta" alle associazioni (UC 4.7).

### UC 2 – Modifica trasporto — `TransportsController` + `TransportModificationsController`
- [ ] `UpdateDestination` — `PATCH /transports/:id/destination` → `TransportService.UpdateDestinationAsync`
- [ ] `UpdateSchedule` — `PATCH /transports/:id/schedule` → `TransportService.UpdateScheduleAsync`
- [ ] `UpdateCompanions` — `PUT /transports/:id/companions` → `TransportService.UpdateCompanionsAsync`
  - Ramo `InAttesa` → modifica diretta; ramo `Confermata` → crea `TransportModificationRequest` `InAttesaApprovazione` + notifica associazione (UC 4.8). Solo accompagnatori: aggiornamento diretto + notifica informativa se già accettato.
- [ ] `Approve` — `POST /transports/modification-requests/:id/approve` → `TransportModificationService.ApproveAsync` (applica la modifica, chiude la richiesta, notifica utente – UC 4.5)
- [ ] `Reject` — `POST /transports/modification-requests/:id/reject` → `TransportModificationService.RejectAsync` (viaggio invariato, notifica con opzioni "mantieni / annulla" – PA-15)
- [ ] `ListPendingForAssociation` — `GET /association/modification-requests` → `TransportModificationService.ListPendingAsync` (contatore + filtro – UC 7.3)

### UC 3 – Annulla trasporto
- [ ] `CancelByUser` — `POST /transports/:id/cancel` → `TransportService.CancelByUserAsync`
  - Soft delete (stato `Annullata`, resta nello storico), salva causale; se già preso in carico → notifica associazione (UC 4.9).
- [ ] `CancelByAssociation` — `POST /association/transports/:id/cancel` → `AcceptedTransportService.CancelAsync`
  - Causale obbligatoria; il viaggio torna `InAttesa` e rientra fra le pendenti delle altre associazioni ([DA VALIDARE] anticipo minimo); notifica utente (UC 4.6) + "cerca un'alternativa".

### UC 4 – Stato e notifiche — `NotificationsController` / `INotificationService`, `TripStatusController` / `ITripStatusService`, `DevicesController`
- [ ] `List` — `GET /notifications` → `NotificationService.ListAsync` (paginato, filtro letta/non letta)
- [ ] `MarkRead` / `MarkAllRead` — `POST /notifications/:id/read` · `POST /notifications/read-all` → `NotificationService.MarkReadAsync`
- [ ] `Counters` — `GET /notifications/counters` → `NotificationService.GetCountersAsync` (badge header §11.4)
- [ ] `RegisterDevice` / `UnregisterDevice` — `POST /devices` · `DELETE /devices/:id` → `NotificationService.RegisterDeviceAsync` / `UnregisterDeviceAsync`
- [ ] `Advance` — `POST /association/transports/:id/status` → `TripStatusService.AdvanceAsync`
  - Valida transizione con `TripStateMachine`; registra `TripStatusTransition`; notifica push al gruppo cura (UC 4.6); su `Concluso` chiude il viaggio e lo sposta negli storici.
- [ ] `GetTimeline` — `GET /transports/:id/status-timeline` → `TripStatusService.GetTimelineAsync` (UC 5.5)
- [ ] (Opzionale PA-07) `Suspend` — stato `SospesoImprevisto` + notifica immediata.

### UC 5 – "I miei viaggi" — `TransportsController` / `ITransportService`
- [ ] `ListMine` — `GET /transports?scope=upcoming|history` → `ListMineAsync` (viaggi utente + gruppi cura; ordinamento; paginazione storico)
- [ ] `GetDetail` — `GET /transports/:id` → `GetDetailAsync` (stato modifica + messaggio; contatti associazione solo dopo presa in carico; timeline stato)
- [ ] `GetAssociationPublicContacts` — `GET /associations/:id/contacts` → `ProfileService.GetPublicContactsAsync` (UC 5.3)

### UC 6 – Richieste pendenti, associazione — `AssociationRequestsController` / `IAssociationRequestService`
- [ ] `ListPending` — `GET /association/requests` → `ListPendingAsync` (solo `InAttesa` destinate all'associazione e non accettate; filtri lato server; km precalcolati; **dati minimi assistito** – PA-06)
- [ ] `GetDetail` — `GET /association/requests/:id` → `GetDetailAsync` (verifica disponibilità prima di mostrare le azioni)
- [ ] `Accept` — `POST /association/requests/:id/accept` → `AcceptAsync` (`InAttesa`→`Confermata`, assegnazione, **prima accettazione vince** con `row_version`, notifica UC 4.2)
- [ ] `Decline` — `POST /association/requests/:id/decline` → `DeclineAsync` (`TransportRequestRejection`; `ICoverageEvaluator` → se serve `NonCoperta` + UC 4.1)
- [ ] `GetRequesterContacts` — `GET /association/requests/:id/contacts` → `GetRequesterContactsAsync` (recapiti completi **solo dopo accettazione**; scrive `ContactAccessLog`)

### UC 7 – Trasporti accettati, associazione — `AcceptedTransportsController` / `IAcceptedTransportService`
- [ ] `List` — `GET /association/transports` → `ListAsync` (assegnati `Confermata`/in corso; filtri data/stato; evidenza modifiche pendenti)
- [ ] `GetDetail` — `GET /association/transports/:id` → `GetDetailAsync`
- [ ] `GetHistory` — `GET /association/history` → `GetHistoryAsync` (stati terminali; filtri periodo/esito; riepilogo numerico; dati conservati dopo soft delete)

### UC 8 – Profilo — `ProfilesController` / `IProfileService`, `SavedDestinationsController` / `ISavedDestinationService`
- [ ] `GetMine` — `GET /me/profile` → `GetMineAsync`
- [ ] `UpdateMine` — `PUT /me/profile` → `UpdateMineAsync` (valida telefono/e-mail; snapshot contatti sui viaggi già registrati; cambio e-mail di login delega ad Auth)
- [ ] `List` / `Create` / `Update` / `Delete` destinazioni — `GET|POST|PUT|DELETE /me/destinations[/:id]` → `SavedDestinationService.*` (l'eliminazione non tocca i viaggi già registrati)
- [ ] `GetAssociation` — `GET /association/profile` → `GetAssociationAsync` (sola lettura)
- [ ] `UpdateAssociation` — `PUT /association/profile` → `UpdateAssociationAsync` (almeno un telefono e una e-mail obbligatori; include `AreaOperativita` – PA-05; si riflette subito sui viaggi in carico)

### UC 9 – Gruppi caregiver-assistiti — `CareGroupsController` / `ICareGroupService`
- [ ] `Create` — `POST /care-groups` → `CreateAsync` (creatore = amministratore; nome obbligatorio e univoco per utente)
- [ ] `ListMine` — `GET /care-groups` → `ListMineAsync` (card con n. membri e prossimo viaggio; stato vuoto)
- [ ] `GetDetail` — `GET /care-groups/:id` → `GetDetailAsync`
- [ ] `InviteMember` — `POST /care-groups/:id/members` → `InviteMemberAsync` (solo admin; invito con ruolo; stato `InAttesa`; notifica/e-mail; ingresso previa accettazione – PA-08)
- [ ] `AcceptInvitation` — `POST /care-groups/invitations/:token/accept` → `AcceptInvitationAsync`
- [ ] `RemoveMember` — `DELETE /care-groups/:id/members/:personId` → `RemoveMemberAsync` (solo admin; blocca/conferma se viaggi futuri confermati)
- [ ] `Delete` — `DELETE /care-groups/:id` → `DeleteAsync` (solo admin; soft delete; bloccato con viaggi futuri confermati; notifica ai membri)
- [ ] `ListBookings` — `GET /care-groups/:id/bookings` → `ListBookingsAsync` ("prenotato da X per Y"; separazione prossimi/storico; filtro per assistito; visibilità solo ai membri)

### PA-11 – Accreditamento — `Admin/AssociationAccreditationController`
- [ ] `Accredit` — `POST /admin/associations/:id/accredit` → `ProfileService.AccreditAsync` (pubblica `AssociationAccredited`, sblocca l'operatività)

---

## 9. Fase 7 – `TripStateMachine` (formalizzazione §12.7)

- [ ] **Stati della richiesta:** `InAttesa` → `Confermata` → `InEsecuzione` → `Conclusa`.
  Terminali alternativi: `Rifiutata`/`NonCoperta`, `Annullata`.
- [ ] **Stati di esecuzione** (aggiornati via account associazione): `NonPresoInCarico` → `PresoInCarico` → `InArrivo` → `InVisita` → `InRitorno` → `Concluso`.
- [ ] **Regole (in `Services/Internal/TripStateMachine.cs`, testate una per una):**
  - Sequenza obbligata: nessun salto avanti, nessun ritorno indietro.
  - `SoloAndata` (ricovero, trasferimento) → salta `InRitorno`.
  - `SoloRitorno` (dimissione) → salta `InVisita`.
  - Ogni transizione registra stato + timestamp + operatore (label).
  - `Concluso` chiude il viaggio, lo rende non modificabile, lo sposta negli storici di utente e associazione.
  - Notifiche cambio stato: solo push; e-mail solo per eventi di esito.
- [ ] (PA-07, opzionale v0) `SospesoImprevisto` durante il viaggio: notifica immediata, viaggio resta in carico all'associazione.

---

## 10. Fase 8 – Sicurezza, privacy e conformità

- [ ] Accettazione informativa privacy in registrazione (UC 10.1) + versione salvata.
- [ ] Politica esposizione contatti (PA-06): dati minimi in valutazione, recapiti completi post-accettazione, `ContactAccessLog`.
- [ ] Anonimizzazione post-eliminazione account (PA-14): rimuovere dati personali, mantenere dati di servizio del viaggio nello storico associazione.
- [ ] Accreditamento associazioni (PA-11): stato `InAttesaAccreditamento`, endpoint admin, gate operatività.
- [ ] Hardening: HTTPS obbligatorio, header di sicurezza, CORS ristretto all'app, secret fuori dal repo, rotazione chiavi JWT, hashing robusto, rate limiting login/forgot-password, lockout progressivo.
- [ ] Autorizzazione a livello di risorsa in ogni Service (proprietà viaggio, appartenenza gruppo, associazione assegnataria/destinataria).

---

## 11. Fase 9 – Testing

- [ ] **Unit test dei Service** con `BusinessDbContext`/`AuthDbContext` InMemory o SQLite + fake (`IEmailSender`, `IPushSender`, `IKmCalculator`, `IClock`, `IAccountReader`).
- [ ] **Unit test `TripStateMachine`** (tutte le direzioni) e `CoverageEvaluator`.
- [ ] **Unit test validator** FluentValidation.
- [ ] **Integration test** (`WebApplicationFactory` + Testcontainers PostgreSQL) sui flussi §12:
  - 12.1 Registrazione (+ e-mail duplicata, token scaduto, login non verificato).
  - 12.2 Login (+ credenziali errate, lockout).
  - 12.3 Recupero password (risposta neutra, token monouso, invalidazione sessioni).
  - 12.4 Richiesta → accettazione (+ rifiuto, doppia accettazione concorrente, timeout → non coperta).
  - 12.5 Modifica trasporto (ramo in attesa vs confermato, approva/rifiuta).
  - 12.6 Annullamento utente e disdetta associazione (rientro fra le pendenti).
  - 12.7 Ciclo di stato completo per ogni direzione di viaggio.
  - 12.8 Gestione gruppo cura (invito/accettazione, rimozione con viaggi futuri, eliminazione bloccata).
- [ ] Test dei job (`TripReminderJob`, `CoverageTimeoutJob`) con clock controllato.
- [ ] Test di autorizzazione negativi (accesso a risorse altrui).
- [ ] Contract test sugli eventi di integrazione Auth ↔ Business.

---

## 12. Fase 10 – Osservabilità, configurazione, delivery

- [ ] Health check (`/health/live`, `/health/ready`) con DB e dipendenze esterne.
- [ ] Logging strutturato + correlation id + log degli eventi di dominio chiave.
- [ ] Metriche: richieste create, tasso di copertura, tempo medio di presa in carico, notifiche inviate/fallite.
- [ ] Configurazione per ambiente con `IOptions` validati all'avvio.
- [ ] Dockerfile dell'API; `docker-compose` per stack locale completo.
- [ ] CI: build, `dotnet format --verify`, test, migrazioni verificate, pubblicazione artefatto.
- [ ] Migrazioni in produzione via script SQL applicati in deploy (non auto-migrate).
- [ ] Backup DB e retention.

---

## 13. Fase 11 – Dati demo e onboarding sviluppo

- [ ] Seeder: 1 associazione accreditata, 2 caregiver, 2 assistiti, 1 gruppo cura, richieste in vari stati.
- [ ] README per avviare l'ambiente locale in un comando.
- [ ] Collezione richieste (`.http` / Bruno / Postman) allineata alla tabella endpoint.

---

## 14. Tabella route backend (sintesi)

| Area | Metodo + path | Ruolo | UC |
|---|---|---|---|
| Auth | `POST /auth/register/user` | Pubblico | 10.1 |
| Auth | `POST /auth/register/association` | Pubblico | 10.1 |
| Auth | `GET /auth/verify-email/:token` | Pubblico | 10.1 |
| Auth | `POST /auth/verify-email/resend` | Pubblico | 10.1 |
| Auth | `POST /auth/login` | Pubblico | 10.2 |
| Auth | `POST /auth/refresh` | Pubblico | 10.2 |
| Auth | `POST /auth/logout` | Autenticato | 10.2 |
| Auth | `POST /auth/forgot-password` | Pubblico | 10.3 |
| Auth | `POST /auth/reset-password` | Pubblico | 10.3 |
| Auth | `POST /auth/change-email` | Autenticato | 8.2 |
| Auth | `DELETE /auth/account` | Autenticato | 10.4 |
| Business | `POST /transports` | Caregiver/Assistito | 1.1–1.6 |
| Business | `GET /transports` | Caregiver/Assistito | 5.1, 5.4 |
| Business | `GET /transports/:id` | Caregiver/Assistito | 2.x, 3.1, 5.2, 5.5 |
| Business | `PATCH /transports/:id/destination` | Caregiver/Assistito | 2.1 |
| Business | `PATCH /transports/:id/schedule` | Caregiver/Assistito | 2.2 |
| Business | `PUT /transports/:id/companions` | Caregiver/Assistito | 2.3 |
| Business | `POST /transports/:id/cancel` | Caregiver/Assistito | 3.1 |
| Business | `GET /transports/:id/status-timeline` | Caregiver/Assistito | 5.5 |
| Business | `POST /transports/modification-requests/:id/approve` | Associazione | 2.3 |
| Business | `POST /transports/modification-requests/:id/reject` | Associazione | 2.3 |
| Business | `GET /association/modification-requests` | Associazione | 7.3 |
| Business | `GET /association/requests` | Associazione | 6.1, 6.2 |
| Business | `GET /association/requests/:id` | Associazione | 6.2 |
| Business | `POST /association/requests/:id/accept` | Associazione | 6.3 |
| Business | `POST /association/requests/:id/decline` | Associazione | 6.4 |
| Business | `GET /association/requests/:id/contacts` | Associazione | 6.5 |
| Business | `GET /association/transports` | Associazione | 7.1, 7.3 |
| Business | `GET /association/transports/:id` | Associazione | 7.1, 2.3 |
| Business | `POST /association/transports/:id/status` | Associazione/Operatore | 4.10 |
| Business | `POST /association/transports/:id/cancel` | Associazione | 3.2 |
| Business | `GET /association/history` | Associazione | 7.2 |
| Business | `GET /association/profile` / `PUT /association/profile` | Associazione | 8.5, 8.6 |
| Business | `GET|PUT /me/profile` | Caregiver/Assistito | 8.1–8.3 |
| Business | `GET|POST|PUT|DELETE /me/destinations[/:id]` | Caregiver/Assistito | 8.4 |
| Business | `GET /associations/:id/contacts` | Caregiver/Assistito | 5.3 |
| Business | `POST /care-groups` | Caregiver/Assistito | 9.1 |
| Business | `GET /care-groups` | Caregiver/Assistito | 9.5 |
| Business | `GET /care-groups/:id` | Caregiver/Assistito | 9.5 |
| Business | `POST /care-groups/:id/members` | Amministratore gruppo | 9.2 |
| Business | `POST /care-groups/invitations/:token/accept` | Caregiver/Assistito | 9.2 |
| Business | `DELETE /care-groups/:id/members/:personId` | Amministratore gruppo | 9.3 |
| Business | `DELETE /care-groups/:id` | Amministratore gruppo | 9.4 |
| Business | `GET /care-groups/:id/bookings` | Membro gruppo | 9.6 |
| Business | `GET /notifications` | Tutti | UC 4 |
| Business | `POST /notifications/:id/read` / `read-all` | Tutti | UC 4 |
| Business | `GET /notifications/counters` | Tutti | §11.4 |
| Business | `POST /devices` / `DELETE /devices/:id` | Tutti | UC 4 |
| Admin | `POST /admin/associations/:id/accredit` | Admin GoCare | PA-11 |

---

## 15. Punti aperti che bloccano/condizionano il backend

Da chiudere **prima** di implementare i Service indicati (rif. §13 del documento):

| ID | Punto | Impatto sul backend | Service condizionati |
|---|---|---|---|
| PA-01 | Modello a richiesta vs a slot | Strutturale: entità e flusso principale | UC 1, 4, 5, 6 |
| PA-03 | Chi aggiorna lo stato e come accede | Modello identità operatore | `TripStatusService` |
| PA-04 | Soglia "non coperta" | Regola di `CoverageEvaluator` + `CoverageTimeoutJob` | UC 4.1, 6.4 |
| PA-05 | Visibilità richieste per area | `AreaOperativita` + `TransportRequestRecipient` | UC 6, 8.5 |
| PA-06 | Esposizione dati contatto assistito | Dati minimi vs completi + audit | UC 6.2, 6.5 |
| PA-07 | Gestione guasti in corsa | Stato `SospesoImprevisto` | `TripStatusService` |
| PA-08 | Ruoli/permessi nel gruppo cura | Ruolo admin + invito con accettazione | UC 9 |
| PA-11 | Accreditamento associazioni | Stato account + gate operatività | Auth, Admin |
| PA-14 | Retention dati post-eliminazione | Logica di anonimizzazione | `AccountService`, `AccountDeletedHandler` |
| PA-15 | Esito rifiuto richiesta di modifica | Opzioni nella notifica di esito | UC 2.3, 4.5 |

Fuori scope v0 confermati: trasporti ricorrenti (PA-09), geolocalizzazione mezzo (PA-10), trasporto sociale (PA-13).

---

## 16. Ordine di esecuzione consigliato (milestone)

1. **M0 – Fondamenta**: Fasi 0, 1, 2 (solution, `GoCare.Shared`, i due `DbContext` + migrazione iniziale, docker-compose).
2. **M1 – Autenticazione**: Fase 3 + parte della Fase 4 (JWT, e-mail, `GlobalExceptionHandler`, `ValidationFilter`). Deliverable: registrazione → verifica → login → reset password end-to-end.
3. **M2 – Business: Models + helper + profili**: Fasi 5 e 7 (`TripStateMachine`), UC 8, handler di integrazione.
4. **M3 – Gruppi cura**: UC 9 completo (dopo PA-08).
5. **M4 – Ciclo richiesta trasporto**: UC 1 → UC 6 → UC 7 (dopo PA-01, PA-04, PA-05, PA-06).
6. **M5 – Stato viaggio e notifiche**: `TripStatusService` + UC 4 (centro notifiche, push, e-mail, `TripReminderJob`, `CoverageTimeoutJob`).
7. **M6 – Modifiche e annullamenti**: UC 2 e UC 3 completi (dopo PA-15).
8. **M7 – Hardening**: Fase 8, Fase 10, suite di integrazione §11, seed demo.

---

## 17. Definition of Done del backend v0

- [ ] Tutti gli endpoint della tabella §14 implementati e coperti da test.
- [ ] Tutti i flussi §12 del documento verificati da test di integrazione.
- [ ] `TripStateMachine` conforme alle regole §12.7 per ogni direzione di viaggio.
- [ ] Notifiche push + e-mail secondo le regole di canale.
- [ ] Job schedulati attivi (`TripReminderJob`, `CoverageTimeoutJob`, `TokenCleanupJob`).
- [ ] Moduli Auth e Business isolati: `GoCare.Business` referenzia solo `GoCare.Auth.Contracts`; due `DbContext`, due set di migrazioni.
- [ ] Ogni controller sottile (solo HTTP); tutta la logica nei Service; DTO e Model mai confusi.
- [ ] Punti aperti a impatto alto chiusi o parcheggiati con un default documentato.
