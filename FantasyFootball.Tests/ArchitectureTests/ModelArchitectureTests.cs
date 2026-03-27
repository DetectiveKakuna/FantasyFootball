using ArchUnitNET.Domain;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.MSTestV2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FantasyFootball.Tests.ArchitectureTests;

[TestClass]
[TestCategory("Architecture")]
public class ModelArchitectureTests
{
    private static readonly Architecture Architecture = ArchitectureFixture.Architecture;

    [TestMethod]
    public void ModelClasses_MustBePublic()
    {
        Classes().That().ResideInNamespace("FantasyFootball.Core.Models")
            .Should().BePublic()
            .Check(Architecture);
    }

    [TestMethod]
    public void ModelClasses_MustNotBeAbstract()
    {
        Classes().That().ResideInNamespace("FantasyFootball.Core.Models")
            .Should().NotBeAbstract()
            .Check(Architecture);
    }

    [TestMethod]
    public void ModelClasses_MustOnlyReferenceSystemAndCoreModelTypes()
    {
        Classes().That().ResideInNamespace("FantasyFootball.Core.Models")
            .Should().FollowCustomCondition(
                c => new ConditionResult(c,
                    c.Dependencies.All(d =>
                        d.Target.Namespace?.Name.StartsWith("System") == true ||
                        d.Target.Namespace?.Name.StartsWith("FantasyFootball.Core.Models") == true),
                    "depends on types outside System.* or FantasyFootball.Core.Models"),
                "only depend on System.* or FantasyFootball.Core.Models types")
            .Check(Architecture);
    }
}
