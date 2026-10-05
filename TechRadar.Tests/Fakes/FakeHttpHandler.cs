using System.Net;
using System.Text;

namespace TechRadar.Tests.Fakes;

// Stands in for the real internet, like jest.mock(fetch):
// every request gets back the same canned response.
public class FakeHttpHandler(string body, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
        Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/xml"),
        });

    public static HttpClient ClientReturning(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(new FakeHttpHandler(body, status));
}
