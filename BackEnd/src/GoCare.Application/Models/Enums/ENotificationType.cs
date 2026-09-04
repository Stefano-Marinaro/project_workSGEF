using System;
using System.Collections.Generic;
using System.Text;

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
    TripCancelledByUser,
    TripCancelledByAssociation,
    TripReminder
}
