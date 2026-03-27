using FantasyFootball.Infrastructure.FantasyPros;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FantasyFootball.Tests.FantasyPros;

[TestClass]
public class FantasyProsAccuracyScraperTests
{
    private Mock<IBrowser> _mockBrowser = null!;
    private Mock<IPage> _mockPage = null!;
    private FantasyProsAccuracyScraper _scraper = null!;

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

        _scraper = new FantasyProsAccuracyScraper(_mockBrowser.Object);
    }

    #region ScrapeAsync

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_ReturnsExperts_WithValidData()
    {
        var row = CreateMockRow("1", "Pat Fitzmaurice - FantasyPros", "5", "3", "7", "2", "10", "8", "12");
        SetupTableRows(row);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual(1, result.Count);
        var expert = result[0];
        Assert.AreEqual("Pat Fitzmaurice", expert.Name);
        Assert.AreEqual("FantasyPros", expert.Site);
        Assert.AreEqual("pat-fitzmaurice", expert.Slug);
        Assert.AreEqual(1, expert.OverallRank);
        Assert.AreEqual(5, expert.AccuracyRankQb);
        Assert.AreEqual(3, expert.AccuracyRankRb);
        Assert.AreEqual(7, expert.AccuracyRankWr);
        Assert.AreEqual(2, expert.AccuracyRankTe);
        Assert.AreEqual(10, expert.AccuracyRankK);
        Assert.AreEqual(8, expert.AccuracyRankDst);
        Assert.AreEqual(12, expert.AccuracyRankIdp);
        Assert.AreEqual(2025, expert.AccuracyYear);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_ReturnsNullOverallRank_WhenRankIsDash()
    {
        var row = CreateMockRow("-", "Jake Ciely - The Athletic", "1", "2", "3", "4", "5", "6", "7");
        SetupTableRows(row);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].OverallRank);
        Assert.AreEqual("Jake Ciely", result[0].Name);
        Assert.AreEqual("The Athletic", result[0].Site);
    }

    [DataTestMethod]
    [DataRow("-")]
    [DataRow("")]
    [DataRow(null)]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_ReturnsNullPositionRank_WhenDataSortIsInvalid(string? dataSortValue)
    {
        var row = CreateMockRow("1", "Test Expert - TestSite", dataSortValue, "2", "3", "4", "5", "6", "7");
        SetupTableRows(row);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].AccuracyRankQb);
        Assert.AreEqual(2, result[0].AccuracyRankRb);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_ReturnsNullPositionRank_WhenDataSortIsSentinelValue()
    {
        var row = CreateMockRow("1", "Test Expert - TestSite", "100000", "99999", "10000", "1", "2", "3", "4");
        SetupTableRows(row);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].AccuracyRankQb);
        Assert.IsNull(result[0].AccuracyRankRb);
        Assert.IsNull(result[0].AccuracyRankWr);
        Assert.AreEqual(1, result[0].AccuracyRankTe);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_GeneratesCorrectSlug_FromExpertName()
    {
        var row = CreateMockRow("1", "Joe D'Angelo - ESPN", "1", "2", "3", "4", "5", "6", "7");
        SetupTableRows(row);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual("joe-dangelo", result[0].Slug);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_SlugHandlesMultipleSpacesAndPunctuation()
    {
        var row = CreateMockRow("1", "Dr. John St. Pierre Jr. - NFL Network", "1", "2", "3", "4", "5", "6", "7");
        SetupTableRows(row);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual("dr-john-st-pierre-jr", result[0].Slug);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_ParsesMultipleExperts()
    {
        var row1 = CreateMockRow("1", "Expert One - SiteA", "1", "2", "3", "4", "5", "6", "7");
        var row2 = CreateMockRow("2", "Expert Two - SiteB", "8", "9", "10", "11", "12", "13", "14");
        var row3 = CreateMockRow("3", "Expert Three - SiteC", "15", "-", "17", "-", "19", "20", "-");
        SetupTableRows(row1, row2, row3);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Expert One", result[0].Name);
        Assert.AreEqual("Expert Two", result[1].Name);
        Assert.AreEqual("Expert Three", result[2].Name);
        Assert.AreEqual(1, result[0].OverallRank);
        Assert.AreEqual(2, result[1].OverallRank);
        Assert.AreEqual(3, result[2].OverallRank);
        Assert.IsNull(result[2].AccuracyRankRb);
        Assert.IsNull(result[2].AccuracyRankTe);
        Assert.IsNull(result[2].AccuracyRankIdp);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_ReturnsEmptyList_WhenNoRowsExist()
    {
        SetupTableRows();

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_SkipsRows_WithInsufficientCells()
    {
        var validRow = CreateMockRow("1", "Valid Expert - ValidSite", "1", "2", "3", "4", "5", "6", "7");
        var shortRow = CreateMockRowWithCellCount(3);
        SetupTableRows(shortRow, validRow);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Valid Expert", result[0].Name);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_SkipsRows_WithNoAnchorTag()
    {
        var validRow = CreateMockRow("1", "Valid Expert - ValidSite", "1", "2", "3", "4", "5", "6", "7");
        var noAnchorRow = CreateMockRowWithNoAnchor();
        SetupTableRows(noAnchorRow, validRow);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Valid Expert", result[0].Name);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_SetsAccuracyYear_FromParameter()
    {
        var row = CreateMockRow("1", "Test Expert - TestSite", "1", "2", "3", "4", "5", "6", "7");
        SetupTableRows(row);

        var result2024 = await _scraper.ScrapeAsync(2024);

        Assert.AreEqual(2024, result2024[0].AccuracyYear);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_HandlesNameWithoutSiteSeparator()
    {
        var row = CreateMockRow("1", "SoloExpert", "1", "2", "3", "4", "5", "6", "7");
        SetupTableRows(row);

        var result = await _scraper.ScrapeAsync(2025);

        Assert.AreEqual("SoloExpert", result[0].Name);
        Assert.AreEqual("", result[0].Site);
        Assert.AreEqual("soloexpert", result[0].Slug);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_ClosesPage_AfterSuccessfulScrape()
    {
        SetupTableRows();

        await _scraper.ScrapeAsync(2025);

        _mockPage.Verify(p => p.CloseAsync(It.IsAny<PageCloseOptions>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_ClosesPage_WhenExceptionOccurs()
    {
        _mockPage
            .Setup(p => p.GotoAsync(It.IsAny<string>(), It.IsAny<PageGotoOptions>()))
            .ThrowsAsync(new PlaywrightException("Navigation failed"));

        await Assert.ThrowsExactlyAsync<PlaywrightException>(() =>
            _scraper.ScrapeAsync(2025));

        _mockPage.Verify(p => p.CloseAsync(It.IsAny<PageCloseOptions>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("ScrapeAsync")]
    public async Task ScrapeAsync_NavigatesToCorrectUrl()
    {
        SetupTableRows();

        await _scraper.ScrapeAsync(2025);

        _mockPage.Verify(p => p.GotoAsync(
            "https://www.fantasypros.com/nfl/accuracy/draft.php?year=2025",
            It.IsAny<PageGotoOptions>()), Times.Once);
    }

    #endregion

    #region GenerateSlug

    [DataTestMethod]
    [DataRow("Pat Fitzmaurice", "pat-fitzmaurice")]
    [DataRow("Jake Ciely", "jake-ciely")]
    [DataRow("Joe D'Angelo", "joe-dangelo")]
    [DataRow("Dr. John St. Pierre Jr.", "dr-john-st-pierre-jr")]
    [DataRow("Single", "single")]
    [DataRow("ALL CAPS NAME", "all-caps-name")]
    [TestCategory("GenerateSlug")]
    public void GenerateSlug_ProducesExpectedSlug(string name, string expectedSlug)
    {
        var result = FantasyProsAccuracyScraper.GenerateSlug(name);

        Assert.AreEqual(expectedSlug, result);
    }

    #endregion

    #region Private Helpers

    private void SetupTableRows(params Mock<IElementHandle>[] rows)
    {
        _mockPage
            .Setup(p => p.QuerySelectorAllAsync("table tbody tr"))
            .ReturnsAsync(rows.Select(r => r.Object).ToList<IElementHandle>().AsReadOnly());
    }

    private static Mock<IElementHandle> CreateMockRow(
        string rankText,
        string expertText,
        string? qb, string? rb, string? wr, string? te, string? k, string? dst, string? idp)
    {
        var row = new Mock<IElementHandle>();
        var cells = new List<IElementHandle>();

        // Cell 0: overall rank
        var rankCell = new Mock<IElementHandle>();
        rankCell.Setup(c => c.TextContentAsync()).ReturnsAsync(rankText);
        cells.Add(rankCell.Object);

        // Cell 1: expert name with anchor
        var expertCell = new Mock<IElementHandle>();
        var anchor = new Mock<IElementHandle>();
        anchor.Setup(a => a.TextContentAsync()).ReturnsAsync(expertText);
        expertCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync(anchor.Object);
        cells.Add(expertCell.Object);

        // Cells 2-8: position accuracy ranks
        foreach (var val in new[] { qb, rb, wr, te, k, dst, idp })
        {
            var cell = new Mock<IElementHandle>();
            cell.Setup(c => c.GetAttributeAsync("data-sort")).ReturnsAsync(val);
            cells.Add(cell.Object);
        }

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

        // Cell 0: rank
        var rankCell = new Mock<IElementHandle>();
        rankCell.Setup(c => c.TextContentAsync()).ReturnsAsync("1");
        cells.Add(rankCell.Object);

        // Cell 1: no anchor tag
        var expertCell = new Mock<IElementHandle>();
        expertCell.Setup(c => c.QuerySelectorAsync("a")).ReturnsAsync((IElementHandle?)null);
        cells.Add(expertCell.Object);

        // Cells 2-8: position cells
        for (var i = 0; i < 7; i++)
        {
            var cell = new Mock<IElementHandle>();
            cell.Setup(c => c.GetAttributeAsync("data-sort")).ReturnsAsync("1");
            cells.Add(cell.Object);
        }

        row.Setup(r => r.QuerySelectorAllAsync("td"))
            .ReturnsAsync(cells.AsReadOnly());

        return row;
    }

    #endregion
}
