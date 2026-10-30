using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;
using GoCare.Services.Transport;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace GoCare.Tests.Services.Transport;

// Finto IRoutingService: nessuna chiamata HTTP reale nei test, ritorna sempre una distanza fissa.
// Non serve testare qui il calcolo del prezzo (servirà un test dedicato più avanti), solo evitare
// che CreateTransportService fallisca per mancanza della dipendenza.
file sealed class FakeRoutingService : IRoutingService
{
    public Task<double> GetDistanceKmAsync(
        double startLatitude, double startLongitude,
        double endLatitude, double endLongitude,
        CancellationToken ct = default) => Task.FromResult(10.0);
}

public class CreateTransportServiceTests
{
    // database in memoria con i soli comuni usati dai test
private static GoCareDbContext NewDb()
{
    var db = new GoCareDbContext(new DbContextOptionsBuilder<GoCareDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    db.Comuni.AddRange(
        new Comune(Guid.NewGuid(), "054039", "Perugia", "PG"),
        new Comune(Guid.NewGuid(), "054001", "Assisi", "PG"),
        new Comune(Guid.NewGuid(), "058091", "Roma", "RM"));
    db.SaveChanges();

    return db;
}

private static Address AddressIn(string province) =>
    new("Via Test", "1", "06100", province == "RM" ? "Roma" : "Perugia", province);

// TripAddress: come AddressIn, ma con le coordinate obbligatorie richieste da CreateTransportAsync
// (partenza/destinazione/ritorno di un trasporto, serve per il calcolo del percorso).
private static TripAddress TripAddressIn(string province) =>
    province == "RM"
        ? new("Via Test", "1", "06100", "Roma", "RM", 41.9028, 12.4964)
        : new("Via Test", "1", "06100", "Perugia", "PG", 43.1122, 12.3888);

    [Fact]
    public async Task Throws_WhenCaregiverIsNotLinkedToBeneficiary()
    {
        using var db = NewDb();
        var service = new CreateTransportService(db, new FakeRoutingService());

        await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateTransportAsync(
            requestedById: Guid.NewGuid(),
            beneficiaryId: Guid.NewGuid(),
            ETripType.Visit,
            ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1),
            returnDateHour: null,
            TripAddressIn("PG"),
            TripAddressIn("PG"),
            returnEndAddress: null,
            referencePhone: "333123456",
            referenceEmail: null,
            companion: [],
            CancellationToken.None));
    }

    [Fact]
    public async Task Throws_WhenLinkToBeneficiaryIsRevoked()
    {
        using var db = NewDb();
        var caregiverId = Guid.NewGuid();
        var beneficiaryId = Guid.NewGuid();

        var link = new CaregiverAssistedLink(caregiverId, beneficiaryId, DateTimeOffset.UtcNow);
        link.Revoke(DateTimeOffset.UtcNow);
        db.CaregiverAssistedLinks.Add(link);
        await db.SaveChangesAsync();

        var service = new CreateTransportService(db, new FakeRoutingService());

        await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateTransportAsync(
            caregiverId, beneficiaryId, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1), null, TripAddressIn("PG"), TripAddressIn("PG"), null,
            "333123456", null, [], CancellationToken.None));
    }

    [Fact]
    public async Task CreatesTransportRequest_WhenCaregiverIsLinked()
    {
        using var db = NewDb();
        var caregiverId = Guid.NewGuid();
        var beneficiaryId = Guid.NewGuid();

        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiverId, beneficiaryId, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var service = new CreateTransportService(db, new FakeRoutingService());

        var transportId = await service.CreateTransportAsync(
            caregiverId, beneficiaryId, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1), null, TripAddressIn("PG"), TripAddressIn("PG"), null,
            "333123456", null, [], CancellationToken.None);

        var transport = await db.TransportRequests.SingleAsync(t => t.Id == transportId);
        Assert.Equal(caregiverId, transport.RequestedById);
        Assert.Equal(beneficiaryId, transport.BeneficiaryId);
        Assert.Equal(ETripRequestStatus.Pending, transport.RequestStatus);
    }

    [Fact]
    public async Task CreatesOneCompanionPerEntryProvided()
    {
        using var db = NewDb();
        var caregiverId = Guid.NewGuid();
        var beneficiaryId = Guid.NewGuid();

        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiverId, beneficiaryId, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var service = new CreateTransportService(db, new FakeRoutingService());

        var companions = new[]
        {
            ("Mario", "Rossi", "Figlio", "333111111"),
            ("Anna", "Verdi", "Vicina", "333222222"),
        };

        var transportId = await service.CreateTransportAsync(
            caregiverId, beneficiaryId, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1), null, TripAddressIn("PG"), TripAddressIn("PG"), null,
            "333123456", null, companions, CancellationToken.None);

        var saved = await db.Companions.Where(c => c.TransportRequestId == transportId).ToListAsync();
        Assert.Equal(2, saved.Count);
        Assert.Contains(saved, c => c.Name == "Mario" && c.Relationship == "Figlio");
        Assert.Contains(saved, c => c.Name == "Anna" && c.Relationship == "Vicina");
    }

    [Fact]
    public async Task CandidatesAreEmpty_WhenNoAssociationExists()
    {
        using var db = NewDb();
        var caregiverId = Guid.NewGuid();
        var beneficiaryId = Guid.NewGuid();

        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiverId, beneficiaryId, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();

        var service = new CreateTransportService(db, new FakeRoutingService());

        var transportId = await service.CreateTransportAsync(
            caregiverId, beneficiaryId, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1), null, TripAddressIn("PG"), TripAddressIn("PG"), null,
            "333123456", null, [], CancellationToken.None);

        var candidates = await db.TransportRequestCandidates.Where(c => c.TransportRequestId == transportId).ToListAsync();
        Assert.Empty(candidates);
    }

    [Fact]
    public async Task IncludesAssociation_WhenAccreditedAndCoversStartComune()
    {
        using var db = NewDb();
        var caregiverId = Guid.NewGuid();
        var beneficiaryId = Guid.NewGuid();
        var associationId = Guid.NewGuid();

        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiverId, beneficiaryId, DateTimeOffset.UtcNow));
        db.Associations.Add(MakeAssociation(db, associationId, EAccreditationStatus.Accredited, deleted: false, comuni: ["054039"]));
        await db.SaveChangesAsync();

        var service = new CreateTransportService(db, new FakeRoutingService());

        var transportId = await service.CreateTransportAsync(
            caregiverId, beneficiaryId, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1), null, TripAddressIn("PG"), TripAddressIn("PG"), null,
            "333123456", null, [], CancellationToken.None);

        var candidates = await db.TransportRequestCandidates.Where(c => c.TransportRequestId == transportId).ToListAsync();
        Assert.Single(candidates);
        Assert.Equal(associationId, candidates[0].AssociationId);
    }

    [Fact]
    public async Task ExcludesAssociation_WhenNotAccredited()
    {
        using var db = NewDb();
        var caregiverId = Guid.NewGuid();
        var beneficiaryId = Guid.NewGuid();

        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiverId, beneficiaryId, DateTimeOffset.UtcNow));
        db.Associations.Add(MakeAssociation(db, Guid.NewGuid(), EAccreditationStatus.Pending, deleted: false, comuni: ["054039"]));
        await db.SaveChangesAsync();

        var service = new CreateTransportService(db, new FakeRoutingService());

        var transportId = await service.CreateTransportAsync(
            caregiverId, beneficiaryId, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1), null, TripAddressIn("PG"), TripAddressIn("PG"), null,
            "333123456", null, [], CancellationToken.None);

        var candidates = await db.TransportRequestCandidates.Where(c => c.TransportRequestId == transportId).ToListAsync();
        Assert.Empty(candidates);
    }

    [Fact]
    public async Task ExcludesAssociation_WhenSoftDeleted()
    {
        using var db = NewDb();
        var caregiverId = Guid.NewGuid();
        var beneficiaryId = Guid.NewGuid();

        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiverId, beneficiaryId, DateTimeOffset.UtcNow));
        db.Associations.Add(MakeAssociation(db, Guid.NewGuid(), EAccreditationStatus.Accredited, deleted: true, comuni: ["054039"]));
        await db.SaveChangesAsync();

        var service = new CreateTransportService(db, new FakeRoutingService());

        var transportId = await service.CreateTransportAsync(
            caregiverId, beneficiaryId, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1), null, TripAddressIn("PG"), TripAddressIn("PG"), null,
            "333123456", null, [], CancellationToken.None);

        var candidates = await db.TransportRequestCandidates.Where(c => c.TransportRequestId == transportId).ToListAsync();
        Assert.Empty(candidates);
    }

    [Fact]
    public async Task ExcludesAssociation_WhenComuneNotCovered()
    {
        using var db = NewDb();
        var caregiverId = Guid.NewGuid();
        var beneficiaryId = Guid.NewGuid();

        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiverId, beneficiaryId, DateTimeOffset.UtcNow));
        db.Associations.Add(MakeAssociation(db, Guid.NewGuid(), EAccreditationStatus.Accredited, deleted: false, comuni: ["054001"]));
        await db.SaveChangesAsync();

        var service = new CreateTransportService(db, new FakeRoutingService());

        var transportId = await service.CreateTransportAsync(
            caregiverId, beneficiaryId, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1), null, TripAddressIn("PG"), TripAddressIn("PG"), null,
            "333123456", null, [], CancellationToken.None);

        var candidates = await db.TransportRequestCandidates.Where(c => c.TransportRequestId == transportId).ToListAsync();
        Assert.Empty(candidates);
    }

    [Fact]
    public async Task MatchingIgnoresEndAddressProvince_OnlyStartAddressCounts()
    {
        using var db = NewDb();
        var caregiverId = Guid.NewGuid();
        var beneficiaryId = Guid.NewGuid();

        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiverId, beneficiaryId, DateTimeOffset.UtcNow));
        // coopre la provincia di ARRIVO (PG), non quella di PARTENZA (RM): non deve diventare candidata.
        db.Associations.Add(MakeAssociation(db, Guid.NewGuid(), EAccreditationStatus.Accredited, deleted: false, comuni: ["054039"]));
        await db.SaveChangesAsync();

        var service = new CreateTransportService(db, new FakeRoutingService());

        var transportId = await service.CreateTransportAsync(
            caregiverId, beneficiaryId, ETripType.Visit, ETripDirection.OneWay,
            DateTimeOffset.UtcNow.AddDays(1), null,
            startAddress: TripAddressIn("RM"), endAddress: TripAddressIn("PG"), returnEndAddress: null,
            "333123456", null, [], CancellationToken.None);

        var candidates = await db.TransportRequestCandidates.Where(c => c.TransportRequestId == transportId).ToListAsync();
        Assert.Empty(candidates);
    }

private static Association MakeAssociation(GoCareDbContext db, Guid id, EAccreditationStatus status, bool deleted, List<string> comuni)
{
    var association = new Association(id, $"{id}@associazione.test");
    association.CompleteProfile("Associazione Test", AddressIn("PG"), ["0751234567"]);

    // i comuni coperti stanno nella tabella ponte
    foreach (var codice in comuni)
        db.AssociationCoveredComuni.Add(new AssociationCoveredComune(id, codice));

        if (status == EAccreditationStatus.Accredited)
            association.Accredit();
        else if (status == EAccreditationStatus.Rejected)
            association.Reject();

        if (deleted)
            typeof(Association).GetProperty(nameof(Association.DeletedAt))!
                .SetValue(association, DateTimeOffset.UtcNow);

        return association;
    }
}
