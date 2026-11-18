using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Auth;
using GoCare.Services.Transport;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoCare.Tests.Services.Transport;

public class GenerateOperatorLinkServiceTests
{
    private static GenerateOperatorLinkService NewService(GoCareDbContext db) =>
        new(db, new TokenService(Options.Create(new JwtOptions())),
            Options.Create(new FrontendOptions { OperatorUrl = "https://app.test/operatore" }));

    // trasporto accettato dall'associazione, con un pagamento ancora in attesa
    private static async Task<(TransportRequest Transport, Guid AssociationId, Payment Payment)> SeedAccepted(GoCareDbContext db)
    {
        var transport = TestSupport.NewTransport(Guid.NewGuid());
        var associationId = Guid.NewGuid();
        transport.Accept(associationId, DateTimeOffset.UtcNow);
        var payment = new Payment(Guid.NewGuid(), transport.Id, 10m, "eur", "cs_test", DateTimeOffset.UtcNow);
        db.TransportRequests.Add(transport);
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return (transport, associationId, payment);
    }

    [Fact]
    public async Task Generate_ThrowsConflict_AndCreatesNoLink_WhenTransportIsNotPaid()
    {
        using var db = TestSupport.NewDb();
        var (transport, associationId, _) = await SeedAccepted(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            NewService(db).GenerateOperatorLinkAsync(transport.Id, associationId, CancellationToken.None));

        Assert.Empty(await db.TripOperatorLinks.ToListAsync());
    }

    [Fact]
    public async Task Generate_CreatesLink_WhenTransportIsPaid()
    {
        using var db = TestSupport.NewDb();
        var (transport, associationId, payment) = await SeedAccepted(db);
        payment.MarkSucceeded("pi_test", DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        var response = await NewService(db).GenerateOperatorLinkAsync(transport.Id, associationId, CancellationToken.None);

        var link = await db.TripOperatorLinks.SingleAsync();
        Assert.Equal(transport.Id, link.TransportRequestId);
        Assert.Equal($"https://app.test/operatore/{link.Token}", response.Url);
    }
}
