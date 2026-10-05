using TechRadar.Core.Services;

namespace TechRadar.Api.Jobs;

// Runs on its own inside the API, like a useEffect + setInterval at the top of the app.
// Fetches once on startup, then every 6 hours until the app stops.
public class FetchJob(IServiceScopeFactory scopeFactory, ILogger<FetchJob> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        // do/while: run once right away, then wait for each tick.
        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            // The job lives for the whole app, but NewsCollector is short-lived,
            // so we make a fresh scope (a "mini container") for each run.
            using var scope = scopeFactory.CreateScope();
            var collector = scope.ServiceProvider.GetRequiredService<NewsCollector>();

            var items = await collector.CollectAsync(ct);
            logger.LogInformation("Background fetch found {Count} items", items.Count);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            // Never let one bad run kill the job (or the app).
            logger.LogError(ex, "Background fetch failed");
        }
    }
}
