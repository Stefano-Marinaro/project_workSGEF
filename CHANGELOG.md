# Changelog GoCare

## Modifiche locali — 14 novembre 2026

Riepilogo delle modifiche effettuate durante questa sessione. Le modifiche sono presenti nel progetto locale; non sono state pubblicate su GitHub.

### Registrazione, accesso e profilo

- Aggiunta la pagina **Account creato**, con indicazioni sulla verifica dell’email.
- Nome e cognome vengono inviati al backend e salvati già durante la registrazione, insieme all’account e al token di verifica.
- Corrette le iniziali del profilo al primo accesso; mantenuto un recupero dei nomi salvati sul dispositivo per gli account creati con il precedente flusso.
- Aggiunta un’icona del profilo quando le iniziali non sono disponibili.
- Rimossa la schermata iniziale provvisoria: all’apertura si passa al login oppure alla sezione del ruolo già autenticato.
- Corretto il confronto dell’email in accesso, recupero password e reinvio della verifica, per gestire maiuscole/minuscole in modo coerente.
- Allineati i confronti dei duplicati in registrazione e cambio email.
- Aggiunta la pagina di conferma del cambio email, con ritorno all’accesso dopo la conferma.
- Corretto il rinnovo della sessione: richieste contemporanee condividono un solo refresh del token.
- Un errore temporaneo di rete o del server durante il refresh non cancella più automaticamente la sessione.

### Navigazione e interfaccia

- Riorganizzata la navigazione caregiver, separando le pagine interne dalle tre voci principali: **Trasporti**, **Nuovo trasporto** e **Assistiti**.
- Rimosso il reindirizzamento automatico al profilo incompleto quando si apre Nuovo trasporto.
- Il completamento del referente viene richiesto esplicitamente nel passaggio dedicato ai contatti.
- Aggiunta la conferma del salvataggio del profilo quando si torna alla richiesta di trasporto.
- La richiesta in corso viene mantenuta durante il completamento del profilo.
- Corretto il campo data su web mobile per contenerlo nella larghezza disponibile.
- Reso il box della data di nascita nel profilo più stretto e allineato a sinistra; corretto anche l’allineamento del testo su Safari.
- Aggiunta la favicon con il logo GoCare.
- Aggiunta l’icona Cronologia prima di **Storico dei trasporti**; nella versione web è stato mantenuto il disegno originale riducendone lo spessore.
- Corretti proprietà e import della barra di stato.

### Richiesta di trasporto

- **Ricovero, dimissione e trasferimento** consentono solo andata: nascosta l’opzione Andata e ritorno e allineate le regole del backend.
- Per le visite di andata e ritorno, il ritorno all’indirizzo iniziale è la scelta predefinita; resta possibile scegliere una destinazione diversa.
- Rimosso il campo **Ora di ritorno** dalle nuove richieste di andata e ritorno.
- Il ritorno viene indicato come **Al termine della visita**, senza un orario prefissato.
- Aggiornati validazione backend, riepilogo, dettagli caregiver/associazione e FAQ per il ritorno senza orario.
- Mantenuta la compatibilità con gli orari di ritorno già presenti nei vecchi dati.
- Nel passaggio 4, sostituite le checkbox degli aiuti con radio button: nessun aiuto aggiuntivo, uno dei due aiuti oppure entrambi.
- Rimosso il pulsante **Registra assistito** dal passaggio 4 di 5.
- Sistemato il ritorno alla richiesta dopo la registrazione di un assistito, preservando i dati già inseriti.

### Donazione e totale stimato

- Reso bianco lo sfondo del campo Donazione aggiuntiva.
- Accettati punto o virgola come separatore decimale, con un massimo di due decimali.
- Aggiunta la validazione per valori vuoti, zero, negativi, testo, formati non validi e importi troppo grandi.
- Il totale viene calcolato in centesimi per evitare errori di arrotondamento e valori non finiti.
- Imposto un massimo di **10 cifre complessive**, senza contare punto o virgola, anche quando si incolla un valore.
- In caso di input oltre il limite, mantenuto il valore precedente e mostrato un messaggio.
- Reso il layout del totale adattabile su schermi stretti.
- Evitato che una donazione incompleta blocchi il ritorno ai passaggi precedenti del modulo.

### Annullamento dei viaggi e gestione assistiti

- Corretto l’annullamento dei viaggi non pagati: non richiede più l’inizializzazione di Stripe.
- Stripe viene richiesto solo quando occorre effettuare un rimborso; la mancanza di configurazione produce un messaggio gestito.
- Aggiunto un endpoint dedicato a **Elimina assistito**, distinto dalla rimozione di un caregiver.
- Con altri caregiver presenti, l’assistito viene rimosso solo dalla lista del richiedente.
- Se il richiedente è l’unico caregiver, l’assistito viene archiviato mantenendo lo storico.
- L’archiviazione dell’ultimo caregiver viene bloccata se esistono trasporti in attesa, confermati o in corso.
- Impedito di ricollegare un assistito archiviato tramite un vecchio invito.
- Un invito non è più utilizzabile se il caregiver che lo ha generato non è più collegato all’assistito.
- Corretti i link operatore dopo annullamento o nuova presa in carico: viene restituito un errore gestito invece di un errore interno.

### Aiuto e informazioni

- Aggiunta la sezione **Profilo → Aiuto e informazioni**, riutilizzando componenti, colori e spaziature del progetto.
- Inseriti guida al primo trasporto, FAQ espandibili, spiegazione degli stati, informazioni sui pagamenti e indicazioni per assistenza/segnalazioni.
- Inserito l’avviso che GoCare non è un servizio di emergenza.
- Aggiunte le voci Privacy e Condizioni d’uso.
- Resi configurabili email di assistenza e URL dei documenti; in loro assenza viene segnalata l’indisponibilità.
- Aggiunte le pagine di ritorno dal pagamento. Il ritorno dal browser non viene considerato una conferma dell’addebito: resta necessario l’esito del webhook.

### Configurazione, database e dipendenze

- Configurato l’accesso dal telefono tramite l’indirizzo del PC sulla rete locale.
- Aggiunta `FRONTEND_PUBLIC_URL` per usare un’origine raggiungibile dal telefono nei link di verifica, reset, invito e operatore.
- Configurata localmente la chiave GraphHopper fornita dall’utente, senza inserirla nei file versionati.
- Gestiti con messaggi comprensibili gli errori di configurazione, connessione e risposta del servizio percorsi.
- Corretto il salvataggio delle date di creazione e scadenza degli inviti caregiver, prima escluse dal modello EF.
- Generata e applicata la migrazione `PersistCaregiverInviteLifetime`.
- Aggiornati Expo, Expo Constants, Expo Linking ed Expo Router alle patch compatibili con SDK 57.
- Applicate le correzioni npm compatibili senza forzare cambi di versione maggiore.
- Rigenerato il lockfile con la versione npm di Docker e verificata l’installazione pulita.
- Ricostruiti e aggiornati i container locali dopo le modifiche.

### Verifiche automatiche

- **103 test backend** superati.
- **79 controlli API** superati con dati sintetici in un database separato e temporaneo.
- **3 test di regressione della sessione** superati.
- **5 test di regressione della donazione** superati.
- Verificati sintassi, identificatori non definiti, destinazioni di navigazione, nomi delle icone e corrispondenza delle chiamate API.
- Verificate compatibilità Expo e compilazione Docker.
- Aggiunti script ripetibili nella cartella `scripts/` e un rapporto in `docs/APP_HEALTH_CHECK_2026-11-14.md`.
- Le verifiche sono state effettuate **senza browser**. I controlli API non hanno usato gli account e i viaggi esistenti come dati di prova.

### Aspetti ancora da completare

- Mobilità e aiuti selezionati nel modulo non vengono ancora salvati dal backend.
- La donazione aggiuntiva è validata e mostrata nel totale del frontend, ma non viene ancora salvata né inclusa nell’addebito dal backend.
- Restano da configurare e verificare pagamenti/rimborsi Stripe reali, invio email e notifiche push; manca l’azione di checkout nel dettaglio caregiver.
- Alcuni testi del modulo sui pagamenti devono essere allineati al flusso definitivo.
- Recapiti di assistenza e documenti privacy/condizioni richiedono contenuti reali.
- La schermata di modifica password rimane provvisoria.
- Restano segnalazioni di sicurezza nelle dipendenze, dettagliate nel rapporto di verifica.
- Layout e comportamento effettivo sui dispositivi richiedono prove manuali.

**Nota sugli inviti:** i vecchi link caregiver non hanno una scadenza originale recuperabile e restano scaduti dopo la migrazione. Per usarli occorre generarne di nuovi.
