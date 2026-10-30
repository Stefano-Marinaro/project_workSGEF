using GoCare.Models.Enums;

namespace GoCare.Models.Domain;

public sealed class Payment
{
    private Payment() { } // costruttore vuoto: EF (materializzazione dal DB)

    public Payment(Guid id, Guid transportRequestId, decimal amount, string currency, string stripeSessionId, DateTimeOffset createdAt)
    {
        Id = id;
        TransportRequestId = transportRequestId;
        Amount = amount;
        Currency = currency;
        StripeSessionId = stripeSessionId;
        Status = EPaymentStatus.Pending;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public Guid TransportRequestId { get; }
    public decimal Amount { get; }
    public string Currency { get; } = null!;
    public string StripeSessionId { get; } = null!;
    public string? StripePaymentIntentId { get; private set; }
    public EPaymentStatus Status { get; private set; } = EPaymentStatus.Pending;
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? PaidAt { get; private set; }

    // Chiamato solo dal webhook Stripe (mai dal ritorno del browser): è l'unica fonte attendibile
    // di conferma del pagamento.
    public void MarkSucceeded(string stripePaymentIntentId, DateTimeOffset at)
    {
        if (Status != EPaymentStatus.Pending)
            throw new InvalidOperationException("Solo un pagamento in attesa può essere confermato.");

        Status = EPaymentStatus.Succeeded;
        StripePaymentIntentId = stripePaymentIntentId;
        PaidAt = at;
    }

    public void MarkFailed()
    {
        if (Status != EPaymentStatus.Pending)
            throw new InvalidOperationException("Solo un pagamento in attesa può fallire.");

        Status = EPaymentStatus.Failed;
    }

    public void MarkRefunded()
    {
        if (Status != EPaymentStatus.Succeeded)
            throw new InvalidOperationException("Solo un pagamento riuscito può essere rimborsato.");

        Status = EPaymentStatus.Refunded;
    }
}
