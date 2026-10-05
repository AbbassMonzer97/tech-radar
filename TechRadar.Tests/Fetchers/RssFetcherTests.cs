using System.Net;
using System.Xml;
using TechRadar.Core.Fetchers;
using TechRadar.Core.Options;
using TechRadar.Tests.Fakes;

namespace TechRadar.Tests.Fetchers;

public class RssFetcherTests
{
    private static readonly NewsSourceConfig Source =
        new() { Name = "Test Blog", Type = "rss", Url = "https://example.com/feed" };

    private const string TwoItemFeed = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0">
          <channel>
            <title>Test Blog</title>
            <link>https://example.com</link>
            <description>A feed for tests</description>
            <item>
              <title>First post</title>
              <link>https://example.com/first</link>
              <pubDate>Mon, 05 Oct 2026 08:00:00 GMT</pubDate>
            </item>
            <item>
              <link>https://example.com/untitled</link>
              <pubDate>Sun, 04 Oct 2026 08:00:00 GMT</pubDate>
            </item>
          </channel>
        </rss>
        """;

    [Fact]
    public async Task FetchAsync_ParsesTitleUrlSourceAndDate()
    {
        var fetcher = new RssFetcher(FakeHttpHandler.ClientReturning(TwoItemFeed));

        var items = await fetcher.FetchAsync(Source);

        Assert.Equal(2, items.Count);
        var first = items[0];
        Assert.Equal("First post", first.Title);
        Assert.Equal("https://example.com/first", first.Url);
        Assert.Equal("Test Blog", first.Source);
        Assert.Equal(new DateTimeOffset(2026, 10, 5, 8, 0, 0, TimeSpan.Zero), first.PublishedAt);
    }

    [Fact]
    public async Task FetchAsync_UsesPlaceholder_WhenItemHasNoTitle()
    {
        var fetcher = new RssFetcher(FakeHttpHandler.ClientReturning(TwoItemFeed));

        var items = await fetcher.FetchAsync(Source);

        Assert.Equal("(no title)", items[1].Title);
    }

    [Fact]
    public async Task FetchAsync_ReturnsEmptyList_ForFeedWithNoItems()
    {
        const string emptyFeed = """
            <rss version="2.0"><channel><title>Empty</title><link>https://example.com</link><description>-</description></channel></rss>
            """;
        var fetcher = new RssFetcher(FakeHttpHandler.ClientReturning(emptyFeed));

        var items = await fetcher.FetchAsync(Source);

        Assert.Empty(items);
    }

    [Fact]
    public async Task FetchAsync_Throws_WhenResponseIsNotXml()
    {
        var fetcher = new RssFetcher(FakeHttpHandler.ClientReturning("this is not xml"));

        await Assert.ThrowsAsync<XmlException>(() => fetcher.FetchAsync(Source));
    }

    [Fact]
    public async Task FetchAsync_Throws_WhenServerReturnsError()
    {
        var fetcher = new RssFetcher(FakeHttpHandler.ClientReturning("", HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<HttpRequestException>(() => fetcher.FetchAsync(Source));
    }
}
