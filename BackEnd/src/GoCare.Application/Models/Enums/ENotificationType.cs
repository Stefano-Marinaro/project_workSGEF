namespace GoCare.Application.Models.Enums;

public enum ENotificationType
{
    NewRequest,
    RequestAccepted,
    RequestNotCovered,
    TripStatusChanged,
    ModificationRequested,
    ModificationApproved,
    ModificationRejected,
    CompanionUpdated,
    TripCancelledByUser,
    TripCancelledByAssociation,
    TripReminder
}
