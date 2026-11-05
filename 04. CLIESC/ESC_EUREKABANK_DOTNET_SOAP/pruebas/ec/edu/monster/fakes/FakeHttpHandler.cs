using System.Net.Http;

namespace ec.edu.monster.fakes;

/// <summary>HTTP handler double for the availability check: records the URLs and scripts the answer.</summary>
public sealed class FakeHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    public List<string> Urls { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Urls.Add(request.RequestUri?.ToString() ?? "");
        return Task.FromResult(responder(request));
    }
}
