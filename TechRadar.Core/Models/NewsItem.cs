namespace TechRadar.Core.Models;

public record NewsItem(string Title, string Url, string Source, DateTimeOffset PublishedAt);
