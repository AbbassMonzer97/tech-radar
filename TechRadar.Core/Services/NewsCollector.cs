using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TechRadar.Core.Fetchers;
using TechRadar.Core.Models;
using TechRadar.Core.Options;

namespace TechRadar.Core.Services;

// Runs every configured source with the fetcher that matches its Type.
// DI hands us all registered INewsSource classes at once.
public class NewsCollector(
    IEnumerable<INewsSource> fetchers,
    IOptions<SourcesOptions> options,
    ILogger<NewsCollector> logger)
{
    public async Task<List<NewsItem>> CollectAsync(CancellationToken ct = default)
    {
        var results = await Task.WhenAll(options.Value.Items.Select(s => FetchSafelyAsync(s, ct)));
        return results.SelectMany(items => items).OrderByDescending(i => i.PublishedAt).ToList();
    }

    // Like Promise.allSettled: one broken source logs a warning and returns nothing,
    // instead of crashing the whole request.
    private async Task<List<NewsItem>> FetchSafelyAsync(NewsSourceConfig source, CancellationToken ct)
    {
        var fetcher = fetchers.FirstOrDefault(f => f.Type == source.Type);
        if (fetcher is null)
        {
            logger.LogWarning("No fetcher for source {Source} of type {Type}", source.Name, source.Type);
            return [];
        }

        try
        {
            var items = await fetcher.FetchAsync(source, ct);
            logger.LogInformation("Fetched {Count} items from {Source}", items.Count, source.Name);
            return items;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Failed to fetch {Source}", source.Name);
            return [];
        }
    }
}
