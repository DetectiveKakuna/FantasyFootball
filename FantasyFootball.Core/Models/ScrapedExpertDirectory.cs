namespace FantasyFootball.Core.Models;

public class ScrapedExpertDirectory
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool HasRankings { get; set; }
    public DateTime? LastUpdated { get; set; }
}
