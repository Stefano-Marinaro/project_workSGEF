using GoCare.Models.Auth; // IExpirable + estensione IsExpired

namespace GoCare.Models.Domain;

public sealed class TripOperatorLink : IExpirable
{
    private TripOperatorLink() { } // costruttore vuoto: EF (materializzazione dal DB)

    public TripOperatorLink(Guid id, Guid transportRequestId, string token, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        Id = id;
        TransportRequestId = transportRequestId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public Guid TransportRequestId { get; }   // il SOLO trasporto a cui dà accesso
    public string Token { get; } = null!;
    public DateTimeOffset ExpiresAt { get; }  // calcolata automaticamente da DepartureDateHour + margine, non scelta dall'associazione
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RevokedAt { get; private set; }

    // L'associazione può rigenerare il link (crearne uno nuovo): quello vecchio non viene
    // revocato automaticamente, resta valido finché non scade o viene revocato a parte.
    public bool IsUsable(DateTimeOffset now) => RevokedAt is null && !this.IsExpired(now);

    public void Revoke(DateTimeOffset at) => RevokedAt ??= at;
}
