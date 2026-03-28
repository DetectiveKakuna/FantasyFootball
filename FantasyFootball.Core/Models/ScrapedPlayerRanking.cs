namespace FantasyFootball.Core.Models;

public class ScrapedPlayerRanking
{
    public string? FantasyProsPlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? NflTeam { get; set; }
    public int OverallRank { get; set; }
    public string? PositionRank { get; set; }
    public int? PositionRankNumber { get; set; }
    public int? EcrRank { get; set; }
    public int? DeltaVsEcr { get; set; }
    public string ExpertSlug { get; set; } = string.Empty;
    public string RankingType { get; set; } = string.Empty;
    public string ScoringType { get; set; } = string.Empty;
    public DateTime ScrapedAt { get; set; }
}
