using System.Reflection;
using FantasyFootball.Core.Interfaces;
using FantasyFootball.Infrastructure.Sleeper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FantasyFootball.Tests.ArchitectureTests;

[TestClass]
[TestCategory("Architecture")]
public class NamingArchitectureTests
{
    private static readonly IEnumerable<MethodInfo> ProductionMethods =
        new[] { typeof(SleeperClient).Assembly, typeof(ISleeperClient).Assembly }
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Instance | BindingFlags.Static |
                                          BindingFlags.DeclaredOnly))
            .Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"));

    private static bool ReturnsTask(MethodInfo m) =>
        m.ReturnType == typeof(Task) ||
        m.ReturnType == typeof(ValueTask) ||
        (m.ReturnType.IsGenericType && (m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) ||
                                        m.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)));

    [TestMethod]
    public void AsyncMethods_MustEndWithAsync()
    {
        var violations = ProductionMethods
            .Where(ReturnsTask)
            .Where(m => !m.Name.EndsWith("Async"))
            .Select(m => $"{m.DeclaringType?.Name}.{m.Name}")
            .ToList();

        Assert.AreEqual(0, violations.Count,
            $"Methods returning Task must end in 'Async'. Violations: {string.Join(", ", violations)}");
    }

    [TestMethod]
    public void MethodsEndingWithAsync_MustReturnTask()
    {
        var violations = ProductionMethods
            .Where(m => m.Name.EndsWith("Async"))
            .Where(m => !ReturnsTask(m))
            .Select(m => $"{m.DeclaringType?.Name}.{m.Name}")
            .ToList();

        Assert.AreEqual(0, violations.Count,
            $"Methods ending in 'Async' must return Task or Task<T>. Violations: {string.Join(", ", violations)}");
    }
}
