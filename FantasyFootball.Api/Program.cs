using FantasyFootball.Core.Interfaces;
using FantasyFootball.Infrastructure.Sleeper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<ISleeperClient, SleeperClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Sleeper:BaseUrl"]!);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();