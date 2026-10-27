using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Pagination;
using GoCare.Services.Admin;

namespace GoCare.Tests.Services.Admin;

public class PartnerAssociationRequestsServiceTests
{
    private static readonly Guid AssociationId = Guid.NewGuid();

    private static TransportRequest AddCandidate(GoCareDbContext db)
    {
        var transport = TestSupport.NewTransport(Guid.NewGuid());
        db.TransportRequests.Add(transport);
        db.TransportRequestCandidates.Add(new TransportRequestCandidate(transport.Id, AssociationId));
        return transport;
    }

    private static async Task<GoCareDbContext> SeedOneRequestPerOutcome()
    {
        var db = TestSupport.NewDb();
        db.Associations.Add(new Association(AssociationId, "croce@example.com"));

        AddCandidate(db);                                                          // Pending
        AddCandidate(db).Accept(AssociationId, DateTimeOffset.UtcNow);             // Accepted
        var declined = AddCandidate(db);                                           // Declined
        db.TransportRequestRejections.Add(new TransportRequestRejection(
            Guid.NewGuid(), declined.Id, AssociationId, ERejectionKind.Declined, "Nessun mezzo", DateTimeOffset.UtcNow));
        AddCandidate(db).Accept(Guid.NewGuid(), DateTimeOffset.UtcNow);            // TakenByAnother
        AddCandidate(db).RequestNotCovered(DateTimeOffset.UtcNow);                 // NotCovered
        AddCandidate(db).Cancel(Guid.NewGuid(), DateTimeOffset.UtcNow);            // Cancelled

        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task List_ComputesOutcomeAndIndicators()
    {
        using var db = await SeedOneRequestPerOutcome();

        var result = await new PartnerAssociationRequestsService(db)
            .ListAsync(AssociationId, null, null, null, new PageQuery(), CancellationToken.None);

        Assert.Equal(6, result.Indicators.Received);
        Assert.Equal(1, result.Indicators.Accepted);
        Assert.Equal(1, result.Indicators.Declined);
        Assert.NotNull(result.Indicators.AverageAcceptanceMinutes);
        Assert.Equal(6, result.Requests.TotalCount);
        Assert.Equal(
            Enum.GetValues<ECandidateOutcome>().Where(o => o != ECandidateOutcome.CancelledAfterAcceptance).Order(),
            result.Requests.Items.Select(i => i.Outcome).Order());
        Assert.Equal("Nessun mezzo", result.Requests.Items.Single(i => i.Outcome == ECandidateOutcome.Declined).RejectionReason);
    }

    [Fact]
    public async Task List_FiltersByOutcome_ButKeepsIndicatorsOnWholePeriod()
    {
        using var db = await SeedOneRequestPerOutcome();

        var result = await new PartnerAssociationRequestsService(db)
            .ListAsync(AssociationId, null, null, ECandidateOutcome.Declined, new PageQuery(), CancellationToken.None);

        Assert.Equal(ECandidateOutcome.Declined, Assert.Single(result.Requests.Items).Outcome);
        Assert.Equal(6, result.Indicators.Received);
    }

    [Fact]
    public async Task List_Throws_WhenAssociationDoesNotExist()
    {
        using var db = TestSupport.NewDb();

        await Assert.ThrowsAsync<NotFoundException>(() => new PartnerAssociationRequestsService(db)
            .ListAsync(Guid.NewGuid(), null, null, null, new PageQuery(), CancellationToken.None));
    }
}
