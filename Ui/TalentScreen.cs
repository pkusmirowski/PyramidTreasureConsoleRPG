namespace PyramidTreasureConsoleRPG.Ui;

/// <summary>Wybór talentu za osiągnięty poziom (5, 10, ...). Jeden z dwóch na klasę.</summary>
public sealed class TalentScreen(IGameIO io)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        foreach (int level in hero.PendingTalentLevels().ToList())
        {
            IReadOnlyList<TalentDefinition> choices = TalentCatalog.ForClassAtLevel(hero.HeroClass, level);
            if (choices.Count == 0)
            {
                continue;
            }

            io.Header($"Talent za poziom {level}");
            io.ShowInfo("Wybór jest ostateczny.");
            int choice = io.Menu("Który talent?", choices.Select(t => $"{t.Name} – {t.Description}").ToArray());
            hero.ChooseTalent(choices[choice - 1].Id);
            io.Clear();
            io.ShowSuccess($"Wybrano talent: {choices[choice - 1].Name}.");
        }
    }
}
