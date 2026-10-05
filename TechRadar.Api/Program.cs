using Microsoft.Extensions.Options;
using TechRadar.Core.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Read the "Sources" section of appsettings.json into SourcesOptions.
builder.Services.Configure<SourcesOptions>(
    builder.Configuration.GetSection(SourcesOptions.SectionName));

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

app.Run();
