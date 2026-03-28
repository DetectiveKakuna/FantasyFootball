using System.Text.RegularExpressions;
using FantasyFootball.Core.Interfaces;
using FantasyFootball.Core.Models;
using Microsoft.Playwright;

namespace FantasyFootball.Infrastructure.FantasyPros;

public class FantasyProsRankingsScraper(IBrowser browser, string baseUrl) : IFantasyProsRankingsScraper
{
    private readonly string _baseUrl = baseUrl.TrimEnd('/');
    private const string RowSelector = "#ranking-data tbody tr";
    private const char NonBreakingSpace = '\u00A0';

    private static readonly Regex PositionRankRegex = new(@"^([A-Z]+)(\d+)$", RegexOptions.Compiled);
    private static readonly Regex FpIdRegex = new(@"fp-id-(\S+)", RegexOptions.Compiled);

    private readonly IBrowser _browser = browser;

    public async Task<List<ScrapedPlayerRanking>> ScrapeExpertRankingsAsync(string slug, string rankingType, string scoringType)
    {
        var page = await _browser.NewPageAsync();
        page.SetDefaultTimeout(5000);
        try
        {
            var url = $"{_baseUrl}/nfl/rankings/{Uri.EscapeDataString(slug)}.php" +
                      $"?position=ALL&type={Uri.EscapeDataString(rankingType)}&scoring={Uri.EscapeDataString(scoringType)}";
            await page.GotoAsync(url);

            try
            {
                await page.WaitForSelectorAsync(RowSelector);
            }
            catch (TimeoutException)
            {
                return [];
            }

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
                    var match = FpIdRegex.Match(fpIdClass);
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

                var teamText = (await cells[3].TextContentAsync())?.Replace(NonBreakingSpace, ' ').Trim();
                string? nflTeam = string.IsNullOrWhiteSpace(teamText) ? null : teamText;

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
