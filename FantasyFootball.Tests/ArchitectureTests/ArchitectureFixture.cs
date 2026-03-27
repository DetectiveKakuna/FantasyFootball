using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

namespace FantasyFootball.Tests.ArchitectureTests;

internal static class ArchitectureFixture
{
    internal static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(LoadProductionAssemblies())
        .Build();

    private static System.Reflection.Assembly[] LoadProductionAssemblies() =>
        [.. AppDomain.CurrentDomain.GetAssemblies()
            .Where(a =>
                a.GetName().Name?.StartsWith("FantasyFootball") == true &&
                a.GetName().Name?.Contains("Tests") == false)];
}
