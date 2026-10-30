namespace GoCare.Abstractions;

public interface IRoutingService
{
    // Distanza stradale reale (non in linea d'aria) tra due punti, in chilometri.
    Task<double> GetDistanceKmAsync(
        double startLatitude, double startLongitude,
        double endLatitude, double endLongitude,
        CancellationToken ct = default);
}
