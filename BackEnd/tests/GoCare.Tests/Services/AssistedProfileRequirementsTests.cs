using GoCare.Controllers.Auth;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Services.Domain;

namespace GoCare.Tests.Services;

public class AssistedProfileRequirementsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_RejectsMissingOrIncompleteProfile_WithoutSavingAssisted(bool hasProfile)
    {
        using var db = TestSupport.NewDb();
        var caregiverId = Guid.NewGuid();
        if (hasProfile)
            db.Persons.Add(new Person(caregiverId, "caregiver@example.com", "Mario", "Rossi"));
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(() => new AssistedPersonService(db).CreateAsync(
            caregiverId, "Anna", "Rossi", new DateOnly(1950, 1, 1), "3331234567", null, null,
            CancellationToken.None));

        Assert.Empty(db.AssistedPeople);
        Assert.Empty(db.CaregiverAssistedLinks);
    }

    [Fact]
    public async Task Create_AllowsCompleteProfile_AndLinksAssistedToCaregiver()
    {
        using var db = TestSupport.NewDb();
        var caregiver = new Person(Guid.NewGuid(), "caregiver@example.com");
        caregiver.CompleteProfile("Mario", "Rossi", new DateOnly(1980, 1, 1), "3331234567");
        db.Persons.Add(caregiver);
        await db.SaveChangesAsync();

        var id = await new AssistedPersonService(db).CreateAsync(
            caregiver.Id, "Anna", "Rossi", new DateOnly(1950, 1, 1), "3331234567", null, null,
            CancellationToken.None);

        Assert.Equal(id, Assert.Single(db.AssistedPeople).Id);
        var link = Assert.Single(db.CaregiverAssistedLinks);
        Assert.Equal(id, link.AssistedPersonId);
        Assert.Equal(caregiver.Id, link.CaregiverId);
    }

    [Fact]
    public void PublicAssociationRegistration_IsForbidden() =>
        Assert.Throws<ForbiddenException>(() => new RegisterAssociationController().RegisterAssociation());
}
