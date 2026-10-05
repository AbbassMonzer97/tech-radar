using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TechRadar.Core.Models;
using TechRadar.Core.Options;

namespace TechRadar.Core.Fetchers;

public class GitHubFetcher : INewsSource
{
    private const int ReposPerTopic = 10;
    private readonly HttpClient _http;

    public GitHubFetcher(HttpClient http)
    {
        _http = http;
        // GitHub rejects requests without a User-Agent header.
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("TechRadar/1.0");
    }

    public string Type => "github";

    public async Task<List<NewsItem>> FetchAsync(NewsSourceConfig source, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");

        // One search per topic: repos created in the last 7 days, most stars first.
        var results = await Task.WhenAll(source.Topics.Select(topic =>
            _http.GetFromJsonAsync<SearchResult>(
                $"{source.Url}search/repositories?q=topic:{topic}+created:>{since}&sort=stars&order=desc&per_page={ReposPerTopic}",
                ct)));

        return results
            .SelectMany(r => r?.Items ?? [])
            .DistinctBy(repo => repo.HtmlUrl)   // a repo can have several of our topics
            .Select(repo => new NewsItem(
                Title: $"{repo.FullName} (★{repo.Stars}): {repo.Description}",
                Url: repo.HtmlUrl,
                Source: source.Name,
                PublishedAt: repo.CreatedAt))
            .ToList();
    }

    // GitHub uses snake_case names, so we map them to C# names.
    private record SearchResult(List<Repo> Items);

    private record Repo(
        [property: JsonPropertyName("full_name")] string FullName,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        string? Description,
        [property: JsonPropertyName("stargazers_count")] int Stars,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);
}
