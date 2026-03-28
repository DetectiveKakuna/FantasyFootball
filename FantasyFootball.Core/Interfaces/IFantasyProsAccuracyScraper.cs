using FantasyFootball.Core.Models;

namespace FantasyFootball.Core.Interfaces;

public interface IFantasyProsAccuracyScraper
{
    Task<List<ScrapedExpertAccuracy>> ScrapeAsync(int year);
}
