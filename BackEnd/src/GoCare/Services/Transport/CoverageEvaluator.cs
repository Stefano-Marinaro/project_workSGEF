using GoCare.Models.Enums;

namespace GoCare.Services.Transport;

// Logica pura (PA-01): decide se una richiesta va dichiarata "non coperta". Nessuna
// dipendenza da EF, così è testabile in isolamento — i conteggi li passa il chiamante.
public static class CoverageEvaluator
{
    public static bool IsNotCovered(
        ETripRequestStatus status,
        DateTimeOffset departureDateHour,
        int candidateCount,
        int rejectionCount,
        DateTimeOffset now,
        TimeSpan threshold)
    {
        if (status != ETripRequestStatus.Pending)
            return false;

        // tutte le associazioni candidate hanno rifiutato
        if (candidateCount > 0 && rejectionCount >= candidateCount)
            return true;

        // sotto soglia rispetto alla partenza, nessuna ha ancora accettato
        return departureDateHour - now < threshold;
    }
}
