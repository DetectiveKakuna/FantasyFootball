namespace FantasyFootball.Core.Models;

public class ScrapedExpert
{
    public string Name { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int? OverallRank { get; set; }
    public int? AccuracyRankQb { get; set; }
    public int? AccuracyRankRb { get; set; }
    public int? AccuracyRankWr { get; set; }
    public int? AccuracyRankTe { get; set; }
    public int? AccuracyRankK { get; set; }
    public int? AccuracyRankDst { get; set; }
    public int? AccuracyRankIdp { get; set; }
    public int AccuracyYear { get; set; }
}
