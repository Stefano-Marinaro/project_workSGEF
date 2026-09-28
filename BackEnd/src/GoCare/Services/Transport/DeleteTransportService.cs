using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Transport;

public class DeleteTransportService(GoCareDbContext db)
{
    public async Task DeleteTransport(Guid id, Guid deletedBy, CancellationToken ct)
    {
        var transport = await db.TransportRequests.SingleOrDefaultAsync(t => t.Id == id, ct);
        if (transport is null)
            throw new NotFoundException("Errore nel recupero dell'ID.");

        var isLinked = await db.CaregiverAssistedLinks.AnyAsync(
            l => l.CaregiverId == deletedBy && l.AssistedPersonId == transport.BeneficiaryId && l.DeletedAt == null, ct);
        if (!isLinked)
            throw new ForbiddenException("Non sei collegato a questo assistito.");

        if (transport.RequestStatus is not ETripRequestStatus.Pending && transport.RequestStatus is not ETripRequestStatus.Confirmed)
            throw new ForbiddenException("Impossibile annullare una richiesta già in corso, conclusa o non coperta.");

        var now = DateTimeOffset.UtcNow;

        transport.Cancel(deletedBy, now);

        await db.SaveChangesAsync(ct);
    }
}
