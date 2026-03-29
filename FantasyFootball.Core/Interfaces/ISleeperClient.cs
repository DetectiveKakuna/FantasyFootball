using FantasyFootball.Core.Models;

namespace FantasyFootball.Core.Interfaces;

public interface ISleeperClient
{
    Task<SleeperLeague?> GetLeagueAsync(string leagueId);
    Task<List<SleeperRoster>?> GetRostersAsync(string leagueId);
    Task<List<SleeperUser>?> GetUsersAsync(string leagueId);
    Task<List<SleeperDraft>?> GetDraftsAsync(string leagueId);
    Task<Dictionary<string, SleeperPlayer>?> GetAllPlayersAsync();
}