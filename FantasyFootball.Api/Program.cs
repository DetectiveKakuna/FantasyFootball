using FantasyFootball.Core.Interfaces;
using FantasyFootball.Infrastructure.FantasyPros;
using FantasyFootball.Infrastructure.Sleeper;
using Microsoft.Playwright;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<IFantasyProsAccuracyScraper, FantasyProsAccuracyScraper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();