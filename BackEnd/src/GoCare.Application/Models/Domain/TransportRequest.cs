using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class TransportRequest(
        Guid id, Guid requestedById, Guid beneficiaryId, Guid? careGroupId, ETripType tripType, ETripDirection tripDirection,
        DateTimeOffset onWayDateHour, Address startAddress, Address endAddress, string referencePhone, string referenceEmail)
{
    public Guid Id { get; } = id;
    public Guid RequestedById { get; } = requestedById;
    public Guid BeneficiaryId { get; } = beneficiaryId;
    public Guid? CareGroupId { get; private set; } = careGroupId;
    public ETripType TripType { get; private set; } = tripType;
    public ETripDirection TripDirection { get; private set; } = tripDirection;
    public DateTimeOffset OnWayDateHour { get; private set; } = onWayDateHour;
    public DateTimeOffset? ReturnDateHour { get; private set; } 
    public Address StartAddress { get; private set; } = startAddress;
    public Address EndAddress { get; private set; } = endAddress;
    public string ReferencePhone { get; private set; } = referencePhone;
    public string ReferenceEmail { get; private set;} = referenceEmail;
    public ETripRequestStatus RequestStatus { get; private set; } = ETripRequestStatus.Pending;
    public Guid? AssignedAssociationId { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public DateTimeOffset? NotCoveredAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

}
