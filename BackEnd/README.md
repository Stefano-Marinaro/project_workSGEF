# GoCare BackEnd — setup locale

Guida rapida per clonare, buildare ed eseguire il progetto. Per il contesto funzionale/architetturale vedi `Project_GoCare.md` e `PIANO_BACKEND_GoCare.md`.

## Prerequisiti

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) in esecuzione su `localhost:5432`
- EF Core tools:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## 1. Clona ed entra nella cartella

```bash
git clone <url-repo>
cd BackEnd
```

## 2. Crea i due database

Il progetto usa **due database separati** (uno per l'autenticazione, uno per il dominio):

```bash
psql -U postgres -c "CREATE DATABASE gocare_auth;"
psql -U postgres -c "CREATE DATABASE gocare_business;"
```

Le connection string di default (in `src/GoCare.Api/appsettings.json`) assumono utente `postgres`, password `postgres`, su `localhost:5432`. Se il tuo PostgreSQL locale ha credenziali diverse, sovrascrivile in `src/GoCare.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "AuthDb": "Host=localhost;Port=5432;Database=gocare_auth;Username=...;Password=...",
    "BusinessDb": "Host=localhost;Port=5432;Database=gocare_business;Username=...;Password=..."
  }
}
```

## 3. Applica le migrazioni

Le migrazioni sono già nel repo (`src/GoCare.Application/Data/Auth/Migrations`, `.../Data/Business/Migrations`) — qui si **applicano**, non si generano:

```bash
dotnet ef database update --context AuthDbContext --project src/GoCare.Application --startup-project src/GoCare.Api

dotnet ef database update --context BusinessDbContext --project src/GoCare.Application --startup-project src/GoCare.Api
```

## 4. Build e avvio

```bash
dotnet build
dotnet run --project src/GoCare.Api
```

Oppure apri `GoCare.slnx` in Visual Studio e premi F5.

Swagger: <https://localhost:7076/swagger> (o <http://localhost:5161/swagger>).

---

## Problemi comuni

**`dotnet ef` fallisce con "Could not load assembly" / errore di firma / criterio bloccato** — su Windows 11 con **Controllo app intelligente** (Smart App Control) attivo, i comandi EF possono essere bloccati perché caricano una DLL compilata in locale, non firmata. Verifica in Impostazioni → Sicurezza di Windows → Protezione da app e browser. Se è "Attivo", lancia i comandi `dotnet ef` da WSL2 invece che da Windows nativo (build/run restano invariati, non ne risentono).
