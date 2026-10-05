using System.ServiceModel.Syndication;
using System.Xml;
using TechRadar.Core.Models;
using TechRadar.Core.Options;

namespace TechRadar.Core.Fetchers;

// The HttpClient is handed in through DI ("constructor injection").
public class RssFetcher(HttpClient http) : INewsSource
{
    public string Type => "rss";

    public async Task<List<NewsItem>> FetchAsync(NewsSourceConfig source, CancellationToken ct = default)
    {
        var xml = await http.GetStringAsync(source.Url, ct);
        using var reader = XmlReader.Create(new StringReader(xml));
        var feed = SyndicationFeed.Load(reader);

        return feed.Items.Select(item => new NewsItem(
            Title: item.Title?.Text ?? "(no title)",
            Url: item.Links.FirstOrDefault()?.Uri.ToString() ?? "",
            Source: source.Name,
            PublishedAt: item.PublishDate != default ? item.PublishDate : item.LastUpdatedTime
        )).ToList();
    }
}
