using System.Net.Http.Json;
using TechRadar.Core.Models;
using TechRadar.Core.Options;

namespace TechRadar.Core.Fetchers;

public class HackerNewsFetcher(HttpClient http) : INewsSource
{
    private const int MaxStories = 30;

    public string Type => "hackernews";

    public async Task<List<NewsItem>> FetchAsync(NewsSourceConfig source, CancellationToken ct = default)
    {
        // topstories.json is just a list of story IDs, best first.
        var ids = await http.GetFromJsonAsync<List<int>>($"{source.Url}topstories.json", ct) ?? [];

        // Fetch the top stories at the same time (like Promise.all).
        var stories = await Task.WhenAll(ids.Take(MaxStories).Select(id =>
            http.GetFromJsonAsync<HnStory>($"{source.Url}item/{id}.json", ct)));

        return stories
            .Where(s => s?.Title is not null)
            .Select(s => new NewsItem(
                Title: s!.Title!,
                // "Ask HN" posts have no url, so link to the discussion instead.
                Url: s.Url ?? $"https://news.ycombinator.com/item?id={s.Id}",
                Source: source.Name,
                PublishedAt: DateTimeOffset.FromUnixTimeSeconds(s.Time)))
            .ToList();
    }

    private record HnStory(int Id, string? Title, string? Url, long Time);
}
