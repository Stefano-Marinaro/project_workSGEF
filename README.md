# GoCare

Progetto composto da:

- `BackEnd/` — API ASP.NET Core (.NET 10) + PostgreSQL
- `frontend/` — app Expo / React Native (in Docker viene servita la versione **web**)

## Avvio con Docker (consigliato)

Unico prerequisito: [Docker Desktop](https://www.docker.com/products/docker-desktop/) (o Docker Engine + Compose).

```bash
cp .env.example .env      # poi inserisci le chiavi (GraphHopper, Stripe, ...) se ti servono
docker compose up --build
```

| Servizio  | Indirizzo                                 |
|-----------|-------------------------------------------|
| Frontend  | http://localhost:8081                     |
| Backend   | http://localhost:5161 — Swagger su `/swagger` |
| Database  | `localhost:5433` (utente/password `postgres`, db `gocare`) |

Al primo avvio il backend applica da solo le migrazioni e importa i comuni ISTAT.

### ⚠️ Dopo ogni `git pull`: ricostruire le immagini

I container **non si aggiornano da soli** quando cambia il codice: girano su un'immagine costruita
in un momento preciso. Dopo ogni aggiornamento del repository rilancia:

```bash
git pull
docker compose up --build
```

Docker ricostruisce solo ciò che è cambiato (es. solo il frontend) e i dati del database restano.
Per ricostruire un solo servizio: `docker compose up --build frontend`.

I file Docker vanno modificati solo se si aggiunge una nuova variabile di configurazione/chiave
(`docker-compose.yml` + `.env.example`), si cambia versione di Node/.NET o si aggiunge un nuovo servizio.
Nuove schermate, librerie o migrazioni entrano automaticamente al prossimo `--build`.

Comandi utili:

```bash
docker compose down        # ferma tutto (i dati del DB restano nel volume)
docker compose down -v     # ferma tutto e CANCELLA il database
docker compose logs -f backend
```

### E-mail

Senza credenziali SMTP le e-mail **non partono**: il backend le scrive nel log
(`docker compose logs backend`), compresi i link di verifica, reset password e invito associazione.
Per l'invio vero compila le variabili `SMTP_*` nel file `.env` (istruzioni per Mailtrap e Gmail in `.env.example`);
senza Docker, la sezione `Smtp` di `appsettings.Local.json` (vedi `appsettings.Local.json.example`).

Note:

- Il frontend in Docker è la versione web. Per provare l'app sul telefono con Expo Go si continua a usare `npx expo start` dentro `frontend/` (il backend può restare in Docker: usa l'IP di LAN del PC e la porta 5161).
- Per aprire la versione web da un altro dispositivo della rete, imposta `API_PUBLIC_URL=http://<IP_LAN>:5161` nel file `.env` e rilancia `docker compose up --build`.

## Avvio senza Docker

Vedi `BackEnd/README.md` per il backend e `npx expo start` in `frontend/` per l'app.
