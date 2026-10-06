namespace TechRadar.Core.Data;

// One row in the NewsItems table.
// NewsItem (the record) is what fetchers return; this class is what we store.
public class NewsItemEntity
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Url { get; set; }
    public required string Source { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public DateTimeOffset FetchedAt { get; set; }
}
