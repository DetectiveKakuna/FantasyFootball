using FantasyFootball.Core.Interfaces;
using FantasyFootball.Infrastructure.Extensions;
using FantasyFootball.Infrastructure.FantasyPros;
using FantasyFootball.Infrastructure.Persistence;
using FantasyFootball.Infrastructure.Sleeper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

var builder = WebApplication.CreateBuilder(args);

var fantasyProsBaseUrl = builder.Configuration.GetRequiredBaseUrl("FantasyPros:BaseUrl").ToString();
var sleeperBaseUri = builder.Configuration.GetRequiredBaseUrl("Sleeper:BaseUrl");

builder.Services.AddDbContext<FantasyFootballDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<ISleeperClient, SleeperClient>(client =>
{
    client.BaseAddress = sleeperBaseUri;
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