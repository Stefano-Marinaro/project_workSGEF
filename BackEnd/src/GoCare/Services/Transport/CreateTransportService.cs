using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Models.Enums;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;
using GoCare.Errors;

namespace GoCare.Services.Transport;

public sealed class CreateTransportService(GoCareDbContext db, IRoutingService routing)
{
    public async Task<Guid> CreateTransportAsync(
        Guid requestedById, //Id dell'utente 
        Guid beneficiaryId, //Id della persona assistita 
        ETripType tripType, //tipo di viaggio(solo andata/ andata e ritorno)
        ETripDirection tripDirection, //direzione del viaggio
        DateTimeOffset departureDateHour, //data e ora di partenza, TimeOffset include anche il fuso orario
        DateTimeOffset? returnDateHour, //data e ora del ritorno, ? lo rende all'occorrenza nullable se è solo andata
        TripAddress startAddress, // indirizzo di partenza (con coordinate obbligatorie, servono per il calcolo del percorso)
        TripAddress endAddress, //indirizzo di destinazione
        TripAddress? returnEndAddress, //di ritorno, nullable in caso
        string referencePhone,
        string? referenceEmail,
        IReadOnlyList<(string Name, string Surname, string Relationship, string Phone)> companion, //lista in sola lettura degli accompagnatori, una tupla con campi dotati di nome
        CancellationToken ct) //Token di cancellazione: se il client chiude la connessione o la richiesta HTTP viene annullata, le operazioni sul DB vengono interrotte invece di proseguire inutilmente
    {
        //CONTROLLO DI AUTORIZZAZIONE
        
        // isLinked: esiste un collegamento tra caregiver e assistito?,
        // AnyAsync traduce la condizione in una query SQL tipo SELECT EXISTS e restituisce true/false,
        //DeletedAt == null, il soft delete i record non vengono cancellati in hard ma marcati con una data di cancellazione 
        var isLinked = await db.CaregiverAssistedLinks.AnyAsync(
            l => l.CaregiverId == requestedById && l.AssistedPersonId == beneficiaryId && l.DeletedAt == null, ct);
        if (!isLinked)
            throw new ForbiddenException("Non sei collegato a questo assistito.");

        // CALCOLO DEL PERCORSO (km), serve per il prezzo: andata sempre, + ritorno se RoundTrip
        // (verso returnEndAddress se indicato, altrimenti verso l'indirizzo di partenza - stessa
        // regola già usata da NewTransportValidator per il significato di "ritorno assente").
        var distanceKm = await routing.GetDistanceKmAsync(
            startAddress.Latitude, startAddress.Longitude, endAddress.Latitude, endAddress.Longitude, ct);

        if (tripDirection == ETripDirection.RoundTrip)
        {
            var returnDestination = returnEndAddress ?? startAddress;
            distanceKm += await routing.GetDistanceKmAsync(
                endAddress.Latitude, endAddress.Longitude, returnDestination.Latitude, returnDestination.Longitude, ct);
        }

        // CREAZIONE DELLA RICHIESTA
        var now = DateTimeOffset.UtcNow;
        var transportId = Guid.NewGuid(); // crea l'Id per il trasporto

        var transport = new TransportRequest(
            transportId, requestedById, beneficiaryId, tripType, tripDirection, departureDateHour,
            returnDateHour, startAddress, endAddress, returnEndAddress, referencePhone, referenceEmail, distanceKm, now);

        db.TransportRequests.Add(transport);

        //ACCOMPAGNATORI
        foreach (var c in companion)
            db.Companions.Add(new Companion(Guid.NewGuid(), transportId, c.Name, c.Surname, c.Relationship, c.Phone));

            //ASSOCIAZIONI CANDIDATE
            // codice ISTAT del comune di partenza (AddressValidator ha già verificato che esista)
            var startCodiceIstat = await db.Comuni
                .Where(c => c.NomeComune == startAddress.City && c.Provincia == startAddress.Province)
                .Select(c => c.CodiceIstat)
                .SingleAsync(ct);

            // associazioni attive che hanno quel comune fra i propri comuni coperti
            var candidateAssociationIds = await db.Associations
                .Where(a => a.Status == EAccreditationStatus.Accredited
                        && a.DeletedAt == null
                        && db.AssociationCoveredComuni.Any(c => c.AssociationId == a.Id && c.ComuneCodiceIstat == startCodiceIstat))
                .Select(a => a.Id)
                .ToListAsync(ct);

        // Per ogni associazione trovata registra una riga TransportRequestCandidate, permetterà a ciascuna associazione di vedere l'elenco delle richieste pendenti nella propria zona
        foreach (var associationId in candidateAssociationIds)
            db.TransportRequestCandidates.Add(new TransportRequestCandidate(transportId, associationId));

        await db.SaveChangesAsync(ct); //genera tutte le INSERT

        return transportId; //restituisce l'Id della richiesta appena creata, cosi che il chiamante (il controller), può rispondere al client ad esempio con un 201 Created che contiene l'Id
    }
}


