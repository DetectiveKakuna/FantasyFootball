using System.Net;
using System.Text.Json;
using FantasyFootball.Core.Models;
using FantasyFootball.Infrastructure.Sleeper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace FantasyFootball.Tests.Sleeper;

[TestClass]
public class SleeperClientTests
{
    private Mock<HttpMessageHandler> _handlerMock = null!;
    private SleeperClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost/")
        };
        _client = new SleeperClient(httpClient);
    }

    #region GetLeagueAsync

    [TestMethod]
    [TestCategory("GetLeagueAsync")]
    public async Task GetLeagueAsync_ReturnsLeague_WhenApiRespondsSuccessfully()
    {
        var expected = new SleeperLeague
        {
            LeagueId = "test_league_id",
            Name = "Test League",
            Season = "2000",
            Status = "complete",
            DraftId = "test_draft_id",
            TotalRosters = 12,
            RosterPositions = ["QB", "RB", "WR", "TE", "K", "DEF"]
        };
        SetupResponse("league/test_league_id", expected);

        var result = await _client.GetLeagueAsync("test_league_id");

        Assert.IsNotNull(result);
        AssertAreEqualByJson(expected, result);
    }

    [DataTestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [TestCategory("GetLeagueAsync")]
    public async Task GetLeagueAsync_ThrowsHttpRequestException_OnHttpError(HttpStatusCode statusCode)
    {
        SetupErrorResponse("league/invalid", statusCode);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
            _client.GetLeagueAsync("invalid"));
    }

    #endregion

    #region GetRostersAsync

    [TestMethod]
    [TestCategory("GetRostersAsync")]
    public async Task GetRostersAsync_ReturnsRosters_WhenApiRespondsSuccessfully()
    {
        var expected = new List<SleeperRoster>
        {
            new() { RosterId = 1, OwnerId = "test_user_001", LeagueId = "test_league_id", Players = ["test_player_1", "test_player_2"], Starters = ["test_player_1"] },
            new() { RosterId = 2, OwnerId = "test_user_002", LeagueId = "test_league_id", Players = ["test_player_3"], Starters = ["test_player_3"], Reserve = ["test_player_4"] }
        };
        SetupResponse("rosters", expected);

        var result = await _client.GetRostersAsync("test_league_id");

        Assert.IsNotNull(result);
        AssertAreEqualByJson(expected, result);
    }

    [TestMethod]
    [TestCategory("GetRostersAsync")]
    public async Task GetRostersAsync_ReturnsEmptyList_WhenLeagueHasNoRosters()
    {
        SetupResponse("rosters", new List<SleeperRoster>());

        var result = await _client.GetRostersAsync("test_league_id");

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [DataTestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [TestCategory("GetRostersAsync")]
    public async Task GetRostersAsync_ThrowsHttpRequestException_OnHttpError(HttpStatusCode statusCode)
    {
        SetupErrorResponse("rosters", statusCode);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
            _client.GetRostersAsync("invalid"));
    }

    #endregion

    #region GetUsersAsync

    [TestMethod]
    [TestCategory("GetUsersAsync")]
    [Timeout(5000)]
    public async Task GetUsersAsync_ReturnsUsers_WhenApiRespondsSuccessfully()
    {
        var expected = new List<SleeperUser>
        {
            new() { UserId = "test_user_001", DisplayName = "TestCommish", LeagueId = "test_league_id", IsOwner = true },
            new() { UserId = "test_user_002", DisplayName = "TestUser", LeagueId = "test_league_id", IsOwner = false }
        };
        SetupResponse("users", expected);

        var result = await _client.GetUsersAsync("test_league_id");

        Assert.IsNotNull(result);
        AssertAreEqualByJson(expected, result);
    }

    [TestMethod]
    [TestCategory("GetUsersAsync")]
    [Timeout(5000)]
    public async Task GetUsersAsync_IdentifiesLeagueOwnerCorrectly()
    {
        var expected = new List<SleeperUser>
        {
            new() { UserId = "111", DisplayName = "User1", LeagueId = "test_league_id", IsOwner = false },
            new() { UserId = "222", DisplayName = "Owner", LeagueId = "test_league_id", IsOwner = true },
            new() { UserId = "333", DisplayName = "User3", LeagueId = "test_league_id", IsOwner = false }
        };
        SetupResponse("users", expected);

        var result = await _client.GetUsersAsync("test_league_id");

        var owner = result!.Single(u => u.IsOwner);
        Assert.AreEqual("222", owner.UserId);
        Assert.AreEqual("Owner", owner.DisplayName);
    }

    [DataTestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [TestCategory("GetUsersAsync")]
    public async Task GetUsersAsync_ThrowsHttpRequestException_OnHttpError(HttpStatusCode statusCode)
    {
        SetupErrorResponse("users", statusCode);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
            _client.GetUsersAsync("invalid"));
    }

    #endregion

    #region GetDraftsAsync

    [TestMethod]
    [TestCategory("GetDraftsAsync")]
    public async Task GetDraftsAsync_ReturnsDrafts_WhenApiRespondsSuccessfully()
    {
        var expected = new List<SleeperDraft>
        {
            new()
            {
                DraftId = "test_draft_id",
                LeagueId = "test_league_id",
                Status = "complete",
                Type = "snake",
                Season = "2000",
                Settings = new SleeperDraftSettings { Rounds = 16, Teams = 12, PickTimer = 90 }
            }
        };
        SetupResponse("drafts", expected);

        var result = await _client.GetDraftsAsync("test_league_id");

        Assert.IsNotNull(result);
        AssertAreEqualByJson(expected, result);
    }

    [TestMethod]
    [TestCategory("GetDraftsAsync")]
    public async Task GetDraftsAsync_ReturnsEmptyList_WhenNoDraftsExist()
    {
        SetupResponse("drafts", new List<SleeperDraft>());

        var result = await _client.GetDraftsAsync("test_league_id");

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [DataTestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [TestCategory("GetDraftsAsync")]
    public async Task GetDraftsAsync_ThrowsHttpRequestException_OnHttpError(HttpStatusCode statusCode)
    {
        SetupErrorResponse("drafts", statusCode);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
            _client.GetDraftsAsync("invalid"));
    }

    #endregion

    #region GetAllPlayersAsync

    [TestMethod]
    [TestCategory("GetAllPlayersAsync")]
    public async Task GetAllPlayersAsync_ReturnsPlayerDictionary_WhenApiRespondsSuccessfully()
    {
        var expected = new Dictionary<string, SleeperPlayer>
        {
            ["test_player_1"] = new() { PlayerId = "test_player_1", FirstName = "Test", LastName = "PlayerOne", FullName = "Test PlayerOne", Position = "RB", Team = "TST", Active = true, Age = 25, YearsExp = 3, DepthChartOrder = 1 },
            ["test_player_2"] = new() { PlayerId = "test_player_2", FirstName = "Test", LastName = "PlayerTwo", FullName = "Test PlayerTwo", Position = "WR", Team = "TST", Active = true, Age = 24, YearsExp = 2 }
        };
        SetupResponse("players/nfl", expected);

        var result = await _client.GetAllPlayersAsync();

        Assert.IsNotNull(result);
        AssertAreEqualByJson(expected, result);
    }

    [TestMethod]
    [TestCategory("GetAllPlayersAsync")]
    public async Task GetAllPlayersAsync_HandlesPlayerWithNullOptionalFields()
    {
        var expected = new Dictionary<string, SleeperPlayer>
        {
            ["test_player_unknown"] = new() { PlayerId = "test_player_unknown", FirstName = "Unknown", LastName = "Player", Position = null, Team = null, Active = false, InjuryStatus = null, DepthChartOrder = null }
        };
        SetupResponse("players/nfl", expected);

        var result = await _client.GetAllPlayersAsync();

        Assert.IsNotNull(result);
        AssertAreEqualByJson(expected, result);
    }

    [TestMethod]
    [TestCategory("GetAllPlayersAsync")]
    public async Task GetAllPlayersAsync_ReturnsEmptyDictionary_WhenNoPlayersExist()
    {
        SetupResponse("players/nfl", new Dictionary<string, SleeperPlayer>());

        var result = await _client.GetAllPlayersAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [DataTestMethod]
    [DataRow(HttpStatusCode.NotFound)]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.ServiceUnavailable)]
    [TestCategory("GetAllPlayersAsync")]
    public async Task GetAllPlayersAsync_ThrowsHttpRequestException_OnHttpError(HttpStatusCode statusCode)
    {
        SetupErrorResponse("players/nfl", statusCode);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(_client.GetAllPlayersAsync);
    }

    #endregion

    #region Private Helpers

    private static void AssertAreEqualByJson<T>(T expected, T actual)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        Assert.AreEqual(
            JsonSerializer.Serialize(expected, options),
            JsonSerializer.Serialize(actual, options));
    }

    private void SetupResponse<T>(string url, T responseObject, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains(url)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonSerializer.Serialize(responseObject))
            });
    }

    private void SetupErrorResponse(string url, HttpStatusCode statusCode)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.ToString().Contains(url)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(string.Empty)
            });
    }

    #endregion
}
