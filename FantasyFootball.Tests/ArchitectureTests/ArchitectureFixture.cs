using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

namespace FantasyFootball.Tests.ArchitectureTests;

internal static class ArchitectureFixture
{
    internal static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(LoadProductionAssemblies())
        .Build();

    private static System.Reflection.Assembly[] LoadProductionAssemblies() =>
    [
        typeof(Core.Interfaces.IFantasyProsAccuracyScraper).Assembly,
        typeof(Infrastructure.FantasyPros.FantasyProsAccuracyScraper).Assembly,
    ];
}
