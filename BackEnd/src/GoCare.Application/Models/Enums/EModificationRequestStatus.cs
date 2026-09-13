namespace GoCare.Application.Models.Enums;

public enum EModificationRequestStatus
{
    PendingApproval,
    Approved,
    Rejected,
    Retracted // chi ha chiesto la modifica la annulla prima che sia approvata o rifiutata.
}
