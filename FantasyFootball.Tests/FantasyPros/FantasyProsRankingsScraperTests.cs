using FantasyFootball.Infrastructure.FantasyPros;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FantasyFootball.Tests.FantasyPros;

[TestClass]
public class FantasyProsRankingsScraperTests
{
    private Mock<IBrowser> _mockBrowser = null!;
    private Mock<IPage> _mockPage = null!;
    private FantasyProsRankingsScraper _scraper = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockBrowser = new Mock<IBrowser>();
        _mockPage = new Mock<IPage>();

        _mockBrowser
            .Setup(b => b.NewPageAsync(It.IsAny<BrowserNewPageOptions>()))
            .ReturnsAsync(_mockPage.Object);

        _mockPage
            .Setup(p => p.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions>()))
            .ReturnsAsync((IResponse?)null);

        _mockPage
            .Setup(p => p.WaitForSelectorAsync(It.IsAny<string>(), It.IsAny<PageWaitForSelectorOptions>()))
            .ReturnsAsync((IElementHandle?)null);

        _mockPage
            .Setup(p => p.CloseAsync(It.IsAny<PageCloseOptions>()))
            .Returns(Task.CompletedTask);

        _scraper = new FantasyProsRankingsScraper(_mockBrowser.Object, "https://fake-base.test");
    }

    #region ScrapeExpertRankingsAsync

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_ReturnsRankings_WithValidSkillPlayerData()
    {
        var row = CreateMockSkillPlayerRow("1", "12345", "Alpha Bravo", "QB1", "TMA", "10", "1", "-1");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        var ranking = result[0];
        Assert.AreEqual("12345", ranking.FantasyProsPlayerId);
        Assert.AreEqual("Alpha Bravo", ranking.PlayerName);
        Assert.AreEqual("QB", ranking.Position);
        Assert.AreEqual("TMA", ranking.NflTeam);
        Assert.AreEqual(1, ranking.OverallRank);
        Assert.AreEqual("QB1", ranking.PositionRank);
        Assert.AreEqual(1, ranking.PositionRankNumber);
        Assert.AreEqual(1, ranking.EcrRank);
        Assert.AreEqual(-1, ranking.DeltaVsEcr);
        Assert.AreEqual("test-expert", ranking.ExpertSlug);
        Assert.AreEqual("draft", ranking.RankingType);
        Assert.AreEqual("ppr", ranking.ScoringType);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_HandlesDstRows_WithNullPlayerId()
    {
        var row = CreateMockDstRow("25", "Charlie Defense", "DST2", "TMB", "", "30", "5");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        var ranking = result[0];
        Assert.IsNull(ranking.FantasyProsPlayerId);
        Assert.AreEqual("Charlie Defense", ranking.PlayerName);
        Assert.AreEqual("DST", ranking.Position);
        Assert.AreEqual("TMB", ranking.NflTeam);
        Assert.AreEqual(25, ranking.OverallRank);
        Assert.AreEqual("DST2", ranking.PositionRank);
        Assert.AreEqual(2, ranking.PositionRankNumber);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_SetsNullNflTeam_WhenTeamIsNbsp()
    {
        var row = CreateMockSkillPlayerRow("5", "999", "Delta Echo", "WR10", "\u00a0", "", "N/A", "");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].NflTeam);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_SetsNullNflTeam_WhenTeamIsEmpty()
    {
        var row = CreateMockSkillPlayerRow("5", "999", "Foxtrot Golf", "WR10", "", "", "N/A", "");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].NflTeam);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_SetsNullEcrRank_WhenValueIsNA()
    {
        var row = CreateMockSkillPlayerRow("10", "111", "Hotel India", "RB3", "TMC", "", "N/A", "");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].EcrRank);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_SetsNullDeltaVsEcr_WhenEmpty()
    {
        var row = CreateMockSkillPlayerRow("10", "111", "Juliet Kilo", "RB3", "TMC", "", "5", "");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].DeltaVsEcr);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_ParsesMultiplePlayers()
    {
        var row1 = CreateMockSkillPlayerRow("1", "100", "Lima Mike", "WR1", "TMA", "", "2", "-1");
        var row2 = CreateMockSkillPlayerRow("2", "200", "November Oscar", "RB1", "TMB", "", "1", "1");
        var row3 = CreateMockDstRow("3", "Papa Defense", "DST1", "TMB", "", "5", "-2");
        SetupTableRows(row1, row2, row3);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Lima Mike", result[0].PlayerName);
        Assert.AreEqual("November Oscar", result[1].PlayerName);
        Assert.AreEqual("Papa Defense", result[2].PlayerName);
        Assert.IsNull(result[2].FantasyProsPlayerId);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_ReturnsEmptyList_WhenNoRowsExist()
    {
        SetupTableRows();

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_SkipsRows_WithInsufficientCells()
    {
        var validRow = CreateMockSkillPlayerRow("1", "100", "Quebec Romeo", "WR1", "TMA", "", "2", "-1");
        var shortRow = CreateMockRowWithCellCount(3);
        SetupTableRows(shortRow, validRow);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Quebec Romeo", result[0].PlayerName);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_SkipsRows_WithNoAnchorTag()
    {
        var validRow = CreateMockSkillPlayerRow("1", "100", "Sierra Tango", "WR1", "TMA", "", "2", "-1");
        var noAnchorRow = CreateMockRowWithNoAnchor();
        SetupTableRows(noAnchorRow, validRow);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Sierra Tango", result[0].PlayerName);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_SkipsRows_WithNonNumericRank()
    {
        var validRow = CreateMockSkillPlayerRow("1", "100", "Uniform Victor", "WR1", "TMA", "", "2", "-1");
        var badRankRow = CreateMockSkillPlayerRow("abc", "200", "Whiskey Xray", "RB1", "TMB", "", "1", "");
        SetupTableRows(badRankRow, validRow);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Uniform Victor", result[0].PlayerName);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_NavigatesToCorrectUrl()
    {
        SetupTableRows();

        await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        _mockPage.Verify(p => p.GotoAsync(
            "https://fake-base.test/nfl/rankings/test-expert.php?position=ALL&type=draft&scoring=ppr",
            It.IsAny<PageGotoOptions>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_ClosesPage_AfterSuccessfulScrape()
    {
        SetupTableRows();

        await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        _mockPage.Verify(p => p.CloseAsync(It.IsAny<PageCloseOptions>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_ClosesPage_WhenExceptionOccurs()
    {
        _mockPage
            .Setup(p => p.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions>()))
            .ThrowsAsync(new PlaywrightException("Navigation failed"));

        await Assert.ThrowsExactlyAsync<PlaywrightException>(() =>
            _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr"));

        _mockPage.Verify(p => p.CloseAsync(It.IsAny<PageCloseOptions>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_SetsScrapedAtToUtcNow()
    {
        var row = CreateMockSkillPlayerRow("1", "100", "Yankee Zulu", "WR1", "TMA", "", "2", "-1");
        SetupTableRows(row);

        var before = DateTime.UtcNow;
        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");
        var after = DateTime.UtcNow;

        Assert.IsTrue(result[0].ScrapedAt >= before && result[0].ScrapedAt <= after);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_PassesRankingTypeAndScoringType()
    {
        var row = CreateMockSkillPlayerRow("1", "100", "Alpha Charlie", "WR1", "TMA", "", "2", "-1");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "weekly", "half");

        Assert.AreEqual("weekly", result[0].RankingType);
        Assert.AreEqual("half", result[0].ScoringType);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertRankingsAsync")]
    public async Task ScrapeExpertRankingsAsync_FallsBackToTextContent_WhenFpPlayerNameMissing()
    {
        var row = CreateMockRowWithFallbackName("1", "100", "Bravo Delta", "TE1", "TMD", "", "3", "");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertRankingsAsync("test-expert", "draft", "ppr");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Bravo Delta", result[0].PlayerName);
        Assert.AreEqual("100", result[0].FantasyProsPlayerId);
    }

    #endregion

    #region Private Helpers

    private void SetupTableRows(params Mock<IElementHandle>[] rows)
    {
        _mockPage
            .Setup(p => p.QuerySelectorAllAsync("#ranking-data tbody tr"))
            .ReturnsAsync(rows.Select(r => r.Object).ToList<IElementHandle>().AsReadOnly());
    }

    private static Mock<IElementHandle> CreateMockSkillPlayerRow(
        string rankText, string fpId, string playerName, string positionRank,
        string team, string bye, string ecrRank, string delta)
    {
        var row = new Mock<IElementHandle>();
        var cells = new List<IElementHandle>();

        // Cell 0: overall rank
        var rankCell = new Mock<IElementHandle>();
        rankCell.Setup(c => c.TextContentAsync()).ReturnsAsync(rankText);
        cells.Add(rankCell.Object);

        // Cell 1: player name with fp-player-link anchor
        var playerCell = new Mock<IElementHandle>();
        var anchor = new Mock<IElementHandle>();
        anchor.Setup(a => a.GetAttributeAsync("fp-player-name")).ReturnsAsync(playerName);
        anchor.Setup(a => a.GetAttributeAsync("class")).ReturnsAsync($"fp-player-link fp-id-{fpId}");
        anchor.Setup(a => a.TextContentAsync()).ReturnsAsync(playerName);
        playerCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync(anchor.Object);
        cells.Add(playerCell.Object);

        // Cell 2: position rank
        var posRankCell = new Mock<IElementHandle>();
        posRankCell.Setup(c => c.TextContentAsync()).ReturnsAsync(positionRank);
        cells.Add(posRankCell.Object);

        // Cell 3: team
        var teamCell = new Mock<IElementHandle>();
        teamCell.Setup(c => c.TextContentAsync()).ReturnsAsync(team);
        cells.Add(teamCell.Object);

        // Cell 4: bye week
        var byeCell = new Mock<IElementHandle>();
        byeCell.Setup(c => c.TextContentAsync()).ReturnsAsync(bye);
        cells.Add(byeCell.Object);

        // Cell 5: ECR rank
        var ecrCell = new Mock<IElementHandle>();
        ecrCell.Setup(c => c.TextContentAsync()).ReturnsAsync(ecrRank);
        cells.Add(ecrCell.Object);

        // Cell 6: delta vs ECR
        var deltaCell = new Mock<IElementHandle>();
        deltaCell.Setup(c => c.TextContentAsync()).ReturnsAsync(delta);
        cells.Add(deltaCell.Object);

        row.Setup(r => r.QuerySelectorAllAsync("td"))
            .ReturnsAsync(cells.AsReadOnly());

        return row;
    }

    private static Mock<IElementHandle> CreateMockDstRow(
        string rankText, string teamName, string positionRank,
        string team, string bye, string ecrRank, string delta)
    {
        var row = new Mock<IElementHandle>();
        var cells = new List<IElementHandle>();

        // Cell 0: overall rank
        var rankCell = new Mock<IElementHandle>();
        rankCell.Setup(c => c.TextContentAsync()).ReturnsAsync(rankText);
        cells.Add(rankCell.Object);

        // Cell 1: DST anchor (no fp-id class)
        var playerCell = new Mock<IElementHandle>();
        var anchor = new Mock<IElementHandle>();
        anchor.Setup(a => a.GetAttributeAsync("fp-player-name")).ReturnsAsync((string?)null);
        anchor.Setup(a => a.GetAttributeAsync("class")).ReturnsAsync("");
        anchor.Setup(a => a.TextContentAsync()).ReturnsAsync(teamName);
        playerCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync(anchor.Object);
        cells.Add(playerCell.Object);

        // Cell 2: position rank
        var posRankCell = new Mock<IElementHandle>();
        posRankCell.Setup(c => c.TextContentAsync()).ReturnsAsync(positionRank);
        cells.Add(posRankCell.Object);

        // Cell 3: team
        var teamCell = new Mock<IElementHandle>();
        teamCell.Setup(c => c.TextContentAsync()).ReturnsAsync(team);
        cells.Add(teamCell.Object);

        // Cell 4: bye week
        var byeCell = new Mock<IElementHandle>();
        byeCell.Setup(c => c.TextContentAsync()).ReturnsAsync(bye);
        cells.Add(byeCell.Object);

        // Cell 5: ECR rank
        var ecrCell = new Mock<IElementHandle>();
        ecrCell.Setup(c => c.TextContentAsync()).ReturnsAsync(ecrRank);
        cells.Add(ecrCell.Object);

        // Cell 6: delta vs ECR
        var deltaCell = new Mock<IElementHandle>();
        deltaCell.Setup(c => c.TextContentAsync()).ReturnsAsync(delta);
        cells.Add(deltaCell.Object);

        row.Setup(r => r.QuerySelectorAllAsync("td"))
            .ReturnsAsync(cells.AsReadOnly());

        return row;
    }

    private static Mock<IElementHandle> CreateMockRowWithFallbackName(
        string rankText, string fpId, string fallbackName, string positionRank,
        string team, string bye, string ecrRank, string delta)
    {
        var row = new Mock<IElementHandle>();
        var cells = new List<IElementHandle>();

        var rankCell = new Mock<IElementHandle>();
        rankCell.Setup(c => c.TextContentAsync()).ReturnsAsync(rankText);
        cells.Add(rankCell.Object);

        var playerCell = new Mock<IElementHandle>();
        var anchor = new Mock<IElementHandle>();
        anchor.Setup(a => a.GetAttributeAsync("fp-player-name")).ReturnsAsync((string?)null);
        anchor.Setup(a => a.GetAttributeAsync("class")).ReturnsAsync($"fp-player-link fp-id-{fpId}");
        anchor.Setup(a => a.TextContentAsync()).ReturnsAsync(fallbackName);
        playerCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync(anchor.Object);
        cells.Add(playerCell.Object);

        var posRankCell = new Mock<IElementHandle>();
        posRankCell.Setup(c => c.TextContentAsync()).ReturnsAsync(positionRank);
        cells.Add(posRankCell.Object);

        var teamCell = new Mock<IElementHandle>();
        teamCell.Setup(c => c.TextContentAsync()).ReturnsAsync(team);
        cells.Add(teamCell.Object);

        var byeCell = new Mock<IElementHandle>();
        byeCell.Setup(c => c.TextContentAsync()).ReturnsAsync(bye);
        cells.Add(byeCell.Object);

        var ecrCell = new Mock<IElementHandle>();
        ecrCell.Setup(c => c.TextContentAsync()).ReturnsAsync(ecrRank);
        cells.Add(ecrCell.Object);

        var deltaCell = new Mock<IElementHandle>();
        deltaCell.Setup(c => c.TextContentAsync()).ReturnsAsync(delta);
        cells.Add(deltaCell.Object);

        row.Setup(r => r.QuerySelectorAllAsync("td"))
            .ReturnsAsync(cells.AsReadOnly());

        return row;
    }

    private static Mock<IElementHandle> CreateMockRowWithCellCount(int cellCount)
    {
        var row = new Mock<IElementHandle>();
        var cells = new List<IElementHandle>();

        for (var i = 0; i < cellCount; i++)
        {
            cells.Add(new Mock<IElementHandle>().Object);
        }

        row.Setup(r => r.QuerySelectorAllAsync("td"))
            .ReturnsAsync(cells.AsReadOnly());

        return row;
    }

    private static Mock<IElementHandle> CreateMockRowWithNoAnchor()
    {
        var row = new Mock<IElementHandle>();
        var cells = new List<IElementHandle>();

        var rankCell = new Mock<IElementHandle>();
        rankCell.Setup(c => c.TextContentAsync()).ReturnsAsync("1");
        cells.Add(rankCell.Object);

        var playerCell = new Mock<IElementHandle>();
        playerCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync((IElementHandle?)null);
        cells.Add(playerCell.Object);

        for (var i = 0; i < 5; i++)
        {
            var cell = new Mock<IElementHandle>();
            cell.Setup(c => c.TextContentAsync()).ReturnsAsync("");
            cells.Add(cell.Object);
        }

        row.Setup(r => r.QuerySelectorAllAsync("td"))
            .ReturnsAsync(cells.AsReadOnly());

        return row;
    }

    #endregion
}
