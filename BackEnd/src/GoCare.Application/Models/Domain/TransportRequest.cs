using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;
public sealed class TransportRequest
{
    // Costruttore vuoto privato: lo usa SOLO EF per materializzare dal DB.
    // Non passa l'invariante (i dati sul DB sono già validi), quindi non chiama EnsureScheduleConsistency.
    private TransportRequest() { }

    // Costruttore vero, usato dall'applicazione.
    public TransportRequest(
        Guid id, Guid requestedById, Guid beneficiaryId, Guid? careGroupId, ETripType tripType, ETripDirection tripDirection,
        DateTimeOffset departureDateHour, DateTimeOffset? returnDateHour, Address startAddress, Address endAddress, Address? returnEndAddress,
        string referencePhone, string referenceEmail, DateTimeOffset createdAt)
    {
        Id = id;
        RequestedById = requestedById;
        BeneficiaryId = beneficiaryId;
        CareGroupId = careGroupId;
        TripType = tripType;
        TripDirection = tripDirection;
        DepartureDateHour = departureDateHour;
        ReturnDateHour = returnDateHour;
        StartAddress = startAddress;
        EndAddress = endAddress;
        ReturnEndAddress = returnEndAddress;
        ReferencePhone = referencePhone;
        ReferenceEmail = referenceEmail;
        CreatedAt = createdAt;

        EnsureScheduleConsistency(departureDateHour, returnDateHour, returnEndAddress);
    }

    public Guid Id { get; }
    public Guid RequestedById { get; }
    public Guid BeneficiaryId { get; }
    public DateTimeOffset CreatedAt { get; }

    public Guid? CareGroupId { get; private set; }
    public ETripType TripType { get; private set; }
    public ETripDirection TripDirection { get; private set; }
    public DateTimeOffset DepartureDateHour { get; private set; }
    public DateTimeOffset? ReturnDateHour { get; private set; }
    public Address StartAddress { get; private set; } = null!;
    public Address EndAddress { get; private set; } = null!;
    public Address? ReturnEndAddress { get; private set; }
    public string ReferencePhone { get; private set; } = null!;
    public string ReferenceEmail { get; private set; } = null!;
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
