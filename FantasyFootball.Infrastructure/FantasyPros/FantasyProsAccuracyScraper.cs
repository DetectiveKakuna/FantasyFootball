using FantasyFootball.Core.Interfaces;
using FantasyFootball.Core.Models;
using Microsoft.Playwright;

namespace FantasyFootball.Infrastructure.FantasyPros;

public class FantasyProsAccuracyScraper(IBrowser browser, string baseUrl) : IFantasyProsAccuracyScraper
{
    private readonly string _urlTemplate = $"{baseUrl.TrimEnd('/')}/nfl/accuracy/draft.php?year={{0}}";
    private const string RowSelector = "table tbody tr";

    private readonly IBrowser _browser = browser;

    public async Task<List<ScrapedExpertAccuracy>> ScrapeAsync(int year)
    {
        var page = await _browser.NewPageAsync();
        try
        {
            await page.GotoAsync(string.Format(_urlTemplate, year));
            await page.WaitForSelectorAsync(RowSelector);

            var rows = await page.QuerySelectorAllAsync(RowSelector);
            var experts = new List<ScrapedExpertAccuracy>();

            foreach (var row in rows)
            {
                var cells = await row.QuerySelectorAllAsync("td");
                if (cells.Count < 9)
                    continue;

                var anchor = await cells[1].QuerySelectorAsync("a");
                if (anchor is null)
                    continue;

                var fullText = (await anchor.TextContentAsync())?.Trim() ?? "";
                var parts = fullText.Split(" - ", 2);
                var name = parts[0].Trim();

                var rankText = (await cells[0].TextContentAsync())?.Trim();

                experts.Add(new ScrapedExpertAccuracy
                {
                    Name = name,
                    OverallRank = int.TryParse(rankText, out var rank) ? rank : null,
                    AccuracyRankQb = await ParseDataSortAsync(cells[2]),
                    AccuracyRankRb = await ParseDataSortAsync(cells[3]),
                    AccuracyRankWr = await ParseDataSortAsync(cells[4]),
                    AccuracyRankTe = await ParseDataSortAsync(cells[5]),
                    AccuracyRankK = await ParseDataSortAsync(cells[6]),
                    AccuracyRankDst = await ParseDataSortAsync(cells[7]),
                    AccuracyRankIdp = await ParseDataSortAsync(cells[8]),
                });
            }

            return experts;
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static async Task<int?> ParseDataSortAsync(IElementHandle cell)
    {
        var value = await cell.GetAttributeAsync("data-sort");
        if (string.IsNullOrWhiteSpace(value) || value == "-")
            return null;
        return int.TryParse(value, out var parsed) && parsed > 0 && parsed < 10000 ? parsed : null;
    }
}
