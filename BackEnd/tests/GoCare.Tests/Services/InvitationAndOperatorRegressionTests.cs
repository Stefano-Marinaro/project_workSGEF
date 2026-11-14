using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Domain;
using GoCare.Services.Transport;
using Microsoft.EntityFrameworkCore;

namespace GoCare.Tests.Services;

public class InvitationAndOperatorRegressionTests
{
    [Fact]
    public async Task InvitationLifetimeSurvivesDatabaseReload()
    {
        using var db = TestSupport.NewDb();
        var now = DateTimeOffset.UtcNow;
        var id = Guid.NewGuid();
        db.CaregiverInviteLinks.Add(new CaregiverInviteLink(id, Guid.NewGuid(), Guid.NewGuid(), "fixture", now.AddDays(7), now));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var saved = await db.CaregiverInviteLinks.SingleAsync(i => i.Id == id);
        Assert.Equal(now.AddDays(7), saved.ExpiresAt);
        Assert.Equal(now, saved.CreatedAt);
        Assert.True(saved.IsUsable(now));
    }

    [Fact]
    public async Task ArchivedAssistedCannotBeRelinkedThroughAnOldInvitation()
    {
        using var db = TestSupport.NewDb();
        var caregiver = Guid.NewGuid();
        var person = new AssistedPerson(Guid.NewGuid(), "Audit", "Fixture", new DateOnly(1940, 1, 1), "3331234567", null, caregiver);
        person.Archive(DateTimeOffset.UtcNow);
        db.AssistedPeople.Add(person);
        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiver, person.Id, DateTimeOffset.UtcNow));
        db.CaregiverInviteLinks.Add(new CaregiverInviteLink(Guid.NewGuid(), person.Id, caregiver, "fixture", DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => new AcceptCaregiverInviteService(db).AcceptAsync("fixture", Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task CancelledTransportCannotBeStartedUsingItsOperatorLink()
    {
        using var db = TestSupport.NewDb();
        var trip = TestSupport.NewTransport(Guid.NewGuid());
        trip.Accept(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-1));
        db.TransportRequests.Add(trip);
        db.TripOperatorLinks.Add(new TripOperatorLink(Guid.NewGuid(), trip.Id, "fixture", DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow));
        trip.Cancel(trip.RequestedById, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ForbiddenException>(() => new AdvanceTripStatusService(db).AdvanceAsync("fixture", ETripOperatorAction.Advance, "Audit", CancellationToken.None));
    }
}
