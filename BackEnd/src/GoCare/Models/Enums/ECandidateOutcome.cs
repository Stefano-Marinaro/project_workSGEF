namespace GoCare.Models.Enums;

// esito di una richiesta per UNA associazione candidata (storico visto dall'admin)
public enum ECandidateOutcome
{
    Pending,                  // ancora in attesa, l'associazione non ha deciso
    Accepted,                 // accettata da lei (confermata, in corso o conclusa)
    Declined,                 // rifiutata prima di accettare
    CancelledAfterAcceptance, // accettata e poi disdetta
    TakenByAnother,           // accettata da un'altra associazione
    NotCovered,               // nessuna associazione l'ha presa
    Cancelled                 // annullata dal caregiver
}
