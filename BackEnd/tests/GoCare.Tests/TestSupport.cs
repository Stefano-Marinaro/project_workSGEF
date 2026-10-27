using GoCare.Abstractions;
using GoCare.Data;
using GoCare.Models.Auth;
using GoCare.Models.Domain;
using GoCare.Models.Enums;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Tests;

// strumenti comuni ai test dei service: database in memoria, finti invii di email/push e dati di esempio
internal static class TestSupport
{
    public static GoCareDbContext NewDb() =>
        new(new DbContextOptionsBuilder<GoCareDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public static Address SampleAddress() => new("Via Roma", "1", "06100", "Perugia", "PG");

    public static TransportRequest NewTransport(Guid requestedById, DateTimeOffset? departure = null) =>
        new(Guid.NewGuid(), requestedById, Guid.NewGuid(), ETripType.Visit, ETripDirection.OneWay,
            departure ?? DateTimeOffset.UtcNow.AddDays(2), null, SampleAddress(), SampleAddress(), null,
            "333123456", null, DateTimeOffset.UtcNow);

    public static Account NewAccount(EAccountRole role, string email, bool verified = true)
    {
        var account = new Account(Guid.NewGuid(), email, "hash", role, DateTimeOffset.UtcNow);
        if (verified)
            account.Verify(DateTimeOffset.UtcNow);
        return account;
    }
}

internal sealed class FakeEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> Sent { get; } = [];

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        Sent.Add((to, subject, htmlBody));
        return Task.CompletedTask;
    }
}

internal sealed class FakePushSender : IPushSender
{
    public Task SendAsync(string deviceToken, string title, string body, CancellationToken ct = default) =>
        Task.CompletedTask;
}
