using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Tests.Services.Transport;

public class RequesterContactsServiceTests
{
    private static async Task<(TransportRequest Transport, Guid AssociationId)> SeedAccepted(GoCareDbContext db)
    {
        var requester = new Person(Guid.NewGuid(), "mario@example.com");
        requester.CompleteProfile("Mario", "Rossi", new DateOnly(1970, 1, 1), "333111222");
        var beneficiary = new AssistedPerson(
            Guid.NewGuid(), "Anna", "Rossi", new DateOnly(1940, 1, 1), "333444555", null, requester.Id);

        var transport = new TransportRequest(
            Guid.NewGuid(), requester.Id, beneficiary.Id, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(2), null, TestSupport.SampleAddress(), TestSupport.SampleAddress(), null,
            "333999888", null, DateTimeOffset.UtcNow);
        var associationId = Guid.NewGuid();
        transport.Accept(associationId, DateTimeOffset.UtcNow);

        db.Persons.Add(requester);
        db.AssistedPeople.Add(beneficiary);
        db.TransportRequests.Add(transport);
        await db.SaveChangesAsync();
        return (transport, associationId);
    }

    [Fact]
    public async Task Get_ReturnsContacts_AndLogsBothAccesses()
    {
        using var db = TestSupport.NewDb();
        var (transport, associationId) = await SeedAccepted(db);

        var contacts = await new RequesterContactsService(db).GetAsync(associationId, transport.Id, CancellationToken.None);

        Assert.Equal("Mario", contacts.RequesterName);
        Assert.Equal("333111222", contacts.RequesterPhone);
        Assert.Equal("333999888", contacts.ReferencePhone);
        Assert.Equal("Anna", contacts.BeneficiaryName);

        var logs = await db.ContactAccessLogs.ToListAsync();
        Assert.Equal(2, logs.Count);
        Assert.All(logs, l => Assert.Equal(associationId, l.AssociationId));
        Assert.Contains(logs, l => l.DataKind == EContactDataKind.Requester);
        Assert.Contains(logs, l => l.DataKind == EContactDataKind.Beneficiary);
    }

    [Fact]
    public async Task Get_Throws_WhenTransportIsAssignedToAnotherAssociation()
    {
        using var db = TestSupport.NewDb();
        var (transport, _) = await SeedAccepted(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            new RequesterContactsService(db).GetAsync(Guid.NewGuid(), transport.Id, CancellationToken.None));
        Assert.Empty(db.ContactAccessLogs);
    }

    [Fact]
    public async Task Get_Throws_WhenTripIsCompleted()
    {
        using var db = TestSupport.NewDb();
        var (transport, associationId) = await SeedAccepted(db);
        transport.StartExecution();
        transport.CompleteExecution();
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() =>
            new RequesterContactsService(db).GetAsync(associationId, transport.Id, CancellationToken.None));
    }
}
