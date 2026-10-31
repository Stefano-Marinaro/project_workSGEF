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

// L'admin registra l'associazione partner (UC 11.1 · AD-01): account senza password scelta + profilo completo.
// L'associazione riceve un'email di invito e imposta la password con POST /auth/set-password.
public sealed class CreatePartnerAssociationService(
    GoCareDbContext db,
    PasswordService passwordService,
    TokenService tokenService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions)
{
    private static readonly TimeSpan InviteValidity = TimeSpan.FromDays(7);
    private readonly FrontendOptions _frontend = frontendOptions.Value;

    public async Task<Guid> CreateAsync(
        string email,
        string name,
        Address headquarter,
        List<string> phones,
        List<string> coveredComuni,
        string? availabilityHours,
        CancellationToken ct)
    {
        var emailExist = await db.Accounts.AnyAsync(a => a.Email == email, ct);
        if (emailExist)
            throw new ConflictException("Email già registrata");

        var accountId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        // password casuale che nessuno conosce: finché l'associazione non sceglie la sua, l'account resta
        // Unverified e il login è comunque bloccato (CanLogIn richiede Active)
        var account = new Account(
            accountId, email, passwordService.Hash(tokenService.GenerateRefreshTokenValue()), EAccountRole.Association, now);

        // stesso Id dell'account, come per la registrazione
        var association = new Association(accountId, email);
        association.UpdateProfile(name, headquarter, phones, availabilityHours);
        association.Accredit(); // registrata dall'admin = subito operativa (PA-05 superato)

        // l'invito riusa i token di reset password: monouso e a scadenza, nessuna tabella nuova
        var inviteToken = new PasswordResetToken(
            Guid.NewGuid(), accountId, tokenService.GenerateRefreshTokenValue(), expiresAt: now.Add(InviteValidity));

        db.Accounts.Add(account);
        db.Associations.Add(association);
        db.PasswordResetTokens.Add(inviteToken);
        await CoveredComuniWriter.ReplaceAsync(db, accountId, coveredComuni, ct);

        await db.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            email, "Benvenuti in GoCare",
            $"La vostra associazione è stata registrata su GoCare. " +
            $"<a href=\"{_frontend.SetPasswordUrl}?token={inviteToken.Token}\">Clicca qui per impostare la password</a> " +
            $"(il link scade tra {InviteValidity.TotalDays} giorni).", ct);

        return accountId;
    }
}
