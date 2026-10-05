using Microsoft.Extensions.Options;
using TechRadar.Api.Jobs;
using TechRadar.Core.Fetchers;
using TechRadar.Core.Options;
using TechRadar.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Read the "Sources" section of appsettings.json into SourcesOptions.
builder.Services.Configure<SourcesOptions>(
    builder.Configuration.GetSection(SourcesOptions.SectionName));

// Register each fetcher with its own ready-made HttpClient,
// then also register it as an INewsSource so NewsCollector gets all of them.
builder.Services.AddHttpClient<RssFetcher>();
builder.Services.AddHttpClient<HackerNewsFetcher>();
builder.Services.AddHttpClient<GitHubFetcher>();
builder.Services.AddTransient<INewsSource>(sp => sp.GetRequiredService<RssFetcher>());
builder.Services.AddTransient<INewsSource>(sp => sp.GetRequiredService<HackerNewsFetcher>());
builder.Services.AddTransient<INewsSource>(sp => sp.GetRequiredService<GitHubFetcher>());
builder.Services.AddTransient<NewsCollector>();

// Fetch on startup and every 6 hours in the background.
builder.Services.AddHostedService<FetchJob>();

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

// Fetch every source at the same time and merge them, newest first.
app.MapGet("/items", (NewsCollector collector, CancellationToken ct) => collector.CollectAsync(ct))
    .WithName("GetItems");

app.Run();
