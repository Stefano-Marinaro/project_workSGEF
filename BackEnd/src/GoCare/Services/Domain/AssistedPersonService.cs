using GoCare.Data;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Services.Domain;

public sealed class AssistedPersonService(GoCareDbContext db)
{
    public async Task<Guid> CreateAsync(
        Guid caregiverId,
        string name,
        string surname,
        DateOnly birthDate,
        string phone,
        Address? homeAddress,
        CancellationToken ct)
    {
        var assistedPersonId = Guid.NewGuid();
        var assistedPerson = new AssistedPerson(
            assistedPersonId,
            name,
            surname,
            birthDate,
            phone,
            homeAddress,
            caregiverId);

        db.AssistedPeople.Add(assistedPerson);
        db.CaregiverAssistedLinks.Add(
            new CaregiverAssistedLink(caregiverId, assistedPersonId, DateTimeOffset.UtcNow));

        await db.SaveChangesAsync(ct);
        return assistedPersonId;
    }

    public async Task<IReadOnlyList<AssistedPersonSummary>> ListAsync(
        Guid caregiverId,
        CancellationToken ct)
    {
        var people = await (
            from link in db.CaregiverAssistedLinks.AsNoTracking()
            join assisted in db.AssistedPeople.AsNoTracking()
                on link.AssistedPersonId equals assisted.Id
            where link.CaregiverId == caregiverId
                && link.DeletedAt == null
                && assisted.DeletedAt == null
            orderby assisted.Surname, assisted.Name
            select assisted)
            .ToListAsync(ct);

        if (people.Count == 0)
            return [];

        var assistedPersonIds = people.Select(person => person.Id).ToArray();
        var now = DateTimeOffset.UtcNow;

        var futureTrips = await db.TransportRequests
            .AsNoTracking()
            .Where(trip => assistedPersonIds.Contains(trip.BeneficiaryId)
                && trip.DepartureDateHour >= now
                && trip.RequestStatus != ETripRequestStatus.Cancelled
                && trip.RequestStatus != ETripRequestStatus.NotCovered
                && trip.RequestStatus != ETripRequestStatus.Completed)
            .OrderBy(trip => trip.DepartureDateHour)
            .ToListAsync(ct);

        var nextTripByAssistedPerson = futureTrips
            .GroupBy(trip => trip.BeneficiaryId)
            .ToDictionary(group => group.Key, group => ToTripSummary(group.First()));

        return people
            .Select(person => new AssistedPersonSummary(
                person.Id,
                person.Name,
                person.Surname,
                person.BirthDate,
                person.Phone,
                person.HomeAddress,
                nextTripByAssistedPerson.GetValueOrDefault(person.Id)))
            .ToList();
    }

    public async Task<AssistedPersonDetail> GetDetailAsync(
        Guid caregiverId,
        Guid assistedPersonId,
        CancellationToken ct)
    {
        var assistedPerson = await FindAccessibleAsync(caregiverId, assistedPersonId, tracked: false, ct);

        var caregivers = await (
            from link in db.CaregiverAssistedLinks.AsNoTracking()
            join person in db.Persons.AsNoTracking() on link.CaregiverId equals person.Id
            where link.AssistedPersonId == assistedPersonId
                && link.DeletedAt == null
                && person.DeletedAt == null
            orderby person.Surname, person.Name
            select new CaregiverSummary(person.Id, person.Name, person.Surname, person.Email))
            .ToListAsync(ct);

        var trips = await db.TransportRequests
            .AsNoTracking()
            .Where(trip => trip.BeneficiaryId == assistedPersonId)
            .OrderByDescending(trip => trip.DepartureDateHour)
            .ToListAsync(ct);

        return new AssistedPersonDetail(
            assistedPerson.Id,
            assistedPerson.Name,
            assistedPerson.Surname,
            assistedPerson.BirthDate,
            assistedPerson.Phone,
            assistedPerson.HomeAddress,
            caregivers,
            trips.Select(ToTripSummary).ToList());
    }

    public async Task UpdateAsync(
        Guid caregiverId,
        Guid assistedPersonId,
        string name,
        string surname,
        DateOnly birthDate,
        string phone,
        Address? homeAddress,
        CancellationToken ct)
    {
        var assistedPerson = await FindAccessibleAsync(caregiverId, assistedPersonId, tracked: true, ct);
        assistedPerson.Update(name, surname, birthDate, phone, homeAddress);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddCaregiverAsync(
        Guid caregiverId,
        Guid assistedPersonId,
        string caregiverEmail,
        CancellationToken ct)
    {
        await EnsureAccessibleAsync(caregiverId, assistedPersonId, ct);

        var emailPattern = caregiverEmail.Trim()
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
        var newCaregiverId = await db.Accounts
            .AsNoTracking()
            .Where(account => EF.Functions.ILike(account.Email, emailPattern, "\\")
                && account.Role == EAccountRole.Person
                && account.Status != EAccountStatus.Deleted)
            .Select(account => (Guid?)account.Id)
            .SingleOrDefaultAsync(ct);

        if (newCaregiverId is null)
            throw new NotFoundException("Non esiste un account caregiver con questa email.");

        var existingLink = await db.CaregiverAssistedLinks.SingleOrDefaultAsync(
            link => link.CaregiverId == newCaregiverId.Value
                && link.AssistedPersonId == assistedPersonId,
            ct);

        if (existingLink is not null && existingLink.DeletedAt is null)
            throw new ConflictException("Il caregiver è già collegato a questo assistito.");

        if (existingLink is null)
        {
            db.CaregiverAssistedLinks.Add(
                new CaregiverAssistedLink(newCaregiverId.Value, assistedPersonId, DateTimeOffset.UtcNow));
        }
        else
        {
            existingLink.Restore();
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveCaregiverAsync(
        Guid caregiverId,
        Guid assistedPersonId,
        Guid caregiverToRemoveId,
        CancellationToken ct)
    {
        await EnsureAccessibleAsync(caregiverId, assistedPersonId, ct);

        var link = await db.CaregiverAssistedLinks.SingleOrDefaultAsync(
            current => current.CaregiverId == caregiverToRemoveId
                && current.AssistedPersonId == assistedPersonId
                && current.DeletedAt == null,
            ct);

        if (link is null)
            throw new NotFoundException("Il caregiver non è collegato a questo assistito.");

        var activeCaregivers = await db.CaregiverAssistedLinks.CountAsync(
            current => current.AssistedPersonId == assistedPersonId && current.DeletedAt == null,
            ct);

        if (activeCaregivers <= 1)
            throw new ConflictException("Non è possibile rimuovere l'unico caregiver dell'assistito.");

        link.Revoke(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(ct);
    }

    private async Task<AssistedPerson> FindAccessibleAsync(
        Guid caregiverId,
        Guid assistedPersonId,
        bool tracked,
        CancellationToken ct)
    {
        IQueryable<AssistedPerson> query = db.AssistedPeople;
        if (!tracked)
            query = query.AsNoTracking();

        var assistedPerson = await query.SingleOrDefaultAsync(
            person => person.Id == assistedPersonId
                && person.DeletedAt == null
                && db.CaregiverAssistedLinks.Any(link =>
                    link.CaregiverId == caregiverId
                    && link.AssistedPersonId == person.Id
                    && link.DeletedAt == null),
            ct);

        return assistedPerson
            ?? throw new NotFoundException("Assistito non trovato o non accessibile.");
    }

    private async Task EnsureAccessibleAsync(
        Guid caregiverId,
        Guid assistedPersonId,
        CancellationToken ct)
    {
        var isAccessible = await db.CaregiverAssistedLinks.AnyAsync(
            link => link.CaregiverId == caregiverId
                && link.AssistedPersonId == assistedPersonId
                && link.DeletedAt == null
                && db.AssistedPeople.Any(person =>
                    person.Id == assistedPersonId && person.DeletedAt == null),
            ct);

        if (!isAccessible)
            throw new NotFoundException("Assistito non trovato o non accessibile.");
    }

    private static AssistedTripSummary ToTripSummary(TransportRequest trip) => new(
        trip.Id,
        trip.RequestedById,
        trip.TripType,
        trip.TripDirection,
        trip.DepartureDateHour,
        trip.ReturnDateHour,
        trip.RequestStatus);
}

public sealed record AssistedPersonSummary(
    Guid Id,
    string Name,
    string Surname,
    DateOnly BirthDate,
    string Phone,
    Address? HomeAddress,
    AssistedTripSummary? NextTrip);

public sealed record AssistedPersonDetail(
    Guid Id,
    string Name,
    string Surname,
    DateOnly BirthDate,
    string Phone,
    Address? HomeAddress,
    IReadOnlyList<CaregiverSummary> Caregivers,
    IReadOnlyList<AssistedTripSummary> Trips);

public sealed record CaregiverSummary(
    Guid Id,
    string? Name,
    string? Surname,
    string Email);

public sealed record AssistedTripSummary(
    Guid Id,
    Guid RequestedById,
    ETripType TripType,
    ETripDirection TripDirection,
    DateTimeOffset DepartureDateHour,
    DateTimeOffset? ReturnDateHour,
    ETripRequestStatus Status);
