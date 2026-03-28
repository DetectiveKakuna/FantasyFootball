using System.Text.RegularExpressions;
using FantasyFootball.Core.Interfaces;
using FantasyFootball.Core.Models;
using Microsoft.Playwright;

namespace FantasyFootball.Infrastructure.FantasyPros;

public class FantasyProsRankingsScraper(IBrowser browser, string baseUrl) : IFantasyProsRankingsScraper
{
    private readonly string _urlTemplate = $"{baseUrl.TrimEnd('/')}/nfl/rankings/{{0}}.php?position=ALL&type={{1}}&scoring={{2}}";
    private const string RowSelector = "table tbody tr";
    private const char NonBreakingSpace = '\u00A0';

    private static readonly Regex PositionRankRegex = new(@"^([A-Z]+)(\d+)$", RegexOptions.Compiled);

    private readonly IBrowser _browser = browser;

    public async Task<List<ScrapedPlayerRanking>> ScrapeExpertRankingsAsync(string slug, string rankingType, string scoringType)
    {
        var page = await _browser.NewPageAsync();
        try
        {
            var url = string.Format(_urlTemplate, slug, rankingType, scoringType);
            await page.GotoAsync(url);
            await page.WaitForSelectorAsync(RowSelector);

            var rows = await page.QuerySelectorAllAsync(RowSelector);
            var rankings = new List<ScrapedPlayerRanking>();
            var scrapedAt = DateTime.UtcNow;

            foreach (var row in rows)
            {
                var cells = await row.QuerySelectorAllAsync("td");
                if (cells.Count < 7)
                    continue;

                var rankText = (await cells[0].TextContentAsync())?.Trim();
                if (!int.TryParse(rankText, out var overallRank))
                    continue;

                var anchor = await cells[1].QuerySelectorAsync("a");
                if (anchor is null)
                    continue;

                var playerName = (await anchor.GetAttributeAsync("fp-player-name"))?.Trim();
                var fpIdClass = (await anchor.GetAttributeAsync("class")) ?? "";
                string? fantasyProsPlayerId = null;

                if (fpIdClass.Contains("fp-id-"))
                {
                    var match = Regex.Match(fpIdClass, @"fp-id-(\S+)");
                    if (match.Success)
                        fantasyProsPlayerId = match.Groups[1].Value;
                }

                if (string.IsNullOrWhiteSpace(playerName))
                    playerName = (await anchor.TextContentAsync())?.Trim() ?? "";

                var positionRankText = (await cells[2].TextContentAsync())?.Trim();
                string? position = null;
                int? positionRankNumber = null;

                if (!string.IsNullOrWhiteSpace(positionRankText))
                {
                    var posMatch = PositionRankRegex.Match(positionRankText);
                    if (posMatch.Success)
                    {
                        position = posMatch.Groups[1].Value;
                        positionRankNumber = int.Parse(posMatch.Groups[2].Value);
                    }
                }

                var teamText = (await cells[3].TextContentAsync())?.Trim();
                string? nflTeam = string.IsNullOrWhiteSpace(teamText) || teamText.Contains(NonBreakingSpace) || teamText == "\u00a0"
                    ? null
                    : teamText;

                var ecrText = (await cells[5].TextContentAsync())?.Trim();
                int? ecrRank = int.TryParse(ecrText, out var ecr) ? ecr : null;

                var deltaText = (await cells[6].TextContentAsync())?.Trim();
                int? deltaVsEcr = int.TryParse(deltaText, out var delta) ? delta : null;

                rankings.Add(new ScrapedPlayerRanking
                {
                    FantasyProsPlayerId = fantasyProsPlayerId,
                    PlayerName = playerName,
                    Position = position ?? "",
                    NflTeam = nflTeam,
                    OverallRank = overallRank,
                    PositionRank = positionRankText,
                    PositionRankNumber = positionRankNumber,
                    EcrRank = ecrRank,
                    DeltaVsEcr = deltaVsEcr,
                    ExpertSlug = slug,
                    RankingType = rankingType,
                    ScoringType = scoringType,
                    ScrapedAt = scrapedAt
                });
            }

            return rankings;
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
