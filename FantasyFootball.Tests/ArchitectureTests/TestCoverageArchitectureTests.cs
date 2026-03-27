using System.Reflection;
using FantasyFootball.Infrastructure.Sleeper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FantasyFootball.Tests.ArchitectureTests;

[TestClass]
[TestCategory("Architecture")]
public class TestCoverageArchitectureTests
{
    [TestMethod]
    public void AllClientMethods_MustHaveAtLeastOneTest()
    {
        var clientTypes = typeof(SleeperClient).Assembly.GetTypes()
            .Where(t => t.Name.EndsWith("Client") && t.IsClass && !t.IsAbstract && t.IsPublic);

        var testMethodNames = typeof(TestCoverageArchitectureTests).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttribute<TestMethodAttribute>() != null ||
                        m.GetCustomAttribute<DataTestMethodAttribute>() != null)
            .Select(m => m.Name)
            .ToHashSet();

        foreach (var clientType in clientTypes)
        {
            // Prefer using the interface so we test the contract, not internal helpers
            var clientInterface = clientType.GetInterfaces()
                .FirstOrDefault(i => i.Name.StartsWith("I") && i.Name.EndsWith("Client"));

            var methods = clientInterface != null
                ? clientInterface.GetMethods()
                : clientType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var hasTest = testMethodNames.Any(name =>
                    name == method.Name ||
                    name.StartsWith(method.Name + "_"));

                Assert.IsTrue(hasTest,
                    $"No test found for {clientType.Name}.{method.Name} — " +
                    $"add a test method whose name contains '{method.Name}'.");
            }
        }
    }
}
