using ArchUnitNET.Domain;
using ArchUnitNET.MSTestV2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FantasyFootball.Tests.ArchitectureTests;

[TestClass]
[TestCategory("Architecture")]
public class PersistenceArchitectureTests
{
    private static readonly Architecture Architecture = ArchitectureFixture.Architecture;

    [TestMethod]
    public void DbContextClasses_MustResideInPersistenceNamespace()
    {
        Classes().That().HaveNameEndingWith("DbContext")
            .Should().ResideInNamespace("FantasyFootball.Infrastructure.Persistence")
            .Check(Architecture);
    }

    [TestMethod]
    public void EntityClasses_MustResideInEntitiesNamespace()
    {
        Classes().That().HaveNameEndingWith("Entity")
            .Should().ResideInNamespace("FantasyFootball.Core.Entities")
            .Check(Architecture);
    }

    [TestMethod]
    public void EntityClasses_MustNotDependOnInfrastructure()
    {
        Classes().That().ResideInNamespace("FantasyFootball.Core.Entities")
            .Should().NotDependOnAnyTypesThat().ResideInNamespace("FantasyFootball.Infrastructure")
            .Check(Architecture);
    }
}
