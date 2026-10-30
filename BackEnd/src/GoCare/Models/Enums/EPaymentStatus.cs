namespace GoCare.Models.Enums;

public enum EPaymentStatus
{
    Pending,   // sessione Stripe creata, in attesa dell'esito
    Succeeded, // confermato dal webhook, non dal ritorno del browser
    Refunded,
    Failed
}
