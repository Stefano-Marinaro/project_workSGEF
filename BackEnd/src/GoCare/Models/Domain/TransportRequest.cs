using GoCare.Models.Enums;

namespace GoCare.Models.Domain;
public sealed class TransportRequest
{
    // Costruttore vuoto privato: lo usa SOLO EF per materializzare dal DB.
    // Non passa l'invariante (i dati sul DB sono già validi), quindi non chiama EnsureScheduleConsistency.
    private TransportRequest() { }

    // Costruttore vero, usato dall'applicazione.
    public TransportRequest(
        Guid id, Guid requestedById, Guid beneficiaryId, ETripType tripType, ETripDirection tripDirection,
        DateTimeOffset departureDateHour, DateTimeOffset? returnDateHour, Address startAddress, Address endAddress, Address? returnEndAddress,
        string referencePhone, string? referenceEmail, Guid preferredContactPersonId, DateTimeOffset createdAt)
    {
        Id = id;
        RequestedById = requestedById;
        BeneficiaryId = beneficiaryId;
        TripType = tripType;
        TripDirection = tripDirection;
        DepartureDateHour = departureDateHour;
        ReturnDateHour = returnDateHour;
        StartAddress = startAddress;
        EndAddress = endAddress;
        ReturnEndAddress = returnEndAddress;
        ReferencePhone = referencePhone;
        ReferenceEmail = referenceEmail;
        PreferredContactPersonId = preferredContactPersonId;
        CreatedAt = createdAt;

        EnsureScheduleConsistency(departureDateHour, returnDateHour, returnEndAddress);
    }

    public Guid Id { get; }
    public Guid RequestedById { get; }     // Person.Id del caregiver che ha richiesto il trasporto
    public Guid BeneficiaryId { get; }     // AssistedPerson.Id del beneficiario
    public DateTimeOffset CreatedAt { get; }
    public ETripType TripType { get; private set; }
    public ETripDirection TripDirection { get; private set; }
    public DateTimeOffset DepartureDateHour { get; private set; }
    public DateTimeOffset? ReturnDateHour { get; private set; }
    public Address StartAddress { get; private set; } = null!;
    public Address EndAddress { get; private set; } = null!;
    public Address? ReturnEndAddress { get; private set; }
    public string ReferencePhone { get; private set; } = null!;
    public string? ReferenceEmail { get; private set; }
    public Guid PreferredContactPersonId { get; private set; } // Person.Id del caregiver scelto come contatto preferenziale (può essere RequestedById o un altro caregiver collegato all'assistito)
    public ETripRequestStatus RequestStatus { get; private set; } = ETripRequestStatus.Pending;
    public Guid? AssignedAssociationId { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public DateTimeOffset? NotCoveredAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    private void EnsureScheduleConsistency(
        DateTimeOffset departure,
        DateTimeOffset? returnDateHour,
        Address? returnEndAddress)
    {
        var isRoundTrip = TripDirection == ETripDirection.RoundTrip;

        if (isRoundTrip && returnDateHour is null)
            throw new InvalidOperationException("Un RoundTrip richiede l'orario di ritorno.");

        if (!isRoundTrip && (returnDateHour is not null || returnEndAddress is not null))
            throw new InvalidOperationException("Solo un RoundTrip può avere orario o indirizzo di ritorno.");

        if (isRoundTrip && (TripType == ETripType.Hospitalization || TripType == ETripType.Discharge || TripType == ETripType.Transfer))
            throw new InvalidOperationException("Ricovero, Dimissione e Trasferimento ammettono solo un viaggio OneWay, non Andata e ritorno.");

        if (returnDateHour is not null && returnDateHour <= departure)
            throw new InvalidOperationException("Il ritorno deve essere successivo alla partenza.");
    }

    public void Accept(Guid associationId, DateTimeOffset at)
    {
        if (RequestStatus != ETripRequestStatus.Pending)
            throw new InvalidOperationException("Si può accettare solo una richiesta in attesa.");

        RequestStatus = ETripRequestStatus.Confirmed;
        AssignedAssociationId = associationId;
        AcceptedAt = at;
    }

    public void ChangeSchedule(DateTimeOffset newDepartureDateHour, DateTimeOffset? newReturnDateHour)
    {
        EnsureScheduleConsistency(newDepartureDateHour, newReturnDateHour, ReturnEndAddress);

        DepartureDateHour = newDepartureDateHour;
        ReturnDateHour = newReturnDateHour;
    }

    public void ChangeDestination(Address newEndAddress, Address? newReturnEndAddress)
    {
        EnsureScheduleConsistency(DepartureDateHour, ReturnDateHour, newReturnEndAddress);

        EndAddress = newEndAddress;
        ReturnEndAddress = newReturnEndAddress;
    }

    public void StartExecution()
    {
        if (RequestStatus != ETripRequestStatus.Confirmed)
            throw new InvalidOperationException("Solo una richiesta confermata può avviare l'esecuzione.");

        RequestStatus = ETripRequestStatus.InProgress;
    }

    public void CompleteExecution()
    {
        if (RequestStatus != ETripRequestStatus.InProgress)
            throw new InvalidOperationException("Solo una richiesta in esecuzione può concludersi.");

        RequestStatus = ETripRequestStatus.Completed;
    }

    public void RequestNotCovered(DateTimeOffset at)
    {
        RequestStatus = ETripRequestStatus.NotCovered;
        NotCoveredAt = at;
    }

    public void Cancel(Guid cancelledBy, DateTimeOffset at)
    {
        RequestStatus = ETripRequestStatus.Cancelled;
        DeletedBy = cancelledBy;
        DeletedAt = at;
    }
}
