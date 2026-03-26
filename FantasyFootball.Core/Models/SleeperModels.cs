using System.Text.Json.Serialization;

namespace FantasyFootball.Core.Models;

public class SleeperLeague
{
    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("season")]
    public string Season { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("draft_id")]
    public string DraftId { get; set; } = string.Empty;

    [JsonPropertyName("total_rosters")]
    public int TotalRosters { get; set; }

    [JsonPropertyName("roster_positions")]
    public List<string> RosterPositions { get; set; } = new();
}

public class SleeperRoster
{
    [JsonPropertyName("roster_id")]
    public int RosterId { get; set; }

    [JsonPropertyName("owner_id")]
    public string OwnerId { get; set; } = string.Empty;

    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; } = string.Empty;

    [JsonPropertyName("players")]
    public List<string> Players { get; set; } = new();

    [JsonPropertyName("starters")]
    public List<string> Starters { get; set; } = new();

    [JsonPropertyName("reserve")]
    public List<string>? Reserve { get; set; }
}

public class SleeperUser
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; } = string.Empty;

    [JsonPropertyName("is_owner")]
    public bool IsOwner { get; set; }
}

public class SleeperDraft
{
    [JsonPropertyName("draft_id")]
    public string DraftId { get; set; } = string.Empty;

    [JsonPropertyName("league_id")]
    public string LeagueId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("season")]
    public string Season { get; set; } = string.Empty;

    [JsonPropertyName("settings")]
    public SleeperDraftSettings? Settings { get; set; }
}

public class SleeperDraftSettings
{
    [JsonPropertyName("rounds")]
    public int Rounds { get; set; }

    [JsonPropertyName("teams")]
    public int Teams { get; set; }

    [JsonPropertyName("pick_timer")]
    public int PickTimer { get; set; }
}

public class SleeperPlayer
{
    [JsonPropertyName("player_id")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("fantasy_positions")]
    public List<string>? FantasyPositions { get; set; }

    [JsonPropertyName("team")]
    public string? Team { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("injury_status")]
    public string? InjuryStatus { get; set; }

    [JsonPropertyName("depth_chart_order")]
    public int? DepthChartOrder { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("years_exp")]
    public int? YearsExp { get; set; }
}