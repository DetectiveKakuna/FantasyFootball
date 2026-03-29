using FantasyFootball.Core.Interfaces;
using FantasyFootball.Infrastructure.FantasyPros;
using FantasyFootball.Infrastructure.Sleeper;
using Microsoft.Playwright;

var builder = WebApplication.CreateBuilder(args);

var fantasyProsBaseUrl = builder.Configuration["FantasyPros:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration key 'FantasyPros:BaseUrl' is missing.");
var sleeperBaseUrl = builder.Configuration["Sleeper:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration key 'Sleeper:BaseUrl' is missing.");

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<ISleeperClient, SleeperClient>(client =>
{
    client.BaseAddress = new Uri(sleeperBaseUrl);
});

builder.Services.AddSingleton<PlaywrightHostedService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<PlaywrightHostedService>());
builder.Services.AddSingleton<IPlaywright>(sp => sp.GetRequiredService<PlaywrightHostedService>().Playwright);
builder.Services.AddSingleton<IBrowser>(sp => sp.GetRequiredService<PlaywrightHostedService>().Browser);

builder.Services.AddSingleton<IFantasyProsAccuracyScraper>(sp =>
    new FantasyProsAccuracyScraper(sp.GetRequiredService<IBrowser>(), fantasyProsBaseUrl));
builder.Services.AddSingleton<IFantasyProsExpertDirectoryScraper>(sp =>
    new FantasyProsExpertDirectoryScraper(sp.GetRequiredService<IBrowser>(), fantasyProsBaseUrl));
builder.Services.AddSingleton<IFantasyProsRankingsScraper>(sp =>
    new FantasyProsRankingsScraper(sp.GetRequiredService<IBrowser>(), fantasyProsBaseUrl));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();