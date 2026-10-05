using Microsoft.Extensions.Options;
using TechRadar.Core.Fetchers;
using TechRadar.Core.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Read the "Sources" section of appsettings.json into SourcesOptions.
builder.Services.Configure<SourcesOptions>(
    builder.Configuration.GetSection(SourcesOptions.SectionName));

// Register RssFetcher and give it a ready-made HttpClient.
builder.Services.AddHttpClient<RssFetcher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => "ok")
    .WithName("GetHealth");

app.MapGet("/sources", (IOptions<SourcesOptions> options) => options.Value.Items)
    .WithName("GetSources");

// Fetch all RSS feeds at the same time and merge them, newest first.
app.MapGet("/items", async (RssFetcher rss, IOptions<SourcesOptions> options, CancellationToken ct) =>
{
    var feeds = options.Value.Items.Where(s => s.Type == "rss");
    var results = await Task.WhenAll(feeds.Select(s => rss.FetchAsync(s, ct)));
    return results.SelectMany(items => items).OrderByDescending(i => i.PublishedAt);
})
.WithName("GetItems");

app.Run();
