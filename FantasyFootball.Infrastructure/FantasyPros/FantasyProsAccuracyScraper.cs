using System.Text.RegularExpressions;
using FantasyFootball.Core.Interfaces;
using FantasyFootball.Core.Models;
using Microsoft.Playwright;

namespace FantasyFootball.Infrastructure.FantasyPros;

public class FantasyProsAccuracyScraper(IBrowser browser) : IFantasyProsAccuracyScraper
{
    private const string UrlTemplate = "https://www.fantasypros.com/nfl/accuracy/draft.php?year={0}";
    private const string RowSelector = "table tbody tr";

    private static readonly Regex PunctuationRegex = new(@"[^\w\s-]", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex MultiHyphenRegex = new(@"-{2,}", RegexOptions.Compiled);

    private readonly IBrowser _browser = browser;

    public async Task<List<ScrapedExpert>> ScrapeAsync(int year)
    {
        var page = await _browser.NewPageAsync();
        try
        {
            await page.GotoAsync(string.Format(UrlTemplate, year));
            await page.WaitForSelectorAsync(RowSelector);

            var rows = await page.QuerySelectorAllAsync(RowSelector);
            var experts = new List<ScrapedExpert>();

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
                var site = parts.Length > 1 ? parts[1].Trim() : "";

                var rankText = (await cells[0].TextContentAsync())?.Trim();

                experts.Add(new ScrapedExpert
                {
                    Name = name,
                    Site = site,
                    Slug = GenerateSlug(name),
                    OverallRank = int.TryParse(rankText, out var rank) ? rank : null,
                    AccuracyRankQb = await ParseDataSortAsync(cells[2]),
                    AccuracyRankRb = await ParseDataSortAsync(cells[3]),
                    AccuracyRankWr = await ParseDataSortAsync(cells[4]),
                    AccuracyRankTe = await ParseDataSortAsync(cells[5]),
                    AccuracyRankK = await ParseDataSortAsync(cells[6]),
                    AccuracyRankDst = await ParseDataSortAsync(cells[7]),
                    AccuracyRankIdp = await ParseDataSortAsync(cells[8]),
                    AccuracyYear = year
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

    public static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = PunctuationRegex.Replace(slug, "");
        slug = WhitespaceRegex.Replace(slug, "-");
        slug = MultiHyphenRegex.Replace(slug, "-");
        return slug.Trim('-');
    }
}
