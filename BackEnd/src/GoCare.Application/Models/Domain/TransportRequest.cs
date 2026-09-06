using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class TransportRequest(
        Guid id, Guid requestedById, Guid beneficiaryId, Guid? careGroupId, ETripType tripType, ETripDirection tripDirection,
        DateTimeOffset departureDateHour, DateTimeOffset? returnDateHour, Address startAddress, Address endAddress, Address? returnEndAddress,
        string referencePhone, string referenceEmail, DateTimeOffset createdAt)
{
    public Guid Id { get; } = id;
    public Guid RequestedById { get; } = requestedById;
    public Guid BeneficiaryId { get; } = beneficiaryId;
    public Guid? CareGroupId { get; private set; } = careGroupId;
    public ETripType TripType { get; private set; } = tripType;
    public ETripDirection TripDirection { get; private set; } = tripDirection;
    public DateTimeOffset DepartureDateHour { get; private set; } = departureDateHour;
    public DateTimeOffset? ReturnDateHour { get; private set; } = returnDateHour;
    public Address StartAddress { get; private set; } = startAddress;
    public Address EndAddress { get; private set; } = endAddress;
    public Address? ReturnEndAddress { get; private set; } = returnEndAddress;
    public string ReferencePhone { get; private set; } = referencePhone;
    public string ReferenceEmail { get; private set;} = referenceEmail;
    public ETripRequestStatus RequestStatus { get; private set; } = ETripRequestStatus.Pending;
    public Guid? AssignedAssociationId { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; } = createdAt;
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
