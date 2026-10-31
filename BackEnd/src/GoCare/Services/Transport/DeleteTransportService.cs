using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public sealed class DeleteTransportService(GoCareDbContext db, Stripe.IStripeClient stripeClient)
{
    public async Task DeleteTransport(Guid id, Guid deletedBy, CancellationToken ct)
    {
        var transport = await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == id, ct);
        if (transport is null)
            throw new NotFoundException("Trasporto non trovato.");

        var isLinked = await db.CaregiverAssistedLinks.AnyAsync(
            l => l.CaregiverId == deletedBy && l.AssistedPersonId == transport.BeneficiaryId && l.DeletedAt == null, ct);
        if (!isLinked)
            throw new ForbiddenException("Non sei collegato a questo assistito.");

        if (transport.RequestStatus is not ETripRequestStatus.Pending && transport.RequestStatus is not ETripRequestStatus.Confirmed)
            throw new ConflictException("Impossibile annullare una richiesta già in corso, conclusa o non coperta.");

        var now = DateTimeOffset.UtcNow;

        transport.Cancel(deletedBy, now);

        // Se il viaggio era già stato pagato, rimborso automatico e integrale (politica v0).
        var payment = await db.Payments.SingleOrDefaultAsync(
            p => p.TransportRequestId == id && p.Status == EPaymentStatus.Succeeded, ct);

        if (payment is not null)
        {
            var refundService = new Stripe.RefundService(stripeClient);
            await refundService.CreateAsync(
                new Stripe.RefundCreateOptions { PaymentIntent = payment.StripePaymentIntentId },
                cancellationToken: ct);

            payment.MarkRefunded();
        }

        await db.SaveChangesAsync(ct);
    }
}
