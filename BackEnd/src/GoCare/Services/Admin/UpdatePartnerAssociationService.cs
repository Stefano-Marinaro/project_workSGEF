using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Auth;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Auth;
using GoCare.Services.Profile;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Services.Admin;

// L'admin modifica un'associazione partner (UC 11.2 · AD-02).
// I comuni coperti valgono solo per le richieste future: le candidate delle richieste già create restano quelle.
// L'email non cambia subito: parte una conferma al nuovo indirizzo, come per il cambio email dal profilo.
public sealed class UpdatePartnerAssociationService(
    GoCareDbContext db,
    TokenService tokenService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions)
{
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    // true se è partita la conferma del nuovo indirizzo email
    public async Task<bool> UpdateAsync(
        Guid associationId,
        string email,
        string name,
        Address headquarter,
        List<string> phones,
        List<string> coveredComuni,
        string? availabilityHours,
        CancellationToken ct)
    {
        var association = await db.Associations.SingleOrDefaultAsync(a => a.Id == associationId && a.DeletedAt == null, ct)
            ?? throw new NotFoundException("Associazione non trovata.");

        var account = await db.Accounts.SingleAsync(a => a.Id == associationId, ct);

        var emailChanged = !string.Equals(account.Email, email, StringComparison.OrdinalIgnoreCase);
        EmailChangeToken? emailToken = null;

        if (emailChanged)
        {
            // la conferma (POST /auth/change-email/confirm) richiede un account attivo
            if (!account.CanLogIn)
                throw new ConflictException("L'email si può cambiare solo dopo che l'associazione ha attivato l'account.");

            var emailAlreadyUsed = await db.Accounts.AnyAsync(a => a.Email == email && a.Id != associationId, ct);
            if (emailAlreadyUsed)
                throw new ConflictException("E-mail già registrata.");

            var now = DateTimeOffset.UtcNow;

            // una sola conferma valida alla volta
            var previousTokens = await db.EmailChangeTokens
                .Where(t => t.AccountId == associationId && t.ConsumedAt == null && t.ExpiresAt > now)
                .ToListAsync(ct);
            foreach (var previousToken in previousTokens)
                previousToken.Consume(now);

            emailToken = new EmailChangeToken(
                Guid.NewGuid(), associationId, email, tokenService.GenerateRefreshTokenValue(), now.AddHours(24));
            db.EmailChangeTokens.Add(emailToken);
        }

        association.UpdateProfile(name, headquarter, phones, availabilityHours);
        await CoveredComuniWriter.ReplaceAsync(db, association.Id, coveredComuni, ct);
        await db.SaveChangesAsync(ct);

        if (emailToken is not null)
            await emailSender.SendAsync(
                email,
                "Conferma la nuova e-mail GoCare",
                $"L'amministratore di GoCare ha indicato questo indirizzo per la vostra associazione. " +
                $"<a href=\"{_frontend.ChangeEmailUrl}?token={emailToken.Token}\">Conferma il cambio di e-mail</a>",
                ct);

        return emailToken is not null;
    }
}
