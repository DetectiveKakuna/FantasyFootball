namespace FantasyFootball.Core.Entities;

public class ExpertEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public double CustomWeight { get; set; } = 1.0;
    public AccuracyEntity? DraftAccuracy { get; set; }
    public AccuracyEntity? WeeklyAccuracy { get; set; }
    public DateTime? DraftRankingsLastUpdated { get; set; }
    public DateTime? WeeklyRankingsLastUpdated { get; set; }
    public DateTime? RosRankingsLastUpdated { get; set; }
    public List<int>? DraftRankings { get; set; }
    public List<int>? WeeklyRankings { get; set; }
    public List<int>? RosRankings { get; set; }
}
