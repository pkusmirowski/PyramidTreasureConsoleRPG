namespace PyramidTreasureConsoleRPG.Ui;

public sealed class TavernScreen(IGameIO io, BarScreen bar, CasinoScreen casino, RestScreen rest)
{
    private readonly IGameIO io = io ?? throw new ArgumentNullException(nameof(io));
    private readonly BarScreen bar = bar ?? throw new ArgumentNullException(nameof(bar));
    private readonly CasinoScreen casino = casino ?? throw new ArgumentNullException(nameof(casino));
    private readonly RestScreen rest = rest ?? throw new ArgumentNullException(nameof(rest));

    /// <summary>Zwraca Defeat, gdy rozmowa na górze skończyła się przegraną walką.</summary>
    public CombatStatus? Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        io.ShowArt(ArtCatalog.Tavern);
        while (true)
        {
            io.Header("Tawerna \"Pod Sokołem\"");
            int choice = io.Menu(
                "Witaj w tawernie! Co chcesz zrobić?",
                hero.CurrentRegion == RegionId.Port && QuestEngine.HasNews(hero, QuestGiverId.Barman) ? "Podejdź do baru (barman ma wieści!)" : "Podejdź do baru",
                "Podejdź do kasyna i spróbuj szczęścia",
                "Zapytaj o pokój na górze",
                "Wyjdź z tawerny");
            io.Clear();
            switch (choice)
            {
                case 1:
                    bar.Run(hero);
                    break;
                case 2:
                    casino.Run(hero);
                    break;
                case 3:
                    if (rest.Run(hero) == CombatStatus.Defeat)
                    {
                        return CombatStatus.Defeat;
                    }

                    break;
                default:
                    io.WriteLine("Do zobaczenia!");
                    return null;
            }
        }
    }
}
