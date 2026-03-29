using FantasyFootball.Core.Enums;

namespace FantasyFootball.Core.Entities;

public class PlayerEntity
{
    public int Id { get; set; }
    public int SleeperId { get; set; }
    public int FantasyProsId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Position Position { get; set; }
    public List<Position> FantasyPositions { get; set; } = [];
    public NflTeam Team { get; set; }
    public bool Active { get; set; }
    public InjuryStatus? InjuryStatus { get; set; }
    public int? DepthChartOrder { get; set; }
    public int? Age { get; set; }
    public int? YearsExp { get; set; }
    public DateTime LastUpdated { get; set; }
}
