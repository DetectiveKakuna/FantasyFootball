using FantasyFootball.Core.Models;

namespace FantasyFootball.Core.Interfaces;

public interface IFantasyProsRankingsScraper
{
    Task<List<ScrapedPlayerRanking>> ScrapeExpertRankingsAsync(string slug, string rankingType, string scoringType);
}
