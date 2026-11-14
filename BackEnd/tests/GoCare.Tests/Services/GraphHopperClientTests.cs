using System.Net;
using GoCare.Errors;
using GoCare.Infrastructure;
using Microsoft.Extensions.Options;

namespace GoCare.Tests.Services;

public class GraphHopperClientTests
{
    [Theory]
    [InlineData(401)]
    [InlineData(429)]
    [InlineData(503)]
    public async Task ProviderFailure_ReturnsActionableError(int status)
    {
        using var http = new HttpClient(new StubHandler((HttpStatusCode)status));
        var client = new GraphHopperClient(http, Options.Create(new GraphHopperOptions { ApiKey = "test" }));
        await Assert.ThrowsAsync<ServiceUnavailableException>(() => client.GetDistanceKmAsync(43, 12, 44, 13));
    }

    [Fact]
    public async Task MissingKey_ReturnsConfigurationError()
    {
        using var http = new HttpClient(new StubHandler(HttpStatusCode.OK));
        var client = new GraphHopperClient(http, Options.Create(new GraphHopperOptions { ApiKey = "" }));
        var error = await Assert.ThrowsAsync<ServiceUnavailableException>(() => client.GetDistanceKmAsync(43, 12, 44, 13));
        Assert.Contains("non è configurato", error.Message);
    }

    private sealed class StubHandler(HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(status));
    }
}
