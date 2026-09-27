namespace PyramidTreasureConsoleRPG.Ui;

public sealed class TavernScreen
{
    private readonly IGameIO io;
    private readonly BarScreen bar;
    private readonly CasinoScreen casino;
    private readonly RestScreen rest;

    public TavernScreen(IGameIO io, BarScreen bar, CasinoScreen casino, RestScreen rest)
    {
        this.io = io ?? throw new ArgumentNullException(nameof(io));
        this.bar = bar ?? throw new ArgumentNullException(nameof(bar));
        this.casino = casino ?? throw new ArgumentNullException(nameof(casino));
        this.rest = rest ?? throw new ArgumentNullException(nameof(rest));
    }

    public void Run(Hero hero)
    {
        ArgumentNullException.ThrowIfNull(hero);
        while (true)
        {
            io.Header("Tawerna \"Pod Sokołem\"");
            int choice = io.Menu(
                "Witaj w tawernie! Co chcesz zrobić?",
                Story.BarmanHasNews(hero) ? "Podejdź do baru (barman ma wieści!)" : "Podejdź do baru",
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
                    rest.Run(hero);
                    break;
                default:
                    io.WriteLine("Do zobaczenia!");
                    return;
            }
        }
    }
}
