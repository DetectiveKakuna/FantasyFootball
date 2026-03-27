using FantasyFootball.Core.Models;

namespace FantasyFootball.Core.Interfaces;

public interface IFantasyProsAccuracyScraper
{
    Task<List<ScrapedExpert>> ScrapeAsync(int year);
}
