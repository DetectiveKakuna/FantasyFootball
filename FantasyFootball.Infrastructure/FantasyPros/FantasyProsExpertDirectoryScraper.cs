using System.Globalization;
using System.Text.RegularExpressions;
using FantasyFootball.Core.Interfaces;
using FantasyFootball.Core.Models;
using Microsoft.Playwright;

namespace FantasyFootball.Infrastructure.FantasyPros;

public class FantasyProsExpertDirectoryScraper(IBrowser browser, string baseUrl) : IFantasyProsExpertDirectoryScraper
{
    private readonly string _urlTemplate = $"{baseUrl.TrimEnd('/')}/nfl/rankings/?type={{0}}&scoring={{1}}";
    private const string TableSelector = "#expert-data";
    private const string RowSelector = "tbody tr";

    private static readonly Regex SlugRegex = new(@"/nfl/rankings/([^/]+)\.php", RegexOptions.Compiled);

    private readonly IBrowser _browser = browser;

    public async Task<List<ScrapedExpertDirectory>> ScrapeExpertsWithRankingsAsync(string rankingType, string scoringType)
    {
        var page = await _browser.NewPageAsync();
        try
        {
            var url = string.Format(_urlTemplate, rankingType, scoringType);
            await page.GotoAsync(url);
            await page.WaitForSelectorAsync(TableSelector);

            var table = await page.QuerySelectorAsync(TableSelector);
            if (table is null)
                return [];

            var rows = await table.QuerySelectorAllAsync(RowSelector);
            var experts = new List<ScrapedExpertDirectory>();

            foreach (var row in rows)
            {
                var cells = await row.QuerySelectorAllAsync("td");
                if (cells.Count < 4)
                    continue;

                var nameAnchor = await cells[0].QuerySelectorAsync("a");
                if (nameAnchor is null)
                    continue;

                var name = (await nameAnchor.TextContentAsync())?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var rankingsAnchor = await cells[1].QuerySelectorAsync("a");
                if (rankingsAnchor is null)
                    continue;

                var href = await rankingsAnchor.GetAttributeAsync("href");
                if (string.IsNullOrWhiteSpace(href))
                    continue;

                var slugMatch = SlugRegex.Match(href);
                if (!slugMatch.Success)
                    continue;

                var slug = slugMatch.Groups[1].Value;

                var lastUpdatedText = (await cells[3].TextContentAsync())?.Trim();
                DateTime? lastUpdated = null;
                if (!string.IsNullOrWhiteSpace(lastUpdatedText) &&
                    DateTime.TryParseExact(lastUpdatedText, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    lastUpdated = parsed;
                }

                experts.Add(new ScrapedExpertDirectory
                {
                    Name = name,
                    Slug = slug,
                    HasRankings = true,
                    LastUpdated = lastUpdated
                });
            }

            return experts;
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
