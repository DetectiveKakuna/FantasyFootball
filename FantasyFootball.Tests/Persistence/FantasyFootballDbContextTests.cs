using FantasyFootball.Core.Entities;
using FantasyFootball.Core.Enums;
using FantasyFootball.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FantasyFootball.Tests.Persistence;

[TestClass]
[TestCategory("Persistence")]
public class FantasyFootballDbContextTests
{
    private FantasyFootballDbContext _context = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<FantasyFootballDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new FantasyFootballDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    #region PlayerEntity

    [TestMethod]
    public async Task CanInsertAndRetrievePlayer()
    {
        var player = CreateTestPlayer();

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _context.Players.FirstAsync();
        Assert.AreEqual(player.SleeperId, result.SleeperId);
        Assert.AreEqual(player.FirstName, result.FirstName);
        Assert.AreEqual(player.LastName, result.LastName);
        Assert.AreEqual(player.Position, result.Position);
        Assert.AreEqual(player.Team, result.Team);
    }

    [TestMethod]
    public async Task PlayerSleeperId_MustBeUnique()
    {
        var player1 = CreateTestPlayer();
        var player2 = CreateTestPlayer();

        _context.Players.Add(player1);
        _context.Players.Add(player2);

        await Assert.ThrowsExceptionAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [TestMethod]
    public async Task PlayerFantasyPositions_StoredAsCommaSeparatedString()
    {
        var player = CreateTestPlayer();
        player.FantasyPositions = [Position.QB, Position.WR];

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _context.Players.FirstAsync();
        CollectionAssert.AreEqual(new List<Position> { Position.QB, Position.WR }, result.FantasyPositions);
    }

    [TestMethod]
    public async Task PlayerFantasyPositions_EmptyList_RoundTrips()
    {
        var player = CreateTestPlayer();
        player.FantasyPositions = [];

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _context.Players.FirstAsync();
        Assert.AreEqual(0, result.FantasyPositions.Count);
    }

    [TestMethod]
    public async Task PlayerEnums_StoredAsStrings()
    {
        var player = CreateTestPlayer();
        player.InjuryStatus = InjuryStatus.Questionable;

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        var rawValue = await _context.Database
            .SqlQueryRaw<string>("SELECT \"InjuryStatus\" AS \"Value\" FROM \"Players\" LIMIT 1")
            .FirstAsync();

        Assert.AreEqual("Questionable", rawValue);
    }

    [TestMethod]
    public async Task PlayerFantasyProsId_MustBeUnique()
    {
        var player1 = CreateTestPlayer();
        var player2 = CreateTestPlayer();
        player2.SleeperId = 9999;

        _context.Players.Add(player1);
        _context.Players.Add(player2);

        await Assert.ThrowsExceptionAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    #endregion

    #region ExpertEntity

    [TestMethod]
    public async Task CanInsertAndRetrieveExpert()
    {
        var expert = CreateTestExpert();

        _context.Experts.Add(expert);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _context.Experts.FirstAsync();
        Assert.AreEqual(expert.Name, result.Name);
        Assert.AreEqual(expert.Slug, result.Slug);
        Assert.AreEqual(expert.CustomWeight, result.CustomWeight);
    }

    [TestMethod]
    public async Task ExpertSlug_MustBeUnique()
    {
        var expert1 = CreateTestExpert();
        var expert2 = CreateTestExpert();

        _context.Experts.Add(expert1);
        _context.Experts.Add(expert2);

        await Assert.ThrowsExceptionAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [TestMethod]
    public async Task ExpertCustomWeight_DefaultsToOne()
    {
        var expert = new ExpertEntity { Name = "Test", Slug = "test" };

        _context.Experts.Add(expert);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _context.Experts.FirstAsync();
        Assert.AreEqual(1.0, result.CustomWeight);
    }

    [TestMethod]
    public async Task ExpertDraftAccuracy_OwnedEntity_RoundTrips()
    {
        var expert = CreateTestExpert();
        expert.DraftAccuracy = new AccuracyEntity
        {
            OverallRank = 5,
            Qb = 3,
            Rb = 7,
            Wr = 2,
            Te = 10,
            K = 1,
            Dst = 4,
            Idp = 15
        };

        _context.Experts.Add(expert);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _context.Experts.FirstAsync();
        Assert.IsNotNull(result.DraftAccuracy);
        Assert.AreEqual(5, result.DraftAccuracy.OverallRank);
        Assert.AreEqual(3, result.DraftAccuracy.Qb);
        Assert.AreEqual(7, result.DraftAccuracy.Rb);
        Assert.AreEqual(2, result.DraftAccuracy.Wr);
        Assert.AreEqual(10, result.DraftAccuracy.Te);
        Assert.AreEqual(1, result.DraftAccuracy.K);
        Assert.AreEqual(4, result.DraftAccuracy.Dst);
        Assert.AreEqual(15, result.DraftAccuracy.Idp);
    }

    [TestMethod]
    public async Task ExpertDraftAccuracy_ColumnPrefix_IsCorrect()
    {
        var expert = CreateTestExpert();
        expert.DraftAccuracy = new AccuracyEntity { OverallRank = 1 };

        _context.Experts.Add(expert);
        await _context.SaveChangesAsync();

        var rawValue = await _context.Database
            .SqlQueryRaw<int>("SELECT \"DraftAccuracy_OverallRank\" AS \"Value\" FROM \"Experts\" LIMIT 1")
            .FirstAsync();

        Assert.AreEqual(1, rawValue);
    }

    [TestMethod]
    public async Task ExpertRankings_StoredAsJson()
    {
        var expert = CreateTestExpert();
        expert.DraftRankings = [1, 2, 3, 4, 5];
        expert.WeeklyRankings = [10, 20, 30];
        expert.RosRankings = [100];

        _context.Experts.Add(expert);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _context.Experts.FirstAsync();
        CollectionAssert.AreEqual(new List<int> { 1, 2, 3, 4, 5 }, result.DraftRankings);
        CollectionAssert.AreEqual(new List<int> { 10, 20, 30 }, result.WeeklyRankings);
        CollectionAssert.AreEqual(new List<int> { 100 }, result.RosRankings);
    }

    #endregion

    #region Helpers

    private static PlayerEntity CreateTestPlayer() => new()
    {
        SleeperId = 4046,
        FantasyProsId = 0,
        FirstName = "Patrick",
        LastName = "Mahomes",
        FullName = "Patrick Mahomes",
        Position = Position.QB,
        FantasyPositions = [Position.QB],
        Team = NflTeam.KC,
        Active = true,
        LastUpdated = DateTime.UtcNow
    };

    private static ExpertEntity CreateTestExpert() => new()
    {
        Name = "Test Expert",
        Slug = "test-expert",
        CustomWeight = 1.5
    };

    #endregion
}
