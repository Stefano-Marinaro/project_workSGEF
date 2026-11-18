using System.Net;
using GoCare.Errors;
using GoCare.Models.Domain;
using GoCare.Services.Auth;
using GoCare.Services.Payments;
using Microsoft.Extensions.Options;
using Stripe;

namespace GoCare.Tests.Services;

public class CheckoutSessionTests
{
    private sealed class CheckoutHandler : HttpMessageHandler
    {
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Body = await request.Content!.ReadAsStringAsync(ct);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":"cs_test_transport","object":"checkout.session","url":"https://checkout.stripe.com/test"}"""),
            };
        }
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Checkout_AllowsConfirmedUnpaidTrip_AndRejectsPendingOrPaid(bool confirmed, bool paid)
    {
        using var db = TestSupport.NewDb();
        var caregiverId = Guid.NewGuid();
        var trip = TestSupport.NewTransport(caregiverId);
        if (confirmed) trip.Accept(Guid.NewGuid(), DateTimeOffset.UtcNow);
        db.TransportRequests.Add(trip);
        db.CaregiverAssistedLinks.Add(new CaregiverAssistedLink(caregiverId, trip.BeneficiaryId, DateTimeOffset.UtcNow));
        if (paid)
        {
            var payment = new Payment(Guid.NewGuid(), trip.Id, 12m, "EUR", "cs_paid", DateTimeOffset.UtcNow);
            payment.MarkSucceeded("pi_paid", DateTimeOffset.UtcNow);
            db.Payments.Add(payment);
        }
        await db.SaveChangesAsync();

        var handler = new CheckoutHandler();
        using var httpClient = new HttpClient(handler);
        var stripe = new StripeClient("sk_test_fake", httpClient: new SystemNetHttpClient(httpClient));
        var service = new CreateCheckoutSessionService(db, stripe,
            Options.Create(new PricingOptions { PricePerKm = 1.20m, MinimumAmount = 5m }),
            Options.Create(new FrontendOptions
            {
                PaymentSuccessUrl = "http://localhost/pagamento-riuscito",
                PaymentCancelUrl = "http://localhost/pagamento-annullato",
            }));

        if (!confirmed || paid)
        {
            await Assert.ThrowsAsync<ConflictException>(() => service.CreateSessionAsync(caregiverId, trip.Id, CancellationToken.None));
            Assert.Null(handler.Body);
        }
        else
        {
            var url = await service.CreateSessionAsync(caregiverId, trip.Id, CancellationToken.None);
            Assert.Equal("https://checkout.stripe.com/test", url);
            Assert.Equal(12m, Assert.Single(db.Payments).Amount);
            Assert.Contains("line_items[0][price_data][unit_amount]=1200", Uri.UnescapeDataString(handler.Body!));
        }
    }
}
