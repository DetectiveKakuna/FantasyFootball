using FantasyFootball.Core.Models;

namespace FantasyFootball.Core.Interfaces;

public interface IFantasyProsExpertDirectoryScraper
{
    Task<List<ScrapedExpertDirectory>> ScrapeExpertsWithRankingsAsync(string rankingType, string scoringType);
}
