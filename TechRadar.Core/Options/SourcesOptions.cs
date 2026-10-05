namespace TechRadar.Core.Options;

public class SourcesOptions
{
    public const string SectionName = "Sources";

    public List<NewsSourceConfig> Items { get; set; } = [];
}

public class NewsSourceConfig
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";   // "rss", "hackernews", "github"
    public string Url { get; set; } = "";
}
