using System.Net.Http.Json;
using FantasyFootball.Core.Interfaces;
using FantasyFootball.Core.Models;

namespace FantasyFootball.Infrastructure.Sleeper;

public class SleeperClient(HttpClient http) : ISleeperClient
{
    private readonly HttpClient _http = http;

    public async Task<SleeperLeague?> GetLeagueAsync(string leagueId)
    {
        return await _http.GetFromJsonAsync<SleeperLeague>($"league/{leagueId}");
    }

    public async Task<List<SleeperRoster>?> GetRostersAsync(string leagueId)
    {
        return await _http.GetFromJsonAsync<List<SleeperRoster>>($"league/{leagueId}/rosters");
    }

    public async Task<List<SleeperUser>?> GetUsersAsync(string leagueId)
    {
        return await _http.GetFromJsonAsync<List<SleeperUser>>($"league/{leagueId}/users");
    }

    public async Task<List<SleeperDraft>?> GetDraftsAsync(string leagueId)
    {
        return await _http.GetFromJsonAsync<List<SleeperDraft>>($"league/{leagueId}/drafts");
    }

    public async Task<Dictionary<string, SleeperPlayer>?> GetAllPlayersAsync()
    {
        return await _http.GetFromJsonAsync<Dictionary<string, SleeperPlayer>>("players/nfl");
    }
}