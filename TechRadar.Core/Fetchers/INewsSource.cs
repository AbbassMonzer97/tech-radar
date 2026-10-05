using TechRadar.Core.Models;
using TechRadar.Core.Options;

namespace TechRadar.Core.Fetchers;

// Every fetcher (RSS, Hacker News, GitHub) has this same shape,
// so the rest of the app can treat them all the same way.
public interface INewsSource
{
    // Matches "Type" in appsettings.json, e.g. "rss".
    string Type { get; }

    Task<List<NewsItem>> FetchAsync(NewsSourceConfig source, CancellationToken ct = default);
}
