using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Enums;
using GoCare.Services.Auth;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Admin;

// l'admin reinvia l'email di verifica a un caregiver che non l'ha ricevuta (UC 11.5 · AD-05)
public sealed class AdminResendVerificationService(GoCareDbContext db, ResendVerificationEmailService resend)
{
    public async Task ResendAsync(Guid caregiverId, CancellationToken ct)
    {
        var account = await db.Accounts.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == caregiverId && a.Role == EAccountRole.Person, ct)
            ?? throw new NotFoundException("Caregiver non trovato.");

        // a differenza dell'endpoint pubblico, qui l'admin deve sapere perché non parte niente
        if (account.Status is not EAccountStatus.Unverified)
            throw new ConflictException("L'account è già verificato o non è attivo: nessuna email da reinviare.");

        // stessa logica dell'endpoint pubblico: invalida i token precedenti e ne manda uno nuovo
        await resend.ResendVerificationEmailAsync(account.Email, ct);
    }
}
