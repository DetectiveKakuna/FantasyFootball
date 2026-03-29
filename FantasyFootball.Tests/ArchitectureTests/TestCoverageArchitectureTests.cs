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
        AssertAllPublicMethodsHaveTests("Client");
    }

    [TestMethod]
    public void AllScraperMethods_MustHaveAtLeastOneTest()
    {
        AssertAllPublicMethodsHaveTests("Scraper");
    }

    private static void AssertAllPublicMethodsHaveTests(string suffix)
    {
        var types = typeof(SleeperClient).Assembly.GetTypes()
            .Where(t => t.Name.EndsWith(suffix) && t.IsClass && !t.IsAbstract && t.IsPublic);

        var testMethodNames = typeof(TestCoverageArchitectureTests).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttribute<TestMethodAttribute>() != null ||
                        m.GetCustomAttribute<DataTestMethodAttribute>() != null)
            .Select(m => m.Name)
            .ToHashSet();

        foreach (var type in types)
        {
            // Prefer using the interface so we test the contract, not internal helpers
            var contractInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.Name.StartsWith("I") && i.Name.EndsWith(suffix));

            var methods = contractInterface != null
                ? contractInterface.GetMethods()
                : type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var hasTest = testMethodNames.Any(name =>
                    name == method.Name ||
                    name.StartsWith(method.Name + "_"));

                Assert.IsTrue(hasTest,
                    $"No test found for {type.Name}.{method.Name} — " +
                    $"add a test method named '{method.Name}' or starting with '{method.Name}_'.");
            }
        }
    }
}
