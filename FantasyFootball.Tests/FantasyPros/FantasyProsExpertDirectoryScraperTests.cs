using FantasyFootball.Infrastructure.FantasyPros;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FantasyFootball.Tests.FantasyPros;

[TestClass]
public class FantasyProsExpertDirectoryScraperTests
{
    private Mock<IBrowser> _mockBrowser = null!;
    private Mock<IPage> _mockPage = null!;
    private FantasyProsExpertDirectoryScraper _scraper = null!;

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

        _scraper = new FantasyProsExpertDirectoryScraper(_mockBrowser.Object, "https://fake-base.test");
    }

    #region ScrapeExpertsWithRankingsAsync

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_ReturnsExpert_WithValidRankingsLink()
    {
        var row = CreateMockRow("Alpha Bravo", "/nfl/rankings/alpha-bravo.php?type=draft&scoring=PPR", "03/27/2026");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(1, result.Count);
        var expert = result[0];
        Assert.AreEqual("Alpha Bravo", expert.Name);
        Assert.AreEqual("alpha-bravo", expert.Slug);
        Assert.IsTrue(expert.HasRankings);
        Assert.AreEqual(new DateTime(2026, 3, 27), expert.LastUpdated);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_SkipsExpert_WithNoRankingsLink()
    {
        var rowWithRankings = CreateMockRow("Alpha Bravo", "/nfl/rankings/alpha-bravo.php?type=draft&scoring=PPR", "03/27/2026");
        var rowWithoutRankings = CreateMockRowWithNoRankingsLink("Charlie Delta", "03/25/2026");
        SetupTableRows(rowWithoutRankings, rowWithRankings);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Alpha Bravo", result[0].Name);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_ExtractsSlug_FromRankingsHref()
    {
        var row = CreateMockRow("Echo Foxtrot", "/nfl/rankings/echo-foxtrot.php?type=draft&scoring=PPR", "03/20/2026");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual("echo-foxtrot", result[0].Slug);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_ParsesLastUpdatedDate()
    {
        var row = CreateMockRow("Golf Hotel", "/nfl/rankings/golf-hotel.php?type=draft&scoring=PPR", "01/15/2026");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(new DateTime(2026, 1, 15), result[0].LastUpdated);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_SetsNullLastUpdated_WhenDateIsInvalid()
    {
        var row = CreateMockRow("India Juliet", "/nfl/rankings/india-juliet.php?type=draft&scoring=PPR", "invalid-date");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].LastUpdated);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_SetsNullLastUpdated_WhenDateIsEmpty()
    {
        var row = CreateMockRow("Kilo Lima", "/nfl/rankings/kilo-lima.php?type=draft&scoring=PPR", "");
        SetupTableRows(row);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].LastUpdated);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_ParsesMultipleExperts()
    {
        var row1 = CreateMockRow("Expert One", "/nfl/rankings/expert-one.php?type=draft&scoring=PPR", "03/27/2026");
        var row2 = CreateMockRow("Expert Two", "/nfl/rankings/expert-two.php?type=draft&scoring=PPR", "03/26/2026");
        var row3 = CreateMockRow("Expert Three", "/nfl/rankings/expert-three.php?type=draft&scoring=PPR", "03/25/2026");
        SetupTableRows(row1, row2, row3);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Expert One", result[0].Name);
        Assert.AreEqual("Expert Two", result[1].Name);
        Assert.AreEqual("Expert Three", result[2].Name);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_ReturnsEmptyList_WhenNoRowsExist()
    {
        SetupTableRows();

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_ReturnsEmptyList_WhenTableNotFound()
    {
        _mockPage
            .Setup(p => p.WaitForSelectorAsync(It.IsAny<string>(), It.IsAny<PageWaitForSelectorOptions>()))
            .ThrowsAsync(new TimeoutException("Selector not found"));

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_SkipsRows_WithInsufficientCells()
    {
        var validRow = CreateMockRow("Mike November", "/nfl/rankings/mike-november.php?type=draft&scoring=PPR", "03/27/2026");
        var shortRow = CreateMockRowWithCellCount(2);
        SetupTableRows(shortRow, validRow);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Mike November", result[0].Name);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_SkipsRows_WithNoNameAnchor()
    {
        var validRow = CreateMockRow("Oscar Papa", "/nfl/rankings/oscar-papa.php?type=draft&scoring=PPR", "03/27/2026");
        var noNameRow = CreateMockRowWithNoNameAnchor("/nfl/rankings/unknown.php?type=draft&scoring=PPR", "03/27/2026");
        SetupTableRows(noNameRow, validRow);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Oscar Papa", result[0].Name);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_SkipsRows_WithEmptyName()
    {
        var validRow = CreateMockRow("Quebec Romeo", "/nfl/rankings/quebec-romeo.php?type=draft&scoring=PPR", "03/27/2026");
        var emptyNameRow = CreateMockRow("", "/nfl/rankings/empty.php?type=draft&scoring=PPR", "03/27/2026");
        SetupTableRows(emptyNameRow, validRow);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Quebec Romeo", result[0].Name);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_SkipsRows_WithEmptyHref()
    {
        var validRow = CreateMockRow("Sierra Tango", "/nfl/rankings/sierra-tango.php?type=draft&scoring=PPR", "03/27/2026");
        var emptyHrefRow = CreateMockRow("Bad Expert", "", "03/27/2026");
        SetupTableRows(emptyHrefRow, validRow);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Sierra Tango", result[0].Name);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_SkipsRows_WithNonMatchingHref()
    {
        var validRow = CreateMockRow("Uniform Victor", "/nfl/rankings/uniform-victor.php?type=draft&scoring=PPR", "03/27/2026");
        var badHrefRow = CreateMockRow("Bad Href", "/experts/someone.php", "03/27/2026");
        SetupTableRows(badHrefRow, validRow);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Uniform Victor", result[0].Name);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_NavigatesToCorrectUrl_ForDraft()
    {
        SetupTableRows();

        await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        _mockPage.Verify(p => p.GotoAsync(
            "https://fake-base.test/nfl/rankings/?type=draft&scoring=PPR",
            It.IsAny<PageGotoOptions>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_NavigatesToCorrectUrl_ForWeekly()
    {
        SetupTableRows();

        await _scraper.ScrapeExpertsWithRankingsAsync("weekly", "half");

        _mockPage.Verify(p => p.GotoAsync(
            "https://fake-base.test/nfl/rankings/?type=weekly&scoring=half",
            It.IsAny<PageGotoOptions>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_ClosesPage_AfterSuccessfulScrape()
    {
        SetupTableRows();

        await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        _mockPage.Verify(p => p.CloseAsync(It.IsAny<PageCloseOptions>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_ClosesPage_WhenExceptionOccurs()
    {
        _mockPage
            .Setup(p => p.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions>()))
            .ThrowsAsync(new PlaywrightException("Navigation failed"));

        await Assert.ThrowsExactlyAsync<PlaywrightException>(() =>
            _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR"));

        _mockPage.Verify(p => p.CloseAsync(It.IsAny<PageCloseOptions>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("ScrapeExpertsWithRankingsAsync")]
    public async Task ScrapeExpertsWithRankingsAsync_AllReturnedExperts_HaveHasRankingsTrue()
    {
        var row1 = CreateMockRow("Expert One", "/nfl/rankings/expert-one.php?type=draft&scoring=PPR", "03/27/2026");
        var row2 = CreateMockRow("Expert Two", "/nfl/rankings/expert-two.php?type=draft&scoring=PPR", "03/26/2026");
        SetupTableRows(row1, row2);

        var result = await _scraper.ScrapeExpertsWithRankingsAsync("draft", "PPR");

        Assert.IsTrue(result.All(e => e.HasRankings));
    }

    #endregion

    #region Private Helpers

    private void SetupTableRows(params Mock<IElementHandle>[] rows)
    {
        _mockPage
            .Setup(p => p.QuerySelectorAllAsync("#expert-data tbody tr"))
            .ReturnsAsync(rows.Select(r => r.Object).ToList<IElementHandle>().AsReadOnly());
    }

    private static Mock<IElementHandle> CreateMockRow(string expertName, string rankingsHref, string lastUpdated)
    {
        var row = new Mock<IElementHandle>();
        var cells = new List<IElementHandle>();

        // Cell 0: Expert name with anchor
        var nameCell = new Mock<IElementHandle>();
        var nameAnchor = new Mock<IElementHandle>();
        nameAnchor.Setup(a => a.TextContentAsync()).ReturnsAsync(expertName);
        nameCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync(nameAnchor.Object);
        cells.Add(nameCell.Object);

        // Cell 1: Rankings link with anchor (or no anchor if href is empty handled by caller)
        var rankingsCell = new Mock<IElementHandle>();
        if (!string.IsNullOrEmpty(rankingsHref))
        {
            var rankingsAnchor = new Mock<IElementHandle>();
            rankingsAnchor.Setup(a => a.GetAttributeAsync("href")).ReturnsAsync(rankingsHref);
            rankingsCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync(rankingsAnchor.Object);
        }
        else
        {
            rankingsCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync((IElementHandle?)null);
        }
        cells.Add(rankingsCell.Object);

        // Cell 2: Accuracy rank (not used by scraper)
        var accuracyCell = new Mock<IElementHandle>();
        accuracyCell.Setup(c => c.TextContentAsync()).ReturnsAsync("5");
        cells.Add(accuracyCell.Object);

        // Cell 3: Last updated date
        var dateCell = new Mock<IElementHandle>();
        dateCell.Setup(c => c.TextContentAsync()).ReturnsAsync(lastUpdated);
        cells.Add(dateCell.Object);

        row.Setup(r => r.QuerySelectorAllAsync("td"))
            .ReturnsAsync(cells.AsReadOnly());

        return row;
    }

    private static Mock<IElementHandle> CreateMockRowWithNoRankingsLink(string expertName, string lastUpdated)
    {
        var row = new Mock<IElementHandle>();
        var cells = new List<IElementHandle>();

        // Cell 0: Expert name with anchor
        var nameCell = new Mock<IElementHandle>();
        var nameAnchor = new Mock<IElementHandle>();
        nameAnchor.Setup(a => a.TextContentAsync()).ReturnsAsync(expertName);
        nameCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync(nameAnchor.Object);
        cells.Add(nameCell.Object);

        // Cell 1: No rankings link (plain text -)
        var rankingsCell = new Mock<IElementHandle>();
        rankingsCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync((IElementHandle?)null);
        rankingsCell.Setup(c => c.TextContentAsync()).ReturnsAsync("-");
        cells.Add(rankingsCell.Object);

        // Cell 2: Accuracy rank
        var accuracyCell = new Mock<IElementHandle>();
        accuracyCell.Setup(c => c.TextContentAsync()).ReturnsAsync("-");
        cells.Add(accuracyCell.Object);

        // Cell 3: Last updated
        var dateCell = new Mock<IElementHandle>();
        dateCell.Setup(c => c.TextContentAsync()).ReturnsAsync(lastUpdated);
        cells.Add(dateCell.Object);

        row.Setup(r => r.QuerySelectorAllAsync("td"))
            .ReturnsAsync(cells.AsReadOnly());

        return row;
    }

    private static Mock<IElementHandle> CreateMockRowWithNoNameAnchor(string rankingsHref, string lastUpdated)
    {
        var row = new Mock<IElementHandle>();
        var cells = new List<IElementHandle>();

        // Cell 0: No name anchor
        var nameCell = new Mock<IElementHandle>();
        nameCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync((IElementHandle?)null);
        cells.Add(nameCell.Object);

        // Cell 1: Rankings link
        var rankingsCell = new Mock<IElementHandle>();
        var rankingsAnchor = new Mock<IElementHandle>();
        rankingsAnchor.Setup(a => a.GetAttributeAsync("href")).ReturnsAsync(rankingsHref);
        rankingsCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync(rankingsAnchor.Object);
        cells.Add(rankingsCell.Object);

        // Cell 2: Accuracy rank
        var accuracyCell = new Mock<IElementHandle>();
        accuracyCell.Setup(c => c.TextContentAsync()).ReturnsAsync("5");
        cells.Add(accuracyCell.Object);

        // Cell 3: Last updated
        var dateCell = new Mock<IElementHandle>();
        dateCell.Setup(c => c.TextContentAsync()).ReturnsAsync(lastUpdated);
        cells.Add(dateCell.Object);

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

    #endregion
}
