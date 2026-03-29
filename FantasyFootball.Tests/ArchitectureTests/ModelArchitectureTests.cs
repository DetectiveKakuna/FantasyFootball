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
                        d.Target.Namespace?.Name.StartsWith("FantasyFootball.Core.Models") == true ||
                        d.Target.Namespace?.Name.StartsWith("FantasyFootball.Core.Enums") == true),
                    "depends on types outside System.*, FantasyFootball.Core.Models, or FantasyFootball.Core.Enums"),
                "only depend on System.*, FantasyFootball.Core.Models, or FantasyFootball.Core.Enums types")
            .Check(Architecture);
    }

    [TestMethod]
    public void EntityClasses_MustBePublic()
    {
        Classes().That().ResideInNamespace("FantasyFootball.Core.Entities")
            .Should().BePublic()
            .Check(Architecture);
    }

    [TestMethod]
    public void EntityClasses_MustNotBeAbstract()
    {
        Classes().That().ResideInNamespace("FantasyFootball.Core.Entities")
            .Should().NotBeAbstract()
            .Check(Architecture);
    }

    [TestMethod]
    public void EntityClasses_MustOnlyReferenceSystemAndCoreTypes()
    {
        Classes().That().ResideInNamespace("FantasyFootball.Core.Entities")
            .Should().FollowCustomCondition(
                c => new ConditionResult(c,
                    c.Dependencies.All(d =>
                        d.Target.Namespace?.Name.StartsWith("System") == true ||
                        d.Target.Namespace?.Name.StartsWith("FantasyFootball.Core.Entities") == true ||
                        d.Target.Namespace?.Name.StartsWith("FantasyFootball.Core.Enums") == true),
                    "depends on types outside System.*, FantasyFootball.Core.Entities, or FantasyFootball.Core.Enums"),
                "only depend on System.*, FantasyFootball.Core.Entities, or FantasyFootball.Core.Enums types")
            .Check(Architecture);
    }
}
