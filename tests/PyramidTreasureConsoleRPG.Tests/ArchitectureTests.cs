namespace PyramidTreasureConsoleRPG.Tests;

/// <summary>Warstwy Domain i Engine nie mogą znać konsoli ani interfejsu UI.</summary>
public class ArchitectureTests
{
    private static string RepoRoot()
    {
        string? dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "PyramidTreasureConsoleRPG.csproj")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        return dir ?? throw new InvalidOperationException("Nie znaleziono katalogu projektu.");
    }

    [Theory]
    [InlineData("Domain")]
    [InlineData("Engine")]
    public void PureLayers_DoNotTouchConsoleOrUi(string layer)
    {
        string folder = Path.Combine(RepoRoot(), layer);
        var offenders = Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories)
            .Where(f =>
            {
                string text = File.ReadAllText(f);
                return text.Contains("Console.", StringComparison.Ordinal)
                    || text.Contains("IGameIO", StringComparison.Ordinal)
                    || text.Contains("PyramidTreasureConsoleRPG.Ui", StringComparison.Ordinal);
            })
            .Select(Path.GetFileName)
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void NoStaticMutableState_InProductionCode()
    {
        string root = RepoRoot();
        var offenders = new[] { "Domain", "Engine", "Ui", "Infrastructure" }
            .SelectMany(layer => Directory.EnumerateFiles(Path.Combine(root, layer), "*.cs", SearchOption.AllDirectories))
            .Where(f => System.Text.RegularExpressions.Regex.IsMatch(File.ReadAllText(f), @"public static [^\n]* \{ get; set; \}"))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Empty(offenders);
    }
}
