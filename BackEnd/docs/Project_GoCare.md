# Project GoCare

*Convenzioni di lettura: i punti ancora da decidere sono segnalati in corsivo con l’etichetta [DA VALIDARE] e ripresi tutti nella sezione “Cosa manca / Punti aperti”.*

---

# 1. Target, problema e soluzione

## 1.1 Target

Persone fragili che hanno bisogno del supporto di associazioni di volontariato per il trasporto, i loro caregiver (familiari o assistenti che gestiscono la prenotazione per conto dell’assistito) e le associazioni di volontariato e soccorso che erogano il servizio di trasporto.

## 1.2 Problema

Spesso le persone non hanno chi possa accompagnarle a fare visite mediche, perché non automunite, non autosufficienti o sole.

Problemi collegati:

- Le persone dimenticano gli appuntamenti e le visite programmate.
- Una volta che il paziente è preso in carico, le famiglie devono fare ripetute chiamate all’associazione per sapere lo stato del trasporto.
- In caso di annullamento o modifica del viaggio deve essere trovata una soluzione alternativa, per non lasciare l’assistito senza copertura.
- Le associazioni ricevono le richieste su canali non strutturati (telefono, messaggi) e non hanno una vista unica delle richieste pendenti e dei trasporti già presi in carico.

## 1.3 Soluzione

- Offrire un luogo digitale di prenotazione viaggi in contatto diretto con le associazioni di volontariato e soccorso.
- Offrire funzionalità di registrazione e notifica dei prossimi trasporti.
- Offrire la visualizzazione in tempo reale dello stato del viaggio per familiari e caregiver a distanza (non preso in carico, preso in carico, in arrivo, in visita, in ritorno, concluso).
- Offrire una soluzione alternativa in caso di mancata copertura, anche coinvolgendo un’altra associazione.
- Offrire alle associazioni una dashboard operativa unica per richieste pendenti, trasporti accettati e storico.

---

# 2. Use Cases

Elenco dei macro Use Case e delle relative sotto-voci. Rispetto all’originale la numerazione è stata resa continua e le sovrapposizioni fra Use Case diversi sono state risolte (vedi note).

## UC 1 – RICHIEDI TRASPORTO

- 1.1 Richiedere un trasporto (data, orario, luogo di partenza, destinazione).
- 1.2 Scegliere la direzione del viaggio (andata e ritorno, solo andata, solo ritorno).
- 1.3 Scegliere destinazioni diverse dal domicilio abituale.
- 1.4 Indicare eventuali accompagnatori.
- 1.5 Indicare il tipo/obiettivo del viaggio (visita medica, ricovero, dimissione, trasferimento).
- 1.6 Richiedere un trasporto per un assistito del proprio “gruppo cura” (prenotazione per conto terzi).
- 1.7 Indicare per il ritorno una destinazione diversa da quella di partenza dell’andata.

## UC 2 – MODIFICA TRASPORTO

- 2.1 Richiesta di modifica del trasporto (data e orario, destinazione dell’andata, destinazione del ritorno).
- 2.2 Aggiungere / rimuovere / modificare l’accompagnatore su una richiesta già inviata.
- 2.3 Approvazione o rifiuto della richiesta di modifica da parte dell’associazione.
- 2.4 Ritiro della richiesta di modifica ancora in attesa di approvazione, da parte di chi l’ha inviata.

## UC 3 – ANNULLA TRASPORTO

- 3.1 Annullamento del trasporto da parte di caregiver/assistito.
- 3.2 Annullamento del trasporto da parte dell’associazione.
- 3.3 Specifica della causale di annullamento.

## UC 4 – STATO E NOTIFICHE

Notifiche e stato lato caregiver/assistito:

- 4.1 Stato del trasporto in tempo reale (non preso in carico, preso in carico, in arrivo, in visita, in ritorno, concluso).
- 4.2 Notifica push al cambio di stato del viaggio.
- 4.3 Notifica push per viaggio imminente (giorno precedente).
- 4.4 Notifica di esito della richiesta: presa in carico, rifiuto o mancata copertura.
- 4.5 Notifica di esito della richiesta di modifica (approvata / rifiutata).
- 4.6 Notifica di annullamento del trasporto da parte dell’associazione.

Notifiche lato associazione:

- 4.7 Notifica di nuova richiesta di trasporto.
- 4.8 Notifica di richiesta di modifica di un trasporto.
- 4.9 Notifica di annullamento di un trasporto da parte dell’utente.
- 4.11 Notifica di modifica degli accompagnatori su un trasporto già accettato.

Gestione operativa dello stato:

- 4.10 Aggiornamento dello stato del viaggio da parte dell’associazione/operatore.

## UC 5 – VISUALIZZAZIONE “I MIEI VIAGGI”

- 5.1 Visualizzazione di tutte le richieste di trasporto effettuate dall’utente, con data, orario e destinazioni.
- 5.2 Visualizzazione dello stato della richiesta (in attesa, confermata, in esecuzione, conclusa, non coperta, annullata).
- 5.3 Visualizzazione del nome dell’associazione che ha preso in carico il viaggio, con i relativi contatti.
- 5.4 Visualizzazione dello storico dei viaggi conclusi.
- 5.5 Visualizzazione dello stato di avanzamento del viaggio in corso.

## UC 6 – VISUALIZZAZIONE E GESTIONE RICHIESTE PENDENTI (associazione)

- 6.1 Dashboard con le richieste di trasporto di caregiver/assistiti destinate all’associazione, con filtri (data, orario, destinazione, tipo di viaggio).
- 6.2 Visualizzazione del dettaglio della richiesta (informazioni personali, data, ora, destinazioni, accompagnatori).
- 6.3 Accettazione di una richiesta di trasporto pendente.
- 6.4 Rifiuto / declino di una richiesta di trasporto pendente.
- 6.5 Visualizzazione dei contatti della persona che ha prenotato il trasporto.

## UC 7 – VISUALIZZAZIONE TRASPORTI ACCETTATI (associazione)

- 7.1 Dashboard con tutti i trasporti accettati e le relative informazioni complete.
- 7.2 Storico dei viaggi conclusi dall’associazione.
- 7.3 Evidenza dei trasporti con richiesta di modifica pendente.

## UC 8 – VISUALIZZAZIONE E GESTIONE INFORMAZIONI PROFILO

Profilo caregiver / assistito:

- 8.1 Visualizzazione delle informazioni personali.
- 8.2 Modifica delle informazioni personali.
- 8.3 Visualizzazione e modifica dei dati di contatto.
- 8.4 Gestione delle destinazioni salvate (elenco, aggiunta, modifica, eliminazione).

Profilo associazione:

- 8.5 Visualizzazione delle informazioni dell’associazione, comprese le province coperte.
- 8.6 Aggiunta e modifica delle informazioni di contatto dell’associazione.

## UC 9 – GESTIONE GRUPPI CAREGIVER-ASSISTITI

- 9.1 Creazione di un “gruppo cura”.
- 9.2 Aggiunta di un assistito o di un caregiver al gruppo, tramite invito da accettare.
- 9.3 Rimozione di un assistito o di un caregiver dal gruppo.
- 9.4 Eliminazione di un gruppo cura.
- 9.5 Visualizzazione dei gruppi di cui si fa parte e della pagina di gruppo.
- 9.6 Visualizzazione delle prenotazioni del gruppo, con evidenza di chi ha prenotato e per chi.
- 9.7 Rifiuto dell’invito al gruppo da parte dell’invitato.
- 9.8 Revoca di un invito non ancora accettato, da parte dell’amministratore.
- 9.9 Modifica del ruolo di un membro del gruppo (ruolo nel gruppo e ruolo amministrativo).

## UC 10 – ACCOUNT

- 10.1 Creazione dell’account (caregiver/assistito o associazione).
- 10.2 Login.
- 10.3 Recupero delle credenziali in caso di smarrimento.
- 10.4 Eliminazione dell’account.
- 10.5 Accreditamento dell’associazione da parte di GoCare.

---

# 3. User Stories e mappatura sugli Use Case

Le user story sono riportate con il riferimento allo Use Case (e alla sotto-voce) di appartenenza. La colonna “Note” segnala le mappature corrette rispetto all’originale e le story aggiunte per coprire sotto-voci di Use Case rimaste scoperte.

## 3.1 Stories caregiver / assistito

| ID | User story | UC | Note |
|---|---|---|---|
| CG-01 | Come caregiver/assistito voglio potermi registrare in GoCare. | 10.1 | Confermata |
| CG-02 | Come caregiver/assistito voglio poter effettuare il login. | 10.2 | NUOVO – scorporata da CG-01 |
| CG-03 | Come caregiver/assistito voglio poter modificare i miei dati personali e di contatto. | 8.2, 8.3 | Confermata |
| CG-04 | Come caregiver/assistito voglio poter recuperare la password in caso di smarrimento. | 10.3 | Confermata |
| CG-05 | Come caregiver/assistito voglio poter eliminare il mio account. | 10.4 | Confermata |
| CG-06 | Come caregiver voglio poter avere più assistiti a carico. | 9.1, 9.2 | Confermata |
| CG-07 | Come caregiver/assistito voglio poter creare un “gruppo cura”. | 9.1 | Confermata |
| CG-08 | Come caregiver/assistito voglio poter aggiungere un assistito/caregiver al gruppo. | 9.2 | Confermata |
| CG-09 | Come caregiver/assistito voglio poter rimuovere un assistito/caregiver dal gruppo. | 9.3 | Confermata |
| CG-10 | Come caregiver/assistito voglio poter eliminare un “gruppo cura”. | 9.4 | Confermata |
| CG-11 | Come caregiver/assistito voglio poter visualizzare i gruppi a cui appartengo. | 9.5 | Confermata |
| CG-12 | Come caregiver/assistito voglio poter visualizzare le prenotazioni effettuate nel gruppo. | 9.6 | Confermata |
| CG-13 | Come assistito voglio poter registrare i miei futuri trasporti. | 1.1 | Confermata |
| CG-14 | Come caregiver voglio poter registrare il prossimo trasporto del mio assistito. | 1.1, 1.6 | Confermata |
| CG-15 | Come caregiver/assistito voglio poter scegliere la direzione del viaggio (andata e ritorno, solo andata, solo ritorno). | 1.2 | NUOVA – UC 1.2 era senza story |
| CG-16 | Come caregiver/assistito voglio poter indicare il tipo di viaggio (visita, ricovero, dimissione, trasferimento). | 1.5 | NUOVA – tipologia presente nel doc ma senza story |
| CG-17 | Come caregiver/assistito voglio poter scegliere una destinazione diversa dal mio domicilio abituale. | 1.3, 2.1 | Confermata |
| CG-18 | Come caregiver/assistito voglio poter salvare, modificare ed eliminare le mie destinazioni ricorrenti. | 8.4 | NUOVA – era solo nel dettaglio SPRINT (voce 2.2) |
| CG-19 | Come caregiver/assistito voglio poter indicare un accompagnatore per il viaggio. | 1.4 | Confermata |
| CG-20 | Come caregiver/assistito voglio poter aggiungere/modificare/rimuovere l’accompagnatore anche dopo aver creato la richiesta. | 2.2 | Confermata |
| CG-21 | Come caregiver/assistito voglio poter modificare la data del mio viaggio in caso di imprevisto. | 2.1 | Confermata |
| CG-22 | Come caregiver/assistito voglio sapere se la mia richiesta di modifica è stata accettata o rifiutata. | 4.5, 5.2 | Confermata |
| CG-23 | Come caregiver/assistito voglio poter annullare il viaggio. | 3.1 | Confermata |
| CG-24 | Come caregiver/assistito voglio poter indicare il motivo dell’annullamento. | 3.3 | NUOVA – UC 3 prevedeva la causale solo lato associazione |
| CG-25 | Come caregiver/assistito voglio sapere se il viaggio non può essere coperto. | 4.4 | Confermata |
| CG-26 | Come caregiver/assistito voglio ricevere notifica quando il viaggio è preso in carico dall’associazione. | 4.4 | Confermata |
| CG-27 | Come caregiver/assistito voglio essere avvisato tempestivamente se un viaggio già preso in carico viene annullato dall’associazione, così da poter cercare un’alternativa. | 4.6 | Confermata |
| CG-28 | Come caregiver/assistito voglio poter ricevere notifica dei prossimi trasporti programmati. | 4.3 | Confermata |
| CG-29 | Come caregiver voglio poter vedere lo stato in tempo reale del viaggio dell’assistito preso in carico. | 4.1, 4.2, 5.5 | Confermata – la posizione GPS è esclusa: vedi Punti aperti |
| CG-30 | Come caregiver/assistito voglio poter vedere lo stato della mia richiesta (in attesa, confermata, in esecuzione, conclusa, non coperta, annullata). | 5.2 | NUOVA – UC 5.2 era senza story dedicata |
| CG-31 | Come caregiver/assistito voglio poter vedere i contatti dell’associazione che ha preso in carico il mio viaggio, per poterla contattare in caso di necessità. | 5.3 | Confermata |
| CG-32 | Come caregiver/assistito voglio poter avere uno storico delle visite fatte. | 5.4 | Confermata |
| CG-33 | Come caregiver/assistito voglio poter registrare un trasporto ricorrente (es. dialisi ogni martedì), così da non dover ripetere la richiesta ogni volta. | 1.1 | RINVIATA a v1 – vedi Punti aperti |
| CG-34 | Come caregiver/assistito voglio poter ritirare una richiesta di modifica ancora in attesa di approvazione. | 2.4 | NUOVA – UC 2.4 era senza story |
| CG-35 | Come caregiver/assistito voglio poter rifiutare un invito a un gruppo cura. | 9.7 | NUOVA – l’invito prevedeva solo l’accettazione |
| CG-36 | Come amministratore del gruppo voglio poter revocare un invito che non è ancora stato accettato. | 9.8 | NUOVA – UC 9.8 era senza story |
| CG-37 | Come amministratore del gruppo voglio poter cambiare il ruolo di un membro, anche promuoverlo ad amministratore. | 9.9 | NUOVA – UC 9.9 era senza story |
| CG-38 | Come caregiver/assistito voglio poter indicare per il ritorno una destinazione diversa da quella da cui sono partito. | 1.7 | NUOVA – UC 1.7 era senza story |

## 3.2 Stories associazione

| ID | User story | UC | Note |
|---|---|---|---|
| AS-01 | Come associazione voglio potermi registrare in GoCare. | 10.1 | Confermata |
| AS-02 | Come associazione voglio poter effettuare il login. | 10.2 | NUOVA – scorporata da AS-01 |
| AS-03 | Come associazione voglio poter recuperare la password in caso di smarrimento. | 10.3 | Confermata |
| AS-04 | Come associazione voglio poter eliminare il mio account. | 10.4 | Confermata |
| AS-05 | Come associazione voglio poter visualizzare e modificare i dati, i contatti e le province coperte dalla mia associazione. | 8.5, 8.6 | Confermata |
| AS-06 | Come associazione voglio ricevere notifica quando un utente invia una richiesta di trasporto. | 4.7 | Confermata |
| AS-07 | Come associazione voglio poter vedere tutte le richieste di trasporto pendenti a me destinate nella mia dashboard, con filtri (data, orario, destinazione, tipo di viaggio). | 6.1, 6.2 | Confermata |
| AS-08 | Come associazione voglio poter accettare una richiesta di trasporto pendente. | 6.3 | Confermata |
| AS-09 | Come associazione voglio poter rifiutare/declinare una richiesta di trasporto pendente. | 6.4 | Confermata |
| AS-10 | Come associazione voglio poter contattare la persona che ha prenotato un trasporto (contatto visibile). | 6.5 | Confermata – trattamento dati sensibili: vedi Punti aperti |
| AS-11 | Come associazione voglio poter vedere tutti i trasporti in carico nella mia dashboard. | 7.1 | Confermata |
| AS-12 | Come associazione voglio poter vedere uno storico dei trasporti effettuati. | 7.2 | Confermata |
| AS-13 | Come associazione voglio ricevere notifica quando viene richiesta la modifica di un trasporto. | 4.8 | Confermata |
| AS-14 | Come associazione voglio poter approvare o rifiutare una modifica alla destinazione di un trasporto. | 2.3 | Confermata |
| AS-15 | Come associazione voglio poter approvare o rifiutare una modifica alla data/orario di un trasporto. | 2.3 | Confermata |
| AS-16 | Come associazione voglio poter individuare rapidamente i trasporti con richieste di modifica pendenti. | 7.3 | NUOVA – era solo nel dettaglio SPRINT |
| AS-17 | Come associazione voglio poter disdire un trasporto specificando la causa. | 3.2, 3.3 | Confermata |
| AS-18 | Come associazione voglio ricevere notifica quando un trasporto viene annullato dall’utente. | 4.9 | Confermata |
| AS-19 | Come operatore dell’associazione voglio poter aggiornare lo stato del viaggio (preso in carico, in arrivo, in visita, in ritorno, concluso). | 4.10 | NUOVA – lacuna critica dell’originale: nessuno alimentava lo stato |
| AS-20 | Come associazione voglio ricevere notifica quando cambiano gli accompagnatori di un trasporto che ho già accettato. | 4.11 | NUOVA – era solo nel dettaglio SPRINT (voce 2.3) |

## 3.3 Stories amministratore GoCare

| ID | User story | UC | Note |
|---|---|---|---|
| AD-01 | Come amministratore GoCare voglio poter accreditare o rifiutare un’associazione registrata, così che solo gli enti verificati possano operare sulle richieste. | 10.5 | NUOVA – UC 10.5 era senza story |

## 3.4 Matrice di copertura Use Case → User Stories

Verifica che ogni sotto-voce di Use Case abbia almeno una user story associata.

| Use Case | User stories | Esito |
|---|---|---|
| 1.1 | CG-13, CG-14, CG-33 | Coperto |
| 1.2 | CG-15 | Coperto con story nuova |
| 1.3 | CG-17 | Coperto |
| 1.4 | CG-19 | Coperto |
| 1.5 | CG-16 | Coperto con story nuova |
| 1.6 | CG-14 | Coperto |
| 1.7 | CG-38 | Coperto con story nuova |
| 2.1 | CG-17, CG-21 | Coperto |
| 2.2 | CG-20 | Coperto |
| 2.3 | AS-14, AS-15 | Coperto |
| 2.4 | CG-34 | Coperto con story nuova |
| 3.1 | CG-23 | Coperto |
| 3.2 | AS-17 | Coperto |
| 3.3 | CG-24, AS-17 | Coperto con story nuova lato utente |
| 4.1 | CG-29 | Coperto |
| 4.2 | CG-29 | Coperto |
| 4.3 | CG-28 | Coperto |
| 4.4 | CG-25, CG-26 | Coperto |
| 4.5 | CG-22 | Coperto |
| 4.6 | CG-27 | Coperto |
| 4.7 | AS-06 | Coperto |
| 4.8 | AS-13 | Coperto |
| 4.9 | AS-18 | Coperto |
| 4.10 | AS-19 | Coperto con story nuova |
| 4.11 | AS-20 | Coperto con story nuova |
| 5.1 | CG-13, CG-32 | Coperto |
| 5.2 | CG-22, CG-30 | Coperto con story nuova |
| 5.3 | CG-31 | Coperto |
| 5.4 | CG-32 | Coperto |
| 5.5 | CG-29 | Coperto |
| 6.1 | AS-07 | Coperto |
| 6.2 | AS-07 | Coperto |
| 6.3 | AS-08 | Coperto |
| 6.4 | AS-09 | Coperto |
| 6.5 | AS-10 | Coperto |
| 7.1 | AS-11 | Coperto |
| 7.2 | AS-12 | Coperto |
| 7.3 | AS-16 | Coperto con story nuova |
| 8.1 / 8.2 / 8.3 | CG-03 | Coperto |
| 8.4 | CG-18 | Coperto con story nuova |
| 8.5 / 8.6 | AS-05 | Coperto |
| 9.1 | CG-06, CG-07 | Coperto |
| 9.2 | CG-06, CG-08 | Coperto |
| 9.3 | CG-09 | Coperto |
| 9.4 | CG-10 | Coperto |
| 9.5 | CG-11 | Coperto |
| 9.6 | CG-12 | Coperto |
| 9.7 | CG-35 | Coperto con story nuova |
| 9.8 | CG-36 | Coperto con story nuova |
| 9.9 | CG-37 | Coperto con story nuova |
| 10.1 | CG-01, AS-01 | Coperto |
| 10.2 | CG-02, AS-02 | Coperto con story nuove |
| 10.3 | CG-04, AS-03 | Coperto |
| 10.4 | CG-05, AS-04 | Coperto |
| 10.5 | AD-01 | Coperto con story nuova |

---

# 4. Attori

| Attore | Descrizione | Ruolo nel sistema |
|---|---|---|
| Assistito | Persona fragile destinataria del trasporto. | Può registrarsi autonomamente, richiedere trasporti per sé, appartenere a uno o più gruppi cura. Il ruolo di assistito non è una proprietà della persona: esiste solo come ruolo all’interno di uno specifico gruppo cura. |
| Caregiver | Familiare o assistente che gestisce l’assistito. | Può richiedere trasporti per gli assistiti del proprio gruppo cura, gestire il gruppo, seguire lo stato dei viaggi. Anche il ruolo di caregiver è definito per singolo gruppo cura. |
| Associazione | Ente di volontariato/soccorso che eroga il trasporto. | Riceve le richieste a sé destinate, accetta o rifiuta, gestisce i trasporti presi in carico e il loro stato. Opera solo se accreditata. |
| Operatore dell’associazione | Volontario che esegue materialmente il trasporto. | Aggiorna lo stato del viaggio durante l’esecuzione. In v0 non ha un account proprio: opera dall’account dell’associazione e viene registrato come etichetta testuale sulla transizione di stato; il modello di accesso resta da confermare (PA-03). |
| Amministratore GoCare | Chi gestisce la piattaforma. | Accredita o rifiuta le associazioni registrate prima che possano operare sulle richieste (PA-11). |
| GoCare (sistema) | La piattaforma. | Instrada le richieste alle associazioni competenti per provincia, genera notifiche push ed e-mail, conserva lo storico. |

---

# 5. Tipi di viaggio

| Tipo di viaggio | Direzione tipica | Scope |
|---|---|---|
| Visita medica | Andata e ritorno | In scope v0 |
| Ricovero | Solo andata | In scope v0 |
| Dimissione | Solo ritorno | In scope v0 |
| Trasferimento | Solo andata (struttura → struttura) | In scope v0 |
| Trasporto sociale (spesa, posta, ecc.) | Variabile | Rinviato alla v1 |

Il tipo di viaggio determina il valore di default della direzione nel form dell’UC 1 (1.2 e 1.5), che resta comunque modificabile dall’utente. Il trasporto sociale è rinviato alla v1 (vedi Punti aperti).

---

# 6. Decisioni consolidate

- Dashboard interna per le associazioni presente e obbligatoria in V.0.
- Le disponibilità delle associazioni non sono gestite dalla piattaforma: le associazioni accettano le richieste secondo la propria disponibilità (punto da chiarire n.1 dell’originale, risolto).
- Flusso API bidirezionale in V.1: tutte le chiamate sono in scrittura e ogni chiamata corrisponde a un cambio di stato.
- Colonne Trello: To do, Ready for development, In progress (una sola card alla volta), Ready for testing, In testing, Tested, Released.
- Il calcolo dei km previsti è rinviato alla v1: la richiesta non porta un campo km e le dashboard dell’associazione non hanno filtri né colonne per i km.
- Il ruolo di caregiver o di assistito non è una proprietà della persona ma dell’appartenenza al singolo gruppo cura: la stessa persona può essere caregiver in un gruppo e assistito in un altro. La registrazione distingue solo fra account persona e account associazione.
- Il rifiuto di una singola associazione non è uno stato della richiesta: la richiesta resta “in attesa” e visibile alle altre associazioni destinatarie. L’esito negativo aggregato è sempre “non coperta”.
- Ogni evento genera una sola notifica, con i canali (push, e-mail) combinati sulla stessa voce: il centro notifiche non mostra righe duplicate per lo stesso evento.
- Gli indirizzi sono strutturati (via, civico, CAP, città, provincia) e vengono copiati sulla richiesta al momento della creazione: modificare o eliminare una destinazione salvata non altera i viaggi già registrati.
- L’instradamento delle richieste avviene per provincia: ogni associazione dichiara le province coperte e riceve solo le richieste con partenza in una di esse.
- Autenticazione e dominio sono su due database separati; l’account e il profilo (persona o associazione) sono due entità distinte, su database distinti, che **condividono lo stesso identificatore** (nessuna foreign key tra i due database). In v0 ogni profilo di dominio ha esattamente un account: non si opera su persone non registrate.

---

# 7. Stack tecnologico

- Backend: C# / .NET 10, ASP.NET Core Web API con controller MVC
- Frontend: React Native
- Database: PostgreSQL, due database distinti (`gocare_auth` per l’autenticazione, `gocare_business` per il dominio), accesso via EF Core + Npgsql
- Struttura della soluzione: un progetto applicativo (aree autenticazione e dominio), un progetto di componenti trasversali, un progetto host che espone l’API
- Notifiche: push applicative + e-mail di conferma

---

# 8. Perimetro versione BETA e suddivisione del lavoro

Versione BETA:

- Dashboard con login per i tre ruoli (caregiver, assistiti, associazioni).
- Calendario e interfaccia di prenotazione.

Suddivisione del lavoro (ad alto livello):

- Stefano: grafica React Native
- Giorgia: grafica React Native
- Francesco: logica C# – area di dominio (richieste di trasporto, gruppi cura, notifiche)
- Elio: logica C# – area di autenticazione (account, token, sessioni)
- Tutti: database (Francesco ed Elio potranno modificarlo per la logica)

Note dalla riunione Discord del 03/08/26:

- Gestione dei guasti da parte dell’associazione (aperto – vedi Punti aperti).
- Slot viaggi e conferma post-prenotazione tramite e-mail (chiuso – vedi Punti aperti).
- UX/UI: facilità per l’utente di capire quali giorni sono disponibili e quali no.
- Definiti i prossimi due giorni di lavoro (martedì 17:00 / mercoledì 13:00).

---

# 9. SPRINT – dettaglio Frontend / Backend per Use Case

Ogni voce riporta la user story di riferimento (ID della sezione 3) e il breakdown FRONTEND/BACKEND.

## UC 1 – RICHIEDI TRASPORTO

**1.1 Come assistito voglio poter registrare i miei futuri trasporti. (CG-13)**

FRONTEND:

- Pagina con form di aggiunta di un nuovo trasporto.
- Campi: per chi, data, orario, indirizzo di partenza, destinazione, tipo di viaggio, direzione, accompagnatori, contatti.
- Selezione con tendina delle destinazioni già salvate, in alternativa bottone di aggiunta con comparsa del form di aggiunta destinazione.
- Validazione lato client dei campi obbligatori e della data (non nel passato).
- Pop-up di conferma invio: “La tua richiesta è stata inviata alle associazioni. Riceverai una notifica appena verrà presa in carico.”

BACKEND:

- Validazione dati e form.
- Creazione della richiesta in stato “in attesa”, con doppio riferimento a richiedente e beneficiario e riferimento facoltativo al gruppo cura.
- Copia sulla richiesta degli indirizzi (via, civico, CAP, città, provincia) e dei contatti di riferimento: sono uno scatto al momento della richiesta e non seguono le modifiche successive dell’anagrafica.
- Congelamento della lista delle associazioni destinatarie: al momento della creazione vengono selezionate le associazioni accreditate che coprono la provincia di partenza, e l’elenco resta immutabile per tutta la vita della richiesta.
- Invio della notifica di nuova richiesta alle associazioni destinatarie (UC 4.7).

**1.2 Come caregiver voglio poter registrare il prossimo trasporto del mio assistito. (CG-14)**

FRONTEND:

- Selezione con tendina dell’assistito del “gruppo cura” ricevente del trasporto.
- La tendina è visibile solo se l’utente appartiene ad almeno un gruppo cura con assistiti associati.
- Precompilazione dei dati di contatto e dell’indirizzo abituale dell’assistito selezionato.

BACKEND:

- Verifica che il richiedente appartenga con invito accettato al gruppo cura dell’assistito indicato (autorizzazione).
- Salvataggio della richiesta con doppio riferimento: richiedente e beneficiario (alimenta l’UC 9.6).

**1.3 Come caregiver/assistito voglio poter scegliere la direzione del viaggio. (CG-15)**

FRONTEND:

- Selettore a tre opzioni: andata e ritorno / solo andata / solo ritorno.
- In caso di “andata e ritorno” compare il campo orario di ritorno.

BACKEND:

- Validazione della coerenza fra direzione, orari e indirizzi: un “andata e ritorno” richiede obbligatoriamente l’orario di ritorno; le altre due direzioni non possono avere né orario né indirizzo di ritorno; l’orario di ritorno deve essere successivo a quello di partenza. La regola vale sia alla creazione sia a ogni modifica successiva.
- Persistenza della direzione sulla richiesta: determina gli stati attraversati dal viaggio (UC 4.1).

**1.4 Come caregiver/assistito voglio poter indicare il tipo di viaggio. (CG-16)**

FRONTEND:

- Tendina con i tipi di viaggio previsti (visita medica, ricovero, dimissione, trasferimento).
- La selezione imposta il valore di default della direzione, che resta modificabile.

BACKEND:

- Validazione del valore rispetto alla lista dei tipi ammessi.
- Salvataggio del tipo sulla richiesta (dato visibile all’associazione nell’UC 6.2).

**1.5 Come caregiver/assistito voglio poter indicare un accompagnatore per il viaggio. (CG-19)**

FRONTEND:

- Form di inserimento dei dati generali dell’accompagnatore (nome, cognome, parentela, contatto).
- Possibilità di indicare più di un accompagnatore o nessuno.

BACKEND:

- Validazione dati e form.
- Salvataggio degli accompagnatori collegati alla richiesta. L’accompagnatore è un dato immutabile: le modifiche si fanno sostituendo l’intera lista.

**1.6 Come caregiver/assistito voglio poter scegliere una destinazione diversa dal mio domicilio abituale. (CG-17)**

FRONTEND:

- Form di aggiunta nuova destinazione, con bottone “aggiungi destinazione”.
- Comparsa del pop-up “Vuoi aggiungere questa destinazione alla tua lista destinazioni? SI/NO”.

BACKEND:

- Validazione dati e form.
- Se l’utente conferma, salvataggio della destinazione nella lista personale (UC 8.4).

**1.7 Come caregiver/assistito voglio poter registrare un trasporto ricorrente. (CG-33)** *[RINVIATA a v1 – vedi Punti aperti]*

La raccomandazione è escluderla dalla v0: la ricorrenza implica generazione automatica di richieste, gestione delle eccezioni sulle singole occorrenze e annullamento in blocco, con un impatto sul modello dati sproporzionato rispetto al valore in fase di validazione del prodotto.

**1.8 Come caregiver/assistito voglio poter indicare per il ritorno una destinazione diversa da quella da cui sono partito. (CG-38)**

FRONTEND:

- Nel form di nuovo trasporto, solo con direzione “andata e ritorno”, checkbox “il ritorno è verso un indirizzo diverso”.
- Se selezionata, compare un terzo campo indirizzo, con la stessa tendina delle destinazioni salvate.
- Se non selezionata, il ritorno è verso l’indirizzo di partenza dell’andata.

BACKEND:

- Terzo indirizzo facoltativo sulla richiesta, valorizzabile solo con direzione “andata e ritorno”; se assente, il ritorno si intende verso l’indirizzo di partenza.
- Stessa validazione di coerenza direzione/indirizzi della 1.3.
- L’indirizzo di ritorno è visibile all’associazione nel dettaglio della richiesta (UC 6.2) ed è un campo modificabile con richiesta di approvazione (UC 2.1).

## UC 2 – MODIFICA TRASPORTO

**2.1 Come caregiver/assistito voglio poter modificare la destinazione del mio viaggio. (CG-17)**

FRONTEND:

- Form di modifica destinazione, precompilato con i dati attuali.
- Bottone “modifica” al posto di “aggiungi”.
- Campi distinti per la destinazione dell’andata e per quella del ritorno.
- Se il viaggio è già stato accettato da un’associazione, avviso: “La modifica dovrà essere approvata dall’associazione”.

BACKEND:

- Dati già presenti nel form, validazione del form.
- Se la richiesta è ancora “in attesa”: modifica diretta.
- Se la richiesta è già “confermata”: creazione di una richiesta di modifica in stato “in attesa di approvazione” e notifica all’associazione (UC 4.8).
- La richiesta di modifica indica il campo interessato (data e orario, destinazione dell’andata, destinazione del ritorno) e conserva sia il valore precedente sia quello proposto.

**2.2 Come caregiver/assistito voglio poter modificare la data del mio viaggio in caso di imprevisto. (CG-21)**

FRONTEND:

- Form (anche solo un pop-up) con la sola modifica della data e dell’orario.
- Stesso avviso della 2.1 se il viaggio è già accettato.

BACKEND:

- Validazione dati (data non nel passato, coerenza con la direzione, ritorno successivo alla partenza).
- Stessa logica a due rami della 2.1: modifica diretta se in attesa, richiesta di approvazione se confermata.
- Notifica all’associazione (UC 4.8).

**2.3 Come caregiver/assistito voglio poter aggiungere/modificare/rimuovere l’accompagnatore anche dopo aver creato la richiesta. (CG-20)**

FRONTEND:

- Eliminazione: bottone elimina con pop-up di conferma “Vuoi rimuovere questo accompagnatore? SI/NO”.
- Modifica: bottone modifica, apertura del form di modifica accompagnatore, pop-up di conferma.
- Aggiunta: bottone aggiungi, apertura del form di aggiunta, pop-up di conferma.

BACKEND:

- Validazione dei form.
- Aggiornamento degli accompagnatori collegati alla richiesta per sostituzione integrale della lista.
- L’accompagnatore non passa dal circuito di approvazione: la modifica è sempre diretta.
- Notifica informativa all’associazione se il viaggio è già stato accettato (UC 4.11): il numero di passeggeri incide sul mezzo.

**2.4 Come caregiver/assistito voglio poter aggiungere o modificare le mie destinazioni salvate. (CG-18)**

La gestione delle destinazioni salvate non modifica alcun trasporto: appartiene all’anagrafica personale (UC 8.4). Il dettaglio è riportato al punto 8.3 di questa sezione; qui resta il solo rimando.

**2.5 Come associazione voglio poter approvare o rifiutare una modifica alla destinazione di un trasporto. (AS-14)**

FRONTEND:

- Nella lista dei viaggi accettati i viaggi sottoposti a modifica vengono segnalati in maniera visiva.
- Icona con contatore delle richieste di modifica pendenti.
- Filtro nella pagina dei viaggi approvati per selezionare solo quelli con richiesta di modifica.
- Riquadro che mostra la richiesta (valore precedente → valore proposto) e bottoni “approva / rifiuta” con campo per il messaggio esplicativo.

BACKEND:

- Contatore delle richieste pendenti per l’icona.
- Approvazione e rifiuto sono ammessi solo su una richiesta ancora “in attesa di approvazione”; ogni altro stato viene respinto.
- Se approvata: applicazione della modifica al viaggio e chiusura della richiesta con data di risoluzione.
- Se rifiutata: il viaggio resta invariato, il messaggio esplicativo è obbligatorio e la richiesta viene chiusa con esito negativo.
- In entrambi i casi notifica all’utente (UC 4.5).

**2.6 Come associazione voglio poter approvare o rifiutare una modifica alla data/orario di un trasporto. (AS-15)**

FRONTEND:

- Stessa logica della modifica destinazione (2.5).

BACKEND:

- Stessa logica della 2.5.
- Notifica all’utente con il nuovo orario confermato (UC 4.5).

**2.7 Come caregiver/assistito voglio poter ritirare una richiesta di modifica ancora in attesa di approvazione. (CG-34)**

FRONTEND:

- Nel dettaglio del viaggio, accanto al badge “in attesa di approvazione”, bottone “ritira richiesta” con pop-up di conferma.
- Il bottone scompare non appena l’associazione ha approvato o rifiutato.

BACKEND:

- Il ritiro è ammesso solo su una richiesta ancora “in attesa di approvazione”: la richiesta passa in stato “ritirata” e viene chiusa con data di risoluzione.
- Il viaggio resta invariato alle condizioni originali.
- Decremento del contatore delle richieste di modifica pendenti dell’associazione.

## UC 3 – ANNULLA TRASPORTO

**3.1 Come caregiver/assistito voglio poter annullare il viaggio, indicando il motivo. (CG-23, CG-24)**

FRONTEND:

- Bottone di annullamento del viaggio.
- Pop-up di conferma dell’annullamento: “Sei sicuro di voler eliminare questo trasporto? Una volta eliminato si dovrà effettuare una nuova richiesta per richiedere un nuovo trasporto.”
- Tendina con le causali più frequenti + campo di testo libero opzionale.

BACKEND:

- Soft delete dal database (il viaggio resta nello storico con stato “annullato”), con registrazione di chi ha annullato e quando.
- Salvataggio della causale.
- Notifica all’associazione (UC 4.10), se il viaggio era già stato preso in carico.

**3.2 Come associazione voglio poter disdire un trasporto specificando la causa. (AS-17)**

FRONTEND:

- Apertura del form con area di testo per scrivere il motivo, o tendina a scomparsa per sceglierlo.
- Pop-up di conferma.

BACKEND:

- Validazione del form (causale obbligatoria).
- Registrazione della disdetta come rifiuto post-accettazione di quella associazione, distinto dal semplice declino di una richiesta pendente: per la disdetta la causale è obbligatoria, per il declino è facoltativa.
- Il viaggio torna in stato “in attesa” e rientra nella dashboard delle richieste pendenti delle altre associazioni destinatarie, così da consentire una copertura alternativa.
- Invio della notifica all’utente con il testo della causale (UC 4.6).

*[DA VALIDARE] Il rientro automatico del viaggio fra le richieste pendenti è la risposta al problema n.3 dichiarato in apertura del documento (“deve essere trovata una soluzione per assicurare un’alternativa”). Va confermato che sia il comportamento desiderato e con quale anticipo minimo rispetto alla data del viaggio.*

## UC 4 – STATO E NOTIFICHE

**4.1 Come caregiver/assistito voglio sapere se il viaggio non può essere coperto. (CG-25)**

FRONTEND:

- Pop-up con messaggio di esito negativo e mancanza di soluzioni.
- Badge “non coperto” sulla card del viaggio in “I miei viaggi”.

BACKEND:

- Notifica ed e-mail con esito negativo.
- Regola di scatto dell’esito negativo: nessuna associazione destinataria ha accettato entro la soglia definita (vedi Punti aperti), valutata da un job schedulato che confronta i rifiuti registrati con la lista congelata delle associazioni destinatarie.
- Il passaggio a “non coperta” registra la data dell’esito.

**4.2 Come caregiver/assistito voglio ricevere notifica quando il viaggio è preso in carico. (CG-26)**

FRONTEND:

- Apertura del pop-up con avviso di conferma del viaggio al login, con link alla scheda del viaggio.
- Aggiornamento del badge di stato sulla card.

BACKEND:

- Notifica ed e-mail di conferma, con nome e contatti dell’associazione.

**4.3 Come caregiver/assistito voglio essere avvisato se un viaggio già preso in carico viene annullato dall’associazione. (CG-27)**

FRONTEND:

- Pop-up al login con avviso e testo del motivo della disdetta.
- Bottone “cerca un’alternativa” che riporta il viaggio in stato di richiesta.

BACKEND:

- Notifica ed e-mail.
- Registrazione della causale nello storico del viaggio.

**4.4 Come caregiver/assistito voglio sapere se la mia richiesta di modifica è stata accettata o rifiutata. (CG-22)**

FRONTEND:

- Badge sulla card del viaggio: “in attesa di approvazione / approvata / rifiutata / ritirata”.
- Pop-up di notifica con il messaggio esplicativo dell’associazione in caso di rifiuto.

BACKEND:

- Notifica ed e-mail all’esito della valutazione dell’associazione (UC 2.3).
- Il ritiro da parte dell’utente non genera notifica di esito.

**4.5 Come caregiver voglio poter vedere lo stato in tempo reale del viaggio dell’assistito. (CG-29)**

FRONTEND:

- Stato mostrato nella card del viaggio, nella pagina dei viaggi del “gruppo cura” e nella pagina “I miei viaggi”.
- Timeline degli stati attraversati con l’orario di ciascun passaggio.
- Pagina/schermata in cui l’operatore gestisce lo stato dell’assistito.

BACKEND:

- Persistenza dei cambi di stato in un registro append-only: ogni riga porta stato, timestamp, associazione che l’ha effettuato ed etichetta testuale dell’operatore. Lo stato corrente è la riga più recente.
- Cambio di stato gestito dall’operatore dall’account dell’associazione (modello di accesso da confermare: vedi Punti aperti).
- Ogni cambio di stato genera la notifica dell’UC 4.6.

*[DA VALIDARE] La visualizzazione della posizione GPS del mezzo è esclusa dalla v0: richiede tracciamento continuo, consenso esplicito dell’operatore e trattamento di dati di geolocalizzazione. Lo stato testuale copre l’esigenza dichiarata nel problema n.2 (evitare le ripetute chiamate all’associazione).*

**4.6 Come caregiver/assistito voglio ricevere notifica a ogni cambio di stato del viaggio. (CG-29)**

FRONTEND:

- Notifica push con il nuovo stato, es. “Il tuo assistito è stato preso in carico”, “Viaggio concluso”.

BACKEND:

- Invio della notifica push a tutti i membri del gruppo cura interessati dal viaggio.
- Nessuna e-mail per i cambi di stato intermedi, per non generare rumore.
- L’invio push richiede che il dispositivo sia registrato: ogni utente e ogni associazione registra i propri token push, con la piattaforma di provenienza (iOS, Android, Web); i token possono essere disattivati al logout o alla disinstallazione.

**4.7 Come caregiver/assistito voglio ricevere notifica dei prossimi trasporti programmati. (CG-28)**

FRONTEND:

- Pop-up: “Ricordati il trasporto di domani alle 17:25 da X a Y”.

BACKEND:

- Notifica ed e-mail il giorno precedente il viaggio.
- Job schedulato giornaliero che seleziona i viaggi confermati con data uguale a domani.

**4.8 Come associazione voglio ricevere notifica se un utente invia una richiesta di trasporto. (AS-06)**

FRONTEND:

- Notifica “Ci sono nuove richieste, entra nell’app per visualizzarle”.
- Badge con contatore sulla voce di header “Richieste”.

BACKEND:

- Notifica ed e-mail a tutte le associazioni destinatarie della richiesta, cioè quelle congelate nella lista dei candidati alla creazione.

**4.9 Come associazione voglio ricevere notifica se un trasporto viene modificato. (AS-13)**

FRONTEND:

- Notifica con il riferimento al viaggio e il campo modificato.
- Il viaggio viene evidenziato nella lista dei trasporti accettati (UC 7.3).

BACKEND:

- Notifica ed e-mail.
- Incremento del contatore delle richieste di modifica pendenti.

**4.10 Come associazione voglio ricevere notifica se un trasporto viene annullato dall’utente. (AS-18)**

FRONTEND:

- Pop-up al login: “Il viaggio del [data e ora] è stato disdetto.”
- Bottone “cerca un nuovo viaggio” che porta alla pagina delle richieste di trasporto.

BACKEND:

- Notifica ed e-mail.
- Rimozione del viaggio dalla lista dei trasporti accettati e spostamento nello storico con stato “annullato”.

**4.11 Come operatore dell’associazione voglio poter aggiornare lo stato del viaggio. (AS-19)**

FRONTEND:

- Schermata di dettaglio del viaggio in corso con i pulsanti di avanzamento di stato, uno per volta e in sequenza: preso in carico → in arrivo → in visita → in ritorno → concluso.
- Il pulsante mostra sempre e solo lo stato successivo, per ridurre gli errori di inserimento.
- Campo con il nome dell’operatore che sta effettuando il passaggio.
- Pop-up di conferma sul passaggio a “concluso”, che chiude il viaggio.

BACKEND:

- Validazione della transizione di stato rispetto alla macchina a stati (UC 4.1) e alla direzione del viaggio: un viaggio “solo andata” non attraversa lo stato “in ritorno”, un viaggio “solo ritorno” non attraversa lo stato “in visita”.
- Registrazione di stato, timestamp, associazione ed etichetta dell’operatore in una nuova riga del registro delle transizioni; nessuna riga viene mai modificata o cancellata.
- Allineamento dello stato della richiesta: alla prima transizione la richiesta passa in “in esecuzione”, con il passaggio a “concluso” passa in “conclusa”.
- Emissione della notifica dell’UC 4.6 verso il gruppo cura.

**4.12 Come associazione voglio ricevere notifica se cambiano gli accompagnatori di un trasporto che ho accettato. (AS-20)**

FRONTEND:

- Notifica con il riferimento al viaggio e il numero aggiornato di accompagnatori.
- Aggiornamento della card del trasporto accettato con la nuova lista.

BACKEND:

- Notifica emessa solo se il viaggio è già stato accettato; se è ancora “in attesa” la modifica è silenziosa.
- Notifica informativa, senza richiesta di approvazione: la lista accompagnatori non passa dal circuito di modifica dell’UC 2.

## UC 5 – VISUALIZZAZIONE “I MIEI VIAGGI”

**5.1 Come caregiver/assistito voglio vedere l’elenco dei miei viaggi. (CG-13, CG-30)**

FRONTEND:

- Bottone “aggiungi” per aprire il form di aggiunta viaggio (rimanda all’UC 1).
- Lista di tutti i viaggi in forma di card.
- Contenuto della card: data e ora, associazione, contatto dell’associazione, indirizzo di partenza, indirizzo di destinazione, eventuale indirizzo di ritorno, accompagnatore, badge di stato.
- Separazione fra “prossimi viaggi” e “storico”.

BACKEND:

- Lettura dei viaggi dell’utente, di quelli in cui è beneficiario e di quelli dei gruppi cura di cui fa parte.
- Ordinamento per data crescente sui prossimi viaggi, decrescente sullo storico.
- Paginazione dello storico.

*[DA VALIDARE] Va deciso se esporre il contatto generico dell’associazione e, durante il viaggio, quello dell’operatore, oppure solo quello generico. Ripreso nella sezione Punti aperti.*

**5.2 Come caregiver/assistito voglio sapere se la mia richiesta di modifica è stata accettata o rifiutata. (CG-22)**

FRONTEND:

- In caso di modifica appare un badge colorato in alto a destra della card con scritto “in attesa di approvazione / rifiutata / approvata / ritirata”.
- Toccando il badge si apre il dettaglio con il messaggio dell’associazione.

BACKEND:

- Esposizione dello stato della richiesta di modifica insieme ai dati del viaggio.
- Storicizzazione delle richieste di modifica con campo interessato, valore precedente, valore proposto, esito, messaggio e data di risoluzione.

**5.3 Come caregiver/assistito voglio poter vedere i contatti dell’associazione che ha preso in carico il mio viaggio. (CG-31)**

FRONTEND:

- Contatto riportato nella card del viaggio.
- Contatti completi nella pagina dell’associazione, raggiungibile dalla card.
- I contatti compaiono solo dopo la presa in carico.

BACKEND:

- Esposizione dei dati di contatto dell’associazione assegnataria: telefoni, e-mail e orari di reperibilità.
- Nessun contatto esposto per i viaggi in stato “in attesa”.

**5.4 Come caregiver/assistito voglio poter avere uno storico delle visite fatte. (CG-32)**

FRONTEND:

- Pagina/sezione “Storico viaggi”.
- Lista di card in sola lettura, con stato finale (concluso / annullato / non coperto).
- Filtro per assistito e per periodo.

BACKEND:

- Query sui viaggi in stato terminale, con paginazione.
- I viaggi soft-deleted restano visibili nello storico con stato “annullato”.

**5.5 Come caregiver voglio vedere lo stato di avanzamento del viaggio in corso. (CG-29)**

FRONTEND:

- Card del viaggio in corso in evidenza in cima alla lista, con la timeline degli stati.
- Aggiornamento della card alla ricezione della notifica di cambio stato.

BACKEND:

- Esposizione dello stato corrente e della cronologia dei passaggi, letta dal registro delle transizioni ordinato per timestamp.
- Lettura in polling alla riapertura della schermata (il push copre l’aggiornamento immediato).

## UC 6 – RICHIESTE PENDENTI (associazione)

**6.1 Come associazione voglio vedere tutte le richieste pendenti nella mia dashboard, con filtri. (AS-07)**

FRONTEND:

- Dashboard con lista delle richieste in stato “in attesa”, in forma di card o righe di tabella.
- Barra filtri: data, fascia oraria, destinazione, tipo di viaggio.
- Ordinamento per data del viaggio.
- Stato vuoto: “Non ci sono richieste al momento”.

BACKEND:

- Query delle richieste in stato “in attesa” per cui l’associazione compare fra i candidati congelati alla creazione e non ha già rifiutato.
- Applicazione dei filtri lato server e paginazione.
- Nessun dato esposto alle associazioni non destinatarie.

**6.2 Come associazione voglio vedere il dettaglio di una richiesta. (AS-07)**

FRONTEND:

- Pagina o pannello di dettaglio: dati dell’assistito, data e ora, indirizzo di partenza, destinazione, eventuale indirizzo di ritorno, tipo di viaggio, direzione, accompagnatori, note.
- Bottoni “accetta” e “rifiuta” in fondo al dettaglio.

BACKEND:

- Lettura completa della richiesta.
- Verifica che la richiesta sia ancora disponibile prima di mostrare le azioni.

**6.3 Come associazione voglio poter accettare una richiesta di trasporto pendente. (AS-08)**

FRONTEND:

- Bottone “accetta” con pop-up di conferma.
- Al termine, la richiesta esce dalla dashboard delle pendenti e compare fra i trasporti accettati.
- Messaggio di errore dedicato se la richiesta è stata nel frattempo presa da un’altra associazione.

BACKEND:

- Passaggio di stato della richiesta da “in attesa” a “confermata”, con assegnazione all’associazione e registrazione della data di accettazione. Il passaggio è ammesso solo da “in attesa”.
- Controllo di concorrenza ottimistico sulla richiesta: la prima accettazione vince, le successive ricevono un errore gestito.
- Notifica di presa in carico all’utente (UC 4.2).

*[DA VALIDARE] Il modello “prima accettazione vince” discende dalla decisione consolidata che le disponibilità non sono gestite dalla piattaforma. Va confermato il criterio con cui si compone l’area di competenza: in v0 la richiesta è visibile alle sole associazioni che coprono la provincia di partenza.*

**6.4 Come associazione voglio poter rifiutare o declinare una richiesta pendente. (AS-09)**

FRONTEND:

- Bottone “rifiuta” con pop-up di conferma e causale facoltativa.
- La richiesta scompare dalla dashboard dell’associazione che ha rifiutato.

BACKEND:

- Registrazione del rifiuto per quella associazione, come declino pre-accettazione con causale facoltativa: la richiesta resta “in attesa” e visibile alle altre destinatarie.
- Se tutte le associazioni destinatarie hanno rifiutato, o è scaduta la soglia temporale, la richiesta passa in stato “non coperta” e scatta la notifica dell’UC 4.1.

**6.5 Come associazione voglio poter contattare la persona che ha prenotato un trasporto. (AS-10)**

FRONTEND:

- Contatto telefonico visibile nel dettaglio della richiesta, con azione “chiama”.
- Contatto dell’accompagnatore, se indicato.

BACKEND:

- Esposizione dei dati di contatto solo alle associazioni destinatarie della richiesta.
- Tracciamento degli accessi ai dati di contatto, distinguendo i recapiti del richiedente da quelli del beneficiario: ogni accesso registra richiesta, associazione, tipo di dato e istante.

*[DA VALIDARE] Il recapito della persona che ha prenotato è un dato sensibile. Raccomandazione: mostrare il recapito completo solo dopo l’accettazione, e limitarsi al nome e alla fascia oraria nella fase di valutazione della richiesta. Punto ripreso in “Cosa manca / Punti aperti”.*

## UC 7 – TRASPORTI ACCETTATI (associazione)

**7.1 Come associazione voglio vedere tutti i trasporti in carico nella mia dashboard. (AS-11)**

FRONTEND:

- Lista dei trasporti accettati e non ancora conclusi, in forma di card.
- Contenuto della card: data e ora, assistito, contatti, partenza, destinazione, eventuale indirizzo di ritorno, accompagnatori, stato corrente.
- Filtri per data e per stato; ordinamento per data crescente.
- Accesso alla schermata di aggiornamento stato (UC 4.11) dalla card.

BACKEND:

- Query dei viaggi assegnati all’associazione in stato “confermato” o “in esecuzione”.
- Esposizione dello stato corrente e dell’eventuale richiesta di modifica pendente.

**7.2 Come associazione voglio individuare i trasporti con richieste di modifica pendenti. (AS-16)**

FRONTEND:

- Evidenza visiva sulla card (bordo o badge colorato).
- Icona con contatore nell’header e filtro dedicato “solo con richiesta di modifica”.
- Riquadro di approvazione/rifiuto raggiungibile dalla card (rimanda all’UC 2.5).

BACKEND:

- Contatore delle richieste di modifica pendenti per associazione.
- Filtro lato server sui viaggi con modifica in attesa di approvazione.

**7.3 Come associazione voglio poter vedere uno storico dei trasporti effettuati. (AS-12)**

FRONTEND:

- Sezione “Storico” con lista in sola lettura dei viaggi conclusi, annullati e rifiutati.
- Filtri per periodo e per esito.
- Riepilogo numerico dei viaggi conclusi nel periodo selezionato.

BACKEND:

- Query sui viaggi in stato terminale assegnati all’associazione, con paginazione.
- Conservazione dei dati anche dopo il soft delete del viaggio.

## UC 8 – GESTIONE INFORMAZIONI PROFILO

**8.1 Come caregiver/assistito voglio visualizzare le mie informazioni personali. (CG-03)**

FRONTEND:

- Pagina “Profilo” con i dati personali in sola lettura: nome, cognome, data di nascita, indirizzo di domicilio, telefono, e-mail.
- Bottone “modifica” che attiva la modalità di editing.
- Sezione separata con i dati di accesso (e-mail di login) e i gruppi cura di appartenenza, con il ruolo ricoperto in ciascuno.

BACKEND:

- Lettura dei dati dell’utente autenticato.
- L’indirizzo di domicilio è facoltativo e strutturato in via, civico, CAP, città e provincia.
- Nessun dato di altri utenti esposto su questo endpoint.

**8.2 Come caregiver/assistito voglio modificare le mie informazioni personali e di contatto. (CG-03)**

FRONTEND:

- Form di modifica precompilato, con i campi non modificabili disabilitati.
- Validazione lato client di telefono ed e-mail.
- Pop-up di conferma “Vuoi salvare le modifiche? SI/NO” e messaggio di esito.
- Se viene modificata l’e-mail, avviso che sarà necessaria una nuova verifica.

BACKEND:

- Validazione dei dati e del form.
- Aggiornamento dell’anagrafica utente.
- Se cambia l’e-mail di login: invio dell’e-mail di verifica al nuovo indirizzo e conferma del cambio solo dopo la validazione. L’e-mail di accesso vive nell’area autenticazione, l’e-mail di contatto nel profilo di dominio: le due modifiche sono distinte.
- I viaggi già registrati mantengono i contatti indicati al momento della richiesta.

*[DA VALIDARE] Se un contatto cambia mentre un viaggio è in corso, va deciso se l’associazione debba vedere il contatto aggiornato o quello registrato con la richiesta.*

**8.3 Come caregiver/assistito voglio gestire le mie destinazioni salvate. (CG-18)**

FRONTEND:

- Sezione della pagina profilo (o pagina dedicata) “Le mie destinazioni”.
- Lista di tutte le destinazioni salvate, con bottoni di modifica ed eliminazione accanto a ciascuna.
- Form di aggiunta/modifica: etichetta (es. “Ospedale Sant’Anna”), indirizzo completo, note.
- Pop-up di conferma per l’eliminazione.
- Le destinazioni salvate alimentano la tendina del form di nuovo trasporto (UC 1.6).

BACKEND:

- CRUD delle destinazioni collegate all’utente.
- Validazione dell’indirizzo (via, civico, CAP, città, provincia) e dei campi obbligatori.
- L’eliminazione di una destinazione non modifica i viaggi già registrati che la utilizzano (l’indirizzo è copiato sulla richiesta).

**8.4 Come associazione voglio visualizzare e modificare le informazioni della mia associazione. (AS-05)**

FRONTEND:

- Pagina informazioni associazione in sola lettura: denominazione, sede, province coperte, recapiti telefonici, e-mail, orari di reperibilità, stato di accreditamento.
- Pagina di modifica delle informazioni, con form precompilato e pop-up di conferma.
- Le province coperte si gestiscono come elenco, con aggiunta e rimozione delle singole voci.
- Anteprima di come i contatti vengono visti dagli utenti (UC 5.3).

BACKEND:

- Lettura e aggiornamento dei dati dell’associazione autenticata: denominazione, sede (indirizzo strutturato, obbligatoria), elenco dei telefoni, una e-mail, orari di reperibilità facoltativi, elenco delle province coperte.
- Validazione dei recapiti (almeno un telefono e una e-mail obbligatori, perché esposti agli utenti) e di almeno una provincia coperta, perché da essa dipende l’instradamento delle richieste.
- La modifica dei contatti si riflette immediatamente sui viaggi in carico.
- La modifica delle province coperte vale per le richieste future: le liste di destinatari già congelate non vengono ricalcolate.

## UC 9 – GESTIONE GRUPPI CAREGIVER-ASSISTITI

**9.1 Come caregiver/assistito voglio poter creare un “gruppo cura”. (CG-07)**

FRONTEND:

- Bottone “crea gruppo” nella pagina principale caregiver/assistito.
- Form: nome del gruppo, descrizione facoltativa.
- Al salvataggio, il gruppo compare come nuova card nella pagina principale.
- Il creatore assume il ruolo di amministratore del gruppo.

BACKEND:

- Creazione del gruppo con il creatore come primo membro, con ruolo amministrativo di amministratore, ruolo nel gruppo di caregiver e invito già accettato.
- Validazione del nome (obbligatorio, univoco per utente).

*[DA VALIDARE] È necessario stabilire se il gruppo abbia un amministratore con poteri esclusivi (aggiunta/rimozione membri, eliminazione gruppo) o se tutti i membri abbiano gli stessi diritti. Raccomandazione: introdurre il ruolo di amministratore, altrimenti qualunque membro potrebbe rimuovere gli altri o eliminare il gruppo.*

**9.2 Come caregiver/assistito voglio poter aggiungere un assistito o un caregiver al gruppo. (CG-06, CG-08)**

FRONTEND:

- Bottone “aggiungi membro” nella pagina di gruppo.
- Form di invito tramite e-mail, con scelta del ruolo nel gruppo (caregiver / assistito).
- Stato dell’invito visibile nella lista membri: “in attesa di accettazione”.
- Un caregiver può appartenere a più gruppi e avere più assistiti a carico (CG-06).

BACKEND:

- Creazione dell’invito con e-mail del destinatario e token univoco, e invio della notifica/e-mail con il link di accettazione.
- L’invito nasce con ruolo amministrativo di semplice membro: l’amministratore si assegna solo con la 9.9.
- Accettazione dell’invito da parte del destinatario prima dell’ingresso effettivo nel gruppo; l’accettazione registra la data di risposta.
- Una persona non può comparire due volte nello stesso gruppo.
- Verifica che chi invita abbia i permessi necessari.
- L’appartenenza al gruppo con invito accettato abilita la prenotazione per conto terzi (UC 1.2).

*[DA VALIDARE] Va definito come si aggiunge un membro. L’invito con accettazione è la raccomandazione: aggiungere una persona al gruppo senza il suo consenso significherebbe consentire a terzi di prenotare trasporti a suo nome e di vederne i dati.*

**9.3 Come caregiver/assistito voglio poter rimuovere un assistito o un caregiver dal gruppo. (CG-09)**

FRONTEND:

- Bottone di rimozione accanto a ciascun membro nella lista.
- Pop-up di conferma: “Vuoi rimuovere [nome] dal gruppo? Non potrà più prenotare trasporti per gli assistiti del gruppo. SI/NO”.
- Il bottone è visibile solo all’amministratore del gruppo.

BACKEND:

- Rimozione dell’appartenenza al gruppo: ammessa solo su un membro con invito accettato e non già rimosso; l’appartenenza viene disattivata, non cancellata.
- Verifica dei permessi di chi effettua la rimozione.
- Blocco della rimozione di un assistito con viaggi futuri già confermati, o richiesta di conferma esplicita.
- I viaggi passati restano nello storico del gruppo.

**9.4 Come caregiver/assistito voglio poter eliminare un “gruppo cura”. (CG-10)**

FRONTEND:

- Bottone “elimina gruppo” nella pagina di gestione del gruppo.
- Pop-up di conferma con avviso esplicito sulla perdita della vista condivisa dei viaggi.
- Azione riservata all’amministratore.

BACKEND:

- Soft delete del gruppo.
- Blocco dell’eliminazione se esistono viaggi futuri confermati collegati al gruppo.
- I viaggi già effettuati restano visibili nello storico personale dei singoli membri.
- Notifica ai membri dell’avvenuta eliminazione.

**9.5 Come caregiver/assistito voglio visualizzare i gruppi a cui appartengo e la pagina di gruppo. (CG-11)**

FRONTEND:

- Pagina principale con una card per ogni gruppo cura di appartenenza.
- La card mostra: nome del gruppo, numero di membri, prossimo viaggio in programma.
- Cliccando la card si apre la pagina di gruppo con: lista assistiti, lista caregiver, prossimi viaggi in programma.
- Stato vuoto con invito a creare il primo gruppo.

BACKEND:

- Lettura dei gruppi dell’utente autenticato e dei relativi membri, con ruolo nel gruppo, ruolo amministrativo e stato dell’invito.
- Aggregazione del prossimo viaggio per ciascun gruppo.

**9.6 Come caregiver/assistito voglio visualizzare le prenotazioni effettuate nel gruppo, con chi ha prenotato e per chi. (CG-12)**

FRONTEND:

- Sezione “Viaggi del gruppo” nella pagina di gruppo, con card che riportano esplicitamente “prenotato da [caregiver] per [assistito]”.
- Separazione fra prossimi viaggi e storico delle prenotazioni.
- Filtro per assistito.

BACKEND:

- Query dei viaggi collegati al gruppo, con i riferimenti a richiedente e beneficiario salvati nell’UC 1.2.
- Visibilità limitata ai soli membri del gruppo con invito accettato.

**9.7 Come caregiver/assistito voglio poter rifiutare un invito a un gruppo cura. (CG-35)**

FRONTEND:

- Nella pagina raggiunta dal link di invito, due azioni: “accetta” e “rifiuta”.
- Messaggio di esito e ritorno alla pagina principale.

BACKEND:

- Il rifiuto è ammesso solo su un invito ancora in attesa: l’invito passa in stato “rifiutato” e registra la data di risposta.
- L’invito rifiutato non dà accesso al gruppo e resta tracciato nella lista membri dell’amministratore.

**9.8 Come amministratore del gruppo voglio poter revocare un invito non ancora accettato. (CG-36)**

FRONTEND:

- Bottone “revoca invito” accanto ai membri in stato “in attesa di accettazione”.
- Pop-up di conferma.

BACKEND:

- La revoca è ammessa solo su un invito ancora in attesa: l’invito passa in stato “revocato” e l’appartenenza viene disattivata.
- Il token di invito non è più utilizzabile.

**9.9 Come amministratore del gruppo voglio poter cambiare il ruolo di un membro. (CG-37)**

FRONTEND:

- Nella lista membri, tendina per il ruolo nel gruppo (caregiver / assistito) e interruttore per il ruolo amministrativo.
- Pop-up di conferma sulla promozione ad amministratore.

BACKEND:

- Il cambio di ruolo, sia nel gruppo sia amministrativo, è ammesso solo su un membro con invito accettato.
- Verifica dei permessi di chi effettua il cambio.
- Il cambio di ruolo non retroagisce sui viaggi già registrati.

## UC 10 – ACCOUNT

**10.1 Come caregiver/assistito o associazione voglio potermi registrare in GoCare. (CG-01, AS-01)**

FRONTEND:

- Pagina di registrazione senza header, con selezione iniziale del tipo di account: “Sono una persona” / “Sono un’associazione”.
- Form utente: nome, cognome, data di nascita, indirizzo, telefono, e-mail, password, conferma password.
- Form associazione: denominazione, sede, province coperte, recapiti, e-mail, password.
- Indicatore di robustezza della password e checkbox di accettazione dell’informativa privacy.
- Messaggio finale: “Ti abbiamo inviato un’e-mail di verifica”.

BACKEND:

- Validazione dei dati e del form; controllo di unicità dell’e-mail.
- Hashing della password.
- Creazione dell’account in stato “non verificato” e invio dell’e-mail di verifica con token a scadenza.
- Creazione contestuale del profilo di dominio corrispondente (persona o associazione) con lo **stesso identificatore** dell’account: i due dati vivono su database distinti, senza foreign key.
- Attivazione dell’account al click sul link di verifica.
- Per le associazioni, dopo l’attivazione l’account è a tutti gli effetti attivo; è il profilo associazione a restare in stato “in attesa di accreditamento” finché non interviene l’UC 10.5, e l’operatività sulle richieste è subordinata all’accreditamento del profilo.

*[DA VALIDARE] Va deciso se la registrazione di un’associazione richieda una validazione manuale da parte di GoCare prima dell’abilitazione operativa. Raccomandazione: sì, perché un’associazione accreditata accede a dati personali di persone fragili.*

**10.2 Come utente voglio poter effettuare il login. (CG-02, AS-02)**

FRONTEND:

- Pagina di login senza header: e-mail, password, “ricordami”, link “password dimenticata”, link alla registrazione.
- Messaggio di errore generico in caso di credenziali errate.
- Dopo l’accesso, reindirizzamento alla pagina principale del ruolo: dashboard richieste per l’associazione, pagina dei gruppi cura per caregiver/assistito.
- Al login vengono mostrati gli eventuali pop-up di notifica pendenti (UC 4).

BACKEND:

- Verifica delle credenziali ed emissione del token di sessione, con token di rinnovo revocabile.
- Blocco dell’accesso agli account non verificati, con messaggio dedicato.
- Limitazione dei tentativi di accesso falliti, con blocco temporaneo.
- Restituzione del ruolo, che determina la navigazione e le voci di header. Il ruolo distingue solo persona e associazione: caregiver e assistito non sono ruoli di account.

**10.3 Come utente voglio poter recuperare la password in caso di smarrimento. (CG-04, AS-03)**

FRONTEND:

- Pagina “Password dimenticata” senza header, con il solo campo e-mail.
- Messaggio neutro: “Se l’indirizzo è registrato, riceverai un’e-mail con le istruzioni”.
- Pagina di impostazione della nuova password, raggiunta dal link ricevuto via e-mail.
- Messaggio di esito e reindirizzamento al login.

BACKEND:

- Generazione di un token di reset a scadenza, monouso.
- Invio dell’e-mail con il link di reset.
- Nessuna informazione sull’esistenza dell’account nella risposta.
- Aggiornamento della password e invalidazione delle sessioni attive.

**10.4 Come utente voglio poter eliminare il mio account. (CG-05, AS-04)**

FRONTEND:

- Bottone “elimina account” in fondo alla pagina di gestione dati personali.
- Pop-up di conferma con reinserimento della password e avviso sulle conseguenze (perdita dell’accesso allo storico, uscita dai gruppi cura).
- Blocco con messaggio esplicativo se esistono viaggi futuri confermati.
- Per l’associazione: blocco se esistono trasporti accettati non ancora conclusi.

BACKEND:

- Soft delete dell’account e del profilo di dominio, e anonimizzazione dei dati personali con registrazione della data di anonimizzazione.
- Verifica dell’assenza di viaggi futuri attivi prima di procedere.
- Rimozione dell’utente dai gruppi cura; se era l’unico amministratore, il ruolo passa a un altro membro o il gruppo viene chiuso.
- Invalidazione di tutte le sessioni e invio dell’e-mail di conferma.

*[DA VALIDARE] Va definita la politica di conservazione dei dati dopo l’eliminazione dell’account: i viaggi conclusi restano nello storico dell’associazione anche dopo l’anonimizzazione dell’utente?*

**10.5 Come amministratore GoCare voglio poter accreditare o rifiutare un’associazione. (AD-01)**

FRONTEND:

- Elenco delle associazioni in attesa di accreditamento, con i dati dichiarati in registrazione.
- Azioni “accredita” e “rifiuta”, con pop-up di conferma.
- Nel profilo dell’associazione, badge con lo stato di accreditamento (in attesa / accreditata / rifiutata).

BACKEND:

- L’associazione nasce in stato “in attesa di accreditamento”; l’azione dell’amministratore la porta in “accreditata” o “rifiutata”.
- Solo le associazioni accreditate entrano nelle liste di destinatari delle nuove richieste (UC 1.1) e possono accettare trasporti.
- In v0 la verifica dell’ente può essere una procedura esterna alla piattaforma: sulla piattaforma resta la sola registrazione dell’esito.

---

# 10. Tabella delle Route

## 10.1 Pagine dell’applicazione

Elenco completo delle pagine dell’applicazione, con path proposto, ruoli abilitati e presenza dell’header. Le pagine di autenticazione sono le uniche prive di header.

| Pagina | Route | Ruoli | Header | UC |
|---|---|---|---|---|
| Registrazione | /register | Pubblico | NO | 10.1 |
| Login | /login | Pubblico | NO | 10.2 |
| Recupero password | /forgot-password | Pubblico | NO | 10.3 |
| Reimposta password | /reset-password/:token | Pubblico | NO | 10.3 |
| Verifica e-mail | /verify-email/:token | Pubblico | NO | 10.1 |
| Pagina principale associazione (dashboard nuove richieste + filtri) | /associazione/richieste | Associazione | SÌ | 6.1, 6.2 |
| Dettaglio richiesta pendente | /associazione/richieste/:id | Associazione | SÌ | 6.2, 6.3, 6.4, 6.5 |
| Pagina privata associazione (richieste accettate, info associazione, richieste di modifica pendenti) | /associazione/trasporti | Associazione | SÌ | 7.1, 7.2, 7.3 |
| Dettaglio trasporto accettato | /associazione/trasporti/:id | Associazione | SÌ | 7.1, 2.3 |
| Gestione stato viaggio (operatore) | /associazione/trasporti/:id/stato | Associazione / Operatore | SÌ | 4.10 |
| Storico trasporti associazione | /associazione/storico | Associazione | SÌ | 7.2 |
| Pagina di modifica informazioni associazione | /associazione/profilo/modifica | Associazione | SÌ | 8.5, 8.6 |
| Profilo associazione (sola lettura) | /associazione/profilo | Associazione | SÌ | 8.5 |
| Pagina principale caregiver/assistito (card dei gruppi cura) | /gruppi | Caregiver / Assistito | SÌ | 9.5 |
| Pagina gruppo cura (assistiti, caregiver, prossimi viaggi, chi ha prenotato per chi) | /gruppi/:id | Caregiver / Assistito | SÌ | 9.5, 9.6 |
| Gestione membri del gruppo | /gruppi/:id/membri | Caregiver / Assistito (amministratore) | SÌ | 9.2, 9.3, 9.4, 9.8, 9.9 |
| Accettazione invito al gruppo cura | /gruppi/inviti/:token | Caregiver / Assistito | SÌ | 9.2, 9.7 |
| Form nuovo trasporto (per chi, data, destinazione, tipo viaggio, accompagnatori, contatti) | /viaggi/nuovo | Caregiver / Assistito | SÌ | 1.1 – 1.7 |
| Dettaglio / modifica viaggio | /viaggi/:id | Caregiver / Assistito | SÌ | 2.1, 2.2, 2.4, 3.1, 5.2, 5.5 |
| Pagina “I miei viaggi” (prossimi viaggi + storico) | /viaggi | Caregiver / Assistito | SÌ | 5.1, 5.2, 5.4 |
| Storico viaggi | /viaggi/storico | Caregiver / Assistito | SÌ | 5.4 |
| Pagina gestione dati personali account | /profilo | Caregiver / Assistito | SÌ | 8.1, 8.2, 8.3, 10.4 |
| Gestione destinazioni salvate | /profilo/destinazioni | Caregiver / Assistito | SÌ | 8.4 |
| Pagina pubblica associazione (contatti) | /associazioni/:id | Caregiver / Assistito | SÌ | 5.3 |
| Centro notifiche | /notifiche | Caregiver / Assistito / Associazione | SÌ | UC 4 |

Note sulle scelte: (a) “Reimposta password” e “Verifica e-mail” sono pagine pubbliche raggiunte da link e-mail, necessarie perché i flussi di registrazione e recupero credenziali si concludano; (b) la pagina di gestione stato è separata dal dettaglio trasporto perché è l’unica schermata usata sul campo dall’operatore durante il viaggio; (c) “Storico viaggi” e “Storico trasporti” possono essere realizzate come tab della pagina principale corrispondente anziché come route autonome: la scelta è indifferente sul piano funzionale; (d) il centro notifiche raccoglie lo storico delle notifiche dell’UC 4, che altrimenti sarebbero consultabili solo al momento della ricezione; (e) la pagina di accettazione invito è raggiunta dal link e-mail dell’UC 9.2 e ospita anche il rifiuto (UC 9.7).

## 10.2 Endpoint di back-end

Elenco degli endpoint che servono le pagine della tabella precedente, con il ruolo abilitato e lo Use Case di riferimento. Gli endpoint dell’area autenticazione lavorano sul database degli account, quelli dell’area dominio sul database di dominio.

| Area | Metodo e path | Ruoli | UC |
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
| Dominio | `POST /transports` | Caregiver / Assistito | 1.1 – 1.7 |
| Dominio | `GET /transports` | Caregiver / Assistito | 5.1, 5.4 |
| Dominio | `GET /transports/:id` | Caregiver / Assistito | 2.x, 3.1, 5.2, 5.5 |
| Dominio | `PATCH /transports/:id/destination` | Caregiver / Assistito | 2.1 |
| Dominio | `PATCH /transports/:id/schedule` | Caregiver / Assistito | 2.2 |
| Dominio | `PUT /transports/:id/companions` | Caregiver / Assistito | 2.3 |
| Dominio | `POST /transports/:id/cancel` | Caregiver / Assistito | 3.1 |
| Dominio | `GET /transports/:id/status-timeline` | Caregiver / Assistito | 5.5 |
| Dominio | `POST /transports/modification-requests/:id/retract` | Caregiver / Assistito | 2.4 |
| Dominio | `POST /transports/modification-requests/:id/approve` | Associazione | 2.3 |
| Dominio | `POST /transports/modification-requests/:id/reject` | Associazione | 2.3 |
| Dominio | `GET /association/modification-requests` | Associazione | 7.3 |
| Dominio | `GET /association/requests` | Associazione | 6.1, 6.2 |
| Dominio | `GET /association/requests/:id` | Associazione | 6.2 |
| Dominio | `POST /association/requests/:id/accept` | Associazione | 6.3 |
| Dominio | `POST /association/requests/:id/decline` | Associazione | 6.4 |
| Dominio | `GET /association/requests/:id/contacts` | Associazione | 6.5 |
| Dominio | `GET /association/transports` | Associazione | 7.1, 7.3 |
| Dominio | `GET /association/transports/:id` | Associazione | 7.1, 2.3 |
| Dominio | `POST /association/transports/:id/status` | Associazione / Operatore | 4.10 |
| Dominio | `POST /association/transports/:id/cancel` | Associazione | 3.2 |
| Dominio | `GET /association/history` | Associazione | 7.2 |
| Dominio | `GET /association/profile` · `PUT /association/profile` | Associazione | 8.5, 8.6 |
| Dominio | `GET /me/profile` · `PUT /me/profile` | Caregiver / Assistito | 8.1 – 8.3 |
| Dominio | `GET · POST · PUT · DELETE /me/destinations[/:id]` | Caregiver / Assistito | 8.4 |
| Dominio | `GET /associations/:id/contacts` | Caregiver / Assistito | 5.3 |
| Dominio | `POST /care-groups` | Caregiver / Assistito | 9.1 |
| Dominio | `GET /care-groups` | Caregiver / Assistito | 9.5 |
| Dominio | `GET /care-groups/:id` | Caregiver / Assistito | 9.5 |
| Dominio | `POST /care-groups/:id/members` | Amministratore gruppo | 9.2 |
| Dominio | `POST /care-groups/invitations/:token/accept` | Caregiver / Assistito | 9.2 |
| Dominio | `POST /care-groups/invitations/:token/decline` | Caregiver / Assistito | 9.7 |
| Dominio | `POST /care-groups/:id/members/:personId/revoke` | Amministratore gruppo | 9.8 |
| Dominio | `PATCH /care-groups/:id/members/:personId/role` | Amministratore gruppo | 9.9 |
| Dominio | `DELETE /care-groups/:id/members/:personId` | Amministratore gruppo | 9.3 |
| Dominio | `DELETE /care-groups/:id` | Amministratore gruppo | 9.4 |
| Dominio | `GET /care-groups/:id/bookings` | Membro gruppo | 9.6 |
| Dominio | `GET /notifications` | Tutti | UC 4 |
| Dominio | `POST /notifications/:id/read` · `POST /notifications/read-all` | Tutti | UC 4 |
| Dominio | `GET /notifications/counters` | Tutti | 11.4 |
| Dominio | `POST /devices` · `DELETE /devices/:id` | Tutti | UC 4 |
| Admin | `POST /admin/associations/:id/accredit` | Amministratore GoCare | 10.5 |
| Admin | `POST /admin/associations/:id/reject` | Amministratore GoCare | 10.5 |

---

# 11. Definizione dell’Header

L’header è presente su tutte le pagine successive all’autenticazione. La struttura è la stessa per tutti i ruoli; cambiano le voci di navigazione centrali.

## 11.1 Struttura comune

| Posizione | Elemento | Comportamento |
|---|---|---|
| Sinistra | Logo “GoCare” | Riporta alla pagina principale del ruolo. |
| Centro | Voci di navigazione | Variabili per ruolo (vedi 11.2 e 11.3). |
| Destra | Icona notifiche con badge numerico | Apre il centro notifiche. Il badge conta le notifiche non lette (UC 4). |
| Destra | Avatar / menu utente | Menu a tendina: Profilo, Impostazioni, Logout. |

## 11.2 Voci di navigazione – Caregiver / Assistito

| Voce | Route | UC che la giustifica | Motivazione |
|---|---|---|---|
| I miei gruppi | /gruppi | 9.5 | Pagina principale del ruolo: le card dei gruppi cura. |
| I miei viaggi | /viaggi | 5.1, 5.4 | Prossimi viaggi e storico. Voce già richiesta come minimo indispensabile. |
| Nuovo trasporto | /viaggi/nuovo | 1.1 | Azione principale del prodotto: resa raggiungibile da ogni pagina. |
| Profilo | /profilo | 8.1 – 8.4 | Dati personali, contatti, destinazioni salvate, eliminazione account. Voce già richiesta come minimo indispensabile. |

“Nuovo trasporto” può essere realizzata come pulsante di azione in evidenza anziché come voce di navigazione: è la funzione più frequente e sarebbe penalizzata se raggiungibile solo dalla pagina “I miei viaggi”.

## 11.3 Voci di navigazione – Associazione

| Voce | Route | UC che la giustifica | Motivazione |
|---|---|---|---|
| Richieste | /associazione/richieste | 6.1, 6.2 | Dashboard delle richieste pendenti con filtri. È la pagina operativa principale dell’associazione; porta un badge con il numero di richieste non ancora valutate (UC 4.7). |
| Trasporti accettati | /associazione/trasporti | 7.1, 7.3 | Vista dei trasporti presi in carico, con evidenza delle richieste di modifica pendenti; da qui si accede alla gestione dello stato del viaggio (UC 4.10). |
| Storico | /associazione/storico | 7.2 | Viaggi conclusi, annullati e rifiutati. Separato dai trasporti attivi per non appesantire la vista operativa. |
| Profilo associazione | /associazione/profilo | 8.5, 8.6 | Informazioni, contatti e province coperte dell’associazione. I contatti sono esposti agli utenti (UC 5.3) e le province determinano quali richieste arrivano, quindi devono essere aggiornabili facilmente. |

L’originale specificava le voci di header solo per caregiver/assistito (“I miei viaggi” e “Profilo”). Le quattro voci del ruolo Associazione sono derivate dagli Use Case 6, 7 e 8: senza “Richieste” e “Trasporti accettati” le due dashboard previste dagli UC 6 e 7 non sarebbero raggiungibili.

## 11.4 Badge e contatori

| Elemento | Ruolo | Cosa conta | UC |
|---|---|---|---|
| Icona notifiche | Tutti | Notifiche non lette | 4.2 – 4.9 |
| Voce “Richieste” | Associazione | Richieste pendenti non ancora valutate | 4.7, 6.1 |
| Voce “Trasporti accettati” | Associazione | Richieste di modifica in attesa di approvazione | 4.8, 7.3 |
| Voce “I miei viaggi” | Caregiver / Assistito | Viaggi con esito o cambio di stato non ancora visualizzato | 4.4, 4.5 |

---

# 12. Flussi

Sequenza logica dei processi principali. I flussi descrivono solo la successione dei passi e i cambi di stato, senza indicazioni grafiche.

## 12.1 Registrazione

1. L’utente apre /register e sceglie il tipo di account (persona oppure associazione).
2. Compila il form corrispondente e accetta l’informativa privacy.
3. Il sistema valida i dati e verifica che l’e-mail non sia già registrata.
4. L’account viene creato in stato “non verificato” e, contestualmente, viene creato il profilo di dominio corrispondente (persona o associazione) con lo stesso identificatore dell’account.
5. Viene inviata l’e-mail di verifica con token a scadenza.
6. L’utente apre il link ricevuto: l’account passa in stato “attivo”.
7. Per le associazioni, l’account è attivo come per una persona; è il profilo associazione a restare in stato “in attesa di accreditamento” da parte di GoCare finché l’amministratore non interviene, e solo l’accreditamento del profilo abilita l’operatività sulle richieste (UC 10.5).
8. L’utente viene reindirizzato al login.

Casi alternativi:

- E-mail già registrata → messaggio di errore sul campo, nessun account creato.
- Link di verifica scaduto → pagina con possibilità di richiedere un nuovo invio.
- Login con account non verificato → accesso negato con invito a verificare l’e-mail.
- Creazione dell’account riuscita ma creazione del profilo fallita → l’incoerenza fra i due database viene recuperata da una riconciliazione, non lasciata all’utente.

## 12.2 Login

1. L’utente apre /login e inserisce e-mail e password.
2. Il sistema verifica le credenziali e lo stato dell’account.
3. In caso di esito positivo viene emesso il token di sessione e restituito il ruolo.
4. L’utente viene reindirizzato alla pagina principale del proprio ruolo: /associazione/richieste per l’associazione, /gruppi per le persone.
5. Vengono mostrati gli eventuali pop-up di notifica pendenti (conferma viaggio, disdetta, esito modifica).

Casi alternativi:

- Credenziali errate → messaggio generico, senza indicare quale campo è sbagliato.
- Superamento del numero di tentativi → blocco temporaneo dell’accesso.
- Associazione non ancora accreditata → accesso consentito al solo profilo, nessuna richiesta visibile.

## 12.3 Recupero password

1. Dalla pagina di login l’utente apre /forgot-password e inserisce la propria e-mail.
2. Il sistema mostra sempre lo stesso messaggio neutro, indipendentemente dall’esistenza dell’account.
3. Se l’account esiste, viene generato un token monouso a scadenza e inviata l’e-mail con il link di reset.
4. L’utente apre /reset-password/:token e imposta la nuova password.
5. Il sistema aggiorna la password, invalida il token e tutte le sessioni attive.
6. L’utente viene reindirizzato al login.

## 12.4 Richiesta di un nuovo trasporto

1. Caregiver o assistito apre /viaggi/nuovo.
2. Se appartiene a un gruppo cura, seleziona il beneficiario del trasporto; altrimenti il beneficiario è l’utente stesso.
3. Indica tipo di viaggio, direzione, data e orario; con “andata e ritorno” indica anche l’orario di ritorno.
4. Seleziona la destinazione dalla lista delle destinazioni salvate oppure ne inserisce una nuova; in questo secondo caso il sistema chiede se salvarla per il futuro. Se il ritorno è verso un indirizzo diverso da quello di partenza, lo indica come terzo indirizzo.
5. Indica gli eventuali accompagnatori e i contatti di riferimento.
6. Il sistema valida i dati e la coerenza fra direzione, orari e indirizzi, copia gli indirizzi e i contatti sulla richiesta e la crea in stato “in attesa”.
7. Il sistema calcola le associazioni destinatarie, cioè quelle accreditate che coprono la provincia di partenza, e congela l’elenco sulla richiesta.
8. Il sistema invia la notifica di nuova richiesta alle associazioni destinatarie (UC 4.7).
9. La richiesta compare nella dashboard /associazione/richieste di ciascuna destinataria, con i relativi filtri.
10. Un’associazione apre il dettaglio e accetta: la richiesta passa in stato “confermata” e le viene assegnata l’associazione.
11. Il sistema notifica l’utente della presa in carico e rende visibili i contatti dell’associazione (UC 4.2, 5.3).
12. La richiesta esce dalla dashboard delle pendenti di tutte le associazioni e compare in /associazione/trasporti dell’associazione assegnataria.

Casi alternativi:

- Un’associazione rifiuta: il rifiuto viene registrato per quella sola associazione, la richiesta resta “in attesa” e continua a essere visibile alle altre destinatarie.
- Due associazioni accettano contemporaneamente: vince la prima, la seconda riceve un messaggio di errore gestito dal controllo di concorrenza sulla richiesta.
- Nessuna associazione accetta entro la soglia definita *[DA VALIDARE]*: la richiesta passa in stato “non coperta” e l’utente riceve la notifica di esito negativo (UC 4.1).
- Nessuna associazione accreditata copre la provincia di partenza: la richiesta nasce senza destinatari e viene dichiarata “non coperta”.

## 12.5 Modifica di un trasporto già richiesto

1. L’utente apre il dettaglio del viaggio da /viaggi e sceglie il campo da modificare (data e orario, destinazione dell’andata, destinazione del ritorno, accompagnatori).
2. Se il viaggio è ancora in stato “in attesa”, la modifica viene applicata direttamente e il flusso termina.
3. Se il viaggio è già “confermato”, il sistema crea una richiesta di modifica in stato “in attesa di approvazione”, con valore precedente e valore proposto, e avvisa l’utente che dovrà essere approvata. La sola modifica degli accompagnatori non passa dall’approvazione: è sempre diretta e genera una notifica informativa (UC 4.12).
4. Il sistema notifica l’associazione assegnataria (UC 4.8) e incrementa il contatore delle modifiche pendenti.
5. Nella pagina /associazione/trasporti il viaggio viene evidenziato ed è filtrabile fra quelli con modifica pendente.
6. L’associazione apre il riquadro di valutazione, che mostra il valore precedente e quello proposto, e approva o rifiuta, con un messaggio esplicativo obbligatorio in caso di rifiuto.
7. Se approvata: la modifica viene applicata al viaggio e la richiesta viene chiusa con esito positivo e data di risoluzione.
8. Se rifiutata: il viaggio resta invariato e la richiesta viene chiusa con esito negativo.
9. In entrambi i casi il sistema notifica l’utente (UC 4.4) e aggiorna il badge sulla card del viaggio (“approvata” / “rifiutata”).

Caso alternativo:

- Finché la richiesta è “in attesa di approvazione”, l’utente può ritirarla: la richiesta passa in stato “ritirata”, il viaggio resta invariato e il contatore dell’associazione viene decrementato. Dopo l’approvazione o il rifiuto il ritiro non è più possibile.

*[DA VALIDARE] Se la richiesta di modifica viene rifiutata, il viaggio resta valido alle condizioni originali. Va confermato che sia il comportamento atteso, o se l’utente debba poter annullare il viaggio contestualmente al rifiuto.*

## 12.6 Annullamento del trasporto

Annullamento da parte dell’utente:

1. L’utente apre il dettaglio del viaggio e preme “annulla viaggio”.
2. Compare il pop-up di conferma con l’avviso che sarà necessaria una nuova richiesta.
3. L’utente seleziona la causale dalla tendina o la scrive nel campo libero e conferma.
4. Il sistema esegue il soft delete: il viaggio passa in stato “annullato”, registra chi ha annullato e quando, e resta nello storico.
5. Se il viaggio era già stato preso in carico, il sistema notifica l’associazione (UC 4.10) e lo rimuove dalla sua lista dei trasporti attivi.

Annullamento da parte dell’associazione (disdetta):

1. L’associazione apre il trasporto accettato e preme “disdici”.
2. Compila la causale, obbligatoria, e conferma nel pop-up.
3. Il sistema registra la disdetta come rifiuto post-accettazione di quella associazione e il viaggio viene rimosso dai trasporti in carico.
4. Il viaggio torna in stato “in attesa” e rientra nella dashboard delle richieste pendenti delle altre associazioni destinatarie, per consentire una copertura alternativa *[DA VALIDARE]*.
5. Il sistema notifica l’utente con il testo della causale (UC 4.3) e gli propone l’azione “cerca un’alternativa”.
6. Se nessun’altra associazione accetta entro la soglia, il viaggio passa in stato “non coperto” e l’utente riceve la notifica di esito negativo.

## 12.7 Ciclo di stato del viaggio

Stati della richiesta (dalla creazione alla presa in carico):

in attesa → confermata → in esecuzione → conclusa

Stati terminali alternativi: non coperta (nessuna associazione destinataria ha accettato), annullata (dall’utente o dall’associazione).

Il rifiuto di una singola associazione non è uno stato della richiesta: viene registrato come rifiuto di quella associazione e la richiesta resta “in attesa” per le altre destinatarie. L’unico esito negativo aggregato è “non coperta”.

Stati di esecuzione del viaggio (UC 4.1), aggiornati dall’operatore (UC 4.10):

| Stato | Chi lo imposta | Significato | Notifiche |
|---|---|---|---|
| Non preso in carico | — | Il viaggio è confermato ma l’operatore non ha ancora iniziato. | Nessuna notifica (stato iniziale). |
| Preso in carico | Operatore | L’operatore ha preso in carico l’assistito. | Notifica push al gruppo cura: “L’assistito è stato preso in carico”. |
| In arrivo | Operatore | Il mezzo è in viaggio verso la destinazione. | Notifica push al gruppo cura. |
| In visita | Operatore | L’assistito è alla destinazione (visita, ricovero, esame). | Notifica push al gruppo cura. |
| In ritorno | Operatore | Viaggio di rientro in corso. Stato non attraversato dai viaggi di sola andata. | Notifica push al gruppo cura. |
| Concluso | Operatore | Il viaggio è terminato. Stato terminale. | Notifica push al gruppo cura; il viaggio passa nello storico di utente e associazione. |

Regole di transizione:

- Gli stati si attraversano in sequenza; non sono ammessi salti in avanti né ritorni indietro.
- Un viaggio di sola andata (ricovero, trasferimento) salta lo stato “in ritorno”.
- Un viaggio di solo ritorno (dimissione) non attraversa lo stato “in visita”.
- Ogni transizione registra stato, timestamp, associazione e operatore che l’ha effettuata, come nuova riga di un registro append-only: lo stato corrente è la transizione più recente e nessuna riga viene modificata.
- I due assi di stato avanzano insieme: alla prima transizione di esecuzione la richiesta passa da “confermata” a “in esecuzione”; con “concluso” passa a “conclusa”.
- Il passaggio a “concluso” chiude il viaggio e lo rende non più modificabile.
- Le notifiche di cambio stato sono solo push: le e-mail sono riservate agli eventi di esito (presa in carico, disdetta, mancata copertura, esito modifica).
- Lo stato della richiesta e lo stato della richiesta di modifica sono due assi indipendenti: un viaggio “confermato” può avere in contemporanea una modifica “in attesa di approvazione” senza cambiare il proprio stato.

## 12.8 Gestione del gruppo cura

Creazione:

1. L’utente apre /gruppi e preme “crea gruppo”.
2. Inserisce il nome del gruppo e conferma.
3. Il sistema crea il gruppo con l’utente come primo membro, amministratore, con ruolo di caregiver e invito già accettato.
4. Il gruppo compare come nuova card nella pagina principale.

Aggiunta di un membro:

1. L’amministratore apre /gruppi/:id/membri e preme “aggiungi membro”.
2. Indica l’e-mail della persona e il ruolo nel gruppo (caregiver o assistito).
3. Il sistema crea l’invito con un token univoco e invia la notifica al destinatario.
4. Nella lista membri l’invito compare in stato “in attesa di accettazione”.
5. Il destinatario apre il link e accetta l’invito: entra nel gruppo e da quel momento i caregiver possono prenotare trasporti per gli assistiti del gruppo e tutti vedono i viaggi del gruppo.

Casi alternativi:

- Il destinatario rifiuta l’invito: l’invito passa in stato “rifiutato”, resta tracciato nella lista membri e non dà accesso al gruppo.
- L’amministratore revoca l’invito prima della risposta: l’invito passa in stato “revocato” e il token non è più utilizzabile.

Cambio di ruolo:

1. L’amministratore apre la lista membri e modifica il ruolo nel gruppo o il ruolo amministrativo di un membro.
2. Il sistema verifica che il membro abbia l’invito accettato e applica il cambio, che vale da quel momento in avanti.

Rimozione di un membro:

1. L’amministratore preme il bottone di rimozione accanto al membro.
2. Conferma nel pop-up, che avvisa della perdita dei permessi di prenotazione.
3. Il sistema verifica l’assenza di viaggi futuri confermati collegati a quel membro; se ce ne sono, richiede una conferma esplicita.
4. L’appartenenza viene disattivata; i viaggi passati restano nello storico del gruppo.

Eliminazione del gruppo:

1. L’amministratore preme “elimina gruppo” e conferma nel pop-up.
2. Il sistema blocca l’operazione se esistono viaggi futuri confermati collegati al gruppo.
3. Altrimenti esegue il soft delete del gruppo e notifica i membri.
4. I viaggi già effettuati restano visibili nello storico personale dei singoli membri.

---

# 13. Cosa manca / Punti aperti

Sezione che riprende ed espande i “PUNTI DA CHIARIRE” dell’originale, con l’aggiunta delle ambiguità emerse durante la revisione. Per ciascun punto è indicata un’opzione consigliata, che resta da validare con il team prima dello sviluppo. Dove l’opzione consigliata è già stata recepita dal modello dati della v0 il punto lo dichiara, ma la decisione non è per questo chiusa: dove incide sui flussi resta segnalata con l’etichetta [DA VALIDARE].

Riepilogo:

| ID | Punto aperto | Impatto | Stato | Origine |
|---|---|---|---|---|
| PA-01 | Modello di prenotazione: richiesta utente o slot offerti | Alto | Aperto | Punto da chiarire n.3 dell’originale |
| PA-02 | Gestione dei contatti durante il trasporto | Alto | Chiuso: solo recapito generico dell’associazione | Punto da chiarire n.2 dell’originale |
| PA-03 | Chi aggiorna lo stato del viaggio e come vi accede | Alto | Aperto | Emerso in revisione (UC 4.10) |
| PA-04 | Soglia oltre la quale una richiesta è “non coperta” | Alto | Aperto | Emerso in revisione (UC 4.1, 6.4) |
| PA-05 | Visibilità delle richieste: tutte le associazioni o per area | Alto | Aperto | Emerso in revisione (UC 6.3) |
| PA-06 | Esposizione dei dati di contatto dell’assistito all’associazione | Alto | Aperto | Story AS-10 (“dati sensibili”) |
| PA-07 | Gestione dei guasti e degli imprevisti lato associazione | Medio | Aperto | Riunione Discord 03/08/26 |
| PA-08 | Ruoli e permessi all’interno del gruppo cura | Medio | Aperto | Emerso in revisione (UC 9) |
| PA-09 | Trasporti ricorrenti in v0 o in v1 | Medio | Chiuso: fuori scope v0 | Story CG-33 (“forse meglio nella v1?”) |
| PA-10 | Visualizzazione della posizione GPS del mezzo | Medio | Chiuso: fuori scope v0 | Story CG-29 (“stato e posizione?”) |
| PA-11 | Accreditamento delle associazioni | Medio | Aperto | Emerso in revisione (UC 10.1) |
| PA-12 | Conferma della prenotazione via e-mail e slot viaggi | Medio | Chiuso: conferma alla presa in carico, nessun calendario di disponibilità | Riunione Discord 03/08/26 |
| PA-13 | Trasporto sociale dentro o fuori dallo scope della v0 | Basso | Chiuso: rinviato alla v1 | Tipi di viaggio dell’originale |
| PA-14 | Politica di conservazione dei dati dopo l’eliminazione account | Basso | Aperto | Emerso in revisione (UC 10.4) |
| PA-15 | Esito del rifiuto di una richiesta di modifica | Basso | Aperto | Emerso in revisione (flusso 12.5) |

## PA-01 Modello di prenotazione: richiesta dell’utente o slot offerti dalle associazioni

Problema: L’originale lascia aperta la domanda “L’utente richiede il trasporto? O le associazioni offrono disponibilità e l’utente seleziona lo slot?”. È la decisione più strutturante del prodotto: cambia il modello dati, il flusso principale (12.4), il contenuto della dashboard associazione e il significato stesso degli stati della richiesta.

Opzione consigliata: Mantenere il modello a richiesta, coerente con l’intero documento e con la decisione già consolidata che GoCare non gestisce le disponibilità delle associazioni. Il modello a slot richiederebbe alle associazioni di pubblicare in anticipo i turni disponibili, un onere organizzativo che la maggior parte delle associazioni di volontariato non è in grado di sostenere. Tutto il documento è scritto assumendo il modello a richiesta: se il team scegliesse il modello a slot, gli UC 1, 4, 5 e 6 andrebbero riscritti.

Stato in v0: Il modello dati realizzato è quello a richiesta.

Origine: Punto da chiarire n.3 dell’originale, non risolto.

## PA-02 Gestione dei contatti durante il trasporto

Problema: L’originale segnala il problema e lo ripete nel dettaglio SPRINT 5.1: “va deciso se diamo il contatto dell’associazione generico e durante il viaggio quello dell’operatore, o solo quello generico”. Senza una decisione, la card del viaggio dell’UC 5 non è specificabile.

Opzione consigliata: Esporre sempre e solo il recapito generico dell’associazione. Il recapito personale dell’operatore è un dato del volontario, non dell’ente, e la sua diffusione richiederebbe un consenso individuale; inoltre lo stato in tempo reale (UC 4.1) esiste proprio per ridurre la necessità di chiamare durante il viaggio. Se emergesse la necessità di un canale diretto, la soluzione preferibile è un numero di reperibilità di turno gestito dall’associazione, non il numero personale dell’operatore.

Stato in v0: Chiuso sull’opzione consigliata. L’associazione espone telefoni, e-mail e orari di reperibilità; l’operatore compare solo come etichetta testuale sulla transizione di stato, senza recapiti.

Origine: Punto da chiarire n.2 dell’originale.

## PA-03 Chi aggiorna lo stato del viaggio e come vi accede

Problema: L’originale prometteva lo stato in tempo reale (soluzione n.3, UC 4.1) ma nessun Use Case né user story descriveva chi lo alimenta. Il dettaglio SPRINT 4.4 riportava esplicitamente il dubbio: “cambio di stato gestito dall’operatore (come? gli diamo un link su cui effettuare il cambio? da pensare)”. In questa revisione la funzione è stata introdotta (UC 4.10, story AS-19), ma il modello di accesso resta da decidere.

Opzione consigliata: Per la v0, gestire i cambi di stato dall’account dell’associazione, dalla pagina del trasporto accettato: non richiede account individuali per i volontari né un’app dedicata, e mantiene la tracciabilità dell’operazione. Un account per operatore, o un link monouso inviato via SMS, sono evoluzioni sensate per la v1 ma introducono in v0 una gestione di identità sproporzionata.

Stato in v0: Il modello dati recepisce l’opzione consigliata — ogni transizione registra l’associazione che l’ha effettuata e un’etichetta testuale dell’operatore (es. “Mauro Rossi, autista”) — ma la decisione va confermata.

Origine: Lacuna rilevata in revisione; dubbio già presente nel dettaglio SPRINT dell’originale.

## PA-04 Soglia oltre la quale una richiesta è considerata “non coperta”

Problema: La story CG-25 (“voglio sapere se il viaggio non può essere coperto”) e la notifica dell’UC 4.4 presuppongono un momento in cui il sistema dichiara fallita la ricerca di copertura, ma nessuna regola lo definisce. Senza questa regola l’utente resta indefinitamente in attesa e non può cercare alternative.

Opzione consigliata: Dichiarare la richiesta “non coperta” al verificarsi della prima delle due condizioni: tutte le associazioni destinatarie hanno rifiutato, oppure mancano meno di 24 ore alla data del viaggio e nessuna l’ha accettata. La seconda condizione è quella che conta davvero: lascia all’utente il tempo di organizzarsi diversamente.

Stato in v0: Aperto. Il modello dati registra i rifiuti per associazione e la lista congelata delle destinatarie, quindi entrambe le condizioni sono calcolabili; manca la scelta della soglia.

Origine: Lacuna rilevata in revisione (UC 4.1, 6.4).

## PA-05 Visibilità delle richieste: tutte le associazioni o per area geografica

Problema: La dashboard dell’UC 6 mostra “tutte le richieste di trasporto dei caregiver/assistiti”, ma non è specificato se ogni associazione veda tutte le richieste della piattaforma o solo quelle del proprio territorio.

Opzione consigliata: Introdurre un’area di operatività nel profilo dell’associazione (UC 8.5) e mostrare a ciascuna associazione solo le richieste con partenza entro la propria area. In fase di test, con poche associazioni pilota, la regola può essere disattivata; senza di essa, però, il prodotto non è scalabile oltre una singola città.

Stato in v0: Il modello dati recepisce l’opzione consigliata — l’associazione dichiara un elenco di province coperte, confrontato con la provincia dell’indirizzo di partenza; l’elenco delle destinatarie viene congelato sulla richiesta alla creazione — ma il criterio (provincia anziché raggio in km o comune) va confermato.

Origine: Lacuna rilevata in revisione (UC 6.1, 6.3).

## PA-06 Esposizione dei dati di contatto dell’assistito all’associazione

Problema: La story AS-10 dell’originale è annotata con “dati sensibili”: l’associazione deve poter contattare chi ha prenotato, ma il documento non definisce a quali dati acceda, da quale momento e con quale base giuridica. Il tema riguarda persone fragili e dati sanitari indiretti (il tipo di viaggio rivela la natura della prestazione).

Opzione consigliata: Mostrare nella fase di valutazione della richiesta (UC 6.1, 6.2) solo i dati necessari a decidere: iniziali o nome, fascia oraria, indirizzo di partenza a livello di via, destinazione, esigenze di mobilità. Rendere visibili i recapiti completi solo dopo l’accettazione (UC 6.5), tracciando gli accessi. Va inoltre redatta un’informativa privacy, richiamata in fase di registrazione (UC 10.1).

Stato in v0: Il modello dati recepisce la parte di tracciamento — ogni accesso ai contatti registra richiesta, associazione, tipo di dato (richiedente o beneficiario) e istante — mentre la soglia di visibilità dei dati minimi resta da definire.

Origine: Annotazione “dati sensibili” dell’originale, mai sviluppata.

## PA-07 Gestione dei guasti e degli imprevisti lato associazione

Problema: La riunione del 03/08/26 annota “Guasto, come gestirli da parte Associazione” senza sviluppo. Un guasto del mezzo a viaggio già iniziato è diverso da una disdetta preventiva: l’assistito può trovarsi fuori casa, con un rientro da garantire.

Opzione consigliata: Distinguere due casi. Prima dell’inizio del viaggio: la disdetta con causale già prevista (UC 3.2), con rientro automatico della richiesta fra le pendenti. Durante il viaggio: introdurre lo stato eccezionale “sospeso – imprevisto”, che notifica immediatamente il gruppo cura e mantiene il viaggio in carico all’associazione, che resta responsabile del rientro. Trattare un guasto in corsa come una disdetta ordinaria lascerebbe l’assistito senza referente.

Stato in v0: Aperto. Lo stato “sospeso – imprevisto” non è previsto fra gli stati di esecuzione della v0.

Origine: Riunione Discord del 03/08/26.

## PA-08 Ruoli e permessi all’interno del gruppo cura

Problema: L’UC 9 dell’originale elenca creazione, aggiunta, rimozione ed eliminazione senza indicare chi possa compierle. Se tutti i membri avessero gli stessi diritti, qualunque assistito potrebbe rimuovere il proprio caregiver o eliminare il gruppo. Non è inoltre definito come una persona entri nel gruppo.

Opzione consigliata: Introdurre il ruolo di amministratore (per default il creatore) con diritto esclusivo di aggiungere, rimuovere ed eliminare, e prevedere l’ingresso tramite invito con accettazione esplicita. L’appartenenza al gruppo abilita la prenotazione per conto terzi e la visione dei dati altrui: non può essere unilaterale.

Stato in v0: Il modello dati recepisce l’opzione consigliata — ruolo amministrativo distinto dal ruolo nel gruppo, invito con token, accettazione, rifiuto e revoca — ma resta da definire il perimetro esatto dei permessi dell’amministratore.

Origine: Lacuna rilevata in revisione (UC 9.1 – 9.4).

## PA-09 Trasporti ricorrenti in v0 o in v1

Problema: La story CG-33 riporta l’annotazione dell’originale “forse meglio inserirlo nella v1?” ed è l’unica voce dell’UC 1 rimasta priva di dettaglio FE/BE nella sezione SPRINT.

Opzione consigliata: Escludere dalla v0. La ricorrenza implica generazione automatica delle occorrenze, gestione delle eccezioni sulla singola data, annullamento in blocco e comportamento definito quando un’occorrenza non viene coperta: è un sottosistema, non un campo del form. In v0 l’utente ripete la richiesta.

Stato in v0: Chiuso, fuori scope confermato.

Origine: Annotazione dell’originale.

## PA-10 Visualizzazione della posizione del mezzo

Problema: La story sullo stato in tempo reale dell’originale riporta “stato (e posizione?)”. Il punto interrogativo non è mai stato sciolto e la posizione non compare in nessun Use Case.

Opzione consigliata: Escludere la geolocalizzazione dalla v0. Richiede tracciamento continuo del mezzo, consenso esplicito del volontario e trattamento di dati di localizzazione; lo stato testuale con timeline copre già l’esigenza dichiarata nel problema n.2, cioè evitare le ripetute chiamate all’associazione.

Stato in v0: Chiuso, fuori scope confermato.

Origine: Annotazione dell’originale.

## PA-11 Accreditamento delle associazioni

Problema: L’UC 10 tratta la registrazione delle associazioni come quella di un utente qualsiasi. Un account associazione, però, accede ai dati personali di persone fragili e può prendere in carico trasporti sanitari.

Opzione consigliata: Prevedere una validazione manuale da parte di GoCare prima dell’abilitazione operativa dell’account associazione: registrazione libera, ma stato “in attesa di accreditamento” finché l’ente non viene verificato. In v0 la verifica può essere una procedura esterna alla piattaforma.

Stato in v0: Il modello dati recepisce l’opzione consigliata — l’associazione porta uno stato di accreditamento con tre valori (in attesa, accreditata, rifiutata) e nasce in attesa — ma restano da definire i criteri di verifica e chi li applica.

Origine: Lacuna rilevata in revisione (UC 10.1).

## PA-12 Conferma della prenotazione via e-mail e slot viaggi

Problema: La riunione del 03/08/26 annota “Slot viaggi, conferma post prenotazione tramite mail” e “facilità per l’utente di capire quali giorni sono disponibili”. Entrambe le note presuppongono una nozione di disponibilità che il documento dichiara altrove di non gestire (vedi PA-01): c’è una contraddizione da sciogliere.

Opzione consigliata: Mantenere la conferma via e-mail alla presa in carico (già prevista nell’UC 4.2) ed evitare in v0 qualunque indicazione di disponibilità nel calendario, che il sistema non è in grado di conoscere. Al posto della disponibilità, comunicare chiaramente all’utente che la richiesta è in attesa di accettazione e con quale anticipo conviene inviarla.

Stato in v0: Chiuso sull’opzione consigliata.

Origine: Riunione Discord del 03/08/26; collegato a PA-01.

## PA-13 Trasporto sociale dentro o fuori dallo scope

Problema: Nell’elenco dei tipi di viaggio l’originale annota per il trasporto sociale “[forse fuori scope?]”.

Opzione consigliata: Rinviarlo alla v1 e riproporlo dopo la validazione sul trasporto sanitario. Non introduce complessità tecnica, ma cambia il posizionamento del prodotto e il tipo di associazioni da coinvolgere.

Stato in v0: Chiuso, rinviato alla v1: in v0 i tipi di viaggio previsti sono quattro (visita, ricovero, dimissione, trasferimento).

Origine: Annotazione dell’originale.

## PA-14 Conservazione dei dati dopo l’eliminazione dell’account

Problema: L’UC 10 prevede l’eliminazione dell’account ma non dice cosa accada ai viaggi conclusi presenti nello storico dell’associazione, che potrebbe averne bisogno per rendicontazione.

Opzione consigliata: Anonimizzare i dati personali dell’utente mantenendo il viaggio nello storico dell’associazione con i soli dati di servizio (data, tratta, esito). Da verificare con i requisiti di rendicontazione delle associazioni coinvolte.

Stato in v0: Il modello dati recepisce l’opzione consigliata — la persona porta una data di cancellazione e una data di anonimizzazione distinte, e i viaggi conservano indirizzi e contatti copiati al momento della richiesta — ma la politica di retention va confermata.

Origine: Lacuna rilevata in revisione (UC 10.4).

## PA-15 Esito del rifiuto di una richiesta di modifica

Problema: Se l’associazione rifiuta una modifica di data o destinazione, il viaggio resta valido alle condizioni originali, che però potrebbero non essere più praticabili per l’utente (è per questo che aveva chiesto la modifica).

Opzione consigliata: Nella notifica di rifiuto (UC 4.4) proporre esplicitamente le due azioni: mantenere il viaggio così com’è oppure annullarlo e inviare una nuova richiesta. Lasciare l’utente davanti al solo esito negativo è la causa più probabile di viaggi confermati ma non effettuati.

Stato in v0: Aperto. Il rifiuto porta un messaggio esplicativo obbligatorio, ma le azioni proposte all’utente non sono definite.

Origine: Lacuna rilevata in revisione (flusso 12.5).
