using FantasyFootball.Core.Interfaces;
using FantasyFootball.Infrastructure.FantasyPros;
using FantasyFootball.Infrastructure.Sleeper;
using Microsoft.Playwright;

var builder = WebApplication.CreateBuilder(args);

var fantasyProsBaseUrl = builder.Configuration["FantasyPros:BaseUrl"]!;

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<ISleeperClient, SleeperClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Sleeper:BaseUrl"]!);
});

builder.Services.AddSingleton<IPlaywright>(_ =>
    Playwright.CreateAsync().GetAwaiter().GetResult());
builder.Services.AddSingleton<IBrowser>(sp =>
    sp.GetRequiredService<IPlaywright>().Chromium
        .LaunchAsync(new BrowserTypeLaunchOptions { Headless = true })
        .GetAwaiter().GetResult());

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