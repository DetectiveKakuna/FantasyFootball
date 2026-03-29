using ArchUnitNET.Domain;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.MSTestV2;
using FantasyFootball.Infrastructure.FantasyPros;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FantasyFootball.Tests.ArchitectureTests;

[TestClass]
[TestCategory("Architecture")]
public class ScraperArchitectureTests
{
    private static readonly Architecture Architecture = ArchitectureFixture.Architecture;

    [TestMethod]
    public void ScraperClasses_MustImplementIScraperInterface()
    {
        Classes().That().HaveNameEndingWith("Scraper")
            .Should().FollowCustomCondition(
                c => new ConditionResult(c,
                    c.ImplementedInterfaces.Any(i => i.Name.StartsWith("I") && i.Name.EndsWith("Scraper")),
                    "does not implement an interface named I*Scraper"),
                "implement an interface named I*Scraper")
            .Check(Architecture);
    }

    [TestMethod]
    public void ScraperClasses_MustBePublic()
    {
        Classes().That().HaveNameEndingWith("Scraper")
            .Should().BePublic()
            .Check(Architecture);
    }

    [TestMethod]
    public void ScraperClasses_MustNotBeAbstract()
    {
        Classes().That().HaveNameEndingWith("Scraper")
            .Should().NotBeAbstract()
            .Check(Architecture);
    }

    [TestMethod]
    public void IScraperImplementors_MustHaveNameEndingWithScraper()
    {
        var allTypes = typeof(FantasyProsAccuracyScraper).Assembly.GetTypes()
            .Concat(typeof(Core.Interfaces.IFantasyProsAccuracyScraper).Assembly.GetTypes());

        var violations = allTypes
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => i.Name.StartsWith("I") && i.Name.EndsWith("Scraper")))
            .Where(t => !t.Name.EndsWith("Scraper"))
            .Select(t => t.FullName)
            .ToList();

        Assert.AreEqual(0, violations.Count,
            $"Classes implementing I*Scraper must end in 'Scraper'. Violations: {string.Join(", ", violations)}");
    }
}
