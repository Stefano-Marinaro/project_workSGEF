using FluentValidation.TestHelper;
using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Transport.Validators;
using Xunit;
using GoCare.Data;
using Microsoft.EntityFrameworkCore;

namespace GoCare.Tests.Validators;

public class UpdatePersonProfileValidatorTests
{
    // il validator ora riceve il DbContext (serve ad AddressValidator); qui basta uno in memoria vuoto
    private readonly UpdatePersonProfileValidator _validator = new(
        new GoCareDbContext(new DbContextOptionsBuilder<GoCareDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options));
    [Fact]
    public void Should_have_error_when_name_is_empty()
    {
        var request = new UpdatePersonProfileRequest("", "Rossi", new DateOnly(1990, 1, 1), "333123456", null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_have_error_when_surname_is_empty()
    {
        var request = new UpdatePersonProfileRequest("Mario", "", new DateOnly(1990, 1, 1), "333123456", null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Surname);
    }

    [Fact]
    public void Should_have_error_when_birth_date_is_in_the_future()
    {
        var request = new UpdatePersonProfileRequest(
            "Mario", "Rossi", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "333123456", null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.BirthDate);
    }

    [Fact]
    public void Should_have_error_when_phone_is_empty()
    {
        var request = new UpdatePersonProfileRequest("Mario", "Rossi", new DateOnly(1990, 1, 1), "", null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Should_not_have_error_when_request_is_valid()
    {
        var request = new UpdatePersonProfileRequest("Mario", "Rossi", new DateOnly(1990, 1, 1), "333123456", null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
