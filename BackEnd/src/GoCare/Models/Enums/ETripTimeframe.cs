namespace GoCare.Models.Enums;

// Filtro per GET /transports: "prossimi" vs "passati" (vedi mockup Trasporti/Prossimi e Trasporti/Passati).
public enum ETripTimeframe
{
    Upcoming, // Pending, Confirmed, InProgress — ordinati per partenza crescente (il più vicino prima)
    Past      // Completed, Cancelled, NotCovered — ordinati per partenza decrescente (il più recente prima)
}
