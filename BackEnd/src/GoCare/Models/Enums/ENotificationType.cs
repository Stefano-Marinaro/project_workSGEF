namespace GoCare.Models.Enums;

public enum ENotificationType
{
    NewRequest,
    RequestAccepted,
    RequestNotCovered,
    TripStatusChanged,
    ModificationRequested,
    ModificationApproved,
    ModificationRejected,
    TripCancelledByUser,
    TripCancelledByAssociation,
    TripReminder
}
