using System.Net.Http;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.MSTestV2;
using FantasyFootball.Core.Interfaces;
using FantasyFootball.Infrastructure.Sleeper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace FantasyFootball.Tests.ArchitectureTests;

[TestClass]
[TestCategory("Architecture")]
public class ClientArchitectureTests
{
    private static readonly Architecture Architecture = ArchitectureFixture.Architecture;

    [TestMethod]
    public void ClientClasses_MustImplementIClientInterface()
    {
        Classes().That().HaveNameEndingWith("Client")
            .Should().FollowCustomCondition(
                c => new ConditionResult(c,
                    c.ImplementedInterfaces.Any(i => i.Name.StartsWith("I") && i.Name.EndsWith("Client")),
                    "does not implement an interface named I*Client"),
                "implement an interface named I*Client")
            .Check(Architecture);
    }

    [TestMethod]
    public void ClientClasses_MustBePublic()
    {
        Classes().That().HaveNameEndingWith("Client")
            .Should().BePublic()
            .Check(Architecture);
    }

    [TestMethod]
    public void ClientClasses_MustNotBeAbstract()
    {
        Classes().That().HaveNameEndingWith("Client")
            .Should().NotBeAbstract()
            .Check(Architecture);
    }

    [TestMethod]
    public void ClientClasses_MustAcceptHttpClientInConstructor()
    {
        var clientTypes = typeof(SleeperClient).Assembly.GetTypes()
            .Where(t => t.Name.EndsWith("Client") && t.IsClass && !t.IsAbstract);

        foreach (var type in clientTypes)
        {
            var hasHttpClientCtor = type.GetConstructors()
                .Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(HttpClient)));

            Assert.IsTrue(hasHttpClientCtor,
                $"{type.Name} must have a constructor that accepts HttpClient");
        }
    }

    [TestMethod]
    public void ClientClasses_MustNotInstantiateHttpClientDirectly()
    {
        Classes().That().HaveNameEndingWith("Client")
            .Should().NotCallAny(
                MethodMembers().That()
                    .HaveName(".ctor")
                    .And()
                    .AreDeclaredIn(Classes().That().HaveFullName(typeof(HttpClient).FullName!)))
            .Check(Architecture);
    }

    [TestMethod]
    public void IClientImplementors_MustHaveNameEndingWithClient()
    {
        var allTypes = typeof(SleeperClient).Assembly.GetTypes()
            .Concat(typeof(ISleeperClient).Assembly.GetTypes());

        var violations = allTypes
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => i.Name.StartsWith("I") && i.Name.EndsWith("Client")))
            .Where(t => !t.Name.EndsWith("Client"))
            .Select(t => t.FullName)
            .ToList();

        Assert.AreEqual(0, violations.Count,
            $"Classes implementing I*Client must end in 'Client'. Violations: {string.Join(", ", violations)}");
    }
}
