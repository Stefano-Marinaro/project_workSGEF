using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

// L'admin riattiva un account caregiver (UC 11.5 · AD-05): sospeso, bloccato da troppi login falliti,
// oppure eliminato ma non ancora anonimizzato.
public sealed class RestoreCaregiverAccountService(GoCareDbContext db, ILogger<RestoreCaregiverAccountService> logger)
{
    public async Task RestoreAsync(Guid caregiverId, Guid adminId, CancellationToken ct)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(a => a.Id == caregiverId && a.Role == EAccountRole.Person, ct)
            ?? throw new NotFoundException("Caregiver non trovato.");

        var person = await db.Persons.SingleOrDefaultAsync(p => p.Id == caregiverId, ct)
            ?? throw new NotFoundException("Profilo caregiver non trovato.");

        var failedLogins = await db.FailedLoginAttempts.Where(f => f.Email == account.Email).ToListAsync(ct);

        string action;
        switch (account.Status)
        {
            case EAccountStatus.Suspended:
                account.Reinstate();
                action = "riattivato da sospeso";
                break;

            case EAccountStatus.Deleted:
                if (person.AnonymizedAt is not null)
                    throw new ConflictException("L'account è stato anonimizzato: i dati non sono più recuperabili.");

                account.Restore();
                person.Restore();
                action = "ripristinato da eliminato";
                break;

            default:
                if (failedLogins.Count == 0)
                    throw new ConflictException("L'account non è sospeso, eliminato o bloccato: niente da riattivare.");

                action = "sbloccato dai login falliti";
                break;
        }

        // in ogni caso si riparte da zero con i tentativi falliti
        db.FailedLoginAttempts.RemoveRange(failedLogins);
        await db.SaveChangesAsync(ct);

        // chi, cosa, quando: per ora nel log applicativo (non c'è una tabella di audit)
        logger.LogInformation(
            "Admin {AdminId} ha riattivato il caregiver {CaregiverId} ({Action}) alle {At}",
            adminId, caregiverId, action, DateTimeOffset.UtcNow);
    }
}
